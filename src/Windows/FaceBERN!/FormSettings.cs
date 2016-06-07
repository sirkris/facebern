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

        public FormSettings(bool topMost = false)
        {
            InitializeComponent();
            this.TopMost = topMost;
        }

        private void buttonOk_Click(object sender, EventArgs e)
        {
            buttonApply_Click(sender, e);
            this.Close();
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void buttonApply_Click(object sender, EventArgs e)
        {
            Globals.Config["UseFTBEvents"] = (useFTBEventsCheckbox.Checked ? "1" : "0");
            Globals.Config["UseCustomEvents"] = (useCustomEventsCheckbox.Checked ? "1" : "0");
            Globals.Config["CheckRememberPasswordByDefault"] = (checkRememberPasswordByDefaultCheckbox.Checked ? "1" : "0");
            Globals.Config["AutoUpdate"] = (autoUpdateCheckbox.Checked ? "1" : "0");

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
                    useFTBEventsCheckbox.Checked = (Globals.Config["UseFTBEvents"] == "1" ? true : false);
                    useCustomEventsCheckbox.Checked = (Globals.Config["UseCustomEvents"] == "1" ? true : false);
                    checkRememberPasswordByDefaultCheckbox.Checked = (Globals.Config["CheckRememberPasswordByDefault"] == "1" ? true : false);
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
            }

            buttonApply.Enabled = applyEnabled;
        }

        private void tabGeneral_Load(object sender, EventArgs e)
        {
            SetFormDefaults("general");
        }

        private void tabStates_Load(object sender, EventArgs e)
        {
            SetFormDefaults("states");
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

        private void StateFields_TextChanged(object sender, EventArgs e)
        {
            buttonApply.Enabled = true;
        }

        private void statesComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            selectedStateIndex = statesComboBox.SelectedIndex;
            SetStateFields();
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

        private void button1_Click(object sender, EventArgs e)
        {
            DialogResult dr = MessageBox.Show("Are you sure you want FaceBERN! to forget your Facebook username/password?", "Confirm Deletion", MessageBoxButtons.YesNo);
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
    }
}
