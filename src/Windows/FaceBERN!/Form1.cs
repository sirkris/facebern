using System;
using System.Collections.Generic;
using System.Globalization;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FaceBERN_
{
    public partial class Form1 : Form
    {
        protected const string logName = "FaceBERN!";
        protected string logLibDir = Environment.CurrentDirectory;

        protected Assembly csLog = null;
        protected Type csLogType = null;
        protected object csLogInstance = null;

        protected bool csLogEnabled = true;

        protected string INIPath;

        private Icon trayIcon;

        public Log MainLog = new Log();

        public Form1(bool logging = true)
        {
            InitializeComponent();
            labelVersion.Text = Globals.__VERSION__;
            label3.Visible = false;
            labelInvitesSent.Visible = false;
            this.Resize += Form1_Resize;

            /* Initialize the log.  --Kris */
            if (logging == true)
            {
                InitLog();
            }
        }

        [DllImport("user32.dll", EntryPoint = "ShowCaret")]
        public static extern long ShowCaret(IntPtr hwnd);

        [DllImport("user32.dll", EntryPoint = "HideCaret")]
        public static extern long HideCaret(IntPtr hwnd);

        private void Form1_Load(object sender, EventArgs e)
        {
            SetDefaults();
            InitINI();
            LoadINI();
            SetTrayIcon();
            HideCaret(outBox.Handle);
            Ready();
        }

        public void SetDefaults()
        {
            Globals.Config = new Dictionary<string, string>();

            //Globals.Config.Add("CurrentDirectory", Environment.CurrentDirectory);
            Globals.Config.Add("AutoUpdate", "1");
            Globals.Config.Add("UseFTBEvents", "1");
            Globals.Config.Add("UseCustomEvents", "1");
            Globals.Config.Add("CheckRememberPasswordByDefault", "1");

            /* How long to wait between GOTV checks.  --Kris */
            Globals.Config.Add("GOTVIntervalHours", "24");

            /* Each comma-delineated value represents how many days prior to a state's primary/caucus to execute a GOTV for that state.  Multiple entries means multiple GOTV runs.  --Kris */
            Globals.Config.Add("DefaultGOTVDaysBack", "30,10,1");

            this.INIPath = (Globals.ConfigDir != null ? Globals.ConfigDir : "") 
                + Path.DirectorySeparatorChar 
                + (Globals.MainINI != null ? Globals.MainINI : @"FaceBERN!.ini");
            Directory.CreateDirectory(Globals.ConfigDir);

            Globals.bernieFacebookIDs = new List<string>();
            Globals.bernieFacebookIDs.Add("9124187907");
            Globals.bernieFacebookIDs.Add("124955570892789");

            SetStateDefaults();

            labelVersion.Text = Globals.__VERSION__;

            HideCaret(outBox.Handle);

            /* Disable Awesomium option until we can get it working.  New York is coming up and I don't have time to figure that shit out, sorry.  --Kris */
            browserModeComboBox.SelectedIndex = 1;
            browserModeComboBox.Enabled = false;
        }

        public void SetStateDefaults()
        {
            Dictionary<string, States> states = new Dictionary<string, States>();

            /* Add the state entries.  --Kris */
            states.Add("AK", new States("", "Alaska", "2016-03-26", "Caucus", "Closed", "108083605879747"));
            states.Add("AL", new States("", "Alabama", "2016-03-01", "Primary", "Open", "104037882965264"));
            states.Add("AR", new States("", "Arkansas", "2016-03-01", "Primary", "Open", "111689148842696"));
            states.Add("AZ", new States("", "Arizona", "2016-03-22", "Primary", "Closed", "108296539194138"));
            states.Add("CA", new States("", "California", "2016-06-07", "Primary", "Semi-Closed", "108131585873862", "1558296317802512"));
            states.Add("CO", new States("", "Colorado", "2016-03-01", "Caucus", "Closed", "106153826081984"));
            states.Add("CT", new States("", "Connecticut", "2016-04-26", "Primary", "Closed", "112750485405808", "574021212779237"));
            states.Add("DE", new States("", "Delaware", "2016-04-26", "Primary", "Closed", "105643859470062", "214899885533267"));
            states.Add("FL", new States("", "Florida", "2016-03-15", "Primary", "Closed", "109714185714936"));
            states.Add("GA", new States("", "Georgia", "2016-03-01", "Primary", "Open", "103994709636969"));
            states.Add("HI", new States("", "Hawaii", "2016-03-26", "Caucus", "Semi-Closed", "113667228643818"));
            states.Add("IA", new States("", "Iowa", "2016-02-01", "Caucus", "Semi-Open", "104004246303834"));
            states.Add("ID", new States("", "Idaho", "2016-03-22", "Caucus", "Open", "108037302558105"));
            states.Add("IL", new States("", "Illinois", "2016-03-15", "Primary", "Open", "112386318775352"));
            states.Add("IN", new States("", "Indiana", "2016-05-03", "Primary", "Open", "111957282154793", "133011150427768"));
            states.Add("KS", new States("", "Kansas", "2016-03-05", "Caucus", "Closed", "105493439483468"));
            states.Add("KY", new States("", "Kentucky", "2016-05-17", "Primary", "Closed", "109438335740656", "521701654668687"));
            states.Add("LA", new States("", "Louisiana", "2016-03-05", "Primary", "Closed", "112822538733611"));
            states.Add("MA", new States("", "Massachusetts", "2016-03-01", "Primary", "Semi-Closed", "112439102104396"));
            states.Add("MD", new States("", "Maryland", "2016-04-26", "Primary", "Closed", "108178019209812", "1032906533414147"));
            states.Add("ME", new States("", "Maine", "2016-03-06", "Caucus", "Closed", "108603925831326"));
            states.Add("MI", new States("", "Michigan", "2016-03-08", "Primary", "Open", "109706309047793"));
            states.Add("MN", new States("", "Minnesota", "2016-03-01", "Caucus", "Open", "112577505420980"));
            states.Add("MO", new States("", "Missouri", "2016-03-15", "Primary", "Open", "103118929728297"));
            states.Add("MS", new States("", "Mississippi", "2016-03-08", "Primary", "Open", "113067432040067"));
            states.Add("MT", new States("", "Montana", "2016-06-07", "Primary", "Open", "109983559020167"));
            states.Add("NC", new States("", "North Carolina", "2016-03-15", "Primary", "Semi-Closed", "104083326294266"));
            states.Add("ND", new States("", "North Dakota", "2016-06-07", "Caucus", "Closed", "104131666289619"));
            states.Add("NE", new States("", "Nebraska", "2016-03-05", "Caucus", "Closed", "109306932420886"));
            states.Add("NH", new States("", "New Hampshire", "2016-02-09", "Primary", "Semi-Closed", "105486989486087"));
            states.Add("NJ", new States("", "New Jersey", "2016-06-07", "Primary", "Semi-Closed", "108325505857259", "229216130762869"));
            states.Add("NM", new States("", "New Mexico", "2016-06-07", "Primary", "Closed", "108301835856691"));
            states.Add("NV", new States("", "Nevada", "2016-02-20", "Caucus", "Closed", "109176885767113"));
            states.Add("NY", new States("", "New York", "2016-04-19", "Primary", "Closed", "112825018731802", "811998435572003"));
            states.Add("OH", new States("", "Ohio", "2016-03-15", "Primary", "Semi-Open", "104024609634842"));
            states.Add("OK", new States("", "Oklahoma", "2016-03-01", "Primary", "Semi-Closed", "105818976117390"));
            states.Add("OR", new States("", "Oregon", "2016-05-17", "Primary", "Closed", "109564342404151", "605367332973455"));
            states.Add("PA", new States("", "Pennsylvania", "2016-04-26", "Primary", "Closed", "105528489480786", "1721567841460557"));
            states.Add("RI", new States("", "Rhode Island", "2016-04-26", "Primary", "Semi-Closed", "108295552526163", "1776422845918803"));
            states.Add("SC", new States("", "South Carolina", "2016-02-27", "Primary", "Open", "108635949160808"));
            states.Add("SD", new States("", "South Dakota", "2016-06-07", "Primary", "Semi-Closed", "112283278784694"));
            states.Add("TN", new States("", "Tennessee", "2016-03-01", "Primary", "Open", "108545005836236"));
            states.Add("TX", new States("", "Texas", "2016-03-01", "Primary", "Open", "108337852519784"));
            states.Add("UT", new States("", "Utah", "2016-03-22", "Caucus", "Semi-Open", "104164412953145"));
            states.Add("VA", new States("", "Virginia", "2016-03-01", "Primary", "Open", "109564639069465"));
            states.Add("VT", new States("", "Vermont", "2016-03-01", "Primary", "Open", "107907135897622"));
            states.Add("WA", new States("", "Washington", "2016-03-26", "Caucus", "Open", "110453875642908"));
            states.Add("WI", new States("", "Wisconsin", "2016-04-05", "Primary", "Open", "109146809103536"));
            states.Add("WV", new States("", "West Virginia", "2016-05-10", "Primary", "Semi-Closed", "112083625475436", "1582995155349064"));
            states.Add("WY", new States("", "Wyoming", "2016-04-09", "Caucus", "Closed", "104039182964473"));
            states.Add("DC", new States("", "District of Columbia", "2016-06-14", "Primary", "Closed", "110184922344060"));
            states.Add("AS", new States("", "American Samoa", "2016-03-01", "Caucus", "Closed", "112481292099977"));
            states.Add("GU", new States("", "Guam", "2016-05-07", "Caucus", "Closed", "112565748760314", "1703259566578785"));
            states.Add("MP", new States("", "Northern Mariana Islands", "2016-03-12", "Caucus", "Closed", "105540149479841"));
            states.Add("PR", new States("", "Puerto Rico", "2016-06-05", "Caucus", "Open", "108461009175078", "496254420561306"));
            states.Add("VI", new States("", "U.S. Virgin Islands", "2016-06-04", "Caucus", "Open", "111110385577198"));
            states.Add("DA", new States("", "Democrats Abroad", "2016-03-08", "Primary", "Closed", ""));

            foreach (KeyValuePair<string, States> state in states)
            {
                states[state.Key].abbr = state.Key;
            }

            Globals.StateConfigs = states;
        }

        public void SetExecState(int state, string logName = null, Log logObj = null)
        {
            if (logName == null)
            {
                logName = Form1.logName;
            }

            if (logObj == null)
            {
                logObj = this.MainLog;
            }

            string logState = null;
            switch (state)
            {
                default:
                    LogW("Error setting application state : Unknown state " + state.ToString());
                    state = Globals.executionState;
                    break;
                case Globals.STATE_INITIALIZING:
                    logState = "INITIALIZING";
                    break;
                case Globals.STATE_ERROR:
                    logState = "ERROR";
                    break;
                case Globals.STATE_BROKEN:
                    logState = "BROKEN";
                    break;
                case Globals.STATE_READY:
                    logState = "READY";
                    break;
                case Globals.STATE_VALIDATING:
                    logState = "VALIDATING";
                    break;
                case Globals.STATE_WAITING:
                    logState = "WAITING";
                    break;
                case Globals.STATE_SLEEPING:
                    logState = "SLEEPING";
                    break;
                case Globals.STATE_EXECUTING:
                    logState = "EXECUTING";
                    break;
            }

            if (state != Globals.executionState)
            {
                Globals.executionState = state;
                LogW("Execution state changed to:  " + logState, false, true, true, true, logName, logObj);
            }
        }

        private void buttonStart_Click(object sender, EventArgs e)
        {
            if (buttonStart.Enabled == false 
                || Globals.executionState < Globals.STATE_READY)
            {
                buttonStart.Click -= buttonStart_Click;
                return;
            }

            if (Globals.executionState == Globals.STATE_READY)
            {
                SetExecState(Globals.STATE_VALIDATING);

                buttonStart_ToStop();

                Workflow workflow = new Workflow(this);
                Globals.thread = workflow.ExecuteThread();
                //workflow.Execute(browserModeComboBox.SelectedIndex);  // Use this if you want to debug on a single thread (be sure to comment the ExecuteThread() call).  --Kris

                LogW("Re-reticulating previously unreticulated splines....");
            }
            else
            {
                LogW("Stopping....");

                Globals.thread.Abort();
                Globals.thread.Join(60000);

                LogW("Execution terminated by user.");

                buttonStart_ToStart();

                Ready();
            }
        }

        public void buttonStart_ToStart()
        {
            buttonStart.BackgroundImage = FaceBERN_.Properties.Resources.flames_button_bg;
            buttonStart.ForeColor = Color.Yellow;
            buttonStart.Text = "START";
        }

        public void buttonStart_ToStop()
        {
            buttonStart.BackgroundImage = null;
            buttonStart.ForeColor = Color.Red;
            buttonStart.Text = "STOP";
        }

        /* Prevents form flickering.  Taken from MSDN.  --Kris */
        public void EnableDoubleBuffering()
        {
            // Set the value of the double-buffering style bits to true.
            this.SetStyle(ControlStyles.DoubleBuffer |
               ControlStyles.UserPaint |
               ControlStyles.AllPaintingInWmPaint,
               true);
            this.UpdateStyles();
        }
        public void InitINI()
        {
            Globals.sINI = new INI(this.MainLog.csLog, this.MainLog.csLogType, this.MainLog.csLogInstance);
        }

        public void LoadINI()
        {
            if (File.Exists(this.INIPath) == false)
            {
                LogW("Config file '" + this.INIPath + "' does not exist.  Creating....", false);

                Dictionary<string, Dictionary<string, string>> config = new Dictionary<string, Dictionary<string, string>>();

                config["Settings"] = Globals.Config;
                Globals.sINI.Create(this.INIPath, "FaceBERN! Configuration File", "Generated by Version " + Globals.__VERSION__, config);
            }

            Dictionary<string, string> conf = Globals.sINI.Load(this.INIPath);

            foreach (KeyValuePair<string, string> directive in conf)
            {
                if (Globals.Config.ContainsKey(directive.Key))
                {
                    Globals.Config.Remove(directive.Key);
                }

                Globals.Config.Add(directive.Key, directive.Value);
            }

            LogConfig();

            LoadStateINIs();
        }

        public void LoadStateINIs(string sectionName = "Settings")
        {
            string configDir = (Globals.ConfigDir != null ? Globals.ConfigDir : "");
            foreach (KeyValuePair<string, States> state in Globals.StateConfigs)
            {
                string configPath = configDir + Path.DirectorySeparatorChar + state.Key + ".ini";
                if (File.Exists(configPath) == false)
                {
                    LogW("State config file '" + configPath + "' does not exist.  Creating....", false);

                    Dictionary<string, Dictionary<string, string>> config = new Dictionary<string, Dictionary<string, string>>();

                    config[sectionName] = new Dictionary<string, string>();
                    config[sectionName].Add("abbr", state.Value.abbr);
                    config[sectionName].Add("name", state.Value.name);
                    config[sectionName].Add("primaryDate", state.Value.primaryDate.ToString("yyyy-MM-dd"));
                    config[sectionName].Add("primaryType", state.Value.primaryType);
                    config[sectionName].Add("primaryAccess", state.Value.primaryAccess);

                    Globals.sINI.Create(configPath, "FaceBERN! State Configuration File for " + state.Value.name, "Generated by Version " + Globals.__VERSION__, config);
                }

                Dictionary<string, string> conf = Globals.sINI.Load(configPath);

                Globals.StateConfigs[state.Key].abbr = conf["abbr"];
                Globals.StateConfigs[state.Key].name = conf["name"];
                Globals.StateConfigs[state.Key].primaryAccess = conf["primaryAccess"];
                Globals.StateConfigs[state.Key].primaryDate = DateTime.ParseExact(conf["primaryDate"], "yyyy-MM-dd", CultureInfo.InvariantCulture);
                Globals.StateConfigs[state.Key].primaryType = conf["primaryType"];

                LogW("Loaded state configuration for " + state.Value.name, false);
            }
        }

        internal void LogConfig()
        {
            string confLogName = String.Concat(@"Conf_", DateTime.Now.ToString("yyyyMMdd-HHmmss.fffffff"));

            foreach (KeyValuePair<string, string> directive in Globals.Config)
            {
                this.MainLog.Append(confLogName, directive.Key + @" = " + directive.Value, true);
            }

            this.MainLog.Save(confLogName);
        }

        public void SetTrayIcon()
        {
            notifyIcon1.MouseDoubleClick += notifyIcon1_DoubleClick;
            trayIcon = notifyIcon1.Icon;
            //browserModeComboBox.SelectedIndex = 0;  // Uncomment when Awesomium is finally working.  --Kris
            notifyIcon1.Visible = false;
        }

        public void Ready(string logName = null)
        {
            if (logName == null)
            {
                logName = Form1.logName;
            }

            SetExecState(Globals.STATE_READY, logName);
            LogW("Ready.");
        }

        public void InitLog()
        {
            this.MainLog = new Log();
        }

        /* Interact with the log handler.  --Kris */
        internal void Log(string text = null, string action = "append", bool newline = true, string logName = null, Log logObj = null, bool remove = false)
        {
            if (logName == null)
            {
                logName = Form1.logName;
            }

            if (logObj == null)
            {
                //logObj = Globals.getLogObj(logName);
                logObj = MainLog;
            }

            if (csLogEnabled == true)
            {
                switch (action.ToLower())
                {
                    default:
                    case "append":
                        logObj.Append(logName, text, newline);
                        break;
                    case "increment":
                        logObj.Increment(logName, Int32.Parse(text));
                        break;
                    case "decrement":
                        logObj.Decrement(logName, Int32.Parse(text));
                        break;
                    case "save":
                        logObj.Save(logName, remove);
                        break;
                }
            }
        }

        /* Save the log buffer to file.  --Kris */
        internal void saveLog(string logName = null, Log logObj = null)
        {
            Log(null, "save", false, logName, logObj);
        }

        /* Append to log and optionally display in log window (should only include log entries that are relevant/useful to the end-user).  --Kris */
        internal void LogW(string text, bool show = true, bool appendW = true, bool newline = true, bool timestamp = true, string logName = null, Log logObj = null)
        {
            string logText = (timestamp ? "[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "] " : "") + text;

            if (logName == null)
            {
                logName = Form1.logName;
            }

            Log(logText, "append", newline, logName, logObj);

            if (show == true)
            {
                this.outBox.Text = (appendW ? this.outBox.Text : "") + text + (newline ? Environment.NewLine : "");
                this.outBox.SelectionStart = this.outBox.Text.Length;
                this.outBox.ScrollToCaret();

                HideCaret(outBox.Handle);
            }

            saveLog(logName, logObj);
        }

        internal void UpdateInvitationsCount(int n = 1, bool clear = false)
        {
            if (!clear)
            {
                label3.Visible = true;
                labelInvitesSent.Visible = true;

                labelInvitesSent.Text = (long.Parse(labelInvitesSent.Text) + n).ToString();

                LogW("Incremented displayed invitations count by:  " + (n >= 0 ? "+" : "-") + n.ToString(), false);
            }
            else
            {
                labelInvitesSent.Text = "0";

                labelInvitesSent.Visible = false;
                label3.Visible = false;

                LogW("Cleared displayed invitations count and reset to 0.", false);
            }
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                notifyIcon1.Visible = true;
                notifyIcon1.ShowBalloonTip(10000);
                this.Hide();
            }
            else
            {
                notifyIcon1.Visible = false;
            }
        }

        private void openControlCenterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
            notifyIcon1.Visible = false;
        }

        private void notifyIcon1_DoubleClick(object sender, EventArgs e)
        {
            openControlCenterToolStripMenuItem_Click(sender, e);
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void donateToBernieToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ProcessStartInfo pso = new ProcessStartInfo("https://secure.actblue.com/contribute/page/reddit-for-bernie");
            Process.Start(pso);
        }

        private void alwaysOnTopCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (alwaysOnTopCheckBox.Checked)
            {
                this.TopMost = true;
            }
            else
            {
                this.TopMost = false;
            }
        }

        private void settingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // TODO
        }

        private void exitToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void aboutToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            // TODO
        }

        private void startToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (buttonStart.Enabled
                && Globals.executionState == Globals.STATE_READY)
            {
                buttonStart_Click(sender, e);
            }
        }

        private void pauseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (buttonStart.Enabled
                && Globals.executionState > Globals.STATE_READY)
            {
                buttonStart_Click(sender, e);
            }
        }
    }
}
