using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FaceBERN_
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            DateTime? lastException = null;

            /* Don't want this to crash if it can be avoided.  Fault tolerance is essential for this project, given its complexity and intended use by non-tech end-users.  --Kris */
            try
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                if (args.Length > 0)
                {
                    bool updated = false;
                    bool logging = true;
                    bool autoStart = false;
                    for (int i = 0; i < args.Length; i++)
                    {
                        if (args[i].IndexOf(@"=") != -1)
                        {
                            string[] s = args[i].Split('=');
                            switch (s[0].Trim().ToLower())
                            {
                                case "lastException":
                                    try
                                    {
                                        lastException = DateTime.Parse(s[1].Trim('"'));
                                    }
                                    catch (Exception e)
                                    {
                                        lastException = null;
                                    }
                                    break;
                            }
                        }
                        else
                        {
                            switch (args[i].ToLower().Trim())
                            {
                                case @"/autostart":
                                    autoStart = true;
                                    break;
                                case @"/nolog":
                                    logging = false;
                                    break;
                                case @"/updated":
                                    updated = true;
                                    break;
                            }
                        }
                    }

                    Application.Run(new Form1(updated, logging, autoStart, args));
                }
                else
                {
                    Application.Run(new Form1());
                }
            }
            catch (Exception e)
            {
                /* An unhandled exception has managed to bubble up to the surface.  Restart the program, then log/report the error.  Don't restart if last exception was less than a minute ago.  --Kris */
                if (lastException == null
                    || lastException.Value.AddMinutes(1) <= DateTime.Now)
                {
                    RegistryKey softwareKey = Registry.CurrentUser.OpenSubKey("Software", true);
                    RegistryKey appKey = softwareKey.CreateSubKey("FaceBERN!");

                    try
                    {
                        appKey.SetValue("preRecoveryException", JsonConvert.SerializeObject(new ExceptionReport(null, e, "Unhandled exception forced application restart!", false)));
                    }
                    catch (Exception ex)
                    {
                        // Do nothing.  If we can't report it, fine; no need to risk another exception by making this overly complex.  --Kris
                    }

                    appKey.Close();
                    softwareKey.Close();

                    Process.Start(Application.ExecutablePath + " " + String.Join(" ", args));

                    System.Threading.Thread.Sleep(3000);

                    Application.Exit();
                }
            }
        }
    }
}
