using LibGit2Sharp;
using LibGit2Sharp.Core;
using LibGit2Sharp.Handlers;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Installer
{
    public class Workflow
    {
        private Form1 Main;

        public Workflow(Form1 Main)
        {
            this.Main = Main;
        }

        public Thread ExecuteInstallThread(string installerVersion, string repoURL)
        {
            Thread thread = new Thread(() => ExecuteInstall(installerVersion, repoURL));
            thread.Start();
            while (!thread.IsAlive) { }

            return thread;
        }

        public void ExecuteInstall(string installerVersion, string repoURL)
        {
            string installPath = null;
            string branchName = null;
            bool deleteSrc = false;
            using (WorkflowForm1 workflowForm1 = new WorkflowForm1(installerVersion))
            {
                workflowForm1.ShowDialog();
                if (workflowForm1.cancel)
                {
                    Exit();
                    return;
                }
                else
                {
                    using (WorkflowForm2 workflowForm2 = new WorkflowForm2())
                    {
                        workflowForm2.ShowDialog();
                        if (workflowForm2.cancel)
                        {
                            Exit();
                            return;
                        }
                        else
                        {
                            installPath = workflowForm2.installPath;
                            branchName = workflowForm2.branchName;
                            deleteSrc = !(workflowForm2.includeSrcCheckbox.Checked);
                        }
                    }
                }
            }

            if (installPath != null && branchName != null)
            {
                /* Perform the installation.  --Kris */
                SetStatus("Creating installation directory....", 0);

                try
                {
                    Directory.CreateDirectory(installPath);
                }
                catch (Exception ex)
                {
                    SetStatus("Directory creation FAILED : " + ex.Message);
                    return;
                }

                SetStatus("Downloading and unpacking program files....", 20);

                Repository.Clone(repoURL, installPath);

                if (!(branchName.ToLower().Equals("master")))
                {
                    SetStatus("Checking-out " + branchName + " branch....", 60);

                    using (Repository repo = new Repository(installPath))
                    {
                        repo.Checkout(repo.Branches[@"origin/" + branchName]);
                    }
                }

                SetStatus("Finalizing filesystem....", 80);

                /* Copy the executable.  --Kris */
                if (!File.Exists(installPath + Path.DirectorySeparatorChar + @"FaceBERN!.exe"))
                {
                    string binDir = @"src\Windows\FaceBERN!\bin";
                    if (Directory.Exists(installPath + Path.DirectorySeparatorChar + binDir))
                    {
                        if (Directory.Exists(installPath + Path.DirectorySeparatorChar + binDir + Path.DirectorySeparatorChar + "Release")
                            && File.Exists(installPath + Path.DirectorySeparatorChar + binDir + Path.DirectorySeparatorChar + "Release" + Path.DirectorySeparatorChar + @"FaceBERN.exe"))
                        {
                            File.Copy(installPath + Path.DirectorySeparatorChar + binDir + Path.DirectorySeparatorChar + "Release" + Path.DirectorySeparatorChar + @"FaceBERN.exe",
                                        installPath + Path.DirectorySeparatorChar + @"FaceBERN.exe");
                        }
                        else if (Directory.Exists(installPath + Path.DirectorySeparatorChar + binDir + Path.DirectorySeparatorChar + "Debug")
                            && File.Exists(installPath + Path.DirectorySeparatorChar + binDir + Path.DirectorySeparatorChar + "Debug" + Path.DirectorySeparatorChar + @"FaceBERN.exe"))
                        {
                            File.Copy(installPath + Path.DirectorySeparatorChar + binDir + Path.DirectorySeparatorChar + "Debug" + Path.DirectorySeparatorChar + @"FaceBERN.exe",
                                        installPath + Path.DirectorySeparatorChar + @"FaceBERN.exe");
                        }
                        else
                        {
                            SetStatus("ERROR(2)!  FaceBERN.exe not found!");
                            return;
                        }
                    }
                    else
                    {
                        SetStatus("ERROR!  FaceBERN!.exe not found!");
                        return;
                    }
                }

                /* Copy all dependencies.  --Kris */
                string resourceDir = @"src\Windows\FaceBERN!\Resources";
                if (Directory.Exists(installPath + Path.DirectorySeparatorChar + resourceDir))
                {
                    string[] files = Directory.GetFiles(installPath + Path.DirectorySeparatorChar + resourceDir);

                    foreach (string s in files)
                    {
                        if (Path.GetExtension(s).ToLower().Equals("dll"))
                        {
                            File.Copy(s, installPath + Path.DirectorySeparatorChar + Path.GetFileName(s));
                        }
                    }
                }
                else
                {
                    SetStatus("ERROR!  Resources dir not found!");
                    return;
                }

                /* Delete the source directory if specified by the user.  --Kris */
                if (deleteSrc && Directory.Exists(installPath + Path.DirectorySeparatorChar + @"src"))
                {
                    SetStatus("Cleaning-up source files....", 85);

                    Directory.Delete(installPath + Path.DirectorySeparatorChar + @"src", true);
                }

                SetStatus("Updating system registry....", 90);

                RegistryKey softwareKey = Registry.CurrentUser.OpenSubKey("Software", true);
                RegistryKey appKey = softwareKey.CreateSubKey("FaceBERN!");

                appKey.SetValue("Installed", installPath, RegistryValueKind.String);
                appKey.SetValue("BranchName", branchName, RegistryValueKind.String);
                appKey.SetValue("GithubRemoteName", "origin", RegistryValueKind.String);

                appKey.Flush();
                softwareKey.Flush();

                appKey.Close();
                softwareKey.Close();

                SetStatus("Installation complete!", 100);

                System.Threading.Thread.Sleep(1000);

                /* Display the thank you form.  --Kris */
                using (ThankYouForm thankYouForm = new ThankYouForm(installerVersion))
                {
                    thankYouForm.ShowDialog();
                    SetStartAfter(thankYouForm.launchFacebernCheckbox.Checked);
                }

                Exit();
            }
        }

        private void SetStartAfter(bool startAfter)
        {
            if (Main.InvokeRequired)
            {
                Main.BeginInvoke(
                    new MethodInvoker(
                        delegate() { SetStartAfter(startAfter); }));
            }
            else
            {
                Main.SetStartAfter(startAfter);
            }
        }

        private void SetStatus(string text, int percent = -1)
        {
            if (Main.InvokeRequired)
            {
                Main.BeginInvoke(
                    new MethodInvoker(
                        delegate() { SetStatus(text, percent); }));
            }
            else
            {
                Main.SetStatus(text, percent);
            }

            Main.Refresh();
        }

        private void Exit()
        {
            if (Main.InvokeRequired)
            {
                Main.BeginInvoke(
                    new MethodInvoker(
                        delegate() { Exit(); }));
            }
            else
            {
                Main.Exit();
            }
        }
    }
}
