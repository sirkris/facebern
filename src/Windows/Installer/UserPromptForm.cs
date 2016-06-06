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
    public partial class UserPromptForm : Form
    {
        public string installerVersion;
        public string installedPath;

        public bool cancel
        {
            get;
            private set;
        }

        public UserPromptForm(string installerVersion, string installedPath)
        {
            InitializeComponent();
            this.installerVersion = installerVersion;
            this.installedPath = installedPath;
        }

        private void UserPromptForm_Load(object sender, EventArgs e)
        {
            labelVersion.Text = installerVersion;
            labelInstalledPath.Text = installedPath;
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
    }
}
