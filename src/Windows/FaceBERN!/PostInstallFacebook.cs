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

        bool next = false;

        public PostInstallFacebook(Form1 Main, string appVersion)
        {
            InitializeComponent();
            this.Main = Main;
            this.appVersion = appVersion;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (!(button2.Enabled))
            {
                button2.Click -= button2_Click;
                return;
            }

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

            if (enableFacebankingCheckbox.Checked
                && browserModeComboBox.SelectedIndex == 0)
            {
                MessageBox.Show("You must select a web browser if facebanking is enabled.");
                return;
            }

            Globals.Config["EnableFacebanking"] = (enableFacebankingCheckbox.Checked ? "1" : "0");
            Globals.Config["SelectedBrowser"] = Globals.BrowserName(browserModeComboBox.SelectedIndex);

            Globals.sINI.Save(System.IO.Path.Combine(Globals.ConfigDir, Globals.MainINI), Globals.Config);

            PostInstallTwitter postInstallTwitter = new PostInstallTwitter(Main, appVersion);
            postInstallTwitter.Show();

            next = true;
            this.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void PostInstallFacebook_Load(object sender, EventArgs e)
        {
            labelVersion.Text = appVersion;
        }

        private void PostInstallFacebook_Shown(object sender, EventArgs e)
        {
            this.Focus();
            this.BringToFront();
            this.Activate();

            // Populate if they're already in the registry.  Mainly used for DEBUG operations.  --Kris
            Credentials credentials = new Credentials(true);

            if (credentials.GetFacebookUsername() != null && credentials.GetFacebookPassword() != null)
            {
                facebookUsernameTextbox.Text = credentials.ToString(credentials.GetFacebookUsername());
                facebookPasswordTextbox.Text = credentials.ToString(credentials.GetFacebookPassword());

                button2.Focus();
            }

            if (Globals.Config.ContainsKey("SelectedBrowser")
                && Globals.Config["SelectedBrowser"] != null
                && Globals.BrowserConsts().ContainsKey(Globals.Config["SelectedBrowser"].ToLower()))
            {
                browserModeComboBox.SelectedIndex = Globals.BrowserConst(Globals.Config["SelectedBrowser"].ToLower());
            }
            else
            {
                browserModeComboBox.SelectedIndex = 0;
            }

            ToggleNextButton();
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

            ToggleNextButton();
        }

        private void enableFacebankingCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            if (enableFacebankingCheckbox.Checked)
            {
                rememberUsernamePasswordCheckbox.Visible = true;
                ToggleCredentialsFields();

                label7.Visible = true;
                browserModeComboBox.Visible = true;
            }
            else
            {
                rememberUsernamePasswordCheckbox.Visible = false;
                ToggleCredentialsFields(true);

                label7.Visible = false;
                browserModeComboBox.Visible = false;
            }

            ToggleNextButton();
        }

        private void ToggleNextButton()
        {
            if (enableFacebankingCheckbox.Checked)
            {
                if (browserModeComboBox.SelectedIndex == 0
                    || (rememberUsernamePasswordCheckbox.Checked
                        && (facebookUsernameTextbox.Text.Length == 0
                            || facebookPasswordTextbox.Text.Length == 0)))
                {
                    button2.Enabled = false;
                }
                else
                {
                    button2.Enabled = true;
                }
            }
            else
            {
                button2.Enabled = true;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            PostInstall postInstall = new PostInstall(Main, appVersion);
            postInstall.Show();

            next = true;
            this.Close();
        }

        private void browserModeComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            ToggleNextButton();
        }

        private void PostInstallFacebook_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (next == false)
            {
                Main.Exit();
            }
        }
    }
}
