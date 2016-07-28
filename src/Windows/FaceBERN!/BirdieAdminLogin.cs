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
    public partial class BirdieAdminLogin : Form
    {
        internal string username
        {
            get;
            private set;
        }

        internal string password
        {
            get;
            private set;
        }

        public BirdieAdminLogin()
        {
            InitializeComponent();
            button1.Enabled = false;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (button1.Enabled == false)
            {
                button1.Click -= button1_Click;
                return;
            }

            this.DialogResult = System.Windows.Forms.DialogResult.OK;

            if (ValidateFields(true))
            {
                username = usernameTextbox.Text;
                password = passwordTextbox.Text;

                this.Close();
            }
        }

        internal bool ValidateFields(bool verbose = false)
        {
            List<string> errors = new List<string>();
            if (!(usernameTextbox.Text.Equals(usernameTextbox.Text.Trim())))
            {
                errors.Add("Username cannot start or end with a whitespace.");
            }

            if (usernameTextbox.Text.Length < 3)
            {
                errors.Add("Username must be at least 3 characters in length.");
            }

            if (passwordTextbox.Text.Length < 6)
            {
                errors.Add("Password must be at least 6 characters in length.");
            }

            if (errors.Count > 0)
            {
                if (verbose)
                {
                    MessageBox.Show("There were one or more errors in your submission:" + Environment.NewLine + Environment.NewLine + String.Join(Environment.NewLine, errors.ToArray()));
                }
            }

            return (errors.Count == 0);
        }

        private void usernameTextbox_TextChanged(object sender, EventArgs e)
        {
            button1.Enabled = ValidateFields();
        }

        private void passwordTextbox_TextChanged(object sender, EventArgs e)
        {
            button1.Enabled = ValidateFields();
        }
    }
}
