using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Reflection;
using System.Data.Sql;
using System.Data.SqlClient;
using System.Text;
using System.Text.RegularExpressions;
using System.Runtime.Serialization;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Proxies;

namespace SnowLib.DB
{
    /// <summary>
    /// Класс, реализующий основную логику преобразования 
    /// интерфейса в вызовы хранимых процедур
    /// </summary>
    /// <typeparam name="T">Тип интерфейса</typeparam>
    public class SqlCommandProxy<T> : RealProxy
    {

        public static T Create(string sqlConnectionString, SqlCredential sqlCredential)
        {
            SqlCommandProxy<T> realProxy = new SqlCommandProxy<T>(sqlConnectionString, sqlCredential);
            return (T)realProxy.GetTransparentProxy();
        }

        private readonly string connectionString;
        private readonly SqlCredential credential;

        private SqlCommandProxy(string sqlConnectionString, SqlCredential sqlCredential)
            : base(typeof(T))
        {
            if (String.IsNullOrEmpty(sqlConnectionString))
                throw new ArgumentNullException("sqlConnectionString");
            this.connectionString = sqlConnectionString;
            this.credential = sqlCredential;
        }

        public override System.Runtime.Remoting.Messaging.IMessage Invoke(System.Runtime.Remoting.Messaging.IMessage msg)
        {
            IMethodCallMessage methodCall = (IMethodCallMessage)msg;
            MethodInfo methodInfo = (MethodInfo)methodCall.MethodBase;
            ParameterInfo[] parameters = methodInfo.GetParameters();
            SqlCommand command = getSqlCommand(methodInfo);
            bool hasOutput = false;
            SpmSharedItemPool itemPool = null;
            int itemPoolArgIndex = -1;
            for (int i = 0; i < parameters.Length; i++)
            {
                if (parameters[i].ParameterType.Equals(typeof(SpmSharedItemPool)))
                {
                    if (itemPoolArgIndex < 0)
                    {
                        itemPoolArgIndex = i;
                        itemPool = (SpmSharedItemPool)methodCall.Args[i];
                    }
                    else
                        throw new ArgumentException("SpmItemPool в параметрах встречается более одного раза");
                }
                else
                {
                    hasOutput |= command.Parameters.Add(
                        getSqlParameter(parameters[i], methodCall.Args[i])).Direction != ParameterDirection.Input;
                }
            }
            object result = null;
            bool mustClosed = false;
            try
            {
                command.Connection = 
                    new SqlConnection(this.connectionString, this.credential);
                mustClosed = true;
                command.Connection.Open();
                result = executeCommand(methodInfo, command, ref mustClosed, itemPool);
            }
            catch (Exception ex)
            {
                return new ReturnMessage(ex, msg as IMethodCallMessage);
            }
            finally
            {
                if (mustClosed)
                    command.Connection.Close();
                command.Dispose();
            }
            object[] outArgs = null;
            if (hasOutput)
            {
                outArgs = new object[methodCall.ArgCount];
                int currCount = itemPoolArgIndex < 0 ? outArgs.Length : itemPoolArgIndex;
                for (int i = 0; i < currCount; i++)
                    outArgs[i] = parameters[i].ParameterType.IsByRef ? command.Parameters[i].Value : null;
                for (int i = ++currCount; i < outArgs.Length; i++)
                    outArgs[i] = parameters[i-1].ParameterType.IsByRef ? command.Parameters[i-1].Value : null;
            }
            return new ReturnMessage(result, outArgs, outArgs == null ? 0 : outArgs.Length, methodCall.LogicalCallContext, methodCall);
        }

        private SqlCommand getSqlCommand(MethodInfo methodInfo)
        {
            string commandText = null;
            CommandType commandType = CommandType.StoredProcedure;
            int commandTimeout = SqlCommandAttribute.DefaultTimeout;
            SqlCommandAttribute spa = methodInfo.GetCustomAttribute<SqlCommandAttribute>();
            if (spa != null)
            {
                commandText = spa.CommandText;
                commandType = spa.CommandType;
                commandTimeout = spa.CommandTimeout;
            };
            commandText = String.IsNullOrEmpty(commandText) ? methodInfo.Name : commandText;
            return new SqlCommand(commandText)
            {
                CommandType = commandType,
                CommandTimeout = commandTimeout
            };
        }

