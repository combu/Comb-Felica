using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Felica_ChefSharp
{
    public partial class Form4 : Form
    {
        public Form4()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(
                string.Format(
                    "名前 : {0}\r\n"+
                    "カードID : {1}\r\n"+
                    "カードpMid : {2}\r\n"+
                    "カードuMid : {3}\r\n",
                    label2.Text, label4.Text, label5.Text, label7.Text
                    )
                );
            MessageBox.Show("コピーしました。", "完了", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
