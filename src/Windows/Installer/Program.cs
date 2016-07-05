using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
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
            try
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                bool startAfter = false;
                bool cleanup = false;
                bool assumeUpdate = false;
                bool uninstall = false;
                string githubRemoteName = null;
                string branchName = null;
                string[] origArgs = new string[99];
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
                                case "origargs":
                                    s[1] = s[1].Trim('"');
                                    origArgs = s[1].Split(',');
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
                                case @"/assumeupdate":
                                    assumeUpdate = true;
                                    break;
                                case @"/uninstall":
                                    uninstall = true;
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

                Application.Run(new Form1(args, githubRemoteName, branchName, startAfter, cleanup, assumeUpdate, uninstall, retry));
            }
            catch (Exception e)
            {
                string query = @"http://birdie.freeddns.org/exceptions/installerExceptionReport?appName=FaceBERN!"
                                + @"&exType=" + e.GetType().ToString()
                                + @"&exMessage=" + e.Message
                                + @"&exStackTrace=" + e.StackTrace
                                + @"&exSource=" + e.Source
                                + @"&exToString=" + e.ToString()
                                + @"&logMsg=Unhandled Installer Exception";

                query = HttpUtility.UrlEncode(query);

                DialogResult dr = MessageBox.Show("An error occurred installing Birdie.  May I report it to the dev team so they can fix it?", "An error has occurred", MessageBoxButtons.YesNo);
                if (dr == DialogResult.Yes)
                {
                    System.Diagnostics.Process.Start(query);

                    System.Threading.Thread.Sleep(3000);

                    Application.Exit();
                }
            }
        }
    }
}
