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
    public class SqlCommandProxy<T> : RealProxy
    {
        public static T Create(SqlConnection sqlConnection)
        {
            SqlCommandProxy<T> realProxy = new SqlCommandProxy<T>(sqlConnection);
            return (T)realProxy.GetTransparentProxy();
        }

        private readonly SqlConnection connection;

        private SqlCommandProxy(SqlConnection sqlConnection)
            : base(typeof(T))
        {
            if (sqlConnection == null)
                throw new ArgumentNullException("sqlConnection");
            this.connection = sqlConnection;
        }

        public override System.Runtime.Remoting.Messaging.IMessage Invoke(System.Runtime.Remoting.Messaging.IMessage msg)
        {
            IMethodCallMessage methodCall = (IMethodCallMessage)msg;
            MethodInfo methodInfo = (MethodInfo)methodCall.MethodBase;
            ParameterInfo[] parameters = methodInfo.GetParameters();
            /*if (parameters.Length==1 && 
                parameters[0].ParameterType.Equals(typeof(SqlConnection)) && 
                methodInfo.ReturnType.Equals(typeof(void)))
            {
                // special case - set connection
                this.connection = (SqlConnection)methodCall.Args[0];
                return new ReturnMessage(null, null, 0, methodCall.LogicalCallContext, methodCall);
            }*/
            // Casual case - sql command
            SqlCommand command = getSqlCommand(methodInfo);
            bool hasOutput = false;
            for (int i = 0; i < parameters.Length; i++)
                hasOutput |= command.Parameters.Add(
                    getSqlParameter(parameters[i], methodCall.Args[i])).Direction != ParameterDirection.Input;
            command.Connection = this.connection;
            object result = null;
            try
            {
                result = executeCommand(methodInfo, command);
            }
            catch (Exception ex)
            {
                return new ReturnMessage(ex, msg as IMethodCallMessage);
            }
            finally
            {
                if (!(result is SqlDataReader))
                    command.Connection.Close();
            }
            object[] outArgs = null;
            if (hasOutput)
            {
                outArgs = new object[methodCall.ArgCount];
                for (int i = 0; i < outArgs.Length; i++)
                    outArgs[i] = parameters[i].ParameterType.IsByRef ? command.Parameters[i].Value : null;
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
                    sp.SqlDbType = SqlDbType.NVarChar;
                    sp.Size = size;
                }
                else
                {
                    if (realType.Equals(typeof(byte[])))
                    {
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
                            sp.Value = null;
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

        private object executeCommand(MethodInfo methodInfo, SqlCommand command)
        {
            object result = null;
            if (command.Connection.State == ConnectionState.Closed ||
                command.Connection.State == ConnectionState.Broken)
                command.Connection.Open();
            if (typeof(void).Equals(methodInfo.ReturnType))
            {
                // no result, execute non-query
                command.ExecuteNonQuery();
            }
            else
            {
                if (typeof(int).Equals(methodInfo.ReturnType))
                {
                    // no result, execute non-query with return value
                    SqlParameter retVal = command.Parameters.Add(
                        new SqlParameter("@RETURN_VALUE", SqlDbType.Int)
                        {
                            Direction = ParameterDirection.ReturnValue
                        });
                    command.ExecuteNonQuery();
                    result = retVal.Value is int ? retVal.Value : 0;
                }
                else
                {
                    if (typeof(SqlDataReader).Equals(methodInfo.ReturnType))
                        result = command.ExecuteReader(CommandBehavior.CloseConnection);
                    else
                    {
                        if (typeof(DataTable).Equals(methodInfo.ReturnType))
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
                            if (typeof(DataSet).Equals(methodInfo.ReturnType))
                            {
                                using (SqlDataAdapter da = new SqlDataAdapter(command))
                                {
                                    DataSet ds = new DataSet();
                                    da.Fill(ds);
                                    result = ds;
                                }
                            }
                    }
                }
            }
            return result;
        }
    }
}
