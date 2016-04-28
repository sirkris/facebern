using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Installer
{
    public partial class ThankYouForm : Form
    {
        private string installerVersion;

        public ThankYouForm(string installerVersion)
        {
            InitializeComponent();
            this.installerVersion = installerVersion;
        }

        private void ThankYouForm_Load(object sender, EventArgs e)
        {
            labelVersion.Text = installerVersion;
        }

        private void richTextBox1_Enter(object sender, EventArgs e)
        {
            button1.Focus();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
