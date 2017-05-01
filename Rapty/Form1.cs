using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Rapty
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            output.Text = ((input1.Value + input2.Value) / 2).ToString();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            MessageBox.Show("Please, note: this program is actually a trojan horse.\r\nIt has been created by Valentino Giudice to show how a keylogger could work.\r\nDo not use it for evil.\r\nIf you do not intend to have this program, please reboot your computer and delete it.");
        }
    }
}
