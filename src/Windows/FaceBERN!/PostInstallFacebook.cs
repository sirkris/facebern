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
    public partial class PostInstallFacebook : Form
    {
        private Form1 Main;
        private string appVersion;

        public PostInstallFacebook(Form1 Main, string appVersion)
        {
            InitializeComponent();
            this.Main = Main;
            this.appVersion = appVersion;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (enableFacebankingCheckbox.Checked
                && rememberUsernamePasswordCheckbox.Checked)
            {
                if (facebookUsernameTextbox.Text.Trim().Length == 0
                    && facebookPasswordTextbox.Text.Trim().Length == 0)
                {
                    MessageBox.Show("You must enter your Facebook username and password if you want them to be remembered.  You can also proceed by leaving them blank and unchecking the remember box.");
                    return;
                }

                Credentials credentials = new Credentials();
                if (!(credentials.SetFacebook(facebookUsernameTextbox.Text, facebookPasswordTextbox.Text)))
                {
                    MessageBox.Show("Error storing Facebook credentials!");
                    return;
                }

                credentials.Destroy();
                credentials = null;
            }

            Globals.Config["EnableFacebanking"] = (enableFacebankingCheckbox.Checked ? "1" : "0");

            Globals.sINI.Save(System.IO.Path.Combine(Globals.ConfigDir, Globals.MainINI), Globals.Config);


        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
            Main.Exit();
        }

        private void PostInstallFacebook_Load(object sender, EventArgs e)
        {
            labelVersion.Text = appVersion;
        }

        private void rememberUsernamePasswordCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            ToggleCredentialsFields();
        }

        private void ToggleCredentialsFields(bool forceHide = false)
        {
            if (rememberUsernamePasswordCheckbox.Checked 
                && forceHide == false)
            {
                label4.Visible = true;
                label5.Visible = true;
                label6.Visible = true;

                facebookUsernameTextbox.Visible = true;
                facebookPasswordTextbox.Visible = true;
            }
            else
            {
                label4.Visible = false;
                label5.Visible = false;
                label6.Visible = false;

                facebookUsernameTextbox.Visible = false;
                facebookPasswordTextbox.Visible = false;
            }
        }

        private void enableFacebankingCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            if (enableFacebankingCheckbox.Checked)
            {
                rememberUsernamePasswordCheckbox.Visible = true;
                ToggleCredentialsFields();
            }
            else
            {
                rememberUsernamePasswordCheckbox.Visible = false;
                ToggleCredentialsFields(true);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            PostInstall postInstall = new PostInstall(Main, appVersion);
            postInstall.Show();

            this.Close();
        }
    }
}
