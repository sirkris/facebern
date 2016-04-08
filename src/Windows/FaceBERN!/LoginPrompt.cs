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
    public partial class LoginPrompt : Form
    {
        private string site;

        internal string u
        {
            get;
            private set;
        }

        internal string p
        {
            get;
            private set;
        }

        public LoginPrompt(string site)
        {
            this.site = site;
            InitializeComponent();
        }

        private void LoginPrompt_Load(object sender, EventArgs e)
        {
            labelSite.Text = site;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            /* This is just the basic form.  Data is handled by the caller.  --Kris */
            this.u = usernameTextBox.Text;
            this.p = passwordTextBox.Text;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void LoginPrompt_Shown(object sender, EventArgs e)
        {
            this.Activate();
        }
    }
}
