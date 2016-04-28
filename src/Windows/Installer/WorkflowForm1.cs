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
    public partial class WorkflowForm1 : Form
    {
        public string installerVersion;

        public bool cancel
        {
            get;
            private set;
        }

        public WorkflowForm1(string installerVersion)
        {
            InitializeComponent();
            this.installerVersion = installerVersion;
        }

        private void WorkflowForm1_Load(object sender, EventArgs e)
        {
            labelVersion.Text = installerVersion;
            cancel = true;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            cancel = true;
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            cancel = false;
            this.Close();  // Next form will be called from Form1.  --Kris
        }

        private void richTextBox1_Enter(object sender, EventArgs e)
        {
            button2.Focus();
        }
    }
}