        private SqlParameter getSqlParameter(ParameterInfo parameterInfo, object value)
        {
            SqlParameter sp = new SqlParameter();
            SqlCommandParameterAttribute sppa = parameterInfo.GetCustomAttribute<SqlCommandParameterAttribute>();
            int size = 2048;
            string name = null;
            if (sppa != null)
            {
                name = sppa.Name;
                size = sppa.Size;
            }
            name = String.IsNullOrEmpty(name) ? parameterInfo.Name : name;
            sp.ParameterName = '@' + name;
            if (value == null)
            {
                Type realType;
                if (parameterInfo.ParameterType.IsByRef)
                    realType = parameterInfo.ParameterType.GetElementType();
                else
                    realType = parameterInfo.ParameterType;
                if (realType.Equals(typeof(string)))
                {
                    sp.Value = DBNull.Value;
                    sp.SqlDbType = SqlDbType.NVarChar;
                    sp.Size = size;
                }
                else
                {
                    if (realType.Equals(typeof(byte[])))
                    {
                        sp.Value = DBNull.Value;
                        sp.SqlDbType = SqlDbType.VarBinary;
                        sp.Size = size;
                    }
                    else
                    {
                        if (realType.IsGenericType && realType.GetGenericTypeDefinition() == typeof(Nullable<>))
                        {
                            Type underType = realType.GetGenericArguments()[0];
                            sp.Value = Activator.CreateInstance(underType);
                            SqlDbType realDbType = sp.SqlDbType;
                            sp.Value = DBNull.Value;
                            sp.SqlDbType = realDbType;
                        }
                    }
                }
            }
            else
            {
                sp.Value = value;
            }
            if (parameterInfo.IsRetval)
                sp.Direction = ParameterDirection.ReturnValue;
            else
            {
                if (parameterInfo.ParameterType.IsByRef)
                    sp.Direction = parameterInfo.IsOut ? ParameterDirection.Output : ParameterDirection.InputOutput;
            }
            return sp;
        }

        private object executeCommand(MethodInfo methodInfo, SqlCommand command, ref bool mustClosed, SpmSharedItemPool itemPool)
        {
            object result = null;
            Type retType = methodInfo.ReturnType;
            if (typeof(void).Equals(retType))
            {
                // no result, execute non-query
                command.ExecuteNonQuery();
            }
            else
            {
                if (typeof(SqlReturn).Equals(retType))
                {
                    // no result, execute non-query with return value
                    SqlParameter retVal = command.Parameters.Add(
                        new SqlParameter("@RETURN_VALUE", SqlDbType.Int)
                        {
                            Direction = ParameterDirection.ReturnValue
                        });
                    command.ExecuteNonQuery();
                    result = new SqlReturn(retVal.Value is int ? (int)retVal.Value : 0);
                }
                else
                {
                    if (typeof(SqlDataReader).Equals(retType))
                    {
                        result = command.ExecuteReader(CommandBehavior.CloseConnection);
                        mustClosed = false;
                    }
                    else
                    {
                        if (typeof(DataTable).Equals(retType))
                        {
                            using (SqlDataAdapter da = new SqlDataAdapter(command))
                            {
                                DataSet ds = new DataSet();
                                da.Fill(ds);
                                if (ds.Tables.Count > 0)
                                {
                                    result = ds.Tables[0];
                                    ds.Tables.RemoveAt(0);
                                }
                            }
                        }
                        else
                        {
                            if (typeof(DataSet).Equals(retType))
                            {
                                using (SqlDataAdapter da = new SqlDataAdapter(command))
                                {
                                    DataSet ds = new DataSet();
                                    da.Fill(ds);
                                    result = ds;
                                }
                            }
                            else
                            {
                                if (retType.IsGenericType &&
                                    retType.GetGenericTypeDefinition().Equals(typeof(IEnumerable<>)))
                                {
                                    ReadEnumDelegate readEnum = GetReadEnum(retType.GetGenericArguments()[0]);
                                    result = readEnum(command.ExecuteReader(CommandBehavior.CloseConnection), itemPool);
                                    mustClosed = false;
                                }
                                else
                                {
                                    result = command.ExecuteScalar();
                                    //throw new ArgumentException(String.Concat("Unsupported return type: ",
                                    //  methodInfo.ReturnType.ToString()));
                                }
                            }
                        }
                    }
                }
            }
            return result;
        }

        private delegate object ReadEnumDelegate(SqlDataReader dataReader, SpmSharedItemPool itemPool);
        private static Dictionary<Type, ReadEnumDelegate> dictDelegates;

        private static ReadEnumDelegate GetReadEnum(Type itemType)
        {
            if (dictDelegates == null)
                dictDelegates = new Dictionary<Type, ReadEnumDelegate>(30);
            ReadEnumDelegate res;
            if (!dictDelegates.TryGetValue(itemType, out res))
            {
                Type itemReaderType = typeof(SpmReader<>).MakeGenericType(itemType);
                MethodInfo enumMethod = itemReaderType.GetMethod("ReadEnumerable",
                    BindingFlags.Static | BindingFlags.Public | BindingFlags.Instance);
                res = (ReadEnumDelegate)enumMethod.CreateDelegate(typeof(ReadEnumDelegate));
                dictDelegates.Add(itemType, res);
            }
            return res;
        }
    }
}
