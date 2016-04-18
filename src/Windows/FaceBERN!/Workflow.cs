﻿using Microsoft.Win32;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.IO;
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
        public Log WorkflowLog;
        private Form1 Main;
        private int browser = 0;
        private bool ftbFriended = false;

        public Workflow(Form1 Main, Log MainLog = null)
        {
            this.Main = Main;
            if (MainLog == null)
            {
                InitLog();
            }
            else
            {
                WorkflowLog = MainLog;
                WorkflowLog.Init("Workflow");
            }
        }

        public Thread ExecuteThread()
        {
            SetExecState(Globals.STATE_VALIDATING);

            int browser = -1;
            if (Main.InvokeRequired)
            {
                Main.Invoke(new MethodInvoker(delegate() { browser = Main.browserModeComboBox.SelectedIndex; }));
            }
            else
            {
                browser = Main.browserModeComboBox.SelectedIndex;
            }

            Thread thread = new Thread(() => Execute(browser, WorkflowLog));  // Selected index corresponds to global browser constants; don't change the order without changing them!  --Kris

            Main.LogW("Attempting to start Workflow thread....", false);

            thread.Start();
            while (thread.IsAlive == false) { }

            Main.buttonStart_ToStop();

            Main.LogW("Workflow thread started successfully.", false);

            return thread;
        }

        public void Execute(int browser, Log WorkflowLog)
        {
            //InitLog();
            this.WorkflowLog = WorkflowLog;

            this.browser = browser;

            Log("Thread execution initialized.");

            SetExecState(Globals.STATE_WAITING);

            Main.Invoke(new MethodInvoker(delegate() { Main.Refresh(); }));

            // TEST - This will end up going into the loop below.  --Kris
            GOTV();


            /* Loop until terminated by the user.  --Kris */
            while (Globals.executionState > 0)
            {
                // TODO - The actual work.  --Kris
            }

            Ready();

            Main.Invoke(new MethodInvoker(delegate() { Main.MainLog = WorkflowLog; }));
        }

        // TODO - Move these Facebook methods to a new dedicated class.  Will hold off for now because I'm lazy.  --Kris
        private void GOTV()
        {
            int lastState = Globals.executionState;
            SetExecState(Globals.STATE_EXECUTING);

            WebDriver webDriver = FacebookLogin();
            if (webDriver.error > 0)
            {
                Log("Error logging into Facebook.  GOTV aborted.");
                return;
            }

            // TEST (TODO - Move to the state loop below and make sure count > 0 for each given state).
            List<Person> friends = GetFacebookFriendsOfFriends(ref webDriver, "WA");

            // TEST
            CreateGOTVEvent(ref webDriver, ref friends, "NY");
            
            /* Cycle through each state and execute GOTV actions, where appropriate.  --Kris */
            foreach (KeyValuePair<string, States> state in Globals.StateConfigs)
            {
                // TODO - Logic for when to perform GOTV.  --Kris

                /* Attempt to gain access to feelthebern.events event for this state, if applicable.  --Kris */
                int retries = 5;
                if (Globals.Config["UseFTBEvents"].Equals("1") && friends.Count > 0)
                {
                    if (Globals.StateConfigs.ContainsKey("FTBEventId") && Globals.StateConfigs["FTBEventId"] != null)
                    {
                        while (!CheckFTBEventAccess(state.Key, ref webDriver) && retries > 0)
                        {
                            retries--;
                            Log("Access denied for state event page.");

                            if (retries > 0)
                            {
                                RequestFTBInvitation();

                                Wait(Globals.__FTB_REQUEST_ACCESS_WAIT_INTERVAL__, "for retry");

                                /* Check for and accept friend request from feelthebern.events.  --Kris */
                                if (!ftbFriended)
                                {
                                    if ((ftbFriended = AcceptFacebookFTBRequest(ref webDriver)))
                                    {
                                        Wait(Globals.__FTB_REQUEST_ACCESS_WAIT_INTERVAL__, "for event invitations");
                                    }
                                    else
                                    {
                                        Log("Friend request for feelthebern.events not yet found!");
                                    }
                                }
                                else
                                {
                                    Log("Friend request accepted but still no event invitations received.");
                                }
                            }
                            else
                            {
                                Log("Retries exhausted.  Aborting feelthebern.events action for " + state.Key + ".");
                                SetExecState(Globals.STATE_ERROR);
                            }
                        }

                        /* If there are no errors, proceed with the invitations.  --Kris */
                        if (Globals.executionState > 0 && CheckFTBEventAccess(state.Key, ref webDriver))
                        {
                            IWebDriver w = webDriver.GetDriver(browser);
                            webDriver.ClickElement(browser, w.FindElement(By.CssSelector("[data-testid=\"event_invite_button\"]")));

                            System.Threading.Thread.Sleep(3000);

                            // TODO - Enter the invites and submit.  --Kris
                        }
                    }
                    else
                    {
                        Log("No feelthebern.events event exists for " + state.Key + ".");
                    }
                }

                /* If there are any people left to invite, create a new private event for every 200 people and do the remaining invites there.  --Kris */
                if (Globals.Config["UseCustomEvents"].Equals("1") && friends.Count > 0)
                {
                    int c = friends.Count;
                    while (c > 0)
                    {
                        CreateGOTVEvent(ref webDriver, ref friends, state.Key);

                        if (c == friends.Count)
                        {
                            break;  // To prevent infinite recursion in the event of an unexpected error.  --Kris
                        }
                    }
                }
                // TODO - GOTV.  --Kris
            }

            // TODO - Either find an existing GOTV event for that state or create a new one.  --Kris
            // TODO - Do the magic Bernie friends search.  --Kris
            // TODO - Quit signing my name on every fucking comment.  --Kris
            // TODO - Change my mind and decide to keep doing it just to piss people off.  --Kris
            // TODO - Send the invites.  --Kris
            // TODO - Persist invited users in the registry as an encrypted string; used for stats and avoiding duplicate invites.  --Kris

            SetExecState(lastState);
        }

        private void Wait(int minutes, string reason = "")
        {
            Log("Waiting " + minutes.ToString() + "minutes" + (reason != "" ? " " + reason : "") + "....");

            int lastState = Globals.executionState;
            SetExecState(Globals.STATE_WAITING);

            System.Threading.Thread.Sleep(minutes * 60 * 1000);

            SetExecState(lastState);
        }

        /* Open a new browser session and login to Facebook.  --Kris */
        private WebDriver FacebookLogin(int retry = 5)
        {
            int lastState = Globals.executionState;
            SetExecState(Globals.STATE_EXECUTING);

            /* Initialize the Selenium WebDriver.  --Kris */
            WebDriver webDriver = new WebDriver();

            /* Initialize the browser and navigate to Facebook.  --Kris */
            webDriver.FixtureSetup(browser);
            webDriver.TestSetUp(browser, "http://www.facebook.com");

            /* If needed, prompt the user for username/password or use the encrypted copy in the system registry.  --Kris */
            SetExecState(Globals.STATE_VALIDATING);
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

                SetExecState(Globals.STATE_EXECUTING);

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
                            Log("Unexpected post-login page data for Facebook!");
                            webDriver.error = WebDriver.ERROR_UNEXPECTED;
                        }
                    }
                    else
                    {
                        Log("Login to Facebook FAILED!");
                        // Note - Sometimes this will happen with good credentials due to an odd, intermittent bug where, upon clicking login, the page instead reloads in some Asian language.  --Kris
                        webDriver.error = WebDriver.ERROR_BADCREDENTIALS;
                    }
                }
                else
                {
                    Log("Unable to login to Facebook due to lack of credentials.");
                    webDriver.error = WebDriver.ERROR_NOCREDENTIALS;
                }
            }

            if (webDriver.error > 0)
            {
                SetExecState(Globals.STATE_ERROR);

                Log("Closing browser session....");
                webDriver.FixtureTearDown(browser);

                retry--;
                if (retry > 0)
                {
                    SetExecState(Globals.STATE_VALIDATING);

                    Log("Retrying Facebook login....");
                    return FacebookLogin(retry);
                }
                else
                {
                    Log("Maximum retries reached without success.  Facebook login aborted.");
                }
            }
            else
            {
                /* While we're here, let's grab the Facebook profile URL for this user and store it.  Login may be different so check every time.  --Kris */
                string profileURL = null;
                
                IWebDriver w = webDriver.GetDriver(browser);
                IWebElement ele = w.FindElement(By.CssSelector("[data-testid=\"blue_bar_profile_link\"]"));
                if (ele != null)
                {
                    profileURL = ele.GetAttribute("href");
                    if (profileURL != null && profileURL.Length > 0)
                    {
                        try
                        {
                            RegistryKey softwareKey = Registry.CurrentUser.OpenSubKey("Software", true);
                            RegistryKey appKey = softwareKey.CreateSubKey("FaceBERN!");
                            RegistryKey facebookKey = appKey.CreateSubKey("Facebook");

                            /* I don't think we need to bother encrypting this since it's just a public profile URL; if someone has access to your registry, you've got bigger problems, anyway.  --Kris */
                            facebookKey.SetValue("profileURL", profileURL, RegistryValueKind.String);
                            facebookKey.SetValue("lastLogin", DateTime.Now.Ticks.ToString(), RegistryValueKind.String);

                            facebookKey.Flush();
                            appKey.Flush();
                            softwareKey.Flush();

                            facebookKey.Close();
                            appKey.Close();
                            softwareKey.Close();

                            Log("Facebook profile URL retrieved and stored successfully!");
                        }
                        catch (IOException e)
                        {
                            Log("Warning:  Error storing Facebook profile URL : " + e.Message);
                        }
                    }
                    else
                    {
                        Log("Warning:  NULL or empty value found for Facebook profile URL!");
                    }
                }
                else
                {
                    Log("Warning:  Unable to extract Facebook profile URL from page source!");
                }

                SetExecState(lastState);
            }

            return webDriver;
        }

        private bool CreateGOTVEvent(ref WebDriver webDriver, ref List<Person> friends, string stateAbbr)
        {
            Log("Creating private GOTV event for " + stateAbbr + "....");

            webDriver.GoToUrl(browser, "https://www.facebook.com/events/upcoming");

            IWebDriver w = webDriver.GetDriver(browser);
            webDriver.ClickElement(browser, w.FindElement(By.CssSelector("[data-testid=\"event-create-button\"]")));

            System.Threading.Thread.Sleep(5000);  // This one's a bit slower to load and I don't trust the implicit wait in this case.  --Kris

            States state = Globals.StateConfigs[stateAbbr];

            string indefiniteArticle;
            switch (state.primaryAccess.Substring(0, 1).ToUpper())
            {
                default:
                    indefiniteArticle = "a";
                    break;
                case "A":
                case "E":
                case "I":
                case "O":
                case "U":
                    indefiniteArticle = "an";
                    break;
            }

            /* Default values.  --Kris */
            // TODO - Make these configurable for each state.  --Kris
            // TODO - Allow state subreddit mods to specify these values in a JSON post retrieved automatically by FaceBERN!.  --Kris
            string eventName = "Vote for Bernie in " + state.name + " on " + state.primaryDate.ToString("MMMM d, yyyy");
            string location = state.name;
            string description = state.name + " will be holding " + indefiniteArticle + " " + state.primaryAccess + " " + state.primaryType + " on " + state.primaryDate.ToString("MMMM d, yyyy")
                                + ".  For more information on how/where to vote and when polls open/close, visit http://vote.berniesanders.com/" + stateAbbr.ToUpper();

            /* Enter the values into the form and create the event.  --Kris */
            webDriver.TypeText(browser, webDriver.GetInputElementByPlaceholder(browser, "Include a place or address"), location);
            webDriver.TypeText(browser, webDriver.GetInputElementByPlaceholder(browser, "Add a short, clear name"), eventName);
            webDriver.TypeText(browser, webDriver.GetInputElementByPlaceholder(browser, "mm/dd/yyyy"), OpenQA.Selenium.Keys.Control + "a");
            webDriver.TypeText(browser, webDriver.GetInputElementByPlaceholder(browser, "mm/dd/yyyy"), state.primaryDate.ToString("MM/dd/yyyy"));
            webDriver.TypeText(browser, webDriver.GetElementByTagNameAndAttribute(browser, "div", "title", "Tell people more about the event"), description);
            webDriver.ClickOnXPath(browser, ".//button[.='Create']");

            /* Check to see if we're on the new event page.  --Kris */
            System.Threading.Thread.Sleep(5000);

            IWebElement inviteButton = w.FindElement(By.CssSelector("[data-testid=\"event_invite_button\"]"));
            if (inviteButton == null)
            {
                Log("Event creation failed!  Aborted.");
                return false;
            }
            
            Log("GOTV event for " + stateAbbr + " created at : " + w.Url);

            // TODO - Send invites.  --Kris

            return true;
        }

        private bool AcceptFacebookFTBRequest(ref WebDriver webDriver)
        {
            webDriver.GoToUrl(browser, "http://www.facebook.com");

            IWebDriver w = webDriver.GetDriver( browser );
            webDriver.ClickElement(browser, w.FindElement(By.CssSelector("[data-tooltip-content=\"Friend Requests\"]")));
            
            IWebElement button = webDriver.GetElementByXPath(browser, ".//button[contains(@onclick, '100001066887477')]");

            if (button == null)
            {
                Log("Unable to locate feelthebern.events friend request.  It may simply have not been sent yet.");
                return false;
            }

            Log("Facebook friend request for feelthebern.events found.");

            if (webDriver.ClickElement(browser, button))
            {
                Log("Facebook friend request for feelthebern.events accepted.");
                System.Threading.Thread.Sleep(3000);

                return true;
            }
            else
            {
                Log("Error clicking on friend request accept button!");

                return false;
            }
        }

        private List<Person> GetFacebookFriendsOfFriends(ref WebDriver webDriver, string stateAbbr = null, bool bernieSupportersOnly = true)
        {
            int lastState = Globals.executionState;
            SetExecState(Globals.STATE_EXECUTING);

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
            IWebDriver iWebDriver = webDriver.GetDriver(browser);
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

            Log("Retrieved " + res.Count.ToString() + " results for friends of friends" + logMsg + ".");

            SetExecState(lastState);

            return res;
        }

        /* Load the Facebook event page from feelthebern.events and return whether or not the user has access to the event.  --Kris */
        private bool CheckFTBEventAccess(string stateAbbr, ref WebDriver webDriver)
        {
            int lastState = Globals.executionState;
            SetExecState(Globals.STATE_VALIDATING);

            if (stateAbbr == null || stateAbbr.Length == 0 || !Globals.StateConfigs.ContainsKey(stateAbbr))
            {
                Log("Warning:  Unrecognized stateAbbr '" + stateAbbr + "' sent to CheckFTBEventAccess()!");
                return false;
            }

            if (Globals.StateConfigs[stateAbbr].FTBEventId == null || Globals.StateConfigs[stateAbbr].FTBEventId.Length == 0)
            {
                Log("Warning:  No Facebook event ID found for " + stateAbbr + "!");
                return false;
            }

            webDriver.GoToUrl(browser, "https://www.facebook.com/events/" + Globals.StateConfigs[stateAbbr].FTBEventId);

            System.Threading.Thread.Sleep(3000);  // Just in case the driver doesn't wait long enough on its own.  --Kris

            SetExecState(lastState);

            return (webDriver.GetPageSource(browser).IndexOf("Sorry, this page isn't available") == -1);
        }

        /* Load feelthebern.events in a separate browser session and attempt to get invited to the state events.  Note that it can take over an hour for the request to be processed.  --Kris */
        private void RequestFTBInvitation()
        {
            int lastState = Globals.executionState;
            SetExecState(Globals.STATE_VALIDATING);

            /* Retrieve the Facebook profile URL for the logged-in user from the registry.  It's automatically stored on login.  --Kris */
            RegistryKey softwareKey = Registry.CurrentUser.OpenSubKey("Software", true);
            RegistryKey appKey = softwareKey.CreateSubKey("FaceBERN!");
            RegistryKey facebookKey = appKey.CreateSubKey("Facebook");

            string profileURL = facebookKey.GetValue("profileURL", null, RegistryValueOptions.None).ToString();

            /* Initialize the driver and navigate to feelthebern.events.  --Kris */
            if (profileURL != null)
            {
                Log("Requesting invitation from feelthebern.events....");

                SetExecState(Globals.STATE_EXECUTING);

                WebDriver webDriver2 = new WebDriver();

                webDriver2.FixtureSetup(browser);
                webDriver2.TestSetUp(browser, "http://www.feelthebern.events");

                webDriver2.TypeInXPath(browser, @"/html/body/div[2]/input", profileURL);
                webDriver2.ClickOnXPath(browser, @"/html/body/div[2]/button");

                System.Threading.Thread.Sleep(5000);

                Log("Request sent.");

                webDriver2.FixtureTearDown(browser);
            }
            else
            {
                SetExecState(Globals.STATE_ERROR);

                Log("Unable to request feelthebern.events invitation due to unknown profile URL!");
            }

            SetExecState(lastState);
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
