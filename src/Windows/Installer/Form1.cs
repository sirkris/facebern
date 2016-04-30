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
using System.Reflection;
using System.Text;
using System.Threading;
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

        private string repoURL = @"https://github.com/sirkris/facebern.git";

        public string installerVersion = "1.0.0.a";

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
            
            SetStatus("Extracting Git library....");
            WriteResourceToFile("git2-381caf5.dll");
            WriteResourceToFile("LibGit2Sharp.dll");
            
            SetStatus("Please wait....");
        }

        public static byte[] ReadToEnd(System.IO.Stream stream)
        {
            long originalPosition = 0;

            if (stream.CanSeek)
            {
                originalPosition = stream.Position;
                stream.Position = 0;
            }

            try
            {
                byte[] readBuffer = new byte[4096];

                int totalBytesRead = 0;
                int bytesRead;

                while ((bytesRead = stream.Read(readBuffer, totalBytesRead, readBuffer.Length - totalBytesRead)) > 0)
                {
                    totalBytesRead += bytesRead;

                    if (totalBytesRead == readBuffer.Length)
                    {
                        int nextByte = stream.ReadByte();
                        if (nextByte != -1)
                        {
                            byte[] temp = new byte[readBuffer.Length * 2];
                            Buffer.BlockCopy(readBuffer, 0, temp, 0, readBuffer.Length);
                            Buffer.SetByte(temp, totalBytesRead, (byte)nextByte);
                            readBuffer = temp;
                            totalBytesRead++;
                        }
                    }
                }

                byte[] buffer = readBuffer;
                if (readBuffer.Length != totalBytesRead)
                {
                    buffer = new byte[totalBytesRead];
                    Buffer.BlockCopy(readBuffer, 0, buffer, 0, totalBytesRead);
                }
                return buffer;
            }
            finally
            {
                if (stream.CanSeek)
                {
                    stream.Position = originalPosition;
                }
            }
        }

        public void WriteResourceToFile(string resourceName)
        {
            if (File.Exists(Environment.CurrentDirectory + Path.DirectorySeparatorChar + resourceName))
            {
                try
                {
                    File.Delete(Environment.CurrentDirectory + Path.DirectorySeparatorChar + resourceName);
                }
                catch (Exception e)
                {
                    return;
                }
            }

            Assembly ass = Assembly.GetExecutingAssembly();
            Stream stream = ass.GetManifestResourceStream("Installer.Resources." + resourceName);
            File.WriteAllBytes(Environment.CurrentDirectory + Path.DirectorySeparatorChar + resourceName, ReadToEnd(stream));
            stream.Close();
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            if (installed == null)
            {
                /* Assume first-time installation and prompt the user accordingly.  --Kris */
                Workflow workflow = new Workflow(this);
                Thread thread = workflow.ExecuteInstallThread(installerVersion, repoURL);
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

                    // TODO - Copy Installer.exe to a Git-ignored/untracked name, execute it, then proceed with the update and delete when finished.  This will prevent file-lock conflicts on Git pull.  --Kris
                }
            }
        }

        private void statusTextBox_Enter(object sender, EventArgs e)
        {
            button1.Focus();
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

        public void Exit()
        {
            string appPath = GetAppPath();
            if (startAfter && appPath != null)
            {
                Process process = new Process();
                process.StartInfo.FileName = appPath;
                process.StartInfo.Arguments = @"/updated";  // This will prevent infinite cross-process loops in the event of an unforseen error.  --Kris
                process.StartInfo.LoadUserProfile = true;
                process.StartInfo.UseShellExecute = true;
                process.StartInfo.WorkingDirectory = Path.GetDirectoryName(appPath);
                process.Start();
            }
            
            this.Close();
        }

        public void SetStatus(string text, int percent = -1)
        {
            statusTextBox.Text = text;

            if (percent != -1)
            {
                progressBar1.Value = percent;
                progressBar1.Style = ProgressBarStyle.Continuous;
            }

            this.Refresh();
        }

        public void SetStartAfter(bool startAfter)
        {
            this.startAfter = startAfter;
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
