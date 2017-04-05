using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Linq.Expressions;
using System.Dynamic;
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
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Proxies;
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

        public interface ISqlProcedures
        {
            [SqlCommand(commandText: "MappingTest")]
            int Test(
                [SqlCommandParameter("nIdIn")]
                int id,
                [SqlCommandParameter("cNameIn")]
                string name,
                [SqlCommandParameter("nSomeValue")]
                out int someValue);

            DataTable GetUsers(int? nUserIdIn = null);
        }

        public double MyProp
        {
            get { return 1.0; }
        }

        public double MyField
        {
            get { return 2.0; }
        }

        

        private void btnEvaluate_Click(object sender, EventArgs e)
        {
            MyVal<double> x = new MyVal<double>();
            x.Value = 3.3;
            dynamic myt = Activator.CreateInstance(CreateAnonymousType<double, MyVal<double>>("Val1", "Val2"));
            myt.Val2 = x;
            myt.Val1 = 1.0;

            object result = null;
            try
            {
                result = SnowLib.Scripting.SimpleExpressionParser.GetValue(this.tbExpression.Text, myt);
                
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
            this.tbResult.Text = Convert.ToString(result);


            /*SqlConnection connection = new SqlConnection(
                @"Data Source=SQLTAG\SQL2008;Initial Catalog=IsupDB;Integrated Security=False;User ID=;Password=;Network Library=dbmssocn;Packet Size=4096");
            ISqlProcedures isql = SqlCommandProxy<ISqlProcedures>.Create(connection);*/
            

            //int x = 0;
            //int rv = isql.Test(1, "aaa", out x);

            //DataTable dt = isql.GetUsers(null);


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

        public class MyVal<T>
        {
            public T Value;
            public static implicit operator T(MyVal<T> vt)
            {
                return vt.Value;
            }
        }

        public static Type CreateAnonymousType<TFieldA, TFieldB>(string fieldNameA, string fieldNameB)
        {
            AssemblyName dynamicAssemblyName = new AssemblyName("TempAssembly");
            AssemblyBuilder dynamicAssembly = AssemblyBuilder.DefineDynamicAssembly(dynamicAssemblyName, AssemblyBuilderAccess.Run);
            ModuleBuilder dynamicModule = dynamicAssembly.DefineDynamicModule("TempAssembly");

            TypeBuilder dynamicAnonymousType = dynamicModule.DefineType("AnonymousType", TypeAttributes.Public);

            dynamicAnonymousType.DefineField(fieldNameA, typeof(TFieldA), FieldAttributes.Public);
            dynamicAnonymousType.DefineField(fieldNameB, typeof(TFieldB), FieldAttributes.Public);

            return dynamicAnonymousType.CreateType();
        }
    }
}
