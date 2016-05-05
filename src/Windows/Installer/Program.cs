using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Installer
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            bool startAfter = false;
            bool cleanup = false;
            string githubRemoteName = null;
            string branchName = null;
            int retry = 0;
            if (args.Length > 0)
            {
                for (int i = 0; i < args.Length; i++)
                {
                    if (args[i].IndexOf(@"=") != -1)
                    {
                        string[] s = args[i].Split('=');
                        switch (s[0].Trim().ToLower())
                        {
                            case "branchname":
                                branchName = s[1];
                                break;
                            case "githubremotename":
                                githubRemoteName = s[1];
                                break;
                            case "retry":
                                retry = Int32.Parse(s[1]);
                                break;
                        }
                    }
                    else
                    {
                        switch (args[i].Trim().ToLower())
                        {
                            case @"/startafter":
                                startAfter = true;
                                break;
                            case @"/cleanup":
                                cleanup = true;
                                break;
                        }
                    }
                }
            }

            if (githubRemoteName == null || branchName == null)
            {
                RegistryKey softwareKey = Registry.CurrentUser.OpenSubKey("Software", true);
                RegistryKey appKey = softwareKey.CreateSubKey("FaceBERN!");

                githubRemoteName = (githubRemoteName != null ? githubRemoteName : (string) appKey.GetValue("GithubRemoteName", "origin"));
                branchName = (branchName != null ? branchName : (string) appKey.GetValue("BranchName", "master"));

                appKey.Close();
                softwareKey.Close();
            }

            Application.Run(new Form1(githubRemoteName, branchName, startAfter, cleanup, retry));
        }
    }
}
