using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FaceBERN_
{
    public partial class About : Form
    {
        private string version;

        public About()
        {
            InitializeComponent();

            this.version = Globals.__VERSION__;
        }

        private void About_Load(object sender, EventArgs e)
        {
            labelVersion.Text = version;
        }

        private void richTextBox1_Enter(object sender, EventArgs e)
        {
            button1.Focus();
        }

        private void About_Close(object sender, EventArgs e)
        {
            this.Close();
        }

        private void dontclickme_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            _01001011011100100110100101110011001000000100001101110010011000010110100101100111 aboutme = new _01001011011100100110100101110011001000000100001101110010011000010110100101100111();
            aboutme.Show();
            this.Close();
        }
    }
}
