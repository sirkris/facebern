using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FaceBERN_
{
    public class Workflow
    {
        private string logName = "Workflow";
        protected Log WorkflowLog;
        private Form1 Main;
        private int browser = 0;

        public Workflow(Form1 Main)
        {
            this.Main = Main;
        }

        public Thread ExecuteThread()
        {
            Thread thread = new Thread(() => Execute(Main.browserModeComboBox.SelectedIndex));  // Selected index corresponds to global browser constants; don't change the order without changing them!  --Kris

            Main.LogW("Attempting to start Workflow thread....", false);

            thread.Start();
            while (thread.IsAlive == false) { }

            Main.buttonStart_ToStop();

            Main.LogW("Workflow thread started successfully.", false);

            return thread;
        }

        public void Execute(int browser)
        {
            InitLog();

            this.browser = browser;

            Log("Thread execution initialized.");

            SetExecState(Globals.STATE_WAITING);

            Main.Refresh();

            WebDriver webDriver = new WebDriver();

            webDriver.FixtureSetup(browser);
            webDriver.TestSetUp(browser, "http://www.facebook.com");

            if (webDriver.GetElementById(browser, "loginbutton") != null)
            {
                Credentials credentials = new Credentials();

                SecureString u = null;
                SecureString p = null;

                LoginPrompt loginPrompt = new LoginPrompt("Facebook");
                Main.Invoke((MethodInvoker)delegate()
                {
                    DialogResult res = loginPrompt.ShowDialog();
                    if (res == DialogResult.OK)
                    {
                        u = credentials.ToSecureString(loginPrompt.u);
                        p = credentials.ToSecureString(loginPrompt.p);
                    }
                });

                if (u != null && p != null && u.Length > 0 && p.Length > 0)
                {
                    webDriver.TypeInId(browser, "email", credentials.ToString(u));

                    webDriver.TypeInId(browser, "pass", credentials.ToString(p));

                    dynamic element = webDriver.GetElementById(browser, "u_0_y");
                    webDriver.ClickElement(browser, element);
                }
                else
                {
                    // TODO - Abort if no credentials given.  --Kris
                }
            }

            /* Loop until terminated by the user.  --Kris */
            while (true)
            {
                // TODO - The actual work.  --Kris
            }

            Ready();
        }

        private void Ready()
        {
            if (Main.InvokeRequired)
            {
                Main.BeginInvoke(
                    new MethodInvoker(
                        delegate() { Ready(); }));
            }
            else
            {
                Log("Execution terminated successfully.");

                WorkflowLog.Save();

                Main.buttonStart_ToStart();
                Main.Ready(logName);

                Main.Refresh();
            }
        }

        private void InitLog()
        {
            WorkflowLog = new Log();
            WorkflowLog.Init(logName);
        }

        private void Log(string text, bool show = true, bool appendW = true, bool newline = true, bool timestamp = true)
        {
            if (Main.InvokeRequired)
            {
                Main.BeginInvoke(
                    new MethodInvoker(
                        delegate() { Log(text, show, appendW, newline, timestamp); }));
            }
            else
            {
                Main.LogW(text, show, appendW, newline, timestamp, logName, WorkflowLog);

                Main.Refresh();
            }
        }

        private void SetExecState(int state)
        {
            Main.SetExecState(state, logName, WorkflowLog);
        }
    }
}
