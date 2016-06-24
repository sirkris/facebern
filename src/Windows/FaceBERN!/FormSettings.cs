using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FaceBERN_
{
    public partial class FormSettings : Form
    {
        private Dictionary<int, string> stateIndexes;
        private int selectedStateIndex = 0;

        private Credentials twitterCredentials;

        private Form1 Main;

        public FormSettings(Form1 Main, bool topMost = false)
        {
            InitializeComponent();
            this.Main = Main;
            this.TopMost = topMost;
        }

        private void buttonOk_Click(object sender, EventArgs e)
        {
            buttonApply_Click(sender, e);
            CloseSettings();
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            CloseSettings();
        }

        private void CloseSettings()
        {
            if (twitterCredentials != null)
            {
                twitterCredentials.Destroy();
                twitterCredentials = null;
            }

            this.Close();
        }

        private void buttonApply_Click(object sender, EventArgs e)
        {
            Globals.Config["UseFTBEvents"] = (useFTBEventsCheckbox.Checked ? "1" : "0");
            Globals.Config["UseCustomEvents"] = (useCustomEventsCheckbox.Checked ? "1" : "0");
            Globals.Config["CheckRememberPasswordByDefault"] = (checkRememberPasswordByDefaultCheckbox.Checked ? "1" : "0");
            Globals.Config["AutoUpdate"] = (autoUpdateCheckbox.Checked ? "1" : "0");
            Globals.Config["TwitterCampaignRunBernieRun"] = (cRunBernieRunCheckbox.Checked ? "1" : "0");
            Globals.Config["TwitterCampaignRedditS4P"] = (cMediaBlackoutCompensatorForS4PCheckbox.Checked ? "1" : "0");
            Globals.Config["TwitterCampaignRedditPolRev"] = (cMediaBlackoutCompensatorForPolRevCheckbox.Checked ? "1" : "0");
            Globals.Config["EnableFacebanking"] = (enableFacebankingCheckbox.Checked ? "1" : "0");
            Globals.Config["EnableTwitter"] = (enableTwitterCheckbox.Checked ? "1" : "0");
            Globals.Config["TweetIntervalMinutes"] = tweetIntervalMinutesNumericUpDown.Value.ToString();

            Globals.sINI.Save(Path.Combine(Globals.ConfigDir, Globals.MainINI), Globals.Config);

            if (stateIndexes != null && stateIndexes[selectedStateIndex] != null)
            {
                States state = Globals.StateConfigs[stateIndexes[selectedStateIndex]];

                /* Any configurable values are set here.  --Kris */
                state.facebookId = facebookIDTextBox.Text;
                state.FTBEventId = FTBEventIdTextBox.Text;
                state.enableGOTV = enableGOTVCheckbox.Checked;

                Globals.StateConfigs[state.abbr] = state;

                Dictionary<string, Dictionary<string, string>> config = new Dictionary<string, Dictionary<string, string>>();

                string sectionName = "Settings";

                config[sectionName] = new Dictionary<string, string>();
                config[sectionName].Add("abbr", state.abbr);
                config[sectionName].Add("name", state.name);
                config[sectionName].Add("primaryDate", state.primaryDate.ToString("yyyy-MM-dd"));
                config[sectionName].Add("primaryType", state.primaryType);
                config[sectionName].Add("primaryAccess", state.primaryAccess);
                config[sectionName].Add("facebookId", state.facebookId);
                config[sectionName].Add("FTBEventId", state.FTBEventId);

                config[sectionName].Add("enableGOTV", (state.enableGOTV == true ? "1" : "0"));

                Globals.sINI.Clear(Path.Combine(Globals.ConfigDir, state.abbr + ".ini"));
                Globals.sINI.Save(Path.Combine(Globals.ConfigDir, state.abbr + ".ini"), config);
            }

            buttonApply.Enabled = false;
        }

        private void SetFormDefaults(string tab, bool inFocus = false)
        {
            bool applyEnabled = buttonApply.Enabled;

            switch (tab.ToLower())
            {
                case "general":
                    autoUpdateCheckbox.Checked = (Globals.Config["AutoUpdate"] == "1" ? true : false);

                    break;
                case "states":
                    List<string> territories = new List<string> { "AS", "DC", "GU", "MP", "PR", "VI" };
                    List<string> other = new List<string> { "DA" };  // Democrats Abroad.  --Kris

                    List<States> tS = new List<States>();
                    List<States> oS = new List<States>();
                    foreach (string stateAbbr in territories)
                    {
                        tS.Add(Globals.StateConfigs[stateAbbr]);
                    }
                    foreach (string stateAbbr in other)
                    {
                        oS.Add(Globals.StateConfigs[stateAbbr]);
                    }

                    stateIndexes = new Dictionary<int, string>();
                    statesComboBox.Items.Clear();

                    int i = 0;
                    statesComboBox.Items.Add("-- States --");
                    stateIndexes.Add(i, null);
                    foreach (KeyValuePair<string, States> state in Globals.StateConfigs)
                    {
                        if (territories.Contains(state.Key) == false && other.Contains(state.Key) == false)
                        {
                            i++;
                            statesComboBox.Items.Add(state.Key + " - " + state.Value.name);
                            stateIndexes.Add(i, state.Key);
                        }
                    }

                    i++;
                    statesComboBox.Items.Add("-- Territories --");
                    stateIndexes.Add(i, null);
                    foreach (States state in tS)
                    {
                        i++;
                        statesComboBox.Items.Add(state.abbr + " - " + state.name);
                        stateIndexes.Add(i, state.abbr);
                    }

                    i++;
                    statesComboBox.Items.Add("-- Other --");
                    stateIndexes.Add(i, null);
                    foreach (States state in oS)
                    {
                        i++;
                        statesComboBox.Items.Add(state.abbr + " - " + state.name);
                        stateIndexes.Add(i, state.abbr);
                    }

                    statesComboBox.SelectedIndex = selectedStateIndex;
                    SetStateFields();
                    
                    break;
                case "facebook":
                    enableFacebankingCheckbox.Checked = (Globals.Config["EnableFacebanking"] == "1" ? true : false);
                    useFTBEventsCheckbox.Checked = (Globals.Config["UseFTBEvents"] == "1" ? true : false);
                    useCustomEventsCheckbox.Checked = (Globals.Config["UseCustomEvents"] == "1" ? true : false);
                    checkRememberPasswordByDefaultCheckbox.Checked = (Globals.Config["CheckRememberPasswordByDefault"] == "1" ? true : false);

                    break;
                case "twitter":
                    enableTwitterCheckbox.Checked = (Globals.Config["EnableTwitter"] == "1" ? true : false);
                    cRunBernieRunCheckbox.Checked = (Globals.Config["TwitterCampaignRunBernieRun"] == "1" ? true : false);
                    cMediaBlackoutCompensatorForS4PCheckbox.Checked = (Globals.Config["TwitterCampaignRedditS4P"] == "1" ? true : false);
                    cMediaBlackoutCompensatorForPolRevCheckbox.Checked = (Globals.Config["TwitterCampaignRedditPolRev"] == "1" ? true : false);
                    tweetIntervalMinutesNumericUpDown.Value = Decimal.Parse(Globals.Config["TweetIntervalMinutes"]);

                    ShowTwitterCredentials();

                    break;
            }

            buttonApply.Enabled = applyEnabled;
        }

        // This should be called only when the Twitter tab is active!  --Kris
        private void ShowTwitterCredentials()
        {
            twitterCredentials = new Credentials(false, true);
            twitterUsernameTextbox.Text = twitterCredentials.ToString(twitterCredentials.GetTwitterUsername());
            twitterUserIdTextbox.Text = twitterCredentials.ToString(twitterCredentials.GetTwitterUserID());
            twitterAccessTokenTextbox.Text = twitterCredentials.ToString(twitterCredentials.GetTwitterAccessToken());

            button2.Text = (twitterCredentials.IsAssociated() ? "De-Associate Twitter Account" : "Associate Twitter Account");
        }

        private void tabGeneral_Load(object sender, EventArgs e)
        {
            SetFormDefaults("general");
        }

        private void tabStates_Load(object sender, EventArgs e)
        {
            SetFormDefaults("states");
        }

        private void tabFacebook_Load(object sender, EventArgs e)
        {
            SetFormDefaults("facebook");
        }

        private void tabTwitter_Load(object sender, EventArgs e)
        {
            SetFormDefaults("twitter");
        }

        private void SetStateFields()
        {
            bool applyEnabled = buttonApply.Enabled;

            if (stateIndexes[selectedStateIndex] != null)
            {
                labelAbbreviation.Visible = true;
                labelName.Visible = true;
                labelPrimaryDate.Visible = true;
                labelPrimaryType.Visible = true;
                labelPrimaryAccess.Visible = true;
                labelFacebookID.Visible = true;
                labelFTBEventID.Visible = true;
                labelEnabledTasks.Visible = true;
                labelExpGOTV.Visible = true;

                abbreviationTextBox.Visible = true;
                nameTextBox.Visible = true;
                primaryDateTextBox.Visible = true;
                primaryTypeTextBox.Visible = true;
                primaryAccessTextBox.Visible = true;
                facebookIDTextBox.Visible = true;
                FTBEventIdTextBox.Visible = true;
                enableGOTVCheckbox.Visible = true;

                abbreviationTextBox.Text = Globals.StateConfigs[stateIndexes[selectedStateIndex]].abbr;
                nameTextBox.Text = Globals.StateConfigs[stateIndexes[selectedStateIndex]].name;
                primaryDateTextBox.Text = Globals.StateConfigs[stateIndexes[selectedStateIndex]].primaryDate.ToString("MMMM d, yyyy");
                primaryTypeTextBox.Text = Globals.StateConfigs[stateIndexes[selectedStateIndex]].primaryType;
                primaryAccessTextBox.Text = Globals.StateConfigs[stateIndexes[selectedStateIndex]].primaryAccess;
                facebookIDTextBox.Text = Globals.StateConfigs[stateIndexes[selectedStateIndex]].facebookId;
                FTBEventIdTextBox.Text = Globals.StateConfigs[stateIndexes[selectedStateIndex]].FTBEventId;

                enableGOTVCheckbox.Checked = Globals.StateConfigs[stateIndexes[selectedStateIndex]].enableGOTV;
            }
            else
            {
                labelAbbreviation.Visible = false;
                labelName.Visible = false;
                labelPrimaryDate.Visible = false;
                labelPrimaryType.Visible = false;
                labelPrimaryAccess.Visible = false;
                labelFacebookID.Visible = false;
                labelFTBEventID.Visible = false;
                labelEnabledTasks.Visible = false;
                labelExpGOTV.Visible = false;

                abbreviationTextBox.Visible = false;
                nameTextBox.Visible = false;
                primaryDateTextBox.Visible = false;
                primaryTypeTextBox.Visible = false;
                primaryAccessTextBox.Visible = false;
                facebookIDTextBox.Visible = false;
                FTBEventIdTextBox.Visible = false;
                enableGOTVCheckbox.Visible = false;
            }

            buttonApply.Enabled = applyEnabled;
        }

        private void SetFacebookFields()
        {
            bool applyEnabled = buttonApply.Enabled;

            if (enableFacebankingCheckbox.Checked == true)
            {
                label1.Visible = true;
                label2.Visible = true;
                label3.Visible = true;

                useFTBEventsCheckbox.Visible = true;
                useCustomEventsCheckbox.Visible = true;
                checkRememberPasswordByDefaultCheckbox.Visible = true;
            }
            else
            {
                label1.Visible = false;
                label2.Visible = false;
                label3.Visible = false;

                useFTBEventsCheckbox.Visible = false;
                useCustomEventsCheckbox.Visible = false;
                checkRememberPasswordByDefaultCheckbox.Visible = false;
            }

            buttonApply.Enabled = applyEnabled;
        }

        private void SetTwitterFields()
        {
            bool applyEnabled = buttonApply.Enabled;

            if (enableTwitterCheckbox.Checked == true)
            {
                label9.Visible = true;
                label10.Visible = true;
                label11.Visible = true;
                label12.Visible = true;
                label13.Visible = true;
                label14.Visible = true;
                label15.Visible = true;

                cRunBernieRunCheckbox.Visible = true;
                cMediaBlackoutCompensatorForS4PCheckbox.Visible = true;
                cMediaBlackoutCompensatorForPolRevCheckbox.Visible = true;
                button2.Visible = true;
                twitterUsernameTextbox.Visible = true;
                twitterUserIdTextbox.Visible = true;
                twitterAccessTokenTextbox.Visible = true;
                tweetIntervalMinutesNumericUpDown.Visible = true;
            }
            else
            {
                label9.Visible = false;
                label10.Visible = false;
                label11.Visible = false;
                label12.Visible = false;
                label13.Visible = false;
                label14.Visible = false;
                label15.Visible = false;

                cRunBernieRunCheckbox.Visible = false;
                cMediaBlackoutCompensatorForS4PCheckbox.Visible = false;
                cMediaBlackoutCompensatorForPolRevCheckbox.Visible = false;
                button2.Visible = false;
                twitterUsernameTextbox.Visible = false;
                twitterUserIdTextbox.Visible = false;
                twitterAccessTokenTextbox.Visible = false;
                tweetIntervalMinutesNumericUpDown.Visible = false;
            }

            buttonApply.Enabled = applyEnabled;
        }

        private void StateFields_TextChanged(object sender, EventArgs e)
        {
            buttonApply.Enabled = true;
        }

        private void statesComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            selectedStateIndex = statesComboBox.SelectedIndex;
            SetStateFields();
        }

        private void enableFacebankingCheckbox_CheckChanged(object sender, EventArgs e)
        {
            SetFacebookFields();
            buttonApply.Enabled = true;
        }

        private void enableTwitterCheckbox_CheckChanged(object sender, EventArgs e)
        {
            SetTwitterFields();
            buttonApply.Enabled = true;
        }

        private void useFTPEventsCheckbox_CheckChanged(object sender, EventArgs e)
        {
            buttonApply.Enabled = true;
        }

        private void useCustomEventsCheckbox_CheckChanged(object sender, EventArgs e)
        {
            buttonApply.Enabled = true;
        }

        private void checkRememberPasswordByDefaultCheckbox_CheckChanged(object sender, EventArgs e)
        {
            buttonApply.Enabled = true;
        }

        private void autoUpdateCheckbox_CheckChanged(object sender, EventArgs e)
        {
            buttonApply.Enabled = true;
        }

        private void cRunBernieRunCheckbox_CheckChanged(object sender, EventArgs e)
        {
            buttonApply.Enabled = true;
        }

        private void cMediaBlackoutCompensatorForS4PCheckbox_CheckChanged(object sender, EventArgs e)
        {
            buttonApply.Enabled = true;
        }

        private void cMediaBlackoutCompensatorForPolRevCheckbox_CheckChanged(object sender, EventArgs e)
        {
            buttonApply.Enabled = true;
        }

        private void cRunBernieRunCheckbox_MouseMove(object sender, MouseEventArgs e)
        {
            toolTip1.SetToolTip(cRunBernieRunCheckbox, "Promote a Bernie Sanders candidacy for President of the United States (assuming you're still unhappy about all the election fraud and voter suppression in the Dem primaries).");
        }

        private void cMediaBlackoutCompensatorForS4PCheckbox_MouseMove(object sender, MouseEventArgs e)
        {
            toolTip1.SetToolTip(cMediaBlackoutCompensatorForS4PCheckbox, "Tweet news posts from Reddit /r/SandersForPresident that are flaired by the mods.");
        }

        private void cMediaBlackoutCompensatorForPolRevCheckbox_MouseMove(object sender, MouseEventArgs e)
        {
            toolTip1.SetToolTip(cMediaBlackoutCompensatorForPolRevCheckbox, "Tweet news posts from Reddit /r/Political_Revolution that are flaired by the mods.");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            DialogResult dr = MessageBox.Show("Are you sure you want FaceBERN! to forget your account information for ALL sites?", "Confirm Deletion", MessageBoxButtons.YesNo);
            if (dr == DialogResult.Yes)
            {
                Credentials c = new Credentials();
                c.Destroy(true);
                c = null;

                MessageBox.Show("Stored credentials deleted successfully!", "Success!", MessageBoxButtons.OK);
            }
        }

        private void label7_Click(object sender, EventArgs e)
        {
            enableGOTVCheckbox.Checked = !(enableGOTVCheckbox.Checked);
        }

        private void label4_Click(object sender, EventArgs e)
        {
            autoUpdateCheckbox.Checked = !(autoUpdateCheckbox.Checked);
        }

        private void label7_Click_1(object sender, EventArgs e)
        {
            enableFacebankingCheckbox.Checked = !(enableFacebankingCheckbox.Checked);
        }

        private void label1_Click(object sender, EventArgs e)
        {
            useFTBEventsCheckbox.Checked = !(useFTBEventsCheckbox.Checked);
        }

        private void label2_Click(object sender, EventArgs e)
        {
            useCustomEventsCheckbox.Checked = !(useCustomEventsCheckbox.Checked);
        }

        private void label3_Click(object sender, EventArgs e)
        {
            checkRememberPasswordByDefaultCheckbox.Checked = !(checkRememberPasswordByDefaultCheckbox.Checked);
        }

        private void label8_Click(object sender, EventArgs e)
        {
            enableTwitterCheckbox.Checked = !(enableTwitterCheckbox.Checked);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            DialogResult dr = MessageBox.Show("Are you sure you want FaceBERN! to forget your Facebook username/password?", "Confirm Deletion", MessageBoxButtons.YesNo);
            if (dr == DialogResult.Yes)
            {
                Credentials c = new Credentials();
                c.DestroyFacebook(true);
                c = null;

                MessageBox.Show("Stored Facebook credentials deleted successfully!", "Success!", MessageBoxButtons.OK);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (Globals.executionState != Globals.STATE_READY)
            {
                MessageBox.Show("This operation can only be performed when FaceBERN! is not executing its workflow.  Please click the STOP button then try again.");
                return;
            }
            
            twitterCredentials = new Credentials(false, true);

            if (twitterCredentials.IsAssociated())
            {
                /* De-associate Twitter account.  --Kris */
                DialogResult dr = MessageBox.Show("Are you sure you want FaceBERN! to de-associate your Twitter account?", "Confirm De-Association", MessageBoxButtons.YesNo);
                if (dr == DialogResult.Yes)
                {
                    twitterCredentials.DestroyTwitter(true);
                    twitterCredentials = null;

                    ShowTwitterCredentials();  // Updates the form fields.  --Kris

                    MessageBox.Show("Twitter account removed successfully!", "Success!", MessageBoxButtons.OK);
                }
            }
            else
            {
                /* Associate new Twitter account.  --Kris */
                DialogResult dr = MessageBox.Show("FaceBERN! will open Twitter in a browser window.  You will be asked to enter the PIN you see there.  Are you ready?", "Confirm Twitter Account Association", MessageBoxButtons.YesNo);
                if (dr == DialogResult.Yes)
                {
                    MessageBox.Show("The Settings window will now close and a browser window will open with a temporary PIN.  You will then be prompted to enter that PIN here.");

                    Workflow workflow = new Workflow(Main);
                    Globals.thread = workflow.ExecuteTwitterAuthThread(Main.browserModeComboBox.SelectedIndex);

                    this.Close();
                }
            }
        }

        private void tweetIntervalMinutesNumericUpDown_ValueChanged(object sender, EventArgs e)
        {
            buttonApply.Enabled = true;
        }
    }
}
