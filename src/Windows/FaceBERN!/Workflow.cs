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
            int browser = -1;
            if (Main.InvokeRequired)
            {
                Main.Invoke(new MethodInvoker(delegate() { browser = Main.browserModeComboBox.SelectedIndex; }));
            }
            else
            {
                browser = Main.browserModeComboBox.SelectedIndex;
            }

            Thread thread = new Thread(() => Execute(browser));  // Selected index corresponds to global browser constants; don't change the order without changing them!  --Kris

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

            Main.Invoke(new MethodInvoker(delegate() { Main.Refresh(); }));

            GOTV();


            /* Loop until terminated by the user.  --Kris */
            while (true)
            {
                // TODO - The actual work.  --Kris
            }

            Ready();
        }

        private void GOTV()
        {
            /* Initialize the Selenium WebDriver.  --Kris */
            WebDriver webDriver = new WebDriver();

            /* Initialize the browser and navigate to Facebook.  --Kris */
            webDriver.FixtureSetup(browser);
            webDriver.TestSetUp(browser, "http://www.facebook.com");

            /* If needed, prompt the user for username/password or use the encrypted copy in the system registry.  --Kris */
            if (webDriver.GetElementById(browser, "loginbutton") != null)
            {
                Credentials credentials = new Credentials();

                SecureString u = credentials.GetUsername();  // Load encrypted username if stored in registry.  --Kris
                SecureString p = credentials.GetPassword();  // Load encrypted password if stored in registry.  --Kris
                bool remember = false;

                if (u == null || p == null)
                {
                    Log("No stored credentials found.  Prompting for user input....");
                    LoginPrompt loginPrompt = new LoginPrompt("Facebook");  // Display the login prompt window.  --Kris
                    Main.Invoke((MethodInvoker)delegate()
                    {
                        DialogResult res = loginPrompt.ShowDialog();
                        if (res == DialogResult.OK)
                        {
                            u = credentials.ToSecureString(loginPrompt.u);
                            p = credentials.ToSecureString(loginPrompt.p);
                            remember = loginPrompt.remember;
                        }
                    });
                }
                else
                {
                    Log("Facebook credentials loaded successfully.");
                }

                if (u != null && p != null && u.Length > 0 && p.Length > 0)
                {
                    /* Encrypt and store the credentials in the system registry if the option is checked.  --Kris */
                    if (remember)
                    {
                        if (credentials.SetFacebook(u, p))
                        {
                            Log("Facebook credentials saved successfully.");
                        }
                        else
                        {
                            Log("Error saving Facebook credentials!");
                        }
                    }

                    /* Enter the username and password into the login form on Facebook.  --Kris */
                    webDriver.TypeInId(browser, "email", credentials.ToString(u));
                    webDriver.TypeInId(browser, "pass", credentials.ToString(p));

                    /* Get this sensitive data out of active memory.  --Kris */
                    credentials.Destroy();
                    credentials = null;

                    /* Click the login button on Facebook.  --Kris */
                    dynamic element = webDriver.GetElementById(browser, "u_0_y");
                    webDriver.ClickElement(browser, element);

                    Thread.Sleep(3);  // Give the state a chance to unready itself.  Better safe than sorry.  --Kris
                    webDriver.WaitForPageLoad(browser);

                    /* Check for successful login.  --Kris */
                    if (webDriver.GetElementById(browser, "loginbutton") == null)
                    {
                        if (webDriver.GetElementById(browser, "u_0_1") != null)  // Checks to see if the "Home" link is present.  --Kris
                        {
                            Log("Login to Facebook successful.");
                        }
                        else
                        {
                            Log("Unexpected post-login page data for Facebook!  GOTV aborted.");
                            return;
                        }
                    }
                    else
                    {
                        Log("Login to Facebook FAILED!  GOTV aborted.");
                        return;
                    }

                    // TEST
                    getFacebookFriendsOfFriends(ref webDriver, "WA");

                    /* Cycle through each state and execute GOTV actions, where appropriate.  --Kris */
                    foreach (KeyValuePair<string, States> state in Globals.StateConfigs)
                    {
                        // TODO - Logic for when to perform GOTV.  --Kris
                        // TODO - GOTV.  --Kris
                    }

                    // TODO - Either find an existing GOTV event for that state or create a new one.  --Kris
                    // TODO - Do the magic Bernie friends search.  --Kris
                    // TODO - Quit signing my name on every fucking comment.  --Kris
                    // TODO - Change my mind and decide to keep doing it just to piss people off.  --Kris
                    // TODO - Send the invites.  --Kris
                    // TODO - Persist invited users in the registry as an encrypted string; used for stats and avoiding duplicate invites.  --Kris
                }
                else
                {
                    Log("Unable to login to Facebook due to lack of credentials.  GOTV aborted.");
                    return;
                }
            }
        }

        private List<Person> getFacebookFriendsOfFriends(ref WebDriver webDriver, string stateAbbr = null, bool bernieSupportersOnly = true)
        {
            List<Person> res = new List<Person>();

            string logBaseMsg = "Searching Facebook for friends of friends";
            string logMsg = "";

            /* Build the search URL string.  Graph search syntax is deprecated and too unreliable so we'll just do it this way.  --Kris */
            string URL = "https://www.facebook.com/search";

            if (bernieSupportersOnly)
            {
                foreach (string facebookID in Globals.bernieFacebookIDs)
                {
                    URL += "/" + facebookID + "/likers";
                }

                URL += "/union";
                logMsg += " " + (logMsg == "" ? "who" : "and") + " like Bernie Sanders";
            }

            if (stateAbbr != null)
            {
                URL += "/" + Globals.StateConfigs[stateAbbr].facebookId + "/residents";
                logMsg += " " + (logMsg == "" ? "who" : "and") + " live in " + Globals.StateConfigs[stateAbbr].name;
            }

            URL += "/present/me/friends/friends/intersect";

            Log(logBaseMsg + logMsg + "....");

            /* Navigate to the search page.  --Kris */
            webDriver.GoToUrl(browser, URL);

            /* Keep scrolling to the bottom until all results have been loaded.  --Kris */
            OpenQA.Selenium.IWebDriver iWebDriver = webDriver.GetDriver(browser);
            webDriver.ScrollToBottom(ref iWebDriver);

            /* Scrape the results from the page source.  --Kris */
            string[] resRaw = new string[32767];
            resRaw = webDriver.GetPageSource(browser).Split(new string[] { "<div class=\"_gll\">" }, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 1; i < resRaw.Length; i++)
            {
                if (resRaw[i] == null || resRaw[i].Length < 40 || resRaw[i].Substring(0, 34) != "<a href=\"https://www.facebook.com/")
                {
                    continue;
                }

                /* Thought about using regex, then decided to stab myself in the forehead with an icepick, instead.  I'm happy with my choice.  --Kris */
                string start = "<a href=\"https://www.facebook.com/";
                string end = "\">";
                // NOTE - We're not going to be using these IDs in any graph searches so we don't need to worry about retrieving the actual numeric ID; the username is sufficient.  --Kris
                string userId = resRaw[i].Substring(resRaw[i].IndexOf(start) + start.Length, resRaw[i].IndexOf(end) - (resRaw[i].IndexOf(start) + start.Length));
                if (userId.IndexOf("?") != -1)
                {
                    userId = userId.Substring(0, userId.IndexOf("?"));  // Strips any URL parameters that Facebook might have tagged-on.  --Kris
                }

                start = "<div class=\"_5d-5\">";
                end = "</div>";  // It's the first one so that makes it easy.  --Kris

                string name = resRaw[i].Substring(resRaw[i].IndexOf(start) + start.Length, resRaw[i].IndexOf(end) - (resRaw[i].IndexOf(start) + start.Length));

                Person per = new Person();

                if (bernieSupportersOnly == true)
                {
                    per.setBernieSupporter(true);
                }
                else
                {
                    // TODO - Figure out if this user likes Bernie.  Don't need this for v1.0 since the priority for now is GOTV.  Will revisit when there's more time.  --Kris
                }

                if (stateAbbr != null)
                {
                    per.setStateAbbr(stateAbbr);
                }
                else
                {
                    // TODO - Will be needed if we do any inter-state searches.  Low priority for now since we can just search state-by-state.  --Kris
                }

                per.setFacebookID(userId);
                per.setName(name);

                res.Add(per);
            }

            return res;
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
