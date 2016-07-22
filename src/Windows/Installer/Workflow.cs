using IWshRuntimeLibrary;
using LibGit2Sharp;
using LibGit2Sharp.Core;
using LibGit2Sharp.Handlers;
using Microsoft.Win32;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections;
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
        private RestClient restClient;
        private List<Exception> exceptions;

        private List<string> exemptFiles;

        public Workflow(Form1 Main)
        {
            this.Main = Main;
            exceptions = new List<Exception>();
            exemptFiles = new List<string>();

            /* Initialize the Birdie API client. --Kris */
            restClient = new RestClient("http://birdie.freeddns.org");
        }

        public Workflow()
        {
            this.Main = null;
            exceptions = new List<Exception>();
            exemptFiles = new List<string>();

            /* Initialize the Birdie API client. --Kris */
            restClient = new RestClient("http://birdie.freeddns.org");
        }

        public Thread ExecuteCleanupThread()
        {
            try
            {
                Thread thread = new Thread(() => ExecuteCleanup());
                thread.Start();
                while (!thread.IsAlive) { }

                return thread;
            }
            catch (Exception e)
            {
                ReportException(e, "Unable to start cleanup thread.");

                return null;
            }
        }

        public void ExecuteCleanup()
        {
            SetStatus("Cleaning-up Wall Street....");

            try
            {
                System.IO.File.Delete(Path.Combine(Main.repoBaseDir, "BirdieUpdater.exe"));
            }
            catch (Exception e)
            {
                SetStatus("ERROR!  Unable to delete temp exe!");
                StoreException(e);
                ReportException(e, "Unable to delete temp exe in ExecuteCleanup().");
                return;
            }

            SetStatus("Done!");
        }

        public Thread ExecuteUninstallThread(string installPath)
        {
            try
            {
                Thread thread = new Thread(() => ExecuteUninstall(installPath));
                thread.Start();
                while (!thread.IsAlive) { }

                return thread;
            }
            catch (Exception e)
            {
                ReportException(e, "Unable to start uninstall thread.");

                return null;
            }
        }

        public void ExecuteUninstall(string installPath)
        {
            try
            {
                SetStatus("Performing uninstall....");

                DeleteApplicationDir(installPath);
                DeleteShortcuts();

                RegistryKey softwareKey = Registry.CurrentUser.OpenSubKey("Software", true);

                softwareKey.DeleteSubKeyTree("FaceBERN!", false);
                RegistryKey runKey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true);
                runKey.DeleteValue("Birdie", false);

                softwareKey.Close();

                SetStatus("Done!");

                Main.Close();
            }
            catch (Exception e)
            {
                ReportException(e, "Unable to perform uninstall.");

                MessageBox.Show("Uninstall error!");
            }
        }

        public static void DeleteReadOnlyDirectory(string directory)
        {
            try
            {
                foreach (var subdirectory in Directory.EnumerateDirectories(directory))
                {
                    DeleteReadOnlyDirectory(subdirectory);
                }
                foreach (var fileName in Directory.EnumerateFiles(directory))
                {
                    var fileInfo = new FileInfo(fileName);
                    fileInfo.Attributes = FileAttributes.Normal;
                    fileInfo.Delete();
                }
                Directory.Delete(directory);
            }
            catch (Exception e)
            {
                Workflow workflow = new Workflow();
                workflow.ReportException(e, "Unable to delete directory:  " + directory);
            }
        }

        private void DeleteApplicationDir(string installPath)
        {
            if (Directory.Exists(installPath))
            {
                try
                {
                    DeleteReadOnlyDirectory(installPath);
                }
                catch (Exception e)
                {
                    SetStatus("ERROR!  Unable to delete application directory!");
                    StoreException(e);
                    ReportException(e, "Unable to delete application directory:  " + installPath);
                    return;
                }

                System.Threading.Thread.Sleep(1000);
            }
        }

        public Thread ExecuteUpdateThread(bool startAfter, string[] origArgs)
        {
            try
            {
                Thread thread = new Thread(() => ExecuteUpdate(startAfter, origArgs));
                thread.Start();
                while (!thread.IsAlive) { }

                return thread;
            }
            catch (Exception e)
            {
                ReportException(e, "Unable to execute update thread.");
                return null;
            }
        }

        public void ExecuteUpdate(bool startAfter, string[] origArgs)
        {
            try
            {
                SetStatus("Updating Birdie....");

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
                    SetStatus("ERROR:  Update FAILED!");
                    StoreException(e);
                    ReportException(e, "Update failed.");
                    return;
                }

                SetStatus("Copying executables....", 80);
                CopyExecutables(Main.repoBaseDir);

                SetStatus("Copying dependencies....", 85);
                CopyDependencies(Main.repoBaseDir);

                if (!keepSrc)
                {
                    SetStatus("Cleaning-up the corrupt campagin finance system....", 90);

                    try
                    {
                        if (Directory.Exists(Path.Combine(Main.repoBaseDir, "src")))
                        {
                            Directory.Delete(Path.Combine(Main.repoBaseDir, "src"), true);
                        }
                    }
                    catch (Exception e)
                    {
                        ReportException(e, "Error cleaning-up the corrupt campaign finance system.");
                    }
                }

                SetStatus("Update Complete!", 100);

                System.Threading.Thread.Sleep(1000);

                string installerPath = Path.Combine(Main.repoBaseDir, "BirdieSetup.exe");
                try
                {
                    Process process = new Process();
                    process.StartInfo.FileName = installerPath;
                    process.StartInfo.Arguments = Main.githubRemoteName + " " + Main.branchName + " " + (startAfter ? " /startAfter " : "") 
                        + "/cleanup" + " origArgs=\"" + String.Join(@",", origArgs) + "\"";
                    process.Start();
                }
                catch (Exception ex)
                {
                    SetStatus("ERROR:  Installer re-launch FAILED!");
                    StoreException(ex);
                    ReportException(ex, "Installer re-launch failed.");
                    return;
                }

                Main.Close();
            }
            catch (Exception e)
            {
                ReportException(e, "Unhandled exception in ExecuteUpdate.");
                MessageBox.Show("Update error!");
            }
        }

        public Thread ExecuteInstallThread(string installerVersion, string repoURL)
        {
            try
            {
                Thread thread = new Thread(() => ExecuteInstall(installerVersion, repoURL));
                thread.SetApartmentState(ApartmentState.STA);  // MTA is not compatible with FolderBrowserDialog.  --Kris
                thread.Start();
                while (!thread.IsAlive) { }

                return thread;
            }
            catch (Exception e)
            {
                ReportException(e, "Unable to execute install thread.");
                return null;
            }
        }

        public void ExecuteInstall(string installerVersion, string repoURL)
        {
            try
            {
                string installPath = null;
                string branchName = null;
                bool deleteSrc = false;
                bool createStartMenuShortcut = false;
                bool createDesktopShortcut = false;
                try
                {
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
                }
                catch (Exception e)
                {
                    ReportException(e, "Install failed during user interaction workflow.");

                    MessageBox.Show("Install error during user interaction workflow!");
                }

                if (installPath != null && branchName != null)
                {
                    RegistryKey softwareKey = null;
                    RegistryKey appKey = null;
                    try
                    {
                        /* Check for existing installation and overwrite.  --Kris */
                        softwareKey = Registry.CurrentUser.OpenSubKey("Software", true);
                        appKey = softwareKey.CreateSubKey("FaceBERN!");

                        if (appKey != null)
                        {
                            /* We're NOT checking for/deleting the installedPath because the user might want that preserved.  We'll just clear the registry and make sure the destination directory's clean.  --Kris */
                            appKey.Close();

                            softwareKey.DeleteSubKeyTree("FaceBERN!", false);
                        }
                    }
                    catch (Exception e)
                    {
                        ReportException(e, "Install error trying to open/prepare registry.");

                        MessageBox.Show("Install error during registry setup!");
                    }

                    /* Perform the installation.  --Kris */
                    SetStatus("Creating installation directory....", 0);

                    try
                    {
                        DeleteApplicationDir(installPath);

                        Directory.CreateDirectory(installPath);
                    }
                    catch (Exception ex)
                    {
                        SetStatus("Directory creation FAILED : " + ex.Message);
                        StoreException(ex);
                        ReportException(ex, "Install failed:  Directory creation failed.");
                        return;
                    }

                    SetStatus("Downloading and unpacking program files....", 20);

                    try
                    {
                        Repository.Clone(repoURL, installPath);

                        if (!(branchName.ToLower().Equals("master")))
                        {
                            SetStatus("Checking-out " + branchName + " branch....", 60);

                            using (Repository repo = new Repository(installPath))
                            {
                                LibGit2Sharp.Commands.Checkout(repo, repo.Branches[@"origin/" + branchName]);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        ReportException(e, "Install failed:  Error during Git workflow.");

                        MessageBox.Show("Error downloading/unpacking program files!");
                    }

                    SetStatus("Finalizing filesystem....", 80);

                    try
                    {
                        /* Copy the executables.  --Kris */
                        CopyExecutables(installPath);

                        /* Copy all dependencies.  --Kris */
                        CopyDependencies(installPath);

                        /* Set the directory and file permissions.  --Kris */
                        //SetPermissions(installPath);  // The built-in ACL libraries are terrible and wholly unreliable.  Resorting to Plan B.  --Kris
                        SetACLPermissions(installPath);
                    }
                    catch (Exception e)
                    {
                        ReportException(e, "Install failed:  Error finalizing filesystem.");

                        MessageBox.Show("Install failed during filesystem finalization!");
                    }

                    /* Create shortcuts.  --Kris */
                    if (createStartMenuShortcut || createDesktopShortcut)
                    {
                        SetStatus("Creating shortcut(s)....", 85);

                        try
                        {
                            CreateShortcuts(installPath, createStartMenuShortcut, createDesktopShortcut);
                        }
                        catch (Exception e)
                        {
                            ReportException(e, "Install failed:  Error creating shortcuts.");

                            MessageBox.Show("Unable to create shortcuts!");
                        }
                    }

                    /* Delete the source directory if specified by the user.  --Kris */
                    SetStatus("Cleaning-up the establishment....", 90);

                    try
                    {
                        if (deleteSrc && Directory.Exists(Path.Combine(installPath, "program")))
                        {
                            Directory.Delete(Path.Combine(installPath, "program"), true);
                        }

                        if (deleteSrc && Directory.Exists(Path.Combine(installPath, "src")))
                        {
                            Directory.Delete(Path.Combine(installPath, "src"), true);
                        }
                    }
                    catch (Exception e)
                    {
                        ReportException(e, "Unable to clean-up establishment in ExecuteInstall.");
                    }

                    SetStatus("Updating system registry....", 90);

                    try
                    {
                        appKey = softwareKey.CreateSubKey("FaceBERN!");  // Sticking with the old name because it's more likely to be unique.  --Kris

                        appKey.SetValue("Installed", installPath, RegistryValueKind.String);
                        appKey.SetValue("BranchName", branchName, RegistryValueKind.String);
                        appKey.SetValue("GithubRemoteName", "origin", RegistryValueKind.String);
                        appKey.SetValue("PostInstallNeeded", "1", RegistryValueKind.String);

                        appKey.Flush();
                        softwareKey.Flush();

                        appKey.Close();
                        softwareKey.Close();
                    }
                    catch (Exception e)
                    {
                        ReportException(e, "Install failed:  Error updating system registry.");
                    }

                    SetStatus("Installation complete!", 100);

                    System.Threading.Thread.Sleep(1000);

                    try
                    {
                        /* Display the thank you form.  --Kris */
                        using (ThankYouForm thankYouForm = new ThankYouForm(installerVersion))
                        {
                            thankYouForm.ShowDialog();
                            SetStartAfter(thankYouForm.launchFacebernCheckbox.Checked);
                        }
                    }
                    catch (Exception e)
                    {
                        ReportException(e, "Error displaying thank you form in ExecuteInstall.");
                    }

                    Exit();
                }
            }
            catch (Exception e)
            {
                ReportException(e, "Unhandled exception in ExecuteInstall.");

                MessageBox.Show("Install error!");
            }
        }

        private void CopyDependencies(string installPath)
        {
            string resourceDir = "(unknown)";
            try
            {
                resourceDir = Path.Combine("src", "Windows", @"FaceBERN!", "Resources");
                if (Directory.Exists(Path.Combine(installPath, resourceDir)))
                {
                    string[] files = Directory.GetFiles(Path.Combine(installPath, resourceDir));

                    foreach (string s in files)
                    {
                        if (System.IO.File.Exists(Path.Combine(installPath, Path.GetFileName(s))))
                        {
                            DeleteFile(Path.Combine(installPath, Path.GetFileName(s)));
                        }
                    }

                    foreach (string s in files)
                    {
                        CopyFile(s, Path.Combine(installPath, Path.GetFileName(s)));
                    }
                }
                else
                {
                    SetStatus("ERROR!  Resources dir not found!");
                    throw new Exception("Resources dir '" + resourceDir + "' not found!");
                }
            }
            catch (Exception e)
            {
                ReportException(e, "Error copying dependencies to:  " + installPath);
            }
        }

        private void CopyFile(string src, string dest, int retry = 3)
        {
            if (exemptFiles.Contains(src) 
                || exemptFiles.Contains(dest))
            {
                return;
            }
            
            try
            {
                if (System.IO.File.Exists(src))
                {
                    System.Threading.Thread.Sleep(25);

                    System.IO.File.Copy(src, dest, true);
                }
            }
            catch (Exception e)
            {
                if (retry > 0)
                {
                    System.Threading.Thread.Sleep(3000);

                    retry--;
                    try
                    {
                        System.IO.File.Copy(src, dest, true);
                    }
                    catch (Exception ex)
                    {
                        CopyFile(src, dest, retry);
                        ReportException(ex, "Unable to copy file from '" + src + "' to '" + dest + "'.  Retry = " + retry.ToString());
                    }
                }
                else
                {
                    DialogResult dr = MessageBox.Show("ERROR copying dependency '" + Path.GetFileName(src) + "' : " + e.ToString(), "ERROR!", MessageBoxButtons.OK);
                    if (dr == DialogResult.Yes)
                    {
                        return;
                    }
                }
            }
        }

        private void DeleteFile(string src, int retry = 3)
        {
            try
            {
                if (System.IO.File.Exists(src))
                {
                    System.IO.File.SetAttributes(Path.GetDirectoryName(src), FileAttributes.Normal);
                    System.IO.File.SetAttributes(src, FileAttributes.Normal);

                    System.Threading.Thread.Sleep(25);
                    
                    System.IO.File.Delete(src);
                }
            }
            catch (Exception e)
            {
                ReportException(e, "Error deleting file '" + src + "'.  Retry = " + retry.ToString());

                if (retry > 0)
                {
                    retry--;
                    DeleteFile(src, retry);
                }
                else
                {
                    exemptFiles.Add(src);  // No need to update the bundled Git libraries (FSO locked).  --Kris
                }
            }
        }

        private void CopyExecutables(string installPath)
        {
            List<string> executables = new List<string>();

            try
            {
                executables.Add(@"Birdie");
                executables.Add(@"BirdieSetup");
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
                                throw new Exception("Error(2) locating executable:  " + exe);
                            }
                        }
                        else
                        {
                            SetStatus("ERROR!  " + exe + " not found!");
                            throw new Exception("Error locating executable:  " + exe);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                ReportException(e, "Error copying executables to:  " + installPath);
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

                string shortcutPath = Path.Combine(appSMP, "Birdie.lnk");

                CreateShortcut(shortcutPath, installPath);
            }

            if (desktop)
            {
                string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);  // Use CommonDesktopDirectory for all users.  --Kris

                if (!Directory.Exists(desktopPath))
                {
                    return;  // If your desktop directory doesn't exist, you've got bigger problems to deal with.  --Kris
                }

                string shortcutPath = Path.Combine(desktopPath, "Birdie.lnk");

                CreateShortcut(shortcutPath, installPath);
            }
        }

        private void CreateShortcut(string shortcutPath, string installPath)
        {
            WshShell shell = new WshShell();
            IWshShortcut shortcut = (IWshShortcut) shell.CreateShortcut(shortcutPath);

            shortcut.Description = "Birdie - Because the political revolution begins at home";
            shortcut.TargetPath = Path.Combine(installPath, @"Birdie.exe");
            shortcut.WorkingDirectory = installPath;
            shortcut.Save();
        }

        private void DeleteShortcuts()
        {
            try
            {
                if (System.IO.File.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "Birdie.lnk")))
                {
                    System.IO.File.Delete(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "Birdie.lnk"));
                }

                string userSMP = Environment.GetFolderPath(Environment.SpecialFolder.StartMenu);
                string appSMP = Path.Combine(userSMP, "Programs");
                if (System.IO.File.Exists(Path.Combine(appSMP, "Birdie.lnk")))
                {
                    System.IO.File.Delete(Path.Combine(appSMP, "Birdie.lnk"));
                }
            }
            catch (Exception e)
            {
                SetStatus("Unable to delete shortcuts!");

                System.Threading.Thread.Sleep(1000);
            }
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
                StoreException(ex);
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
            try
            {
                if (Main == null)
                {
                    return;
                }

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
            catch (Exception e)
            {
                ReportException(e, "Exception thrown in SetStartAfter.");
            }
        }

        private void SetStatus(string text, int percent = -1)
        {
            try
            {
                if (Main == null)
                {
                    return;
                }

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
            catch (Exception e)
            {
                ReportException(e, "Exception thrown in SetStatus.");
            }
        }

        private void StoreException(Exception e)
        {
            try
            {
                if (Main == null)
                {
                    return;
                }

                if (Main.InvokeRequired)
                {
                    Main.BeginInvoke(
                        new MethodInvoker(
                            delegate() { StoreException(e); }));
                }
                else
                {
                    Main.StoreException(e);
                }

                Main.Refresh();
            }
            catch (Exception ex)
            {
                ReportException(e, "Exception thrown in StoreException.");
            }
        }

        /* Query the Birdie API and return the raw result.  --Kris */
        internal IRestResponse BirdieQuery(string path, string method = "GET", Dictionary<string, string> queryParams = null, string body = "", bool reportException = true)
        {
            /* We don't ever want this to terminate program execution on failure.  --Kris */
            try
            {
                Method meth;
                switch (method.ToUpper())
                {
                    default:
                        throw new Exception("API ERROR:  Unsupported method '" + method + "'!");
                    case "GET":
                        meth = Method.GET;
                        break;
                    case "POST":
                        meth = Method.POST;
                        break;
                    case "PUT":
                        meth = Method.PUT;
                        break;
                    case "DELETE":
                        meth = Method.DELETE;
                        break;
                }

                RestRequest req = new RestRequest(path, meth);

                req.JsonSerializer.ContentType = "application/json; charset=utf-8";

                if (queryParams != null && queryParams.Count > 0)
                {
                    foreach (KeyValuePair<string, string> pair in queryParams)
                    {
                        req.AddParameter(pair.Key, pair.Value);
                    }
                }

                if (body != null && body.Length > 0 && !(method.ToUpper().Equals("GET")))
                {
                    //req.AddBody(JsonConvert.DeserializeObject<Dictionary<string, string>>(body));
                    req.AddParameter("text/json", body, ParameterType.RequestBody);
                }

                return restClient.Execute(req);
            }
            catch (Exception e)
            {
                if (reportException == true)
                {
                    try
                    {
                        ReportException(e, "Birdie API query error.");

                        SetStatus("Warning:  Birdie API query error : " + e.Message);
                    }
                    catch (Exception) { }
                }

                return null;
            }
        }

        internal void ReportException(Exception ex, string logMsg = null)
        {
            try
            {
                string message = null;
                string stackTrace = null;
                string source = null;
                string type = null;
                DateTime discovered;
                Dictionary<dynamic, dynamic> data = new Dictionary<dynamic, dynamic>();

                message = ex.Message;
                stackTrace = ex.StackTrace;
                source = ex.Source;
                type = ex.GetType().ToString();
                discovered = DateTime.Now;
                if (ex.Data != null && ex.Data.Count > 0)
                {
                    data = new Dictionary<dynamic, dynamic>();
                    foreach (DictionaryEntry pair in ex.Data)
                    {
                        data.Add(pair.Key, pair.Value);
                    }
                }

                Dictionary<string, dynamic> body = new Dictionary<string, dynamic>();
                body.Add("exType", type);
                body.Add("exMessage", message);
                body.Add("exStackTrace", stackTrace);
                body.Add("exSource", source);
                body.Add("exToString", ex.ToString());
                if (data != null)
                {
                    try
                    {
                        body.Add("exData", JsonConvert.SerializeObject(data));
                    }
                    catch (Exception) { }
                }
                body.Add("appName", @"FaceBERN! Installer");
                body.Add("clientId", null);

                IRestResponse res = BirdieQuery(@"/exceptions", "POST", null, JsonConvert.SerializeObject(body), false);
            }
            catch (Exception) { }
        }

        private void Exit()
        {
            try
            {
                if (Main == null)
                {
                    Application.Exit();
                    return;
                }

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
            catch (Exception e)
            {
                ReportException(e, "Exception thrown in Exit.");

                Application.Exit();
            }
        }
    }
}
