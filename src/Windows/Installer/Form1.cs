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
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Installer
{
    public partial class Form1 : Form
    {
        public string[] cliArgs;  // Command-line arguments used to launch the installer.  --Kris
        public string[] origArgs;  // Command-line arguments that were used to launch the application that launched this installer (used for re-launching).  --Kris

        public string repoBaseDir;
        public string githubRemoteName;
        public string branchName;

        private Remote remote;
        private Branch branch;
        private Branch branchRemote;

        private bool startAfter;
        private bool cleanup;
        private bool assumeUpdate;
        private bool uninstall;
        private int retry;
        private bool autoStart;

        private string installed;

        private string repoURL = @"https://github.com/sirkris/facebern.git";

        public string installerVersion = "1.0.0.b";

        public Form1(string[] cliArgs, string githubRemoteName, string branchName, bool startAfter, bool cleanup = false, bool assumeUpdate = false, 
            bool uninstall = false, int retry = 0, bool autoStart = false, string[] origArgs = null)
        {
            InitializeComponent();
            this.githubRemoteName = githubRemoteName;
            this.branchName = branchName;
            this.startAfter = startAfter;
            this.cleanup = cleanup;
            this.assumeUpdate = assumeUpdate;
            this.uninstall = uninstall;
            this.retry = retry;
            this.cliArgs = cliArgs;
            this.origArgs = origArgs;
            this.autoStart = autoStart;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            SetStatus("Checking for existing installation....");

            RegistryKey softwareKey = Registry.CurrentUser.OpenSubKey("Software", true);
            RegistryKey appKey = softwareKey.CreateSubKey("FaceBERN!");

            installed = (string) appKey.GetValue("Installed", null);

            appKey.Close();
            softwareKey.Close();
            
            SetStatus("Please wait....");
        }

        internal void StoreException(Exception e)
        {
            RegistryKey softwareKey = Registry.CurrentUser.OpenSubKey("Software", true);
            RegistryKey appKey = softwareKey.CreateSubKey("FaceBERN!");

            appKey.SetValue("lastInstallerException", e.ToString(), RegistryValueKind.String);

            appKey.Close();
            softwareKey.Close();
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
                    StoreException(e);
                    return;
                }
            }

            Assembly ass = Assembly.GetExecutingAssembly();
            Stream stream = ass.GetManifestResourceStream("Installer.Resources." + resourceName);
            File.WriteAllBytes(Environment.CurrentDirectory + Path.DirectorySeparatorChar + resourceName, ReadToEnd(stream));
            stream.Close();
        }

        private bool IsAdministrator()
        {
            return (new WindowsPrincipal(WindowsIdentity.GetCurrent())).IsInRole(WindowsBuiltInRole.Administrator);
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            if (cleanup)
            {
                SetStatus("Initializing clean-up thread....");

                Workflow workflow = new Workflow(this);
                workflow.ExecuteCleanup();

                Exit();
                return;
            }

            SetStatus("Extracting Git library....");

            WriteResourceToFile("git2-381caf5.dll");
            WriteResourceToFile("LibGit2Sharp.dll");

            SetStatus("Extracting SetACL utility....");

            WriteResourceToFile("SetACL.exe");

            SetStatus("Please wait....");

            /* Running as Administrator solves a lot of FSO issues.  --Kris */
            if (!IsAdministrator() && installed == null)
            {
                /* This will prevent infinite relaunching in the event that it fails to elevate for whatever reason.  --Kris */
                retry++;
                if (retry == 5)
                {
                    SetStatus("ERROR!  Unable to launch as Administrator!");
                    return;
                }

                string executingAssembly = System.Reflection.Assembly.GetEntryAssembly().Location;
                try
                {
                    string args = "";
                    for (int i = 0; i < cliArgs.Length; i++)
                    {
                        args += (args != "" ? " " : "") + cliArgs[i].Trim();
                    }

                    Process process = new Process();
                    process.StartInfo.FileName = executingAssembly;
                    process.StartInfo.Arguments = args + (args != "" ? " " : "") + "retry=" + retry.ToString() + (origArgs != null ? " origArgs=\"" + String.Join(@",", origArgs) + "\"" : "");
                    process.StartInfo.UseShellExecute = true;
                    process.StartInfo.Verb = "runas";  // Run as Administrator.  --Kris
                    process.Start();

                    this.Close();

                    return;
                }
                catch (Exception ex)
                {
                    SetStatus("ERROR:  Updater launch FAILED!");
                    StoreException(ex);
                    return;
                }
            }

            if (installed == null)
            {
                /* Assume first-time installation and prompt the user accordingly.  --Kris */
                Workflow workflow = new Workflow(this);
                Thread thread = workflow.ExecuteInstallThread(installerVersion, repoURL);
            }
            else if (assumeUpdate)
            {
                Update(origArgs);
            }
            else if (uninstall)
            {
                Uninstall();
            }
            else
            {
                /* Installer executable run manually by the user when FaceBERN! is automatically installed.  They can choose to either update or uninstall.  --Kris */
                using (UserPromptForm userPromptForm = new UserPromptForm(installerVersion, installed))
                {
                    userPromptForm.ShowDialog();
                    if (userPromptForm.cancel)
                    {
                        Exit();
                        return;
                    }
                    else
                    {
                        if (userPromptForm.updateRadioButton.Checked)
                        {
                            Update();
                        }
                        else if (userPromptForm.uninstallRadioButton.Checked)
                        {
                            DialogResult dr = MessageBox.Show("Are you sure you want to remove Birdie from your system?", "Confirm Uninstallation", MessageBoxButtons.YesNo);
                            if (dr == DialogResult.Yes)
                            {
                                Uninstall();
                            }
                            else
                            {
                                Exit();
                            }
                        }
                    }
                }
            }
        }

        private void Uninstall()
        {
                /* Copy this executable to an untracked name and run that.  This will enable the installer to uninstall itself, as well.  --Kris */
                string executingAssembly = System.Reflection.Assembly.GetExecutingAssembly().Location;
                string uninstallerName = "BirdieUninstall.exe";

                if (executingAssembly.IndexOf(uninstallerName) == -1)
                {
                    SetStatus("Preparing to launch uninstaller....");

                    repoBaseDir = installed;

                    string uninstallerPath = Path.Combine(Path.GetTempPath(), uninstallerName);
                    try
                    {
                        File.Copy(executingAssembly, uninstallerPath, true);
                    }
                    catch (Exception ex)
                    {
                        SetStatus("ERROR:  Installer copy FAILED!");
                        StoreException(ex);
                        return;
                    }

                    try
                    {
                        Process process = new Process();
                        process.StartInfo.FileName = uninstallerPath;
                        process.StartInfo.Arguments = "/uninstall";
                        process.StartInfo.Verb = "runas";  // Run as Administrator.  --Kris
                        process.StartInfo.UseShellExecute = true;
                        process.Start();
                    }
                    catch (Exception ex)
                    {
                        SetStatus("ERROR:  Uninstaller launch FAILED!");
                        StoreException(ex);
                        return;
                    }

                    this.Close();
                }
                else
                {
                    /* Perform the uninstall.  --Kris */
                    SetStatus("Preparing to uninstall....");

                    Workflow workflow = new Workflow(this);
                    workflow.ExecuteUninstallThread(installed);
                }
        }

        private void Update(string[] origArgs = null)
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
                /* Copy this executable to an untracked name and run that.  This will enable the installer to update itself, as well.  --Kris */
                string executingAssembly = System.Reflection.Assembly.GetExecutingAssembly().Location;
                string updaterName = "BirdieUpdater.exe";

                if (executingAssembly.IndexOf(updaterName) == -1)
                {
                    SetStatus("Update found!  Preparing to launch updater....");

                    string updaterPath = Path.Combine(repoBaseDir, updaterName);
                    try
                    {
                        File.Copy(executingAssembly, updaterPath, true);
                    }
                    catch (Exception ex)
                    {
                        SetStatus("ERROR:  Installer copy FAILED!");
                        StoreException(ex);
                        return;
                    }

                    try
                    {
                        Process process = new Process();
                        process.StartInfo.FileName = updaterPath;
                        process.StartInfo.Arguments = "githubRemoteName=" + githubRemoteName + " branchName=" + branchName + (startAfter ? " /startAfter" : "")
                            + " /assumeUpdate" + (origArgs != null ? " origArgs=\"" + String.Join(@",", origArgs) + "\"" : "");
                        process.Start();
                    }
                    catch (Exception ex)
                    {
                        SetStatus("ERROR:  Updater launch FAILED!");
                        StoreException(ex);
                        return;
                    }

                    this.Close();
                }
                else
                {
                    /* Perform the update!  --Kris */
                    SetStatus("Update found!  Preparing to install....");

                    if (origArgs == null)
                    {
                        origArgs = new string[99];
                    }

                    Workflow workflow = new Workflow(this);
                    workflow.ExecuteUpdateThread(startAfter, origArgs);
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

            return CheckAppPathForAppNames(new List<string> { @"Birdie.exe", @"FaceBERN!.exe" }, guesses);
        }

        /* In case they're running an older version from before the name change.  --Kris */
        public string CheckAppPathForAppNames(List<string> possibleNames, List<string> guesses)
        {
            foreach (string name in possibleNames)
            {
                string executable = Path.DirectorySeparatorChar + name;
                foreach (string guess in guesses)
                {
                    if (File.Exists(guess + executable))
                    {
                        return guess + executable;
                    }
                }
            }

            return null;
        }

        public void Exit()
        {
            string appPath = GetAppPath();
            if (startAfter && appPath != null)
            {
                string args;
                if (origArgs == null || origArgs.Length == 0)
                {
                    args = "";
                }
                else
                {
                    args = String.Join(" ", origArgs);
                }

                Process process = new Process();
                process.StartInfo.FileName = appPath;
                process.StartInfo.Arguments = @"/updated " // This will prevent infinite cross-process loops in the event of an unforseen error.  --Kris
                    + ( autoStart ? @"/autoStart " : "" ) 
                    + args;
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
