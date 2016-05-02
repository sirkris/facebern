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
            if (args.Length > 0)
            {
                for (int i = 0; i < args.Length; i++)
                {
                    switch (args[i].ToLower().Trim())
                    {
                        case @"/startafter":
                            startAfter = true;
                            break;
                        case @"/cleanup":
                            cleanup = true;
                            break;
                    }
                }

                Application.Run(new Form1(args[0], args[1], startAfter, cleanup));
            }
            else
            {
                RegistryKey softwareKey = Registry.CurrentUser.OpenSubKey("Software", true);
                RegistryKey appKey = softwareKey.CreateSubKey("FaceBERN!");

                string githubRemoteName = (string) appKey.GetValue("GithubRemoteName", null);
                string branchName = (string) appKey.GetValue("BranchName", null);

                Application.Run(new Form1((githubRemoteName != null ? githubRemoteName : "origin"), (branchName != null ? branchName : "master"), startAfter));
            }
        }
    }
}
