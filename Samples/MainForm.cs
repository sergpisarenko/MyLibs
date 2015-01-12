using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Samples
{
    public partial class MainForm : Form
    {

        public class My
        {
            public string Name = "Fixed";
        }

        public My FieldTest;


        public MainForm()
        {
            InitializeComponent();
            System.Linq.Expressions.Expression left = System.Linq.Expressions.Expression.Constant(false);
            System.Linq.Expressions.Expression right = System.Linq.Expressions.Expression.Constant(true);
            System.Linq.Expressions.Expression add = System.Linq.Expressions.Expression.Or(left, right);

            Delegate d2 = System.Linq.Expressions.Expression.Lambda(add).Compile();
            object y = d2.DynamicInvoke();


            //Expression

            int x2 = 1;
            int y2 = (2 | 2 << 2) + 1;

            //typeof(System.Math).GetMethod("DivRem", )
            FieldTest = new My();

            SnowLib.Scripting.SimpleExpressionParser sep = new SnowLib.Scripting.SimpleExpressionParser(this);
            
            //sep.UserFunctions = this;


            //this.Text = sep.GetValue("12.3e2 + MyConstantA - MyFuc(33, \"ddd\")").ToString();
            //System.Linq.Expressions.Expression expr = sep.GetExpression("System.String.Concat(-System.Math.Pow(2.0, 3.0).ToString(), 4.0.ToString())");
            System.Linq.Expressions.Expression expr = sep.GetExpression("FieldTest.Name"); //"(3.4+4.54666).ToString(\"0.00\")");
            Delegate d = System.Linq.Expressions.Expression.Lambda(expr).Compile();
            object x = d.DynamicInvoke();
            

        }

        public double? MyFunc(int x, string y)
        {
            return 1;
        }

     

        #region Find List test
        private void btnFLTest_Click(object sender, EventArgs e)
        {
            this.lxFLSource.Items.Clear();
            this.lxFLSource.BeginUpdate();
            int maxLen = Convert.ToInt32(this.numFLMaxLen.Value);
            for (int i = 0; i < this.numFLCount.Value; i++)
                this.lxFLSource.Items.Add(getRandomString(maxLen));
            this.lxFLSource.EndUpdate();
            Application.DoEvents();
            System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
            StringBuilder sb = new StringBuilder();
           
            sb.AppendLine("Addding [ticks/bytes].");
            long tm = GC.GetTotalMemory(true);
            List<string> list = new List<string>(this.lxFLSource.Items.Count);
            watch.Reset();
            watch.Start();
            for (int i = 0; i < this.lxFLSource.Items.Count; i++)
            {
                list.Add((string)this.lxFLSource.Items[i]);
            }
            watch.Stop();
            sb.Append("List: ");
            sb.Append(watch.ElapsedTicks.ToString());
            tm = GC.GetTotalMemory(true) - tm;
            sb.Append('/');
            sb.Append(tm.ToString());

            tm = GC.GetTotalMemory(true);
            watch.Reset();
            watch.Start();
            HashSet<string> hset = new HashSet<string>(list);
            watch.Stop();
            sb.Append("   HashSet: ");
            sb.Append(watch.ElapsedTicks.ToString());
            tm = GC.GetTotalMemory(true) - tm;
            sb.Append('/');
            sb.Append(tm.ToString());

            sb.AppendLine();
            sb.AppendLine("Finding [ticks].");
            watch.Reset();
            watch.Start();
            for (int i = 0; i < this.lxFLSource.Items.Count; i++)
            {
                list.IndexOf((string)this.lxFLSource.Items[i]);
            }
            watch.Stop();
            sb.Append("List: ");
            sb.Append(watch.ElapsedTicks.ToString());

            watch.Reset();
            watch.Start();
            for (int i = 0; i < this.lxFLSource.Items.Count; i++)
            {
                hset.Contains((string)this.lxFLSource.Items[i]);
            }
            watch.Stop();
            sb.Append("   HashSet: ");
            sb.Append(watch.ElapsedTicks.ToString());



            this.tbFLResult.Text = sb.ToString();
        }
        #endregion

        #region Random string generator
        private readonly Random rnd = new Random();
        
        private const string rndChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789 ";
                
        private string getRandomString(int maxLength)
        {
            int size = Math.Max(this.rnd.Next(maxLength), 5);
            char[] buffer = new char[size];
            for (int i = 0; i < size; i++)
                buffer[i] = rndChars[this.rnd.Next(rndChars.Length)];
            return new string(buffer);
        }
        #endregion

    }
}
