using LibGit2Sharp;
using LibGit2Sharp.Core;
using LibGit2Sharp.Handlers;
using Microsoft.Win32;
using Newtonsoft.Json;
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

        protected bool csLogEnabled = false;  // TODO - Fix some minor IO bugs before re-enabling.  Just don't have time to look into it right now.  It's an old mess, anyway.  --Kris

        protected string INIPath;

        private Icon trayIcon;

        public Log MainLog = new Log();

        private bool logging;
        private bool updated;
        private bool autoStart;
        private string[] cliArgs;

        private RegistryKey softwareKey;
        private RegistryKey appKey;

        public bool stop = false;

        private Workflow workflow = null;

        public int localInvitesSent = 0;
        public int remoteInvitesSent = 0;

        public int localTweetsTweeted = 0;
        public int remoteTweetsTweeted = 0;

        public int activeUsers = 0;
        public int totalUsers = 0;

        public Form1(bool updated = false, bool logging = true, bool autoStart = false, string[] cliArgs = null)
        {
            InitializeComponent();

            softwareKey = Registry.CurrentUser.OpenSubKey("Software", true);
            appKey = softwareKey.CreateSubKey("FaceBERN!");

            label3.Visible = false;
            label4.Visible = false;
            label6.Visible = false;
            labelInvitesSent.Visible = false;
            labelTweetsTweeted.Visible = false;
            labelActiveUsers.Visible = false;

            this.Resize += Form1_Resize;

            this.logging = logging;
            this.updated = updated;
            this.autoStart = autoStart;
            this.cliArgs = cliArgs;

            /* Initialize the log.  --Kris */
            logging = false;  // TODO - Fix the logging library then re-enable.  --Kris
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
            if (!updated)
            {
                if (CheckForUpdates(Globals.Config["AutoUpdate"].Equals("1")) && !(Globals.Config["AutoUpdate"].Equals("1")))
                {
                    DialogResult dr = MessageBox.Show("A newer version of Birdie has been found!  Install now?", "Update Found!", MessageBoxButtons.YesNo);
                    if (dr == DialogResult.Yes)
                    {
                        CheckForUpdates(true);
                    }
                    else
                    {
                        LogW("Update found but user chose not to install.  The stability of this software cannot be guaranteed if not promptly updated!");
                    }
                }
            }
            else
            {
                LogW("Launched by updater so no need to check for updates.");
            }

            Ready();
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            try
            {
                int n;
                if (Globals.Config.ContainsKey("SelectedBrowser")
                    && Globals.Config["SelectedBrowser"] != null
                    && Globals.BrowserConsts().ContainsKey(Globals.Config["SelectedBrowser"].ToLower()))
                {
                    browserModeComboBox.SelectedIndex = Globals.BrowserConst(Globals.Config["SelectedBrowser"]);
                }
            }
            catch (Exception ex)
            {
                string msg = "Unable to load browser preference into combobox.";

                Log("Warning:  " + msg);
                // TODO - Report it.  --Kris
            }

            workflow = new Workflow(this);
            workflow.ExecuteInterComThread();

            int retry = 3;
            while (Application.OpenForms[this.Name] == null)
            {
                retry--;
                if (retry == 0)
                {
                    return;
                }

                System.Threading.Thread.Sleep(1000);
            }

            Application.OpenForms[this.Name].Focus();

            if (appKey.GetValue("PostInstallNeeded", null) != null)
            {
                LogW("Launching post-installation wizard....");

                PostInstall postInstall = new PostInstall(this, Globals.__VERSION__);
                postInstall.ShowDialog();
            }
            else if (autoStart)
            {
                LogW("Auto-start initiated.");

                buttonStart.PerformClick();
                this.WindowState = FormWindowState.Minimized;
            }
        }

        public void SetDefaults()
        {
            /* Only show the DEBUG menu if we've launched in DEBUG mode.  --Kris */
#if (!DEBUG)
            DEBUGToolStripMenuItem.Visible = false;
#endif
            Globals.Config = new Dictionary<string, string>();

            //Globals.Config.Add("CurrentDirectory", Environment.CurrentDirectory);
            Globals.Config.Add("AutoUpdate", "1");
            Globals.Config.Add("UseFTBEvents", "1");
            Globals.Config.Add("UseCustomEvents", "0");
            Globals.Config.Add("CheckRememberPasswordByDefault", "1");
            Globals.Config.Add("TwitterCampaignRunBernieRun", "1");
            Globals.Config.Add("TwitterCampaignRedditS4P", "1");
            Globals.Config.Add("TwitterCampaignRedditPolRev", "1");
            Globals.Config.Add("EnableFacebanking", "1");
            Globals.Config.Add("EnableTwitter", "1");
            Globals.Config.Add("TweetIntervalMinutes", "30");
            Globals.Config.Add("HideFacebookBrowser", "1");
            Globals.Config.Add("SelectedBrowser", "Firefox");

            /* How long to wait between GOTV checks.  --Kris */
            Globals.Config.Add("GOTVIntervalHours", "24");

            /* Each comma-delineated value represents how many days prior to a state's primary/caucus to execute a GOTV for that state.  Multiple entries means multiple GOTV runs.  --Kris */
            //Globals.Config.Add("DefaultGOTVDaysBack", "30,10,1");  // DEPRECATED.  Now we just do all upcoming states, in order of which ones come next.  --Kris

            /* Remote name for Github repo.  If you used the installer, it should be origin.  --Kris */
            //Globals.Config.Add("GithubRemoteName", "origin"); // Add to your config if you want to overwrite whatever's in the registry.  --Kris

            /* Branch to use for updates.  It's recommended you go with master or develop.  --Kris */
            //Globals.Config.Add("RepoBranch", "master"); // Add to your config if you want to overwrite whatever's in the registry.  --Kris

            this.INIPath = (Globals.ConfigDir != null ? Globals.ConfigDir : "") 
                + Path.DirectorySeparatorChar 
                + (Globals.MainINI != null ? Globals.MainINI : Globals.__APPNAME__ + @".ini");
            Directory.CreateDirectory(Globals.ConfigDir);

            Globals.bernieFacebookIDs = new List<string>();
            Globals.bernieFacebookIDs.Add("9124187907");
            Globals.bernieFacebookIDs.Add("124955570892789");

            string s = (string) appKey.GetValue("requestedFTBInvite", null, RegistryValueOptions.None);
            if (s != null)
            {
                Globals.requestedFTBInvite = s.Equals("1");
            }

            SetStateDefaults();

            UpdateVersion();  // Update the version text.  --Kris
            SetProgressBar(Globals.PROGRESSBAR_HIDDEN);  // Hide the progress bar.  --Kris

            HideCaret(outBox.Handle);
        }

        public void SetStateDefaults()
        {
            Dictionary<string, States> states = new Dictionary<string, States>();

            /* Add the state entries.  --Kris */
            states.Add("AK", new States("", "Alaska", "2016-03-26", "Caucus", "Closed", 10, 2, 1, "108083605879747"));
            states.Add("AL", new States("", "Alabama", "2016-03-01", "Primary", "Open", 10, 2, 1, "104037882965264"));
            states.Add("AR", new States("", "Arkansas", "2016-03-01", "Primary", "Open", 10, 2, 1, "111689148842696"));
            states.Add("AZ", new States("", "Arizona", "2016-03-22", "Primary", "Closed", 10, 2, 1, "108296539194138"));
            states.Add("CA", new States("", "California", "2016-06-07", "Primary", "Semi-Closed", 250, 3, 3, "108131585873862", "1558296317802512"));
            states.Add("CO", new States("", "Colorado", "2016-03-01", "Caucus", "Closed", 10, 2, 1, "106153826081984"));
            states.Add("CT", new States("", "Connecticut", "2016-04-26", "Primary", "Closed", 10, 2, 1, "112750485405808", "574021212779237"));
            states.Add("DE", new States("", "Delaware", "2016-04-26", "Primary", "Closed", 10, 2, 1, "105643859470062", "214899885533267"));
            states.Add("FL", new States("", "Florida", "2016-03-15", "Primary", "Closed", 50, 3, 2, "109714185714936"));
            states.Add("GA", new States("", "Georgia", "2016-03-01", "Primary", "Open", 10, 2, 1, "103994709636969"));
            states.Add("HI", new States("", "Hawaii", "2016-03-26", "Caucus", "Semi-Closed", 10, 2, 1, "113667228643818"));
            states.Add("IA", new States("", "Iowa", "2016-02-01", "Caucus", "Semi-Open", 10, 2, 1, "104004246303834"));
            states.Add("ID", new States("", "Idaho", "2016-03-22", "Caucus", "Open", 10, 2, 1, "108037302558105"));
            states.Add("IL", new States("", "Illinois", "2016-03-15", "Primary", "Open", 10, 2, 1, "112386318775352"));
            states.Add("IN", new States("", "Indiana", "2016-05-03", "Primary", "Open", 10, 2, 1, "111957282154793", "133011150427768"));
            states.Add("KS", new States("", "Kansas", "2016-03-05", "Caucus", "Closed", 10, 2, 1, "105493439483468"));
            states.Add("KY", new States("", "Kentucky", "2016-05-17", "Primary", "Closed", 10, 2, 1, "109438335740656", "521701654668687"));
            states.Add("LA", new States("", "Louisiana", "2016-03-05", "Primary", "Closed", 10, 2, 1, "112822538733611"));
            states.Add("MA", new States("", "Massachusetts", "2016-03-01", "Primary", "Semi-Closed", 10, 2, 1, "112439102104396"));
            states.Add("MD", new States("", "Maryland", "2016-04-26", "Primary", "Closed", 10, 2, 1, "108178019209812", "1032906533414147"));
            states.Add("ME", new States("", "Maine", "2016-03-06", "Caucus", "Closed", 10, 2, 1, "108603925831326"));
            states.Add("MI", new States("", "Michigan", "2016-03-08", "Primary", "Open", 10, 2, 1, "109706309047793"));
            states.Add("MN", new States("", "Minnesota", "2016-03-01", "Caucus", "Open", 10, 2, 1, "112577505420980"));
            states.Add("MO", new States("", "Missouri", "2016-03-15", "Primary", "Open", 10, 2, 1, "103118929728297"));
            states.Add("MS", new States("", "Mississippi", "2016-03-08", "Primary", "Open", 10, 2, 1, "113067432040067"));
            states.Add("MT", new States("", "Montana", "2016-06-07", "Primary", "Open", 10, 2, 1, "109983559020167", "364344583689477"));
            states.Add("NC", new States("", "North Carolina", "2016-03-15", "Primary", "Semi-Closed", 10, 2, 1, "104083326294266"));
            states.Add("ND", new States("", "North Dakota", "2016-06-07", "Caucus", "Closed", 10, 2, 1, "104131666289619", "160355097698472"));
            states.Add("NE", new States("", "Nebraska", "2016-03-05", "Caucus", "Closed", 10, 2, 1, "109306932420886"));
            states.Add("NH", new States("", "New Hampshire", "2016-02-09", "Primary", "Semi-Closed", 10, 2, 1, "105486989486087"));
            states.Add("NJ", new States("", "New Jersey", "2016-06-07", "Primary", "Semi-Closed", 100, 2, 2, "108325505857259", "229216130762869"));
            states.Add("NM", new States("", "New Mexico", "2016-06-07", "Primary", "Closed", 10, 2, 1, "108301835856691", "279799149019906"));
            states.Add("NV", new States("", "Nevada", "2016-02-20", "Caucus", "Closed", 10, 2, 1, "109176885767113"));
            states.Add("NY", new States("", "New York", "2016-04-19", "Primary", "Closed", 200, 3, 3, "112825018731802", "811998435572003"));
            states.Add("OH", new States("", "Ohio", "2016-03-15", "Primary", "Semi-Open", 10, 2, 1, "104024609634842"));
            states.Add("OK", new States("", "Oklahoma", "2016-03-01", "Primary", "Semi-Closed", 10, 2, 1, "105818976117390"));
            states.Add("OR", new States("", "Oregon", "2016-05-17", "Primary", "Closed", 10, 2, 1, "109564342404151", "605367332973455"));
            states.Add("PA", new States("", "Pennsylvania", "2016-04-26", "Primary", "Closed", 10, 2, 1, "105528489480786", "1721567841460557"));
            states.Add("RI", new States("", "Rhode Island", "2016-04-26", "Primary", "Semi-Closed", 10, 2, 1, "108295552526163", "1776422845918803"));
            states.Add("SC", new States("", "South Carolina", "2016-02-27", "Primary", "Open", 10, 2, 1, "108635949160808"));
            states.Add("SD", new States("", "South Dakota", "2016-06-07", "Primary", "Semi-Closed", 10, 2, 1, "112283278784694", "502120086651398"));
            states.Add("TN", new States("", "Tennessee", "2016-03-01", "Primary", "Open", 10, 2, 1, "108545005836236"));
            states.Add("TX", new States("", "Texas", "2016-03-01", "Primary", "Open", 200, 3, 2, "108337852519784"));
            states.Add("UT", new States("", "Utah", "2016-03-22", "Caucus", "Semi-Open", 10, 2, 1, "104164412953145"));
            states.Add("VA", new States("", "Virginia", "2016-03-01", "Primary", "Open", 10, 2, 1, "109564639069465"));
            states.Add("VT", new States("", "Vermont", "2016-03-01", "Primary", "Open", 10, 2, 1, "107907135897622"));
            states.Add("WA", new States("", "Washington", "2016-03-26", "Caucus", "Open", 10, 2, 1, "110453875642908"));
            states.Add("WI", new States("", "Wisconsin", "2016-04-05", "Primary", "Open", 10, 2, 1, "109146809103536"));
            states.Add("WV", new States("", "West Virginia", "2016-05-10", "Primary", "Semi-Closed", 10, 2, 1, "112083625475436", "1582995155349064"));
            states.Add("WY", new States("", "Wyoming", "2016-04-09", "Caucus", "Closed", 10, 2, 1, "104039182964473"));
            states.Add("DC", new States("", "District of Columbia", "2016-06-14", "Primary", "Closed", 20, 2, 1, "110184922344060", "273158089687943"));
            states.Add("AS", new States("", "American Samoa", "2016-03-01", "Caucus", "Closed", 10, 2, 1, "112481292099977"));
            states.Add("GU", new States("", "Guam", "2016-05-07", "Caucus", "Closed", 10, 2, 1, "112565748760314", "1703259566578785"));
            states.Add("MP", new States("", "Northern Mariana Islands", "2016-03-12", "Caucus", "Closed", 5, 1, 1, "105540149479841"));
            states.Add("PR", new States("", "Puerto Rico", "2016-06-05", "Caucus", "Open", 25, 2, 1, "108461009175078", "496254420561306"));
            states.Add("VI", new States("", "U.S. Virgin Islands", "2016-06-04", "Caucus", "Open", 10, 2, 1, "111110385577198", "700380050102161"));
            states.Add("DA", new States("", "Democrats Abroad", "2016-03-08", "Primary", "Closed", 10, 1, 1, ""));

            foreach (KeyValuePair<string, States> state in states)
            {
                states[state.Key].abbr = state.Key;
            }

            Globals.StateConfigs = states;
        }

        public string GetRepoBaseDir()
        {
            // TODO - Make this smarter.  For now, I just want this to work from the Debug and Release directories to make testing a bit more convenient.  --Kris
            if (Environment.CurrentDirectory.IndexOf("Debug") != -1 || Environment.CurrentDirectory.IndexOf("Release") != -1)
            {
                return Environment.CurrentDirectory.Substring(0, Environment.CurrentDirectory.IndexOf(@"\src\Windows\FaceBERN!\bin\"));
            }
            else
            {
                return Environment.CurrentDirectory;
            }
        }

        public string GetInstallerPath()
        {
            string installed = (string) appKey.GetValue("Installed", null);

            List<string> guesses = new List<string>();
            if (installed != null)
            {
                guesses.Add(installed);
            }
            guesses.Add(Environment.CurrentDirectory);

            string mainBin = Path.DirectorySeparatorChar + Path.Combine(@"FaceBERN!", "bin") + Path.DirectorySeparatorChar;
            string instBin = Path.DirectorySeparatorChar + Path.Combine("Installer", "bin") + Path.DirectorySeparatorChar;

            if (Directory.Exists(mainBin) && Directory.Exists(instBin))
            {
                guesses.Add(Environment.CurrentDirectory.Substring(0, Environment.CurrentDirectory.IndexOf(mainBin)) + instBin + "Release");
                guesses.Add(Environment.CurrentDirectory.Substring(0, Environment.CurrentDirectory.IndexOf(mainBin)) + instBin + "Debug");
            }

            string executable = Path.DirectorySeparatorChar + "BirdieSetup.exe";
            foreach (string guess in guesses)
            {
                if (File.Exists(guess + executable))
                {
                    return guess + executable;
                }
            }

            LogW("Warning:  Unable to locate Birdie Installer!");

            return null;
        }

        public string getGithubRemoteName()
        {
            return (string) appKey.GetValue("GithubRemoteName", (Globals.Config.ContainsKey( "GithubRemoteName" ) ? Globals.Config["GithubRemoteName"] : "origin"));
        }

        public string getBranchName()
        {
            return (string) appKey.GetValue("BranchName", (Globals.Config.ContainsKey( "RepoBranch" ) ? Globals.Config["RepoBranch"] : "master"));
        }

        /* Checks for updates, then either returns whether an update is available or closes this app and runs the installer/updater if there is an update and autoInstall is true.  --Kris */
        public bool CheckForUpdates(bool autoInstall = false)
        {
            LogW("Checking for updates....");

            string githubRemoteName = getGithubRemoteName();
            string branchName = getBranchName();

            string shaLocal;
            string shaRemote;
            using (var repo = new Repository(GetRepoBaseDir()))
            {
                /* Do a git fetch to get the latest remotes data.  --Kris */
                LogW("> git fetch " + githubRemoteName, false);
                Remote remote = repo.Network.Remotes[githubRemoteName];
                repo.Network.Fetch(remote);

                /* Compare the current local revision SHA with the newest revision SHA on the remote copy of the branch.  If they don't match, an update is needed.  --Kris */
                //Branch branch = repo.Head;  // Current/active local branch.  --Kris
                Branch branch = GetBranch(repo, branchName);
                if (branch == null)
                {
                    return false;
                }

                LogW("Active branch is:  " + repo.Head.CanonicalName, false);
                LogW("Using update branch: " + branch.FriendlyName);
                
                shaLocal = branch.Tip.Sha;  // SHA revision string for HEAD.  --Kris
                LogW("Current revision on local....  " + shaLocal);

                Branch branchRemote = repo.Branches[githubRemoteName + @"/" + branch.FriendlyName];
                shaRemote = branchRemote.Tip.Sha;
                LogW("Current revision on remote...  " + shaRemote);

                /* Save these values to the registry so the installer can run without having the branch/remote names passed as arguments.  --Kris */
                appKey.SetValue("GithubRemoteName", githubRemoteName, RegistryValueKind.String);
                appKey.SetValue("BranchName", branchName, RegistryValueKind.String);

                appKey.Flush();
                softwareKey.Flush();
            }

            if (autoInstall == true && !shaLocal.Equals(shaRemote))
            {
                LogW("Update found!  This application will close then restart.  Preparing to run installer....");
                System.Threading.Thread.Sleep(1000);

                ExecuteInstaller();
            }
            
            return !shaLocal.Equals(shaRemote);
        }

        private Branch GetBranch(Repository repo, string branchName)
        {
            string githubRemoteName = getGithubRemoteName();

            Branch branch = repo.Branches[branchName];
            if (branch == null)
            {
                Branch rb = repo.Branches[githubRemoteName + @"/" + branchName];
                branch = repo.CreateBranch(branchName, rb.Tip);
                if (branch == null)
                {
                    LogW("ERROR loading Git branch '" + branchName + "'!  Update check aborted.");

                    foreach (Branch b in repo.Branches)
                    {
                        LogW("DEBUG - Repo branch:  " + b.FriendlyName);  // Uncomment to get a list of available branches if the one attempted comes up null.  --Kris
                    }

                    return null;
                }
                else if (!branch.IsTracking)
                {
                    branch = repo.Branches.Update(branch, b => b.TrackedBranch = rb.CanonicalName);
                }
            }

            return branch;
        }

        private void UpdateVersion()
        {
            /* Add the branch and revision to the version string if we're not on the master branch.  --Kris */
            string branchName = getBranchName();

            if (!(branchName.Equals("master")))
            {
                using (var repo = new Repository(GetRepoBaseDir()))
                {
                    Branch branch = GetBranch(repo, branchName);
                    if (branch == null)
                    {
                        return;
                    }

                    Globals.__VERSION__ += @"." + branchName + @"." + repo.Branches[branchName].Tip.Sha;
                }

                label1.Location = new Point(label1.Location.X - 145, label1.Location.Y);
                labelVersion.Location = new Point(labelVersion.Location.X - 145, labelVersion.Location.Y);
            }

            labelVersion.Text = Globals.__VERSION__;
        }

        private void ExecuteInstaller()
        {
            string installerPath = GetInstallerPath();
            if (installerPath == null)
            {
                LogW("Unable to run installer.  Aborted.");
            }
            else
            {
                Process process = new Process();
                process.StartInfo.FileName = installerPath;
                process.StartInfo.Arguments = "githubRemoteName=" + getGithubRemoteName() + " branchName=" + getBranchName() 
                    + ( cliArgs != null ? " origArgs=\"" + String.Join( @",", cliArgs ) + "\"" : "" ) + " /startafter /assumeUpdate";
                process.Start();

                Exit();
            }
        }

        internal void Exit()
        {
            appKey.Close();
            softwareKey.Close();

            Application.Exit();
        }

        public void SetExecState(int state, string logName = null, Log logObj = null)
        {
            if (Globals.executionState == Globals.STATE_BROKEN)
            {
                return;
            }

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
                case Globals.STATE_STOPPING:
                    logState = "STOPPING";
                    break;
                case Globals.STATE_RESTARTING:
                    logState = "RESTARTING";
                    break;
                case Globals.STATE_TWITTERPIN:
                    logState = "TWITTERPIN";
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
                || Globals.executionState == Globals.STATE_BROKEN 
                || Globals.executionState == Globals.STATE_STOPPING 
                || Globals.executionState == Globals.STATE_TWITTERPIN)
            {
                buttonStart.Click -= buttonStart_Click;
                return;
            }

            Globals.devOverride = false;
            if (Control.ModifierKeys == Keys.Alt)
            {
                Globals.devOverride = true;

                LogW("** DEV OVERRIDE MODE ENGAGED! **");
            }

            if (workflow == null)
            {
                workflow = new Workflow(this);
            }

            if (Globals.executionState == Globals.STATE_READY)
            {
                SetExecState(Globals.STATE_VALIDATING);

                buttonStart_ToStop();

                Globals.thread = workflow.ExecuteThread();
                //workflow.Execute(browserModeComboBox.SelectedIndex);  // Use this if you want to debug on a single thread (be sure to comment the ExecuteThread() call).  --Kris

                LogW("Re-reticulating previously unreticulated splines....");
            }
            else
            {
                LogW("Stopping....");

                SetExecState(Globals.STATE_STOPPING);
                stop = true;

                Globals.devOverride = false;

                workflow.ExecuteShutdownThread(Globals.thread);
                
                LogW("Execution terminated by user.");

                // Use STATE_BROKEN if you want the error state to persist and prevent re-execution.  --Kris
                if (Globals.executionState == Globals.STATE_ERROR)
                {
                    Ready();
                }
            }
        }

        public void buttonStart_ToStart()
        {
            buttonStart.BackgroundImage = FaceBERN_.Properties.Resources.flames_button_bg;
            buttonStart.ForeColor = Color.Yellow;
            buttonStart.Text = "START";

            browserModeComboBox.Enabled = true;

            SetProgressBar(Globals.PROGRESSBAR_HIDDEN);
        }

        public void buttonStart_ToStop()
        {
            buttonStart.BackgroundImage = null;
            buttonStart.ForeColor = Color.Red;
            buttonStart.Text = "STOP";

            browserModeComboBox.Enabled = false;
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
                Globals.sINI.Create(this.INIPath, "Birdie (FaceBERN!) Configuration File", "Generated by Version " + Globals.__VERSION__, config);
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
                string configPath = Path.Combine(configDir, state.Key + ".ini");
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
                    config[sectionName].Add("facebookId", state.Value.facebookId);
                    config[sectionName].Add("FTBEventId", state.Value.FTBEventId);

                    config[sectionName].Add("enableGOTV", (state.Value.enableGOTV == true ? "1" : "0"));

                    Globals.sINI.Create(configPath, "Birdie (FaceBERN!) State Configuration File for " + state.Value.name, "Generated by Version " + Globals.__VERSION__, config);
                }

                Dictionary<string, string> conf = Globals.sINI.Load(configPath);

                foreach (KeyValuePair<string, string> pair in conf)
                {
                    switch (pair.Key)
                    {
                        case "abbr":
                        case "name":
                        case "primaryAccess":
                        case "primaryType":
                        case "facebookId":
                        case "FTBEventId":
                            Globals.StateConfigs[state.Key].GetType().GetField(pair.Key).SetValue(Globals.StateConfigs[state.Key], pair.Value);
                            break;
                        case "enableGOTV":
                            Globals.StateConfigs[state.Key].GetType().GetField(pair.Key).SetValue(Globals.StateConfigs[state.Key], pair.Value.Equals("1"));
                            break;
                        case "primaryDate":
                            Globals.StateConfigs[state.Key].GetType().GetField(pair.Key).SetValue(Globals.StateConfigs[state.Key], 
                                DateTime.ParseExact(pair.Value, "yyyy-MM-dd", CultureInfo.InvariantCulture));
                            break;
                    }
                }

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
            stop = false;

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

        internal void UpdateInvitationsCount(int x = 1, bool clear = false)
        {
            if (!clear)
            {
                label3.Visible = true;
                labelInvitesSent.Visible = true;

                localInvitesSent += x;

                if (remoteInvitesSent == 0)
                {
                    labelInvitesSent.Text = localInvitesSent.ToString();
                }
                else
                {
                    labelInvitesSent.Text = localInvitesSent.ToString() + @" / " + remoteInvitesSent.ToString();
                }

                LogW("Incremented displayed invitations count by:  " + (x >= 0 ? "+" : "-") + x.ToString(), false);
            }
            else
            {
                labelInvitesSent.Text = "0";

                labelInvitesSent.Visible = false;
                label3.Visible = false;

                LogW("Cleared displayed invitations count and reset to 0.", false);
            }
        }

        internal void UpdateInvitationsCount(int x, int y)
        {
            label3.Visible = true;
            labelInvitesSent.Visible = true;

            localInvitesSent += x;
            remoteInvitesSent = y;

            if (remoteInvitesSent == 0)
            {
                labelInvitesSent.Text = localInvitesSent.ToString();
            }
            else
            {
                labelInvitesSent.Text = localInvitesSent.ToString() + @" / " + remoteInvitesSent.ToString();
            }

            LogW("Incremented local invitations count by:  " + (x >= 0 ? "+" : "-") + x.ToString(), false);
            LogW("Set remote invitions count to:  " + y.ToString(), false);
        }

        internal void SetInvitationsCount(int x, int y)
        {
            label3.Visible = true;
            labelInvitesSent.Visible = true;

            if (x != -1)
            {
                localInvitesSent = x;
            }
            remoteInvitesSent = y;

            if (remoteInvitesSent == 0)
            {
                labelInvitesSent.Text = localInvitesSent.ToString();
            }
            else
            {
                labelInvitesSent.Text = localInvitesSent.ToString() + @" / " + remoteInvitesSent.ToString();
            }

            LogW("Set local invitations count to:  " + x.ToString(), false);
            LogW("Set remote invitions count to:  " + y.ToString(), false);
        }

        internal void SetTweetsTweeted(int x, int y)
        {
            label4.Visible = true;
            labelTweetsTweeted.Visible = true;

            if (x != -1)
            {
                localTweetsTweeted = x;
            }
            remoteTweetsTweeted = y;

            if (remoteInvitesSent == 0)
            {
                labelTweetsTweeted.Text = localTweetsTweeted.ToString();
            }
            else
            {
                labelTweetsTweeted.Text = localTweetsTweeted.ToString() + @" / " + remoteTweetsTweeted.ToString();
            }

            LogW("Set local tweets count to:  " + x.ToString(), false);
            LogW("Set remote tweets count to:  " + y.ToString(), false);
        }

        internal void SetActiveUsers(int active, int total)
        {
            label6.Visible = true;
            labelActiveUsers.Visible = true;

            activeUsers = active;
            totalUsers = total;

            labelActiveUsers.Text = activeUsers.ToString() + @" / " + totalUsers.ToString();

            LogW("Set active users count to:  " + active.ToString(), false);
            LogW("Set total users count to:  " + total.ToString(), false);
        }

        public void SetProgressBar(int percent)
        {
            if (percent < -1)
            {
                mainProgressBar.Visible = false;
                mainProgressBar.Value = 0;
            }
            else if (percent == -1)
            {
                mainProgressBar.Value = 0;
                mainProgressBar.Style = ProgressBarStyle.Marquee;
                mainProgressBar.Visible = true;
            }
            else
            {
                mainProgressBar.Value = percent;
                mainProgressBar.Style = ProgressBarStyle.Continuous;
                mainProgressBar.Visible = true;
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
            Exit();
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
            FormSettings settings = new FormSettings(this, this.TopMost);
            settings.Show(); // TODO - Should probably be ShowDialog(), now that I think of it....  --Kris
        }

        private void exitToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Exit();
        }

        private void aboutToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            About about = new About();
            about.Show();
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

        private void checkForUpdatesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Globals.executionState != Globals.STATE_READY)
            {
                LogW("You cannot check for updates right now!  Sorry.");
            }
            else
            {
                if (CheckForUpdates() == true)
                {
                    DialogResult dr = MessageBox.Show("A newer version of Birdie has been found!  Install now?", "Update Found!", MessageBoxButtons.YesNo);
                    if (dr == DialogResult.Yes)
                    {
                        CheckForUpdates(true);
                    }
                    else
                    {
                        LogW("Update found but user chose not to install.  The stability of this software cannot be guaranteed if not promptly updated!");
                    }
                }
                else
                {
                    LogW("You are running the most current version.  No update is required at this time.");
                }
            }
        }

        private void launchPostInstallerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PostInstall postInstall = new PostInstall(this, Globals.__VERSION__);
            postInstall.Show();
        }

        private void browserModeComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            Globals.Config["SelectedBrowser"] = Globals.BrowserName(browserModeComboBox.SelectedIndex);
            Globals.sINI.Save(Path.Combine(Globals.ConfigDir, Globals.MainINI), Globals.Config);
        }

        private void throwExceptionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            throw new Exception("DEBUG Exception Thrown by User.");
        }
    }
}
