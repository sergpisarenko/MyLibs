using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Reflection;
using System.Data.Sql;
using System.Data.SqlClient;
using System.Drawing;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using SnowLib.Extensions;
using System.Text.RegularExpressions;
using System.Runtime.Serialization;
using SnowLib.DB;

namespace Samples
{
    public partial class MainForm : Form
    {
        #region Constructors and initializers
        public MainForm()
        {
            InitializeComponent();
        }
        #endregion

        private void btnEvaluate_Click(object sender, EventArgs e)
        {
            SqlConnection connection = new SqlConnection(
                @"Data Source=SQLTAG\SQL2008;Initial Catalog=IsupDB;Integrated Security=False;User ID=AdminIsup;Password=AdminAksZF45;Network Library=dbmssocn;Packet Size=4096");
            MappingTest mp = new MappingTest()
            {
                nIdIn = 123,
                cNameIn = "xxx",
                nSomeValue = null
            };
            SqlStoredProcedure.Exec(mp, connection);

            //sp.

            /*long ticks = Environment.TickCount;
            DbContext s = new DbContext();
            for (int i = 0; i < 1000000; i++)
            {
                s.Read(i, i.ToString());
            }
            ticks = Environment.TickCount - ticks;
            this.Text = ticks.ToString();*/
           /* object result;
            try
            {
                result = SnowLib.Scripting.SimpleExpressionParser.GetValue(this.tbExpression.Text, null);
                
            }
            catch(SnowLib.Scripting.SimpleExpressionException see)
            {
                if (see.Position >= 0)
                {
                    this.tbExpression.Select(see.Position, see.Length);
                    this.tbExpression.Focus();
                }
                this.tbResult.ForeColor = Color.Red;
                this.tbResult.Text = see.Message;
                return;
            }
            catch(Exception ex)
            {
                this.tbResult.ForeColor = Color.Red;
                this.tbResult.Text = ex.Message;
                return;

            }
            this.tbResult.ForeColor = SystemColors.WindowText;
            this.tbResult.Text = Convert.ToString(result);*/

        }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple=false)]
    public class SqlStoredProcedureAttribute : Attribute
    {
        public const int DefaultTimeout = 5;
        public readonly string Name;
        public readonly int Timeout;

        public SqlStoredProcedureAttribute(string name = null, int timeout = DefaultTimeout)
        {
            this.Name = name;
            this.Timeout = timeout;
        }
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple=false)]
    public class SqlStoredProcedureParameterAttribute : Attribute
    {
        public readonly string Name;
        public readonly ParameterDirection Direction;
        public readonly int Size;

        public SqlStoredProcedureParameterAttribute(
            string name = null, ParameterDirection direction = ParameterDirection.Input, int size = 0)
        {
            this.Name = name;
            this.Direction = direction;
            this.Size = size;
        }
    }

    public class SqlStoredProcedure
    {
        #region Private static interfaces & classes
        private interface ISPParameter 
        {
            SqlParameter CreateParameter(object item);
            void UpdateValue(SqlParameter source, object item);
            string Name { get; }
        }

        private class SPParameter<TItem, TParam> : ISPParameter
        {
            private readonly Func<TItem, TParam> getValue;
            private readonly Action<TItem, TParam> setValue;
            private readonly string name;
            private readonly ParameterDirection direction;
            private readonly SqlDbType defaultSqlDbType;
            private readonly int defaultSize;

            public SPParameter(PropertyInfo propertyInfo, SqlStoredProcedureParameterAttribute attribute)
            {
                MethodInfo getMethod = propertyInfo.GetGetMethod(true);
                if (getMethod == null)
                    throw new ArgumentException("Property \"" + propertyInfo.Name + "\" of type \"" + propertyInfo.DeclaringType.Name + "\" needs get accessor!");
                this.getValue = (Func<TItem, TParam>)getMethod.CreateDelegate(typeof(Func<TItem, TParam>));
                string defaultName = '@'+propertyInfo.Name;
                if (attribute == null)
                {
                    this.name = defaultName;
                    this.direction = ParameterDirection.Input;
                }
                else
                {
                    this.name = String.IsNullOrEmpty(attribute.Name) ? defaultName : '@'+attribute.Name;
                    this.direction = attribute.Direction;
                }
                Type paramType = typeof(TParam);
                if (paramType.Equals(typeof(byte[])))
                {
                    this.defaultSqlDbType = SqlDbType.VarBinary;
                    this.defaultSize = attribute.Size;
                }
                else
                    if (paramType.Equals(typeof(string)))
                    {
                        this.defaultSqlDbType = SqlDbType.NVarChar;
                        this.defaultSize = attribute.Size;
                    }
                    else
                    {
                        if (paramType.IsGenericType && paramType.GetGenericTypeDefinition() == typeof(Nullable<>))
                        {
                            Type underType = paramType.GetGenericArguments()[0];
                            SqlParameter defaultParam = new SqlParameter();
                            defaultParam.Value = Activator.CreateInstance(underType);
                            this.defaultSqlDbType = defaultParam.SqlDbType;
                            this.defaultSize = defaultParam.Size;
                        }
                    }
                if (direction != ParameterDirection.Input)
                {
                    MethodInfo setMethod = propertyInfo.GetSetMethod(true);
                    if (setMethod == null)
                        throw new ArgumentException("Property \"" + propertyInfo.Name + "\" of type \"" + propertyInfo.DeclaringType.Name +
                            "\" needs set accessor because it corresonds to SQL ParameterDirection=\"" + this.direction.ToString() + "\"");
                    this.setValue = (Action<TItem, TParam>)setMethod.CreateDelegate(typeof(Action<TItem, TParam>));
                }
            }

            public SqlParameter CreateParameter(object item)
            {
                object value = getValue((TItem)item);
                SqlParameter res = new SqlParameter();
                res.ParameterName = this.name;
                res.Direction = this.direction;
                if (value == null)
                {
                    res.Value = DBNull.Value;
                    res.SqlDbType = this.defaultSqlDbType;
                    res.Size = this.defaultSize;
                }
                else
                {
                    res.Value = value;
                }
                return res;
            }

            public void UpdateValue(SqlParameter source, object item)
            {
                if (DBNull.Value.Equals(source.Value))
                    this.setValue((TItem)item, default(TParam));
                else
                    this.setValue((TItem)item, (TParam)source.Value);
            }

            public string Name
            {
                get { return this.name; }
            }

        }
        #endregion

        #region Private static fields
        private static Dictionary<Type, SqlStoredProcedure> procedureMap;
        #endregion

        #region Private fields
        private readonly string Name;
        private readonly int Timeout;
        private readonly ISPParameter[] Parameters;
        #endregion

        private SqlStoredProcedure(Type procedureType)
        {
            SqlStoredProcedureAttribute ssp = procedureType.GetCustomAttribute<SqlStoredProcedureAttribute>();
            if (ssp == null)
            {
                this.Name = procedureType.Name;
                this.Timeout = SqlStoredProcedureAttribute.DefaultTimeout;
            }
            else
            {
                this.Name = String.IsNullOrEmpty(ssp.Name) ? procedureType.Name : ssp.Name;
                this.Timeout = ssp.Timeout;
            }
            PropertyInfo[] properties = procedureType.GetProperties();
            List<ISPParameter> paramList = new List<ISPParameter>(properties.Length);
            for (int i = 0; i < properties.Length; i++)
            {
                PropertyInfo propertyInfo = properties[i];
                SqlStoredProcedureParameterAttribute sppa =
                    propertyInfo.GetCustomAttribute<SqlStoredProcedureParameterAttribute>();
                if (sppa != null)
                {
                    Type parameterInfoType = typeof(SPParameter<,>).MakeGenericType(procedureType, propertyInfo.PropertyType);
                    ISPParameter parameter = (ISPParameter)Activator.CreateInstance(parameterInfoType, propertyInfo, sppa);
                    paramList.Add(parameter);
                }
            }
            this.Parameters = paramList.ToArray();
        }

        public static SqlCommand GetSqlCommand(object target)
        {
            if (target == null)
                throw new ArgumentException("target");
            Type procedureType = target.GetType();
            if (procedureMap == null)
                procedureMap = new Dictionary<Type, SqlStoredProcedure>(30);
            SqlStoredProcedure procedure;
            if (!procedureMap.TryGetValue(procedureType, out procedure))
            {
                procedure = new SqlStoredProcedure(procedureType);
                procedureMap.Add(procedureType, procedure);
            }
            SqlCommand command = new SqlCommand(procedure.Name)
            {
                CommandType = CommandType.StoredProcedure,
                CommandTimeout = procedure.Timeout
            };
            Array.ForEach<ISPParameter>(procedure.Parameters, m => command.Parameters.Add(m.CreateParameter(target)));
            return command;
        }


        public static void UpdateOutput(SqlCommand command, object target)
        {
            if (target == null)
                throw new ArgumentException("target");
            Type procedureType = target.GetType();
            SqlStoredProcedure procedure = procedureMap[procedureType];
            foreach(SqlParameter sp in command.Parameters)
            {
                if (sp.Direction != ParameterDirection.Input)
                {
                    ISPParameter spp = Array.Find<ISPParameter>(procedure.Parameters, m => m.Name == sp.ParameterName);
                    if (spp!=null)
                        spp.UpdateValue(sp, target);
                }
            }
        }


        public static void Exec(object target, SqlConnection connection)
        {
            
            /*SqlCommand cmd = procedure.getCommand(connection, target);
            connection.Open();
            cmd.ExecuteNonQuery();
            procedure.processOutput(cmd, target);*/


            

            //cmd.Ex
        }

        //public SqlConnection Connection { get; set; }
    }

    public static class SqlConnectionExtension
    {

    }

    public class MappingTest
    {
        [SqlStoredProcedureParameter()]
        public int nIdIn { get; set; }

        [SqlStoredProcedureParameter()]
        public string cNameIn { get; set; }

        [SqlStoredProcedureParameter(direction : ParameterDirection.Output)]
        public int? nSomeValue { get; set; }

    }
}
