using LibGit2Sharp;
using LibGit2Sharp.Core;
using LibGit2Sharp.Handlers;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Installer
{
    public partial class Form1 : Form
    {
        private string repoBaseDir;
        private string githubRemoteName;
        private string branchName;

        private Remote remote;
        private Branch branch;
        private Branch branchRemote;

        private bool startAfter;
        private string installed;

        public Form1(string githubRemoteName, string branchName, bool startAfter)
        {
            InitializeComponent();
            this.githubRemoteName = githubRemoteName;
            this.branchName = branchName;
            this.startAfter = startAfter;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            SetStatus("Checking for existing installation....");

            RegistryKey softwareKey = Registry.CurrentUser.OpenSubKey("Software", true);
            RegistryKey appKey = softwareKey.CreateSubKey("FaceBERN!");

            installed = (string) appKey.GetValue("Installed", null);
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            if (installed == null)
            {
                /* Assume first-time installation and prompt the user accordingly.  --Kris */

            }
            else
            {
                /* Already installed so check for updates without user input.  --Kris */
                repoBaseDir = installed;

                SetStatus("Checking for updates....");
                if (!CheckForUpdates())
                {
                    Exit();  // No update needed.  --Kris
                }
                else
                {
                    SetStatus("Update found!  Preparing to install....");


                }
            }
        }

        public string GetAppPath()
        {
            RegistryKey softwareKey = Registry.CurrentUser.OpenSubKey("Software", true);
            RegistryKey appKey = softwareKey.CreateSubKey("FaceBERN!");

            string installed = (string) appKey.GetValue("Installed", null);

            string buildBasePath = @"\Installer\bin\";

            List<string> buildPaths = new List<string>();
            buildPaths.Add(@"\FaceBERN!\bin\Release");
            buildPaths.Add(@"\FaceBERN!\bin\Debug");

            List<string> guesses = new List<string>();
            if (installed != null)
            {
                guesses.Add(installed);
            }
            guesses.Add(Environment.CurrentDirectory);
            if (Environment.CurrentDirectory.IndexOf(buildBasePath) != -1)
            {
                foreach (string buildPath in buildPaths)
                {
                    guesses.Add(Environment.CurrentDirectory.Substring(0, Environment.CurrentDirectory.IndexOf(buildBasePath)) + buildPath);
                }
            }

            string executable = Path.DirectorySeparatorChar + "FaceBERN!.exe";
            foreach (string guess in guesses)
            {
                if (File.Exists(guess + executable))
                {
                    return guess + executable;
                }
            }

            return null;
        }

        private void Exit()
        {
            string appPath = GetAppPath();
            if (startAfter && appPath != null)
            {
                Process process = new Process();
                process.StartInfo.FileName = appPath;
                process.StartInfo.Arguments = @"/updated";  // This will prevent infinite cross-process loops in the event of an unforseen error.  --Kris
                process.Start();
            }
            
            this.Close();
        }

        private void SetStatus(string text, int percent = -1)
        {
            statusTextBox.Text = text;

            if (percent != -1)
            {
                progressBar1.Value = percent;
            }
        }

        public bool CheckForUpdates()
        {
            string shaLocal;
            string shaRemote;
            using (var repo = new Repository(repoBaseDir))
            {
                /* Do a git fetch to get the latest remotes data.  --Kris */
                remote = repo.Network.Remotes[githubRemoteName];
                repo.Network.Fetch(remote);

                /* Compare the current local revision SHA with the newest revision SHA on the remote copy of the branch.  If they don't match, an update is needed.  --Kris */
                //Branch branch = repo.Head;  // Current/active local branch.  --Kris
                branch = repo.Branches[branchName];

                shaLocal = branch.Tip.Sha;  // SHA revision string for HEAD.  --Kris

                branchRemote = repo.Branches[githubRemoteName + @"/" + branch.FriendlyName];
                shaRemote = branchRemote.Tip.Sha;
            }

            return !shaLocal.Equals(shaRemote);
        }
    }
}
