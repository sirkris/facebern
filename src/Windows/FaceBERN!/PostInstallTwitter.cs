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
    public partial class PostInstallTwitter : Form
    {
        private Form1 Main;
        private string appVersion;

        private Credentials twitterCredentials = null;

        bool next = false;

        public PostInstallTwitter(Form1 Main, string appVersion)
        {
            InitializeComponent();
            this.Main = Main;
            this.appVersion = appVersion;

            twitterCredentials = new Credentials(false, true);
        }

        private void PostInstallTwitter_Load(object sender, EventArgs e)
        {
            labelVersion.Text = appVersion;
            button2.Enabled = false;

            HideTwitterCredentials();
            HideCampaignsForm();
        }

        private void PostInstallTwitter_Shown(object sender, EventArgs e)
        {
            if (twitterCredentials.IsAssociated())
            {
                ShowTwitterCredentials();
                ShowCampaignsForm();

                button4.Text = "De-Associate Twitter Account";

                button2.Enabled = true;
            }
        }

        private void cRunBernieRunCheckbox_MouseMove(object sender, EventArgs e)
        {
            toolTip1.SetToolTip(cRunBernieRunCheckbox, "Promote a Bernie Sanders candidacy for President of the United States (assuming you're still unhappy about all the election fraud and voter suppression in the Dem primaries).");
        }

        private void cMediaBlackoutCompensatorForS4PCheckbox_MouseMove(object sender, EventArgs e)
        {
            toolTip1.SetToolTip(cMediaBlackoutCompensatorForS4PCheckbox, "Tweet news posts from Reddit /r/SandersForPresident that are flaired by the mods.");
        }

        private void cMediaBlackoutCompensatorForPolRevCheckbox_MouseMove(object sender, EventArgs e)
        {
            toolTip1.SetToolTip(cMediaBlackoutCompensatorForPolRevCheckbox, "Tweet news posts from Reddit /r/Political_Revolution that are flaired by the mods.");
        }

        private void button3_Click(object sender, EventArgs e)
        {
            PostInstallFacebook postInstallFacebook = new PostInstallFacebook(Main, appVersion);
            postInstallFacebook.Show();

            next = true;
            this.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (!(button2.Enabled))
            {
                button2.Click -= button2_Click;
                return;
            }

            // Twitter credentials are stored elsewhere, so we just need to set the config values here.  --Kris
            Globals.Config["EnableTwitter"] = (enableTwitterCheckbox.Checked ? "1" : "0");
            Globals.Config["TwitterCampaignRunBernieRun"] = (cRunBernieRunCheckbox.Checked ? "1" : "0");
            Globals.Config["TwitterCampaignRedditS4P"] = (cMediaBlackoutCompensatorForS4PCheckbox.Checked ? "1" : "0");
            Globals.Config["TwitterCampaignRedditPolRev"] = (cMediaBlackoutCompensatorForPolRevCheckbox.Checked ? "1" : "0");

            Globals.sINI.Save(System.IO.Path.Combine(Globals.ConfigDir, Globals.MainINI), Globals.Config);

            PostInstallComplete postInstallComplete = new PostInstallComplete(Main, appVersion);
            postInstallComplete.Show();

            next = true;
            this.Close();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (twitterCredentials.IsAssociated())
            {
                /* De-associate Twitter account.  --Kris */
                DialogResult dr = MessageBox.Show("Are you sure you want the client to de-associate your Twitter account?", "Confirm De-Association", MessageBoxButtons.YesNo);
                if (dr == DialogResult.Yes)
                {
                    twitterCredentials.DestroyTwitter(true);

                    twitterCredentials = new Credentials(false, true);

                    HideTwitterCredentials();  // Updates the form fields.  --Kris
                    HideCampaignsForm();

                    button2.Enabled = false;

                    MessageBox.Show("Twitter account removed successfully!", "Success!", MessageBoxButtons.OK);

                    button4.Text = "Associate Twitter Account";
                }
            }
            else
            {
                /* Associate new Twitter account.  --Kris */
                DialogResult dr = MessageBox.Show("The client will open Twitter in a browser window.  You will be asked to enter the PIN you see there.  Are you ready?", 
                    "Confirm Twitter Account Association", MessageBoxButtons.YesNo);
                if (dr == DialogResult.Yes)
                {
                    MessageBox.Show("When you see the PIN page, this client will popup a small window where you'll need to enter the PIN to associate your Twitter account.");

                    this.UseWaitCursor = true;
                    this.TopMost = false;
                    this.Enabled = false;

                    // Don't do it as an asynchronous thread.  Better to lock the UI down here, anyway.  --Kris
                    Workflow workflow = new Workflow(Main);
                    workflow.AuthorizeTwitter(Main.browserModeComboBox.SelectedIndex);

                    twitterCredentials = new Credentials(false, true);

                    this.Enabled = true;
                    this.TopMost = true;
                    this.UseWaitCursor = false;

                    ShowTwitterCredentials();  // Updates the form fields.  --Kris
                    ShowCampaignsForm();

                    button2.Enabled = true;

                    MessageBox.Show("Twitter account added successfully!  Please click \"Next >\" to proceed.", "Success!", MessageBoxButtons.OK);

                    button4.Text = "De-Associate Twitter Account";
                }
            }
        }

        private void ShowTwitterCredentials()
        {
            label4.Visible = true;

            twitterUsernameTextbox.Text = twitterCredentials.ToString(twitterCredentials.GetTwitterUsername());
            twitterUsernameTextbox.Visible = true;
        }

        private void HideTwitterCredentials()
        {
            label4.Visible = false;

            twitterUsernameTextbox.Text = "";
            twitterUsernameTextbox.Visible = false;
        }

        private void ShowAccountForm()
        {
            if (twitterCredentials.IsAssociated())
            {
                ShowTwitterCredentials();

                button2.Enabled = true;
            }
            else if (enableTwitterCheckbox.Checked == true)
            {
                button2.Enabled = false;
            }

            label6.Visible = true;
            button4.Visible = true;
        }

        private void HideAccountForm()
        {
            HideTwitterCredentials();

            label6.Visible = false;
            button4.Visible = false;
        }

        private void ShowCampaignsForm()
        {
            if (twitterCredentials.IsAssociated())
            {
                label5.Visible = true;

                cRunBernieRunCheckbox.Visible = true;
                cMediaBlackoutCompensatorForS4PCheckbox.Visible = true;
                cMediaBlackoutCompensatorForPolRevCheckbox.Visible = true;
            }
        }

        private void HideCampaignsForm()
        {
            label5.Visible = false;

            cRunBernieRunCheckbox.Visible = false;
            cMediaBlackoutCompensatorForS4PCheckbox.Visible = false;
            cMediaBlackoutCompensatorForPolRevCheckbox.Visible = false;
        }

        private void enableTwitterCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            if (enableTwitterCheckbox.Checked == true)
            {
                ShowAccountForm();
                ShowCampaignsForm();
            }
            else
            {
                HideAccountForm();
                HideCampaignsForm();

                button2.Enabled = true;  // We don't have to have an account if Twitter is disabled.  --Kris
            }
        }

        private void PostInstallTwitter_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (next == false)
            {
                Main.Exit();
            }
        }
    }
}
