﻿using AutoIt;
using Microsoft.Win32;
using Newtonsoft.Json;
using OpenQA.Selenium;
using RestSharp;
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

        public long invitesSent = 0;  // Tracked for current session only.  --Kris

        public List<Person> invited;

        private WebDriver webDriver = null;

        private RestClient restClient;

        private int remoteInvitesSent = 0;

        private List<Person> remoteUpdateQueue = null;

        private Random rand;

        public Workflow(Form1 Main, Log MainLog = null)
        {
            rand = new Random();

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

            invited = GetInvitedPeople();  // Get list of people you already invited with this program from the system registry.  --Kris
        }

        /* This thread is designed to run continuously while the program is running.  --Kris */
        public Thread ExecuteInterComThread()
        {
            Thread thread = new Thread(() => ExecuteInterCom());

            Main.LogW("Attempting to start InterCom thread....", false);

            thread.IsBackground = true;

            thread.Start();
            while (thread.IsAlive == false) { }

            Main.LogW("InterCom thread started successfully.", false);

            return thread;
        }

        /* Responsible for any background communications, such as interfacing with the Birdie API to periodically update the invited totals.  --Kris */
        public void ExecuteInterCom()
        {
            /* Initialize the Birdie API client. --Kris */
            restClient = new RestClient("http://birdie.freeddns.org");

            /* Load any invitations persisted in the registry from a previous run.  --Kris */
            LoadLatestInvitesQueue();

            /* The main InterCom loop.  --Kris */
            int i = 0;
            while (true)
            {
                /* Get the Application ID.  If it's not set, generate one.  --Kris */
                GetAppID();
                
                /* Process the remote update queue and send it to Birdie.  --Kris */
                PostLatestInvites((i == 0));

                /* Update our local invitations count for both ours and remote.  --Kris */
                UpdateRemoteInvitesCount();
                invited = GetInvitedPeople();
                UpdateInvitationsCount(invited.Count, remoteInvitesSent);

                System.Threading.Thread.Sleep(Globals.__INTERCOM_WAIT_INTERVAL__ * 60 * 1000);

                i++;
            }
        }

        private List<FacebookUser> PeopleToFacebookUsers(List<Person> people)
        {
            List<FacebookUser> res = new List<FacebookUser>();
            foreach (Person person in people)
            {
                FacebookUser fbUser = new FacebookUser();

                fbUser.fbUserId = person.getFacebookID();
                fbUser.fbId = person.getFacebookInternalID();
                fbUser.name = person.getName();
                fbUser.stateAbbr = person.getStateAbbr();
                fbUser.lastInvited = person.getLastGOTVInviteAsDateTime();
                fbUser.lastInvitedBy = GetAppID();
                fbUser.eventId = person.getFacebookEventID();

                res.Add(fbUser);
            }

            return res;
        }

        private List<Person> FacebookUsersToPeople(List<FacebookUser> facebookUsers)
        {
            List<Person> res = new List<Person>();
            foreach (FacebookUser fbUser in facebookUsers)
            {
                Person person = new Person();

                person.setFacebookID(fbUser.fbUserId);
                person.setFacebookInternalID(fbUser.fbId);
                person.setName(fbUser.name);
                person.setStateAbbr(fbUser.stateAbbr);
                person.setLastGOTVInvite((fbUser.lastInvited != null ? fbUser.lastInvited.Value.Ticks.ToString() : null));
                person.setFacebookEventID(fbUser.eventId);

                res.Add(person);
            }

            return res;
        }

        /* Tell Birdie about the latest people we've invited.  This is queued and called periodically by the InterCom thread in order to prevent query spam.  --Kris */
        private void PostLatestInvites(bool firstRun = false)
        {
            if ( remoteUpdateQueue != null && remoteUpdateQueue.Count > 0 )
            {
                /* Do it only when in a non-executing state to avoid simultaneous access conflicts.  --Kris */
                int i = 200;  // 10 minutes
                while (Globals.executionState == Globals.STATE_EXECUTING
                    || Globals.executionState == Globals.STATE_STOPPING)
                {
                    System.Threading.Thread.Sleep(3000);

                    i--;
                    if (i == 0)
                    {
                        Log("Timeout waiting to update Birdie with new invites.  Will try again later.");

                        return;
                    }
                }

                if (!firstRun)
                {
                    List<Person> queueNew = new List<Person>();
                    foreach (Person person in remoteUpdateQueue)
                    {
                        if (person.getFacebookID() != null && person.getName() != null)
                        {
                            queueNew.Add(person);
                        }
                    }
                    remoteUpdateQueue = queueNew;

                    if (remoteUpdateQueue.Count == 0)
                    {
                        ClearLatestInvitesQueue();
                        return;
                    }
                }

                Log("Updating Birdie with " + remoteUpdateQueue.Count.ToString() + " new invitations we've sent....");
                
                IRestResponse res = BirdieQuery(@"/facebook/invited", "POST", null, JsonConvert.SerializeObject(PeopleToFacebookUsers(remoteUpdateQueue)));
                if (res == null)
                {
                    Log("Warning:  Error querying Birdie API with new invites.");
                    return;
                }
                else
                {
                    if (res.StatusCode == System.Net.HttpStatusCode.Created)
                    {
                        Log("Birdie updated successfully.");

                        ClearLatestInvitesQueue();
                    }
                    else
                    {
                        Log("Warning:  Birdie API query failed.  Unable to send latest invites queue.  Will try again later.");
                    }
                }
            }
        }

        private string GenerateAppID()
        {
            string res = @"facebern_";
            string salt = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz1234567890";
            for (int i = 1; i <= 20; i++)
            {
                res += salt.Substring(rand.Next(0, (salt.Length - 1)), 1);
            }

            RegistryKey softwareKey = Registry.CurrentUser.OpenSubKey("Software", true);
            RegistryKey appKey = softwareKey.CreateSubKey("FaceBERN!");

            appKey.SetValue("appId", res, RegistryValueKind.String);

            appKey.Close();
            softwareKey.Close();

            return res;
        }

        public string GetAppID()
        {
            if (Globals.appId == null)
            {
                RegistryKey softwareKey = Registry.CurrentUser.OpenSubKey("Software", true);
                RegistryKey appKey = softwareKey.CreateSubKey("FaceBERN!");

                Globals.appId = (string) appKey.GetValue("appId", null, RegistryValueOptions.None);
                
                if (Globals.appId == null)
                {
                    Globals.appId = GenerateAppID();
                }

                appKey.Close();
                softwareKey.Close();
            }

            return Globals.appId;
        }

        private void LoadLatestInvitesQueue()
        {
            RegistryKey softwareKey = Registry.CurrentUser.OpenSubKey("Software", true);
            RegistryKey appKey = softwareKey.CreateSubKey("FaceBERN!");
            RegistryKey GOTVKey = appKey.CreateSubKey("GOTV");

            remoteUpdateQueue = JsonConvert.DeserializeObject<List<Person>>((string) GOTVKey.GetValue("invitedQueue", JsonConvert.SerializeObject(new List<Person>()), RegistryValueOptions.None));

            GOTVKey.Close();
            appKey.Close();
            softwareKey.Close();
        }

        private void AppendLatestInvitesQueue(List<Person> invites, bool clear = false)
        {
            if (clear)
            {
                remoteUpdateQueue = new List<Person>();
            }
            else
            {
                remoteUpdateQueue.AddRange(invites);
            }

            RegistryKey softwareKey = Registry.CurrentUser.OpenSubKey("Software", true);
            RegistryKey appKey = softwareKey.CreateSubKey("FaceBERN!");
            RegistryKey GOTVKey = appKey.CreateSubKey("GOTV");

            GOTVKey.SetValue("invitedQueue", JsonConvert.SerializeObject(remoteUpdateQueue), RegistryValueKind.String);

            GOTVKey.Close();
            appKey.Close();
            softwareKey.Close();
        }

        private void AppendLatestInvitesQueue(Person invite)
        {
            AppendLatestInvitesQueue(new List<Person> { invite });
        }

        private void ClearLatestInvitesQueue()
        {
            AppendLatestInvitesQueue(null, true);
        }

        /* Get the number of Facebook users invited by everyone.  --Kris */
        private void UpdateRemoteInvitesCount()
        {
            Dictionary<string, string> queryParams = new Dictionary<string, string>();
            queryParams.Add("return", "count");

            IRestResponse res = BirdieQuery("/facebook/invited", "GET", queryParams);
            int r;
            if (res.StatusCode == System.Net.HttpStatusCode.OK)
            {
                try
                {
                    r = Int32.Parse(res.Content);
                }
                catch (Exception e)
                {
                    Log("Warning:  Unexpected response from Birdie API.  Unable to update remote invites count.");
                    return;
                }
            }
            else
            {
                Log("Warning:  Birdie API query failed.  Unable to update remote invites count.");
                return;
            }

            remoteInvitesSent = r;

            UpdateInvitationsCount(0, r);
        }

        /* Query the Birdie API and return the raw result.  --Kris */
        private IRestResponse BirdieQuery(string path, string method = "GET", Dictionary<string, string> queryParams = null, string body = "")
        {
            /* We don't ever want this to terminate program execution on failure.  --Kris */
            try
            {
                Method meth;
                switch (method.ToUpper())
                {
                    default:
                        Log("API ERROR:  Unsupported method '" + method + "'!");
                        return null;
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
                Log("Warning:  Birdie API query error : " + e.Message);
                return null;
            }
        }

        public Thread ExecuteShutdownThread(Thread workflowThread)
        {
            SetExecState(Globals.STATE_STOPPING);

            Thread thread = new Thread(() => ExecuteShutdown(workflowThread));

            Main.LogW("Attempting to start Shutdown thread....", false);

            thread.Start();
            while (thread.IsAlive == false) { }

            Main.LogW("Shutdown thread started successfully.", false);

            return thread;
        }

        public void ExecuteShutdown(Thread workflowThread)
        {
            Log("Aborting Workflow thread....");

            workflowThread.Abort();

            int i = 600;
            while (workflowThread.IsAlive && i > 0)
            {
                System.Threading.Thread.Sleep(1000);

                i--;
                if (i > 0 && i % 30 == 0)
                {
                    Log("Still waiting for Workflow thread to abort....");
                }
            }

            if (workflowThread.IsAlive)
            {
                Log("ERROR!  Unable to shutdown Workflow thread!");

                SetExecState(Globals.STATE_BROKEN);
            }
            else
            {
                Log("Workflow thread aborted successfully!");

                Main.Invoke(new MethodInvoker(delegate() { Main.buttonStart_ToStart(); }));
                Main.Invoke(new MethodInvoker(delegate() { Main.Ready(); }));
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

            thread.IsBackground = true;

            Main.LogW("Attempting to start Workflow thread....", false);

            thread.Start();
            while (thread.IsAlive == false) { }

            Main.buttonStart_ToStop();

            Main.LogW("Workflow thread started successfully.", false);

            return thread;
        }

        public void Execute(int browser, Log WorkflowLog)
        {
            try
            {
                //InitLog();
                this.WorkflowLog = WorkflowLog;

                this.browser = browser;

                Log("Thread execution initialized.");

                SetExecState(Globals.STATE_EXECUTING);

                Main.Invoke(new MethodInvoker(delegate() { Main.Refresh(); }));

                // TEST - This will end up going into the loop below.  --Kris
                //GOTV();


                /* Loop until terminated by the user.  --Kris */
                while (Globals.executionState > 0)
                {
                    /* Get-out-the-vote!  --Kris */
                    GOTV();

                    /* Wait between loops.  May lower it later if we start doing more time-sensitive crap like notifications/etc.  --Kris */
                    Log("Waiting " + Globals.__WORKFLOW_WAIT_INTERVAL__.ToString() + " minutes....");
                    SetExecState(Globals.STATE_WAITING);
                    System.Threading.Thread.Sleep(Globals.__WORKFLOW_WAIT_INTERVAL__ * 60 * 1000);
                    SetExecState(Globals.STATE_EXECUTING);
                }

                Ready();

                Main.Invoke(new MethodInvoker(delegate() { Main.MainLog = WorkflowLog; }));
            }
            catch (ThreadAbortException e)
            {
                Log("Thread execution aborted.");

                if (webDriver != null)
                {
                    webDriver.FixtureTearDown();
                    webDriver = null;
                }
            }
        }

        // TODO - Move these Facebook methods to a new dedicated class.  Will hold off for now because I'm lazy.  --Kris
        private void GOTV()
        {
            if (Globals.executionState == Globals.STATE_STOPPING || Main.stop)
            {
                Log("Thread stop received.  Workflow aborted.");
                return;
            }

            int lastState = Globals.executionState;
            SetExecState(Globals.STATE_VALIDATING);

            RegistryKey softwareKey = Registry.CurrentUser.OpenSubKey("Software", true);
            RegistryKey appKey = softwareKey.CreateSubKey("FaceBERN!");
            RegistryKey GOTVKey = appKey.CreateSubKey("GOTV");

            string lastCheck;
            if ((lastCheck = GOTVKey.GetValue("LastCheck", "", RegistryValueOptions.None).ToString()) != "" 
                && Globals.devOverride != true)
            {
                if (DateTime.Now.Subtract(new DateTime(long.Parse(lastCheck))).TotalHours < Int32.Parse(Globals.Config["GOTVIntervalHours"]))
                {
                    GOTVKey.Close();
                    appKey.Close();
                    softwareKey.Close();

                    SetExecState(lastState);

                    return;
                }
            }

            Log("Running GOTV checklist....");

            /* Initialize the Selenium WebDriver.  --Kris */
            webDriver = new WebDriver(Main, browser);

            /* Initialize the browser.  --Kris */
            webDriver.FixtureSetup();

            webDriver = FacebookLogin();
            if (webDriver.error > 0)
            {
                GOTVKey.Close();
                appKey.Close();
                softwareKey.Close();

                SetExecState(lastState);

                Log("Error " + webDriver.error + " logging into Facebook.  GOTV aborted.");
                return;
            }

            // TEST (TODO - Move to the state loop below and make sure count > 0 for each given state).
            //List<Person> friends = GetFacebookFriendsOfFriends(ref webDriver, "WA");

            // TEST
            //CreateGOTVEvent(ref webDriver, ref friends, "NY");
            
            /* Cycle through each state and execute GOTV actions, where appropriate.  --Kris */
            string[] defaultGOTVDaysBack = Globals.Config["DefaultGOTVDaysBack"].Split(new char[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries);
            foreach (KeyValuePair<string, States> state in Globals.StateConfigs)
            {
                if (Globals.executionState == Globals.STATE_STOPPING || Main.stop)
                {
                    Log("Thread stop received.  Workflow aborted.");
                    return;
                }

                // DEBUG - Uncomment below if you'd like to force-test a single state.  --Kris

                if (!(state.Key.Equals("NM")))
                {
                    continue;
                }


                Log("Checking GOTV for " + state.Key + "....");

                /* Determine if it's time for GOTV in this state.  --Kris */
                // TODO - Yes, I know this logic is overly broad and simplistic.  We can expand upon it and enable per-state configurations later.  --Kris
                RegistryKey stateKey = GOTVKey.CreateSubKey(state.Key);

                string lastGOTVDaysBack = stateKey.GetValue("LastGOTVDaysBack", "", RegistryValueOptions.None).ToString();
                int last = (lastGOTVDaysBack != "" ? Int32.Parse(lastGOTVDaysBack) : -1);

                bool dateAppropriate = false;
                foreach (string entry in defaultGOTVDaysBack)
                {
                    int milestone = Int32.Parse(entry);
                    if (((last == -1 || last > milestone)
                          && state.Value.primaryDate.Subtract(DateTime.Today).TotalDays >= 0
                          && state.Value.primaryDate.Subtract(DateTime.Today).TotalDays <= milestone)
                        || Globals.devOverride == true)
                    {
                        SetProgressBar(Globals.PROGRESSBAR_MARQUEE);

                        /* Retrieve friends of friends who like Bernie Sanders and live in this state.  --Kris */
                        List<Person> friends = GetFacebookFriendsOfFriends(state.Key);

                        // TODO - Add direct friends who like Bernie Sanders and live in this state.  --Kris

                        if (friends == null || friends.Count == 0)
                        {
                            Log("You have no friends or friends of friends in " + state.Key + " who like Bernie Sanders.  Skipped.");
                        }
                        else
                        {
                            ExecuteGOTV(ref friends, ref stateKey, state.Value, milestone);
                        }

                        dateAppropriate = true;

                        SetProgressBar(Globals.PROGRESSBAR_HIDDEN);

                        break;
                    }
                }

                if (!dateAppropriate)
                {
                    Log("No GOTV needed for " + state.Key + " at this time.");
                }

                stateKey.Close();
            }

            try
            {
                GOTVKey.SetValue("LastCheck", DateTime.Now.Ticks.ToString(), RegistryValueKind.String);
            }
            catch (IOException e)
            {
                Log("Warning:  Error updating last GOTV check : " + e.Message);
            }

            if (webDriver != null)
            {
                webDriver.FixtureTearDown();
                webDriver = null;
            }

            GOTVKey.Close();
            appKey.Close();
            softwareKey.Close();

            SetExecState(lastState);
        }

        private void ExecuteGOTV(ref List<Person> friends, ref RegistryKey stateKey, States state, int milestone)
        {
            if (Globals.executionState == Globals.STATE_STOPPING || Main.stop)
            {
                Log("Thread stop received.  Workflow aborted.");
                return;
            }

            int lastState = Globals.executionState;
            SetExecState(Globals.STATE_EXECUTING);

            Log("Executing GOTV for " + state.name + "....");

            /* Attempt to gain access to feelthebern.events event for this state, if applicable.  --Kris */
            int retries = 5;
            if (Globals.Config["UseFTBEvents"].Equals("1") && friends.Count > 0)
            {
                if (Globals.StateConfigs[state.abbr].FTBEventId != null)
                {
                    while (!CheckFTBEventAccess(state.abbr) && retries > 0)
                    {
                        retries--;
                        Log("Access denied for state event page.");

                        if (retries > 0)
                        {
                            RequestFTBInvitation();

                            // TODO - Remove this temporary block of code when feelthebern.events has made the requisite updates so we can check for their friend requests.  --Kris
                            Log("Please wait for the friend request and accept it, then re-run the GOTV for this event.");
                            return;
                            // End temporary code block.  --Kris

                            Wait(Globals.__FTB_REQUEST_ACCESS_WAIT_INTERVAL__, "for retry");

                            /* Check for and accept friend request from feelthebern.events.  --Kris */
                            if (!ftbFriended)
                            {
                                if ((ftbFriended = AcceptFacebookFTBRequest()))
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
                            Log("Retries exhausted.  Aborting feelthebern.events action for " + state.abbr + ".");
                            SetExecState(Globals.STATE_ERROR);
                        }
                    }

                    /* If there are no errors, proceed with the invitations.  --Kris */
                    if (Globals.executionState > 0 && CheckFTBEventAccess(state.abbr, false))
                    {
                        InviteToEventSidebar(ref friends, state.abbr, milestone.ToString());
                    }
                }
                else
                {
                    Log("No feelthebern.events event exists for " + state.abbr + ".");
                }
            }

            /* If there are any people left to invite, create a new private event for every 200 people and do the remaining invites there.  --Kris */
            if (Globals.Config["UseCustomEvents"].Equals("1") && friends.Count > 0)
            {
                int c = friends.Count;
                int i = 0;
                while (c > 0)
                {
                    if (i > 0)
                    {
                        Wait((10 + (5 * i)), "for ratelimit pause between Facebook event creations.");
                    }
                    i++;

                    CreateGOTVEvent(ref friends, state.abbr);

                    if (c == friends.Count)
                    {
                        break;  // To prevent infinite recursion in the event of an unexpected error.  --Kris
                    }
                }
            }

            try
            {
                stateKey.SetValue("LastGOTVDaysBack", Math.Floor(state.primaryDate.Subtract(DateTime.Now).TotalDays).ToString(), RegistryValueKind.String);
            }
            catch (IOException e)
            {
                Log("Warning:  Error updating last GOTV for " + state.name + " : " + e.Message);
            }

            Log("GOTV for " + state.abbr + " complete!");

            SetExecState(lastState);
        }

        private void Wait(int duration, string reason = "", string unit = "minute")
        {
            if (Globals.executionState == Globals.STATE_STOPPING || Main.stop)
            {
                Log("Thread stop received.  Workflow aborted.");
                return;
            }

            Log("Waiting " + duration.ToString() + (duration != 1 ? " " + unit + "s" : " " + unit) + (reason != "" ? " " + reason : "") + "....");

            System.Threading.Thread.Sleep(100);

            int lastState = Globals.executionState;
            SetExecState(Globals.STATE_WAITING);

            int ms = duration * 1000;
            switch (unit.ToLower())
            {
                default:
                    Log("Warning:  Unrecognized unit '" + unit + "' in Wait().  Assuming seconds.");
                    break;
                case "second":
                    break;
                case "minute":
                    ms *= 60;
                    break;
                case "hour":
                    ms *= (int) Math.Pow(60, 2);
                    break;
            }

            System.Threading.Thread.Sleep(ms);

            SetExecState(lastState);
        }

        /* Open a new browser session and login to Facebook.  --Kris */
        private WebDriver FacebookLogin(int retry = 5)
        {
            if (Globals.executionState == Globals.STATE_STOPPING || Main.stop)
            {
                Log("Thread stop received.  Workflow aborted.");
                return null;
            }

            int lastState = Globals.executionState;
            SetExecState(Globals.STATE_EXECUTING);

            /* Navigate the browser to Facebook.  --Kris */
            webDriver.TestSetUp("http://www.facebook.com");

            // DEBUG
            /*File.WriteAllText(Path.Combine(Environment.CurrentDirectory, "test.txt"), webDriver.GetPageSource(browser));
            Log("DEBUG - " + Path.Combine(Environment.CurrentDirectory, "test.txt"));
            webDriver.error = 1;

            return null;*/

            /* If needed, prompt the user for username/password or use the encrypted copy in the system registry.  --Kris */
            SetExecState(Globals.STATE_VALIDATING);
            if (webDriver.GetElementById("loginbutton") != null)
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

                    Log("Logging-in to Facebook....");

                    /* Enter the username and password into the login form on Facebook.  --Kris */
                    webDriver.TypeInId("email", credentials.ToString(u));
                    webDriver.TypeInId("pass", credentials.ToString(p));

                    /* Get this sensitive data out of active memory.  --Kris */
                    if (credentials != null)
                    {
                        credentials.Destroy();
                        credentials = null;
                    }

                    /* Click the login button on Facebook.  --Kris */
                    //dynamic element = webDriver.GetElementById("u_0_y");
                    dynamic element = webDriver.GetElementByCSSSelector("input[type='submit'][value='Log In']");
                    webDriver.ClickElement(element);

                    //Thread.Sleep(3);  // Give the state a chance to unready itself.  Better safe than sorry.  --Kris
                    //webDriver.WaitForPageLoad();

                    /* Check for successful login.  --Kris */
                    if (webDriver.GetElementById("loginbutton") == null)
                    {
                        if (webDriver.GetElementById("u_0_1") != null)  // Checks to see if the "Home" link is present.  --Kris
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

                Log("Error detected:  " + webDriver.error.ToString() + ".  Closing browser session....");
                webDriver.FixtureTearDown();

                retry--;
                if (retry > 0)
                {
                    SetExecState(Globals.STATE_VALIDATING);

                    webDriver.FixtureSetup();

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
                
                IWebElement ele = webDriver.GetElementByCSSSelector("[data-testid=\"blue_bar_profile_link\"]");
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
            }

            SetExecState(lastState);

            return webDriver;
        }

        private bool CreateGOTVEvent(ref List<Person> friends, string stateAbbr, int retry = 5)
        {
            if (Globals.executionState == Globals.STATE_STOPPING || Main.stop)
            {
                Log("Thread stop received.  Workflow aborted.");
                return false;
            }

            int lastState = Globals.executionState;
            try
            {
                SetExecState(Globals.STATE_EXECUTING);

                Log("Creating private GOTV event for " + stateAbbr + "....");

                webDriver.GoToUrl("https://www.facebook.com/events/upcoming");

                webDriver.ClickElement(webDriver.GetElementByCSSSelector("[data-testid=\"event-create-button\"]"));

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
                webDriver.TypeText(webDriver.GetInputElementByPlaceholder("Include a place or address"), location);
                webDriver.TypeText(webDriver.GetInputElementByPlaceholder("Add a short, clear name"), eventName);
                webDriver.TypeText(webDriver.GetElementByTagNameAndAttribute("div", "title", "Tell people more about the event"), description);
                webDriver.TypeText(webDriver.GetInputElementByPlaceholder("mm/dd/yyyy"), OpenQA.Selenium.Keys.Control + "a");
                webDriver.TypeText(webDriver.GetInputElementByPlaceholder("mm/dd/yyyy"), state.primaryDate.ToString("MM/dd/yyyy"));
                webDriver.ClickOnXPath(".//button[.='Create']");

                /* Check to see if we're on the new event page.  --Kris */
                System.Threading.Thread.Sleep(5000);

                IWebElement inviteButton = webDriver.GetElementByCSSSelector("[data-testid=\"event_invite_button\"]");
                if (inviteButton == null)
                {
                    Log("ERROR:  Event creation failed!  Aborted.");
                    return false;
                }

                Log("GOTV event for " + stateAbbr + " created at : " + webDriver.GetURL());

                InviteToEventSidebar(ref friends, stateAbbr);

                SetExecState(lastState);

                return true;
            }
            /* Any number of random flukes can happen.  Retries will enable the workflow to proceed in the event of an intermittent or one-time error.  --Kris */
            catch (Exception e)
            {
                retry--;
                if (retry == 0)
                {
                    Log("ERROR:  Retries exhausted!  Event creation aborted.");
                    SetExecState(Globals.STATE_BROKEN);

                    return false;
                }

                Log("Retrying " + (5 - retry).ToString() + @"/5 after unexpected error : " + e.Message);

                SetExecState(lastState);

                return CreateGOTVEvent(ref friends, stateAbbr, retry);
            }
        }

        public List<Person> GetInvitedPeople()
        {
            List<Person> invited = new List<Person>();

            try
            {
                RegistryKey softwareKey = Registry.CurrentUser.OpenSubKey("Software", true);
                RegistryKey appKey = softwareKey.CreateSubKey("FaceBERN!");
                RegistryKey GOTVKey = appKey.CreateSubKey("GOTV");

                string invitedJSON = (string)GOTVKey.GetValue("invitedJSON", null);
                if (invitedJSON != null && invitedJSON.Trim() != "")
                {
                    invited = JsonConvert.DeserializeObject<List<Person>>(invitedJSON);
                }

                GOTVKey.Close();
                appKey.Close();
                softwareKey.Close();
            }
            catch (IOException e)
            {
                Log("Warning:  Error reading previous invitations from registry : " + e.Message);

                return null;
            }

            return invited;
        }

        private List<string> GetExclusionList(string excludeDupsContactedAfterTicks = null)
        {
            List<Person> invited = GetInvitedPeople();
            if (invited == null)
            {
                return null;
            }
            
            List<string> exclude = new List<string>();
            if (excludeDupsContactedAfterTicks != null && invited != null && invited.Count > 0)
            {
                foreach (Person inv in invited)
                {
                    if (inv.getLastGOTVInvite() != null && long.Parse(inv.getLastGOTVInvite()) > long.Parse(excludeDupsContactedAfterTicks))
                    {
                        exclude.Add(inv.getFacebookID());
                    }
                }
            }

            return exclude;
        }

        /* Invite up to 200 people to this event.  Must already be on the event page with the invite button!  --Kris */
        // Not currently used but may be useful later.  Please leave this function where it is.  --Kris
        private void InviteToEvent(ref List<Person> friends, string stateAbbr = null, string excludeDupsContactedAfterTicks = null)
        {
            if (Globals.executionState == Globals.STATE_STOPPING || Main.stop)
            {
                Log("Thread stop received.  Workflow aborted.");
                return;
            }

            int lastState = Globals.executionState;
            SetExecState(Globals.STATE_EXECUTING);

            List<Person> invited = GetInvitedPeople();
            List<string> exclude = GetExclusionList();

            /* Remember:  This is intended to run unattended so speed isn't a major concern.  Ratelimiting is needed to keep Facebook from thinking we're a spambot.  --Kris */
            if (friends.Count > 0)
            {
                Log("Sending invitations....");

                IWebDriver w = webDriver.GetDriver();
                IWebElement inviteButton = w.FindElement(By.CssSelector("[data-testid=\"event_invite_button\"]"));
                if (inviteButton == null)
                {
                    Log("Invite button not found!  Invitations aborted.");
                    return;
                }

                webDriver.ClickElement(inviteButton);
                System.Threading.Thread.Sleep(Globals.__BROWSE_DELAY__ * 1000);

                IWebElement searchBox = webDriver.GetInputElementByPlaceholder("Search for people or enter their email address");
                if (searchBox == null)
                {
                    Log("Unable to locate search box!  Invitations aborted.");
                    return;
                }

                int i = 0;
                List<Person> oldFriends = new List<Person>();
                foreach (Person friend in friends)
                {
                    if (exclude != null && exclude.Contains(friend.getFacebookID()))
                    {
                        Log("Invitation has already been sent to " + friend.getName() + ".  Skipped.");
                        continue;
                    }

                    if (IsInvitedBySomeoneElse(friend.getFacebookID()))
                    {
                        Log("This user has already been invited by another Bernie supporter.  Skipped.");
                        continue;
                    }

                    webDriver.TypeText(searchBox, OpenQA.Selenium.Keys.Control + "a");
                    webDriver.TypeText(searchBox, @"@" + friend.getName());

                    /* This is NOT intended as a spam tool.  These delays are necessary to keep Facebook's automated spam checks from throwing a false positive and blocking the user.  --Kris */
                    System.Threading.Thread.Sleep(Globals.__BROWSE_DELAY__ * 1000);
                    webDriver.TypeText(searchBox, OpenQA.Selenium.Keys.Tab);
                    System.Threading.Thread.Sleep(Globals.__BROWSE_DELAY__ * 500);

                    /* We don't want to trigger any false positives from the spam algos.  --Kris */
                    if (i % 100 == 0 && i > 1)
                    {
                        Wait(5, "for 100-interval ratelimit");
                    }
                    else if (i % 50 == 0 && i > 1)
                    {
                        Wait(3, "for 50-interval ratelimit");
                    }

                    else if (i % 20 == 0 && i > 1)
                    {
                        Wait(1, "for 20-interval ratelimit");
                    }

                    if (webDriver.GetElementByXPath(".//div[.='" + friend.getName() + "']", 1) != null)
                    {
                        Log("Added " + friend.getName() + " to invite list.");

                        oldFriends.Add(friend);

                        i++;
                        if (i == 200)  // Leaving one open for good measure.  --Kris
                        {
                            Wait(3, "for max-invites ratelimit");
                            break;
                        }
                    }
                    else
                    {
                        /* Try the lookup by Facebook ID before giving up.  --Kris */
                        webDriver.TypeText(searchBox, OpenQA.Selenium.Keys.Control + "a");
                        webDriver.TypeText(searchBox, @"@" + friend.getFacebookID());

                        System.Threading.Thread.Sleep(Globals.__BROWSE_DELAY__ * 1000);
                        webDriver.TypeText(searchBox, OpenQA.Selenium.Keys.Tab);
                        System.Threading.Thread.Sleep(Globals.__BROWSE_DELAY__ * 1000);

                        if (webDriver.GetElementByXPath(".//div[.='" + friend.getName() + "']", 1) != null)
                        {
                            Log("Added " + friend.getName() + " to invite list.");

                            oldFriends.Add(friend);
                            AppendLatestInvitesQueue(friend);  // This queue stores invited users who will be sent to the Birdie API to prevent spam resulting from duplicate invitations.  --Kris

                            i++;
                            if (i == 200)  // Leaving one open for good measure.  --Kris
                            {
                                break;
                            }
                        }
                        else
                        {
                            oldFriends.Add(friend);

                            Log("Unable to add " + friend.getName() + " to invite list.");
                        }
                    }
                }

                /* Send the invitations!  --Kris */
                //webDriver.ClickOnXPath(browser, ".//button[.='Send Invites']");  // Comment if you want to test without actually sending the invitations.  --Kris

                Log("Successfully invited " + i.ToString() + " " + (i == 1 ? "person" : "people") + " to GOTV event" + (stateAbbr != null ? " for " + stateAbbr : "") + ".");

                UpdateInvitationsCount(i);

                foreach (Person friend in oldFriends)
                {
                    if (invited.Contains(friend))
                    {
                        invited.Remove(friend);
                    }
                    else
                    {
                        Person remove = null;
                        foreach (Person inv in invited)
                        {
                            if (inv.getFacebookID() == friend.getFacebookID())
                            {
                                remove = inv;
                                break;
                            }
                        }

                        if (remove != null)
                        {
                            invited.Remove(remove);
                        }
                    }

                    friends.Remove(friend);

                    friend.setLastGOTVInvite(DateTime.Now);

                    invited.Add(friend);
                }

                string invitedJSON = JsonConvert.SerializeObject(invited);

                try
                {
                    RegistryKey softwareKey = Registry.CurrentUser.OpenSubKey("Software", true);
                    RegistryKey appKey = softwareKey.CreateSubKey("FaceBERN!");
                    RegistryKey GOTVKey = appKey.CreateSubKey("GOTV");

                    GOTVKey.SetValue("invitedJSON", invitedJSON, RegistryValueKind.String);

                    GOTVKey.Flush();
                    appKey.Flush();
                    softwareKey.Flush();

                    GOTVKey.Close();
                    appKey.Close();
                    softwareKey.Close();
                }
                catch (IOException e)
                {
                    Log("Warning:  Error storing updated invitations record : " + e.Message);
                }
            }

            SetExecState(lastState);
        }

        /* Query Facebook's Graph API to see if this user has been invited to the event already by someone else.  Credit to daniel.glecker on Slack for finding it.  --Kris */
        private bool IsInvitedAPI(string eventId, string userId)
        {
            /* The API only accepts numberic user ID.  --Kris */
            int n;
            if (!(int.TryParse(userId, out n)))
            {
                // Use findmyfbid.com to get the userId.  --Kris
            }

            if (userId == null)
            {
                return false;
            }

            // TODO - API doesn't seem to work.  Scrapped for now.  --Kris

            return false;
        }

        /* Check to see if any other FaceBERN! users have invited this user.  --Kris */
        // TODO - Enable check for specific event ID.  --Kris
        private bool IsInvitedBySomeoneElse(string userId, int retry = 2)
        {
            IRestResponse res = BirdieQuery("/facebook/invited/" + userId);
            if (res.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return false;
            }
            else if (res.StatusCode != System.Net.HttpStatusCode.OK)
            {
                Log("Warning:  Birdie query returned error response in IsInvitedBySomeoneElse().");

                return false;
            }

            if (res.Content.Equals(""))
            {
                Log("Warning:  Birdie query returned empty response in IsInvitedBySomeoneElse().");
                
                return false;
            }

            FacebookUser fbUser = new FacebookUser();
            try
            {
                fbUser = JsonConvert.DeserializeObject<FacebookUser>(res.Content);
            }
            catch (JsonSerializationException e)
            {
                retry--;
                if (retry == 0)
                {
                    return false;
                }

                System.Threading.Thread.Sleep(3);

                return IsInvitedBySomeoneElse(userId, retry);
            }
            catch (JsonReaderException e)
            {
                return false;
            }

            return (fbUser != null && fbUser.fbUserId != null && fbUser.fbUserId.ToLower().Equals(userId));
        }

        private List<IWebElement> LoadFriendInEventSearchBox(Person friend, IWebElement searchBox, int delayMultiplier = 1)
        {
            webDriver.TypeText(searchBox, OpenQA.Selenium.Keys.Control + "a");
            webDriver.TypeText(searchBox, friend.getName());

            System.Threading.Thread.Sleep(500 * delayMultiplier);

            /* This is NOT intended as a spam tool.  These delays are necessary to keep Facebook's automated spam checks from throwing a false positive and blocking the user.  --Kris */
            int i = 3;
            List<IWebElement> res = new List<IWebElement>();
            do
            {
                System.Threading.Thread.Sleep(500 * delayMultiplier);

                res = webDriver.GetElementsByTagNameAndAttribute("li", "aria-label", friend.getName());

                i--;
            } while (res.Count == 0 && i > 0);

            return res;
        }

        /* Must already be on the event page with the invite sidebar!  --Kris */
        private void InviteToEventSidebar(ref List<Person> friends, string stateAbbr = null, string excludeDupsContactedAfterTicks = null)
        {
            if (Globals.executionState == Globals.STATE_STOPPING || Main.stop)
            {
                Log("Thread stop received.  Workflow aborted.");
                return;
            }

            int lastState = Globals.executionState;
            SetExecState(Globals.STATE_EXECUTING);

            List<Person> invited = GetInvitedPeople();
            List<string> exclude = GetExclusionList();

            /* Remember:  This is intended to run unattended so speed isn't a major concern.  Ratelimiting is needed to keep Facebook from thinking we're a spambot.  --Kris */
            if (friends.Count > 0)
            {
                Log("Sending invitations....");

                SetProgressBar(Globals.PROGRESSBAR_CONTINUOUS);

                IWebElement searchBox = webDriver.GetElementByTagNameAndAttribute("input", "aria-label", "Add friends to this event");
                if (searchBox == null)
                {
                    Log("Unable to locate search box!  Invitations aborted.");
                    return;
                }

                int i = 0;
                int iteration = 0;
                foreach (Person friend in friends)
                {
                    SetProgressBar((int) Math.Round((decimal) (((double) iteration / friends.Count) * 100), 0, MidpointRounding.AwayFromZero));
                    iteration++;

                    if (exclude != null && exclude.Contains(friend.getFacebookID()))
                    {
                        Log("Invitation has already been sent to " + friend.getName() + ".  Skipped.");
                        continue;
                    }

                    if (IsInvitedBySomeoneElse(friend.getFacebookID()))
                    {
                        Log("This user has already been invited by another Bernie supporter.  Skipped.");
                        continue;
                    }

                    List<IWebElement> res = LoadFriendInEventSearchBox(friend, searchBox);

                    if (res.Count == 0)
                    {
                        /* Retry once; a bit more slowly, this time.  Sometimes it just doesn't load correctly or quickly enough in the browser.  --Kris */
                        res = LoadFriendInEventSearchBox(friend, searchBox, 2);
                    }

                    if (res.Count == 0)
                    {
                        Log("Unable to add " + friend.getName() + " to invite list.");
                    }
                    else if (res.Count == 1)
                    {
                        if (res[0].GetAttribute("class") != null && res[0].GetAttribute("class").Contains("nonInvitable"))
                        {
                            Log("Facebook user " + friend.getName() + " has already been invited.  Skipped.");

                            System.Threading.Thread.Sleep(250);
                        }
                        else
                        {
                            res[0].Click();

                            IWebElement ele;
                            int ii = 10;
                            do
                            {
                                System.Threading.Thread.Sleep(250);

                                ele = webDriver.GetElementById("event_invite_feedback");

                                ii--;
                            } while (ele == null && ii > 0);

                            string msg = ele.Text;
                            if (ele != null && ele.Text == null && ele.GetAttribute("innerHTML") != null)
                            {
                                msg = ele.GetAttribute("innerHTML");
                            }

                            if (ele != null && msg.Contains(" was invited."))
                            {
                                Log("Added " + friend.getName() + " to invite list.");

                                i++;

                                friend.setLastGOTVInvite(DateTime.Now);
                                AppendLatestInvitesQueue(friend);  // This queue stores invited users who will be sent to the Birdie API to prevent spam resulting from duplicate invitations.  --Kris
                                UpdateInvitationsCount();

                                invited.Add(friend);

                                PersistInvited(invited);
                            }
                            else if (ele == null)
                            {
                                Log("Warning:  Unable to confirm whether " + friend.getName() + " was added to the invite list!");
                            }
                            else
                            {
                                Log("Facebook user " + friend.getName() + " may not have been invited (msg=" + msg + ").");
                            }
                        }
                    }
                    else
                    {
                        Log("Multiple people with the same name found.  The feature to handle that hasn't been written yet.  Skipped.");
                    }

                    /* We don't want to trigger any false positives from the spam algos.  --Kris */
                    if (i % 1000 == 0 && i > 1)
                    {
                        Wait(15, "for 1000-interval ratelimit");
                    }
                    if (i % 100 == 0 && i > 1)
                    {
                        Wait(3, "for 100-interval ratelimit");
                    }
                    else if (i % 50 == 0 && i > 1)
                    {
                        Wait(2, "for 50-interval ratelimit");
                    }
                    else if (i % 25 == 0 && i > 1)
                    {
                        Wait(1, "for 25-interval ratelimit");
                    }
                    else if (i % 5 == 0 && i > 1)
                    {
                        Wait(3, "for 5-interval ratelimit", "second");
                    }
                }

                this.invited = GetInvitedPeople();
            }

            SetProgressBar(100);

            /* Allow a few seconds to update Birdie, in case it's waiting.  --Kris */
            SetExecState(Globals.STATE_WAITING);

            System.Threading.Thread.Sleep(5000);

            SetExecState(lastState);
        }

        private void PersistInvited(List<Person> invited)
        {
            string invitedJSON = JsonConvert.SerializeObject(invited);

            try
            {
                RegistryKey softwareKey = Registry.CurrentUser.OpenSubKey("Software", true);
                RegistryKey appKey = softwareKey.CreateSubKey("FaceBERN!");
                RegistryKey GOTVKey = appKey.CreateSubKey("GOTV");

                GOTVKey.SetValue("invitedJSON", invitedJSON, RegistryValueKind.String);

                GOTVKey.Flush();
                appKey.Flush();
                softwareKey.Flush();

                GOTVKey.Close();
                appKey.Close();
                softwareKey.Close();
            }
            catch (IOException e)
            {
                Log("Warning:  Error storing updated invitations record : " + e.Message);
            }
        }

        // TODO - This is currently broken!  The user it's looking for may not be the one who did the invite.  Need to be able to query feelthebern.events for the ID of whoever sent the invite.  --Kris
        private bool AcceptFacebookFTBRequest()
        {
            if (Globals.executionState == Globals.STATE_STOPPING || Main.stop)
            {
                Log("Thread stop received.  Workflow aborted.");
                return false;
            }

            int lastState = Globals.executionState;
            SetExecState(Globals.STATE_EXECUTING);

            webDriver.GoToUrl("http://www.facebook.com");

            IWebDriver w = webDriver.GetDriver();
            webDriver.ClickElement(w.FindElement(By.CssSelector("[data-tooltip-content=\"Friend Requests\"]")));
            
            IWebElement button = webDriver.GetElementByXPath(".//button[contains(@onclick, '100001066887477')]");

            if (button == null)
            {
                Log("Unable to locate feelthebern.events friend request.  It may simply have not been sent yet.");
                return false;
            }

            Log("Facebook friend request for feelthebern.events found.");

            if (webDriver.ClickElement(button))
            {
                Log("Facebook friend request for feelthebern.events accepted.");
                System.Threading.Thread.Sleep(3000);

                SetExecState(lastState);

                return true;
            }
            else
            {
                Log("Error clicking on friend request accept button!");

                SetExecState(lastState);

                return false;
            }
        }

        private List<Person> GetFacebookFriendsOfFriends(string stateAbbr = null, bool bernieSupportersOnly = true)
        {
            if (Globals.executionState == Globals.STATE_STOPPING || Main.stop)
            {
                Log("Thread stop received.  Workflow aborted.");
                return new List<Person>();
            }

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
            webDriver.GoToUrl(URL);

            /* Keep scrolling to the bottom until all results have been loaded.  --Kris */
            IWebDriver iWebDriver = webDriver.GetDriver();
            webDriver.ScrollToBottom(ref iWebDriver, "Preparing to load results....", "Loading results (set $i/$L max sets)....", "Finished loading results!");  // Each "set" is a scroll.  --Kris

            /* Scrape the results from the page source.  --Kris */
            string[] resRaw = new string[32767];
            resRaw = webDriver.GetPageSource(browser).Split(new string[] { "<div class=\"_gll\">" }, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 1; i < resRaw.Length; i++)
            {
                if (Globals.executionState == Globals.STATE_STOPPING || Main.stop)
                {
                    Log("Thread stop received.  Workflow aborted.");
                    return new List<Person>();
                }

                if (resRaw[i] == null || resRaw[i].Length < 40 || resRaw[i].Substring(0, 34) != "<a href=\"https://www.facebook.com/")
                {
                    continue;
                }

                /* Thought about using regex, then decided to stab myself in the forehead with an icepick, instead.  I'm happy with my choice.  --Kris */
                string start = "<a href=\"https://www.facebook.com/";
                string end = "\">";
                // NOTE - We're not going to be using these IDs in any graph searches so we don't need to worry about retrieving the actual numeric ID; the username is sufficient.  --Kris
                string userId = resRaw[i].Substring(resRaw[i].IndexOf(start) + start.Length, resRaw[i].IndexOf(end) - (resRaw[i].IndexOf(start) + start.Length));

                start = "profile.php?id=";
                end = @"&";

                if (userId.IndexOf(start) == 0)
                {
                    userId = userId.Substring(start.Length);
                    if (userId.IndexOf(end) != -1)
                    {
                        userId = userId.Substring(0, userId.IndexOf(end));
                    }
                }

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

            if (res.Count > 0)
            {
                Log("Retrieved " + res.Count.ToString() + " results for friends of friends" + logMsg + ".");
            }
            else
            {
                Log("No results found.");
            }

            SetExecState(lastState);

            return res;
        }

        /* Load the Facebook event page from feelthebern.events and return whether or not the user has access to the event.  --Kris */
        private bool CheckFTBEventAccess(string stateAbbr, bool navigate = true)
        {
            if (Globals.executionState == Globals.STATE_STOPPING || Main.stop)
            {
                Log("Thread stop received.  Workflow aborted.");
                return false;
            }

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

            if (navigate == true)
            {
                webDriver.GoToUrl("https://www.facebook.com/events/" + Globals.StateConfigs[stateAbbr].FTBEventId);
            }

            System.Threading.Thread.Sleep(3000);  // Just in case the driver doesn't wait long enough on its own.  --Kris

            SetExecState(lastState);

            return (webDriver.GetPageSource(browser).IndexOf("Sorry, this page isn't available") == -1);
        }

        /* Load feelthebern.events in a separate browser session and attempt to get invited to the state events.  Note that it can take over an hour for the request to be processed.  --Kris */
        private void RequestFTBInvitation()
        {
            if (Globals.executionState == Globals.STATE_STOPPING || Main.stop)
            {
                Log("Thread stop received.  Workflow aborted.");
                return;
            }

            int lastState = Globals.executionState;
            SetExecState(Globals.STATE_VALIDATING);

            /* Retrieve the Facebook profile URL for the logged-in user from the registry.  It's automatically stored on login.  --Kris */
            RegistryKey softwareKey = Registry.CurrentUser.OpenSubKey("Software", true);
            RegistryKey appKey = softwareKey.CreateSubKey("FaceBERN!");
            RegistryKey facebookKey = appKey.CreateSubKey("Facebook");

            string profileURL = facebookKey.GetValue("profileURL", null, RegistryValueOptions.None).ToString();

            facebookKey.Close();
            appKey.Close();
            softwareKey.Close();

            /* Initialize the driver and navigate to feelthebern.events.  --Kris */
            if (profileURL != null)
            {
                Log("Requesting invitation from feelthebern.events....");

                SetExecState(Globals.STATE_EXECUTING);

                WebDriver webDriver2 = new WebDriver(Main, browser);

                webDriver2.FixtureSetup();
                webDriver2.TestSetUp("http://www.feelthebern.events");

                webDriver2.TypeInXPath(@"/html/body/div[2]/input", profileURL);
                webDriver2.ClickOnXPath(@"/html/body/div[2]/button");

                System.Threading.Thread.Sleep(5000);

                Log("Request sent.");

                webDriver2.FixtureTearDown();
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

        private void UpdateInvitationsCount(int x = 1, bool clear = false)
        {
            if (Main.InvokeRequired)
            {
                Main.BeginInvoke(
                    new MethodInvoker(
                        delegate() { UpdateInvitationsCount(x, clear); }));
            }
            else
            {
                Main.UpdateInvitationsCount(x, clear);

                Main.Refresh();
            }

            if (!clear)
            {
                invitesSent += x;
            }
            else
            {
                invitesSent = 0;
            }
        }

        /* Update local and remote total invites.  If you just want to update the total remote invites, pass 0 for x.  --Kris */
        private void UpdateInvitationsCount(int x, int y, bool hard = true)
        {
            if (Main.InvokeRequired)
            {
                Main.BeginInvoke(
                    new MethodInvoker(
                        delegate() { UpdateInvitationsCount(x, y); }));
            }
            else
            {
                if (hard)
                {
                    Main.SetInvitationsCount(x, y);
                }
                else
                {
                    Main.UpdateInvitationsCount(x, y);
                }

                Main.Refresh();
            }

            invitesSent += x;
        }

        private void SetExecState(int state)
        {
            if (Main.InvokeRequired)
            {
                Main.BeginInvoke(
                    new MethodInvoker(
                        delegate() { SetExecState(state); }));
            }
            else
            {
                Main.SetExecState(state, logName, WorkflowLog);
            }
        }

        private void SetProgressBar(int percent)
        {
            if (Main.InvokeRequired)
            {
                Main.BeginInvoke(
                    new MethodInvoker(
                        delegate() { SetProgressBar(percent); }));
            }
            else
            {
                Main.SetProgressBar(percent);
            }
        }
    }
}
