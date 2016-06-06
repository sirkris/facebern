using IWshRuntimeLibrary;
using LibGit2Sharp;
using LibGit2Sharp.Core;
using LibGit2Sharp.Handlers;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.IO;
using System.Security.AccessControl;
using System.Security.Principal;
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

        public Thread ExecuteCleanupThread()
        {
            Thread thread = new Thread(() => ExecuteCleanup());
            thread.Start();
            while (!thread.IsAlive) { }

            return thread;
        }

        public void ExecuteCleanup()
        {
            SetStatus("Cleaning-up Wall Street....");

            try
            {
                System.IO.File.Delete(Path.Combine(Main.repoBaseDir, "Updater.exe"));
            }
            catch (Exception e)
            {
                SetStatus("ERROR!  Unable to delete temp exe!");
                return;
            }

            SetStatus("Done!");
        }

        public Thread ExecuteUninstallThread(string installPath)
        {
            Thread thread = new Thread(() => ExecuteUninstall(installPath));
            thread.Start();
            while (!thread.IsAlive) { }

            return thread;
        }

        public void ExecuteUninstall(string installPath)
        {
            SetStatus("Performing uninstall....");

            if (Directory.Exists(installPath))
            {
                try
                {
                    System.IO.Directory.Delete(installPath, true);
                }
                catch (Exception e)
                {
                    SetStatus("ERROR!  Unable to delete application directory!");
                    return;
                }
            }

            RegistryKey softwareKey = Registry.CurrentUser.OpenSubKey("Software", true);

            softwareKey.DeleteSubKeyTree("FaceBERN!", false);

            softwareKey.Close();

            SetStatus("Done!");
        }

        public Thread ExecuteUpdateThread()
        {
            Thread thread = new Thread(() => ExecuteUpdate());
            thread.Start();
            while (!thread.IsAlive) { }

            return thread;
        }

        public void ExecuteUpdate()
        {
            SetStatus("Updating FaceBERN!....");

            bool keepSrc = Directory.Exists(Path.Combine(Main.repoBaseDir, "src"));
            try
            {
                RegistryKey softwareKey = Registry.CurrentUser.OpenSubKey("Software", true);
                RegistryKey appKey = softwareKey.CreateSubKey("FaceBERN!");

                using (Repository repo = new Repository(Main.repoBaseDir))
                {
                    repo.Reset(ResetMode.Hard, "HEAD");  // This is necessary to clean things up for Git; configs/logs won't be affected.  --Kris
                    Commands.Checkout(repo, repo.Branches[(string)appKey.GetValue("BranchName", "master")]);  // You should not use the updater on manual installs!  --Kris
                    Commands.Pull(repo, new LibGit2Sharp.Signature("FaceBERN! Updater", "KrisCraig@php.net", new DateTimeOffset()), new PullOptions());  // Just do a git pull.  That's the update.  --Kris
                }

                appKey.Close();
                softwareKey.Close();

                SetACLPermissions(Main.repoBaseDir);
            }
            catch (Exception e)
            {
                //SetStatus("ERROR:  Update FAILED!");
                SetStatus(@"err=" + e.Message);  // Uncomment for DEBUG.  --Kris
                return;
            }

            SetStatus("Copying executables....", 80);
            CopyExecutables(Main.repoBaseDir);

            SetStatus("Copying dependencies....", 85);
            CopyDependencies(Main.repoBaseDir);

            if (!keepSrc)
            {
                SetStatus("Cleaning-up the corrupt campagin finance system....", 90);
                if (Directory.Exists(Path.Combine(Main.repoBaseDir, "src")))
                {
                    Directory.Delete(Path.Combine(Main.repoBaseDir, "src"), true);
                }
            }

            SetStatus("Update Complete!", 100);

            System.Threading.Thread.Sleep(1000);

            string installerPath = Path.Combine(Main.repoBaseDir, "Installer.exe");
            try
            {
                Process process = new Process();
                process.StartInfo.FileName = installerPath;
                process.StartInfo.Arguments = Main.githubRemoteName + " " + Main.branchName + " /startafter /cleanup";
                process.Start();
            }
            catch (Exception ex)
            {
                SetStatus("ERROR:  Installer re-launch FAILED!");
                return;
            }

            Main.Close();
        }

        public Thread ExecuteInstallThread(string installerVersion, string repoURL)
        {
            Thread thread = new Thread(() => ExecuteInstall(installerVersion, repoURL));
            thread.SetApartmentState(ApartmentState.STA);  // MTA is not compatible with FolderBrowserDialog.  --Kris
            thread.Start();
            while (!thread.IsAlive) { }

            return thread;
        }

        public void ExecuteInstall(string installerVersion, string repoURL)
        {
            string installPath = null;
            string branchName = null;
            bool deleteSrc = false;
            bool createStartMenuShortcut = false;
            bool createDesktopShortcut = false;
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
                            createStartMenuShortcut = workflowForm2.createStartMenuFolderCheckbox.Checked;
                            createDesktopShortcut = workflowForm2.createDesktopShortcutCheckbox.Checked;
                        }
                    }
                }
            }

            if (installPath != null && branchName != null)
            {
                /* Check for existing installation and overwrite.  --Kris */
                RegistryKey softwareKey = Registry.CurrentUser.OpenSubKey("Software", true);
                RegistryKey appKey = softwareKey.CreateSubKey("FaceBERN!");

                if (appKey != null)
                {
                    /* We're NOT checking for/deleting the installedPath because the user might want that preserved.  We'll just clear the registry and make sure the destination directory's clean.  --Kris */
                    appKey.Close();

                    Thread uninstallThread = ExecuteUninstallThread(installPath);
                    while (uninstallThread.IsAlive) { }
                }

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
                        LibGit2Sharp.Commands.Checkout(repo, repo.Branches[@"origin/" + branchName]);
                    }
                }

                SetStatus("Finalizing filesystem....", 80);

                /* Copy the executables.  --Kris */
                CopyExecutables(installPath);

                /* Copy all dependencies.  --Kris */
                CopyDependencies(installPath);

                /* Set the directory and file permissions.  --Kris */
                //SetPermissions(installPath);  // The built-in ACL libraries are terrible and wholly unreliable.  Resorting to Plan B.  --Kris
                SetACLPermissions(installPath);

                /* Create shortcuts.  --Kris */
                if (createStartMenuShortcut || createDesktopShortcut)
                {
                    SetStatus("Creating shortcut(s)....", 85);

                    CreateShortcuts(installPath, createStartMenuShortcut, createDesktopShortcut);
                }

                /* Delete the source directory if specified by the user.  --Kris */
                SetStatus("Cleaning-up the establishment....", 90);

                if (deleteSrc && Directory.Exists(Path.Combine(installPath, "program")))
                {
                    Directory.Delete(Path.Combine(installPath, "program"), true);
                }

                if (deleteSrc && Directory.Exists(Path.Combine(installPath, "src")))
                {
                    Directory.Delete(Path.Combine(installPath, "src"), true);
                }

                SetStatus("Updating system registry....", 90);

                appKey = softwareKey.CreateSubKey("FaceBERN!");

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

        private void CopyDependencies(string installPath, int retry = 3)
        {
            string resourceDir = Path.Combine("src", "Windows", @"FaceBERN!", "Resources");
            if (Directory.Exists(Path.Combine(installPath, resourceDir)))
            {
                string[] files = Directory.GetFiles(Path.Combine(installPath, resourceDir));

                foreach (string s in files)
                {
                    if (Path.GetExtension(s).ToLower().Equals(".dll"))
                    {
                        try
                        {
                            if (System.IO.File.Exists(s))
                            {
                                System.Threading.Thread.Sleep(100);

                                System.IO.File.Copy(s, Path.Combine(installPath, Path.GetFileName(s)), true);
                            }
                        }
                        catch (Exception e)
                        {
                            if (retry > 0)
                            {
                                System.Threading.Thread.Sleep(3000);

                                retry--;
                                CopyDependencies(installPath, retry);
                            }
                            else
                            {
                                DialogResult dr = MessageBox.Show("ERROR copying dependency '" + Path.GetFileName(s) + "' : " + e.ToString(), "ERROR!", MessageBoxButtons.OK);
                                if (dr == DialogResult.Yes)
                                {
                                    continue;
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                SetStatus("ERROR!  Resources dir not found!");
                return;
            }
        }

        private void CopyExecutables(string installPath)
        {
            List<string> executables = new List<string>();
            executables.Add(@"FaceBERN!");
            executables.Add(@"Installer");
            foreach (string program in executables)
            {
                string exe = program + ".exe";
                if (Directory.Exists(Path.Combine(installPath, "program"))
                    && System.IO.File.Exists(Path.Combine(installPath, "program", exe)))
                {
                    System.IO.File.Copy(Path.Combine(installPath, "program", exe), Path.Combine(installPath, exe), true);
                }
                else
                {
                    string binDir = Path.Combine("src", "Windows", program, "bin");
                    if (Directory.Exists(Path.Combine(installPath, binDir)))
                    {
                        if (Directory.Exists(Path.Combine(installPath, binDir, "Release"))
                            && System.IO.File.Exists(Path.Combine(installPath, binDir, "Release", exe)))
                        {
                            System.IO.File.Copy(Path.Combine(installPath, binDir, "Release", exe),
                                        Path.Combine(installPath, exe), true);
                        }
                        else if (Directory.Exists(Path.Combine(installPath, binDir, "Debug"))
                            && System.IO.File.Exists(Path.Combine(installPath, binDir, "Debug", exe)))
                        {
                            System.IO.File.Copy(Path.Combine(installPath, binDir, "Debug", exe),
                                        Path.Combine(installPath, exe), true);
                        }
                        else
                        {
                            SetStatus("ERROR(2)!  " + exe + " not found!");
                            return;
                        }
                    }
                    else
                    {
                        SetStatus("ERROR!  " + exe + " not found!");
                        return;
                    }
                }
            }
        }

        private void CreateShortcuts(string installPath, bool startMenu = true, bool desktop = true)
        {
            if (startMenu)
            {
                //string commonSMP = Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu);  // For all users.  May make it an option in the UI later.  --Kris
                string userSMP = Environment.GetFolderPath(Environment.SpecialFolder.StartMenu);
                string appSMP = Path.Combine(userSMP, "Programs");

                if (!Directory.Exists(appSMP))
                {
                    Directory.CreateDirectory(appSMP);
                }

                string shortcutPath = Path.Combine(appSMP, "FaceBERN!.lnk");

                CreateShortcut(shortcutPath, installPath);
            }

            if (desktop)
            {
                string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);  // Use CommonDesktopDirectory for all users.  --Kris

                if (!Directory.Exists(desktopPath))
                {
                    return;  // If your desktop directory doesn't exist, you've got bigger problems to deal with.  --Kris
                }

                string shortcutPath = Path.Combine(desktopPath, "FaceBERN!.lnk");

                CreateShortcut(shortcutPath, installPath);
            }
        }

        private void CreateShortcut(string shortcutPath, string installPath)
        {
            WshShell shell = new WshShell();
            IWshShortcut shortcut = (IWshShortcut) shell.CreateShortcut(shortcutPath);

            shortcut.Description = "FaceBERN! - Because the political revolution begins at home";
            shortcut.TargetPath = Path.Combine(installPath, @"FaceBERN!.exe");
            shortcut.WorkingDirectory = installPath;
            shortcut.Save();
        }

        /* Recursively set permissions so that the application can run freely.  Path should be a directory.  --Kris */
        // DEPRECATED in favor of SetACLPermissions().
        private void SetPermissions(string path)
        {
            if (!Directory.Exists(path))
            {
                return;
            }
            
            /* Set the permissions.  --Kris */
            SetFSOToEveryone(path);

            string[] files = Directory.GetFiles(path);
            string[] subDirs = Directory.GetDirectories(path);

            foreach (string file in files)
            {
                SetFSOToEveryone(file);
            }

            foreach (string dir in subDirs)
            {
                SetPermissions(dir);
            }
        }

        private void SetFSOToEveryone(string path)
        {
            SecurityIdentifier everyone = new SecurityIdentifier(WellKnownSidType.WorldSid, null);
            FileSystemAccessRule perms = new FileSystemAccessRule(everyone, FileSystemRights.FullControl, AccessControlType.Allow);

            if (Directory.Exists(path))
            {
                DirectoryInfo di = new DirectoryInfo(path);
                DirectorySecurity diS = di.GetAccessControl();

                //diS.SetOwner(everyone);  // Throws an InvalidOperationException.  Could probably do it with SetACL but the distribution license for the COM library is incompatible with OSS.  --Kris
                diS.SetAccessRule(perms);

                di.SetAccessControl(diS);
            }
            else if (System.IO.File.Exists(path))
            {
                FileInfo fi = new FileInfo(path);
                FileSecurity fiS = fi.GetAccessControl();

                //fiS.SetOwner(everyone);
                fiS.SetAccessRule(perms);

                fi.SetAccessControl(fiS);
            }
        }

        private bool SetACL(string args)
        {
            try
            {
                Process process = new Process();
                process.StartInfo.FileName = Path.Combine(Environment.CurrentDirectory, "SetACL.exe");
                process.StartInfo.Arguments = args;
                process.StartInfo.WindowStyle = ProcessWindowStyle.Minimized;
                process.Start();
                process.WaitForExit(60000);  // If it takes more than a minute, something must be out of whack.  Should only take a few seconds under normal conditions.  --Kris
            }
            catch (Exception ex)
            {
                SetStatus("ERROR:  SetACL launch FAILED!");
                return false;
            }

            return true;
        }

        private bool SetACLPermissions(string path)
        {
            if (!Directory.Exists(path))
            {
                return false;
            }

            bool res1, res2 = false;

            /* Set the owner to Everyone.  --Kris */
            res1 = SetACL("-on \"" + path + "\" -ot file -actn setowner -ownr n:S-1-1-0 -rec cont_obj");

            /* Give full control to Everyone.  --Kris */
            if (res1)
            {
                res2 = SetACL("-on \"" + path + "\" -ot file -actn ace -ace \"n:S-1-1-0;p:full\" -rec cont_obj");
            }

            return (res1 && res2);
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
