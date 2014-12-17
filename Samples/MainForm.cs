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
        public MainForm()
        {
            InitializeComponent();
            SnowLib.FindList<int> x = new SnowLib.FindList<int>();
            x.Add(11);
        }
    }
}
