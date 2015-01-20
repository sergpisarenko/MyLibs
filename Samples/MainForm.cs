using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SnowLib.Extensions;
using System.Text.RegularExpressions;

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
            object result;
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
            this.tbResult.Text = Convert.ToString(result);

        }
    }
}
