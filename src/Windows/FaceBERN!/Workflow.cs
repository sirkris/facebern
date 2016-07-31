using AutoIt;
using csReddit;
using Microsoft.Win32;
using Newtonsoft.Json;
using OpenQA.Selenium;
using RestSharp;
using RestSharp.Authenticators;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Twitterizer;

namespace FaceBERN_
{
    public class Workflow
    {
        private string logName = "Workflow";
        public Log WorkflowLog;
        private Form1 Main;
        protected Reddit reddit;
        protected int browser = 0;
        protected bool ftbFriended = false;

        public long invitesSent = 0;  // Tracked for current session only.  --Kris

        public List<Person> invited;
        public List<string> tweets;

        protected WebDriver webDriver = null;

        protected RestClient restClient;

        protected int remoteInvitesSent = 0;

        protected List<Person> remoteUpdateQueue = null;

        protected Random rand;

        private string lastLogMsg = null;

        private string twitterConsumerKey = "NB74pt5RpC7QuszjGy8qy7rju";
        private string twitterConsumerSecret = "ifP1aw4ggRjkCktFEnU2T0zS2HA0XxlCpzjb601SMRo7U4HoNR";
        private Credentials twitterAccessCredentials = null;
        private OAuthTokens twitterTokens = null;

        private List<TweetsQueue> tweetsQueue = null;
        private List<TweetsQueue> tweetsHistory = null;

        private TwitterStatusCollection twitterTimelineCache = null;
        private DateTime? twitterTimelineCacheLastRefresh = null;

        private List<ExceptionReport> exceptions;

        private DateTime lastLogClear;

        private DateTime lastUpdate;

        public Workflow(Form1 Main, Log MainLog = null)
        {
            exceptions = new List<ExceptionReport>();

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

            /* Initialize the Birdie API client. --Kris */
            restClient = new RestClient(Globals.BirdieHost);

            reddit = new Reddit(false);

            lastLogClear = DateTime.Now;
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
        public void ExecuteInterCom(bool skipStartup = false)
        {
            try
            {
                DateTime start = DateTime.Now;
                lastUpdate = start;

                if (skipStartup == false)
                {
                    invited = GetInvitedPeople();  // Get list of people you already invited with this program from the system registry.  --Kris

                    /* Register FaceBERN! with the Birdie API if it's not already.  --Kris */
                    if (CheckClientRegistration() == false)
                    {
                        RegisterClient();

                        System.Threading.Thread.Sleep(5000);
                    }

                    /* Load any invitations persisted in the registry from a previous run.  --Kris */
                    LoadLatestInvitesQueue();

                    /* If we relaunched due to an unhandled exception, log/report it.  --Kris */
                    ReportPreRecoveryError();
                }

                /* The main InterCom loop.  --Kris */
                int i = 0;
                while (true)
                {
                    /* Send a keep-alive to the Birdie API.  --Kris */
                    KeepAlive();

                    /* Check for updates every 6 hours if auto-update is enabled and the work flow is idle (READY state).  Occurs in Execute(), otherwise.  --Kris */
                    if (Globals.executionState == Globals.STATE_READY 
                        && Globals.Config["AutoUpdate"].Equals("1")
                        && DateTime.Now.Subtract(lastUpdate).TotalHours >= 6)
                    {
                        lastUpdate = DateTime.Now;
                        Main.Invoke(new MethodInvoker(delegate() { Main.CheckForUpdates(true); }));
                        System.Threading.Thread.Sleep(5000);
                    }

                    /* Update our list of active campaigns.  --Kris */
                    LoadCampaigns();

                    /* Process the remote update queue and send it to Birdie.  --Kris */
                    PostLatestInvites((i == 0));

                    /* Update our local invitations count for both ours and remote.  --Kris */
                    UpdateRemoteInvitesCount();
                    invited = GetInvitedPeople();
                    UpdateInvitationsCount(invited.Count, remoteInvitesSent);

                    /* Update our local tweets count for both ours and remote.  --Kris */
                    UpdateRemoteTweetsCount();

                    /* Update the number of tweets waiting in our local queue.  --Kris */
                    UpdateLocalTweetsQueueCount();

                    /* Update our local count for both active users and total users.  --Kris */
                    UpdateRemoteUsers();

                    if (i == 0)
                    {
                        EnableMain();
                    }

                    System.Threading.Thread.Sleep(Globals.__INTERCOM_WAIT_INTERVAL__ * 60 * 1000);

                    i++;
                }
            }
            catch (Exception e)
            {
                LogAndReportException(e, "Unhandled exception in InterCOM thread.");

                System.Threading.Thread.Sleep(30000);

                Log("Restarting InterCOM....");

                ExecuteInterCom(true);
            }
        }

        // Temporary:  Clear the log every 24 hours as possible workaround to access violation exception in the logging function after prolonged execution.  --Kris
        private void ClearLog()
        {
            try
            {
                if (Main.InvokeRequired)
                {
                    Main.BeginInvoke(
                        new MethodInvoker(
                            delegate() { ClearLog(); }));
                }
                else
                {
                    if (DateTime.Now > lastLogClear.AddHours(24))
                    {
                        Main.ClearLogW();
                        Main.Refresh();

                        lastLogClear = DateTime.Now;
                    }
                }
            }
            catch (Exception e)
            {
                ReportException(e, "Exception raised in Workflow ClearLog method.");
            }
        }

        private void ReportPreRecoveryError()
        {
            RegistryKey softwareKey = Registry.CurrentUser.OpenSubKey("Software", true);
            RegistryKey appKey = softwareKey.CreateSubKey("FaceBERN!");

            ExceptionReport preRecoveryException = JsonConvert.DeserializeObject<ExceptionReport>((string) appKey.GetValue("preRecoveryException", @"{}"));
            appKey.DeleteValue("preRecoveryException", false);

            appKey.Close();
            softwareKey.Close();

            /* If we relaunched due to an exception, log/report it.  --Kris */
            if (preRecoveryException != null 
                && preRecoveryException.ex != null)
            {
                try
                {
                    Log("Recovered from error.  Reporting....");

                    preRecoveryException.Main = Main;
                    preRecoveryException.Send();
                }
                catch (Exception ex)
                {
                    Log("Warning:  Unable to report previous exception : " + ex.Message);

                    try
                    {
                        preRecoveryException = new ExceptionReport(Main, ex, "Failed to report:  Application recovered from unhandled exception.");
                    }
                    catch (Exception ex2)
                    {
                        Log("Warning:  Error reporting appears to be broken : " + ex2.ToString());
                    }
                }
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

        protected void AppendLatestInvitesQueue(List<Person> invites, bool clear = false)
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

        protected void AppendLatestInvitesQueue(Person invite)
        {
            AppendLatestInvitesQueue(new List<Person> { invite });
        }

        protected void ClearLatestInvitesQueue()
        {
            AppendLatestInvitesQueue(null, true);
        }

        private int? GetIntFromRes(IRestResponse res, string op = "complete operation")
        {
            int r;
            if (res.StatusCode == System.Net.HttpStatusCode.OK)
            {
                try
                {
                    r = Int32.Parse(res.Content);
                }
                catch (Exception e)
                {
                    Log("Warning:  Unexpected response from Birdie API.  Unable to " + op + ".");
                    return null;
                }
            }
            else
            {
                Log("Warning:  Birdie API query failed.  Unable to " + op + ".");
                return null;
            }

            return r;
        }

        /* Get the number of Facebook users invited by everyone.  --Kris */
        private void UpdateRemoteInvitesCount()
        {
            Dictionary<string, string> queryParams = new Dictionary<string, string>();
            queryParams.Add("return", "count");
            
            IRestResponse res = BirdieQuery("/facebook/invited", "GET", queryParams);

            int? r = GetIntFromRes(res, "update remote invites count");
            if (r == null)
            {
                return;
            }

            remoteInvitesSent = r.Value;

            UpdateInvitationsCount(-1, r.Value);
        }

        /* Get the number of tweets tweeted by everyone.  --Kris */
        private void UpdateRemoteTweetsCount()
        {
            IRestResponse res = BirdieQuery(@"/twitter/tweets?tweetedBy=" + GetAppID() + "&return=count", "GET");

            int? myTweets = GetIntFromRes(res, "update this client's tweet count");
            if (myTweets == null)
            {
                return;
            }

            res = BirdieQuery(@"/twitter/tweets?return=count", "GET");

            int? totalTweets = GetIntFromRes(res, "update total tweets count");
            if (totalTweets == null)
            {
                return;
            }

            UpdateTweetsCount(myTweets.Value, totalTweets.Value);
        }

        /* Update the number of tweets in the queue displayed on the main form.  --Kris */
        private void UpdateLocalTweetsQueueCount()
        {
            UpdateLocalTweetsQueue(true);
            GetTweetsQueue();

            UpdateTweetsQueuedCount(tweetsQueue.Count);
        }

        /* Get the active campaigns.  --Kris */
        private void LoadCampaigns()
        {
            try
            {
                IRestResponse res = BirdieQuery(@"/campaigns?showActiveOnly", "GET");

                if (res != null && res.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    Globals.campaigns = BirdieToCampaigns(JsonConvert.DeserializeObject(res.Content));

                    SaveCampaignsToRegistry();
                }
                else
                {
                    Log("Warning:  Unable to retrieve updated campaigns from Birdie.  Falling back to local cache....");

                    LoadCampaignsFromRegistry();
                }
            }
            catch (Exception e)
            {
                Log("Warning:  Error loading campaigns : " + e.ToString());

                ReportException(e, "Exception thrown loading campaigns.");
            }
        }

        /* Grab our latest cache if Birdie API is down.  --Kris */
        private void LoadCampaignsFromRegistry()
        {
            try
            {
                RegistryKey softwareKey = Registry.CurrentUser.OpenSubKey("Software", true);
                RegistryKey appKey = softwareKey.CreateSubKey("FaceBERN!");

                Globals.campaigns = BirdieToCampaigns(JsonConvert.DeserializeObject((string) appKey.GetValue("campaignsCache")));

                appKey.Close();
                softwareKey.Close();
            }
            catch (Exception e)
            {
                Log("Warning:  Error loading campaigns from registry : " + e.ToString());

                ReportException(e, "Exception thrown loading campaigns from registry.");
            }
        }

        /* Maintain a local cahce of our current campaigns in case there's an API issue of some kind.  --Kris */
        private void SaveCampaignsToRegistry()
        {
            try
            {
                RegistryKey softwareKey = Registry.CurrentUser.OpenSubKey("Software", true);
                RegistryKey appKey = softwareKey.CreateSubKey("FaceBERN!");

                appKey.SetValue("campaignsCache", JsonConvert.SerializeObject(Globals.campaigns), RegistryValueKind.String);

                appKey.Close();
                softwareKey.Close();
            }
            catch (Exception e)
            {
                Log("Warning:  Error saving campaigns cache to registry : " + e.ToString());

                ReportException(e, "Exception thrown saving campaigns cache to registry.");
            }
        }

        /* Report one or more new tweets to Birdie API.  --Kris */
        private void ReportNewTweets(List<TweetsQueue> tweets)
        {
            for (int i = 0; i < tweets.Count; i++)
            {
                tweets[i].SetTweetedBy(GetAppID());
            }

            IRestResponse res = BirdieQuery(@"/twitter/tweets", "POST", null, JsonConvert.SerializeObject(tweets));

            if (res.StatusCode == System.Net.HttpStatusCode.Created)
            {
                Log("Tweet(s) reported successfully.", false);
            }
            else
            {
                Log("Warning:  Tweets reporting to Birdie API failed : " + res.StatusCode);
            }

            UpdateRemoteTweetsCount();
        }

        private void ReportNewTweet(TweetsQueue tweet)
        {
            ReportNewTweets(new List<TweetsQueue> { tweet });
        }

        private List<TweetsQueue> BirdieToTweetsQueue(dynamic deserializedJSON, bool overwrite = true, bool returnOnly = false)
        {
            if (overwrite || tweetsQueue == null)
            {
                tweetsQueue = new List<TweetsQueue>();
            }

            List<TweetsQueue> res = new List<TweetsQueue>();
            foreach (dynamic o in deserializedJSON)
            {
                if (o != null
                    && o["tweet"] != null
                    && o["entered"] != null
                    && o["enteredBy"] != null
                    && o["start"] != null
                    && o["end"] != null)
                {
                    try
                    {
                        res.Add(new TweetsQueue(o["tweet"].ToString(), (o["source"] != null ? o["source"].ToString() : "Birdie"), null, DateTime.Now, DateTime.Parse(o["entered"].ToString()),
                            o["enteredBy"].ToString(), DateTime.Parse(o["start"].ToString()), DateTime.Parse(o["end"].ToString()), (int?) (o["campaignId"] != null ? o["campaignId"] : null),
                            (o["tid"] != null ? (int) o["tid"] : 0), (o["tweetedAt"] != null ? DateTime.Parse(o["tweetedAt"].ToString()) : null), (string) o["twitterStatusId"]));
                    }
                    catch (Exception e)
                    {
                        Log("Warning:  Unable to import tweets queue from Birdie API : " + e.ToString());

                        ReportException(e, "Warning:  Unable to import tweets queue from Birdie API.");
                    }
                }
            }

            if (returnOnly == false)
            {
                tweetsQueue.AddRange(res);
            }

            return res;
        }

        private List<Campaign> BirdieToCampaigns(dynamic deserializedJSON, bool overwrite = true)
        {
            if (overwrite || Globals.campaigns == null)
            {
                Globals.campaigns = new List<Campaign>();
            }

            /* Wait until the configurations are loaded.  --Kris */
            int i = 150;  // 15 seconds
            while (Globals.CampaignConfigs == null 
                && i > 0)
            {
                System.Threading.Thread.Sleep(100);
                i--;
            }

            foreach (dynamic o in deserializedJSON)
            {
                if (o != null
                    && o["campaignId"] != null
                    && o["campaignTitle"] != null
                    && o["createdByAdminUsername"] != null
                    && o["createdAt"] != null
                    && o["start"] != null
                    && o["enabled"] != null
                    && o["requiresFacebook"] != null
                    && o["approvedByDefault"] != null
                    && o["requiresTwitter"] != null)
                {
                    bool userSelected;
                    if (Globals.CampaignConfigs != null && Globals.CampaignConfigs.ContainsKey((int) o["campaignId"]))
                    {
                        userSelected = Globals.CampaignConfigs[(int) o["campaignId"]];
                    }
                    else
                    {
                        userSelected = ((string) o["approvedByDefault"]).Equals("1");
                    }

                    Campaign addCampaign = new Campaign((int) o["campaignId"], (string) o["campaignTitle"], (string) o["createdByAdminUsername"], (DateTime) o["createdAt"], (DateTime) o["start"],
                                                (bool) ((string) o["enabled"]).Equals("1"), (bool) ((string) o["requiresFacebook"]).Equals("1"), (bool) ((string) o["requiresTwitter"]).Equals("1"), 
                                                userSelected, ((string) o["approvedByDefault"]).Equals("1"), (string) o["campaignDescription"], (string) o["campaignURL"], (int?) o["parentCampaignId"], 
                                                (DateTime?) o["end"]);

                    Globals.campaigns.Add(addCampaign);

                    Globals.CampaignConfigs[(int) o["campaignId"]] = addCampaign.userSelected;
                }
            }

            return Globals.campaigns;
        }

        internal List<TweetsQueue> GetTweetsQueue()
        {
            return tweetsQueue;
        }

        internal bool RemoveFromLocalTweetsQueue(string tweet)
        {
            bool entryFound = false;

            List<TweetsQueue> newQueue = new List<TweetsQueue>();
            foreach (TweetsQueue entry in tweetsQueue)
            {
                if (entry.GetTweet() != tweet)
                {
                    newQueue.Add(entry);
                }
                else
                {
                    entryFound = true;

                    // Adding to the backend "history" to keep it from being re-added.  This will NOT make it appear in the history window.  --Kris
                    AppendTweetsHistory(entry, true);
                }
            }

            tweetsQueue = newQueue;

            PersistLocalTweetsQueue();

            return entryFound;
        }

        /* Update the local cache of this client's tweets queue.  Just doing a straight-up replace since that'll clean-out any expired/disabled entries.  --Kris */
        internal void UpdateLocalTweetsQueue(bool continueIfDisabled = false)
        {
            if (!(Globals.Config["EnableTwitter"].Equals("1")) 
                && continueIfDisabled == false)
            {
                return;
            }

            Log("Updating tweets queue....");

            /* Get any tweets in the Birdie API queue.  --Kris */
            IRestResponse res = BirdieQuery(@"/twitter/tweetsQueue?showActiveOnly&showQueueFor=" + GetAppID(), "GET");

            tweetsQueue = BirdieToTweetsQueue(JsonConvert.DeserializeObject(res.Content));

            /* Grab any local data from the registry.  --Kris */
            List<TweetsQueue> localTweetsQueue;
            try
            {
                RegistryKey softwareKey = Registry.CurrentUser.OpenSubKey("Software", true);
                RegistryKey appKey = softwareKey.CreateSubKey("FaceBERN!");
                RegistryKey twitterKey = appKey.CreateSubKey("Twitter");

                localTweetsQueue = JsonConvert.DeserializeObject<List<TweetsQueue>>((string) twitterKey.GetValue("tweetsQueue", new List<TweetsQueue>()));

                twitterKey.Close();
                appKey.Close();
                softwareKey.Close();
            }
            catch (Exception)
            {
                localTweetsQueue = new List<TweetsQueue>();
            }

            foreach (TweetsQueue entry in localTweetsQueue)
            {
                bool found = false;
                for ( int i = 0; i < tweetsQueue.Count; i++ )
                {
                    if (tweetsQueue[i].GetTweet() == entry.GetTweet())
                    {
                        tweetsQueue[i].SetFailures(entry.GetFailures());
                        found = true;
                    }
                }

                if (found == false)
                {
                    tweetsQueue.Add(entry);
                }
            }

            /* Get any tweets from Reddit, if enabled.  --Kris */
            GetTweetsFromReddit();

            /* Filter anything out that appears in our "history" cache.  --Kris */
            FilterTweetedFromQueue();

            PersistLocalTweetsQueue();
        }

        private void FilterTweetedFromQueue()
        {
            LoadTweetsHistory(true);

            List<TweetsQueue> newQueue = new List<TweetsQueue>();
            foreach (TweetsQueue entry in tweetsQueue)
            {
                bool match = false;
                foreach (TweetsQueue hEntry in tweetsHistory)
                {
                    if (hEntry.GetTweet() == entry.GetTweet())
                    {
                        match = true;
                        break;
                    }
                }

                if (match == false)
                {
                    newQueue.Add(entry);
                }
            }

            tweetsQueue = newQueue;
        }

        /* Persist the queue in the system registry.  --Kris */
        private void PersistLocalTweetsQueue()
        {
            try
            {
                RegistryKey softwareKey = Registry.CurrentUser.OpenSubKey("Software", true);
                RegistryKey appKey = softwareKey.CreateSubKey("FaceBERN!");
                RegistryKey twitterKey = appKey.CreateSubKey("Twitter");

                twitterKey.SetValue("tweetsQueue", JsonConvert.SerializeObject(tweetsQueue), RegistryValueKind.String);

                twitterKey.Close();
                appKey.Close();
                softwareKey.Close();
            }
            catch (Exception e)
            {
                Log("Warning:  Unable to persist tweets queue to registry : " + e.ToString());

                ReportException(e, "Unable to persist tweets queue to registry.");
            }
        }

        /* Update our history of recent tweets.  This is used to prevent duplicate tweet spam in the event of an error or abuse.  --Kris */
        private void AppendTweetsHistory(List<TweetsQueue> entries, bool skipReport = false)
        {
            if (tweetsHistory == null)
            {
                tweetsHistory = new List<TweetsQueue>();
            }

            for (int i = 0; i < entries.Count; i++)
            {
                entries[i].SetTweeted(DateTime.Now);
            }
            
            tweetsHistory.AddRange(entries);

            SanitizeTweetsHistory();  // Removes duplicates and tweets more than 30 days old.  Also sorts by date/time tweeted (ascending).  --Kris

            try
            {
                RegistryKey softwareKey = Registry.CurrentUser.OpenSubKey("Software", true);
                RegistryKey appKey = softwareKey.CreateSubKey("FaceBERN!");
                RegistryKey twitterKey = appKey.CreateSubKey("Twitter");

                twitterKey.SetValue("tweetsHistory", JsonConvert.SerializeObject(tweetsHistory), RegistryValueKind.String);

                twitterKey.Close();
                appKey.Close();
                softwareKey.Close();
            }
            catch (Exception e)
            {
                Log("Warning:  Unable to update tweets queue in registry : " + e.ToString());

                ReportException(e, "Unable to update tweets queue in registry.");
            }

            if (!(skipReport))
            {
                ReportNewTweets(entries);
            }
        }

        private void AppendTweetsHistory(TweetsQueue entry, bool skipReport = false)
        {
            AppendTweetsHistory(new List<TweetsQueue> { entry }, skipReport);
        }

        private void SanitizeTweetsHistory()
        {
            List<TweetsQueue> newTweetsHistory = new List<TweetsQueue>();
            foreach (TweetsQueue entry in tweetsHistory)
            {
                if (!(newTweetsHistory.Contains(entry))
                    && entry.tweeted != null
                    && DateTime.Now <= entry.tweeted.Value.AddDays(30))
                {
                    newTweetsHistory.Add(entry);
                }
            }

            tweetsHistory = newTweetsHistory.OrderBy(entry => entry.tweeted.Value).ToList();
        }

        /* Load recent tweets history.  This is used for backend deduping/etc; the "official" tweets history (that you see in the History window) is pulled separately from Birdie API.  --Kris */
        private List<TweetsQueue> LoadTweetsHistory(bool includeRemote = false)
        {
            try
            {
                RegistryKey softwareKey = Registry.CurrentUser.OpenSubKey("Software", true);
                RegistryKey appKey = softwareKey.CreateSubKey("FaceBERN!");
                RegistryKey twitterKey = appKey.CreateSubKey("Twitter");

                tweetsHistory = JsonConvert.DeserializeObject<List<TweetsQueue>>(
                    (string) twitterKey.GetValue("tweetsHistory", JsonConvert.SerializeObject(new List<TweetsQueue>()), RegistryValueOptions.None)
                );

                twitterKey.Close();
                appKey.Close();
                softwareKey.Close();
            }
            catch (Exception e)
            {
                Log("Warning:  Error loading recent tweets history from registry : " + e.ToString());

                ReportException(e, "Error loading recent tweets history from registry.");

                tweetsHistory = new List<TweetsQueue>();
            }

            if (includeRemote)
            {
                List<TweetsQueue> historyRemote = GetTweetsHistoryFromBirdie();

                List<TweetsQueue> newHistory = new List<TweetsQueue>();
                foreach (TweetsQueue entry in historyRemote)
                {
                    bool match = false;
                    foreach (TweetsQueue lEntry in tweetsHistory)
                    {
                        if (lEntry.GetTweet() == entry.GetTweet())
                        {
                            match = true;
                            break;
                        }
                    }

                    if (match == false)
                    {
                        newHistory.Add(entry);
                    }
                }

                tweetsHistory.AddRange(newHistory);
            }

            SanitizeTweetsHistory();

            return tweetsHistory;
        }

        /* Get the number of active and total users.  --Kris */
        private void UpdateRemoteUsers()
        {
            IRestResponse res = BirdieQuery(@"/clients?active&return=count", "GET");

            int? active = GetIntFromRes(res, "update active users count");
            if (active == null)
            {
                return;
            }

            res = BirdieQuery(@"/clients?return=count", "GET");

            int? total = GetIntFromRes(res, "update total users count");
            if (total == null)
            {
                return;
            }

            UpdateActiveUsers(active.Value, total.Value);
        }

        /* Tell the Birdie API we're still active.  --Kris */
        private void KeepAlive()
        {
            Dictionary<string, string> body = new Dictionary<string, string> { { "appVersion", Globals.__VERSION__ } };

            IRestResponse res = BirdieQuery("/clients/" + GetAppID() + "/keepAlive", "PUT", null, JsonConvert.SerializeObject(body));

            if (res.StatusCode == System.Net.HttpStatusCode.NoContent)
            {
                Log("Sent keep-alive to Birdie API successfully.", false);
            }
            else
            {
                Log("Warning:  Birdie API keep-alive query failed : " + res.StatusCode);
            }
        }

        /* Tell the Birdie API who we are.  --Kris */
        private void RegisterClient()
        {
            Dictionary<string, string> postBody = new Dictionary<string, string>();
            postBody.Add("clientId", GetAppID());
            postBody.Add("appName", @"FaceBERN!");

            IRestResponse res = BirdieQuery(@"/clients", "POST", null, JsonConvert.SerializeObject(postBody));

            if (res.StatusCode == System.Net.HttpStatusCode.Created)
            {
                Log("Client registered with Birdie API successfully.");
            }
            else
            {
                Log("Warning:  Client registration with Birdie API failed : " + res.StatusCode);
            }
        }

        /* Check to see if we're registered with the Birdie API.  --Kris */
        private bool CheckClientRegistration()
        {
            return (BirdieQuery("/clients/" + GetAppID(), "GET").StatusCode == System.Net.HttpStatusCode.OK);
        }

        /* Query the Birdie API and return the raw result.  --Kris */
        internal IRestResponse BirdieQuery(string path, string method = "GET", Dictionary<string, string> queryParams = null, string body = "")
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

                /* If this client is associated with a Birdie admin account, include the Basic Authorization headers.  --Kris */
                Credentials credentials = new Credentials(false, false, true);
                if (credentials.IsBirdieAdmin())
                {
                    restClient.Authenticator = new SimpleAuthenticator(
                                                                        "username", credentials.ToString(credentials.GetBirdieUsername()),
                                                                        "password", credentials.ToString(credentials.GetBirdiePassword())
                                                    );
                }

                credentials.Destroy();
                credentials = null;

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

        internal List<TweetsQueue> GetTweetsHistoryFromBirdie()
        {
            try
            {
                IRestResponse res = BirdieQuery(@"/twitter/tweets?tweetedBy=" + Globals.appId + @"&verbose", "GET");

                if (res.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    return BirdieToTweetsQueue(JsonConvert.DeserializeObject(res.Content), false, true);
                }
                else
                {
                    Log("Warning:  Bad response from API for GetTweetsHistoryFromBirdie() : " + res.StatusDescription);

                    return null;
                }
            }
            catch (Exception e)
            {
                LogAndReportException(e, "Warning:  Failed to retrieve tweets history from Birdie.");

                return null;
            }
        }

        internal bool UpdateBirdieTwitterStatusIDs(List<TweetsQueue> history)
        {
            try
            {
                IRestResponse res = BirdieQuery(@"/twitter/tweets", "PUT", null, JsonConvert.SerializeObject(history));  // API ignores everything except tid and twitterStatusId for PUT.  --Kris

                if (res.StatusCode == System.Net.HttpStatusCode.NoContent)
                {
                    return true;  // Query succeeded and one or more records were updated.  --Kris
                }
                else if (res.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    return true;  // Query succeeded but no records needed to be updated, either because they don't exist or because the value didn't change.  --Kris
                }
                else
                {
                    Log("Warning:  Bad response from API for UpdateBirdieTwitterStatusIDs() : " + res.StatusDescription);

                    return false;  // Query failed.  --Kris
                }
            }
            catch (Exception e)
            {
                LogAndReportException(e, "Warning:  Failed to update tweets history to Birdie.");

                return false;
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
            try
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
            catch (Exception e)
            {
                LogAndReportException(e, "Unhandled exception in Shutdown thread.");

                Log("Workflow broken!  Please restart Birdie.");

                SetExecState(Globals.STATE_BROKEN);
            }
        }

        public Thread ExecuteTwitterAuthThread(int browser)
        {
            SetExecState(Globals.STATE_TWITTERPIN);

            Thread thread = new Thread(() => ExecuteTwitterAuth(browser));

            Main.LogW("Attempting to start TwitterAuth thread....", false);

            thread.Start();
            while (thread.IsAlive == false) { }

            Main.LogW("TwitterAuth thread started successfully.", false);

            return thread;
        }

        public void ExecuteTwitterAuth(int browser)
        {
            try
            {
                Log("Commencing Twitter authorization workflow....");

                AuthorizeTwitter(browser);

                Log("Twitter authorization complete!");

                Ready();
            }
            catch (Exception e)
            {
                LogAndReportException(e, "Unhandled exception in TwitterAuth thread.");
            }
        }

        private void LoadTwitterCredentialsFromRegistry()
        {
            twitterAccessCredentials = new Credentials(false, true);
        }

        private void SaveTwitterCredentialsToRegistry(string accessToken, string accessTokenSecret)
        {
            if (twitterAccessCredentials == null)
            {
                twitterAccessCredentials = new Credentials();
            }

            twitterAccessCredentials.SetTwitter(twitterAccessCredentials.ToSecureString(accessToken), twitterAccessCredentials.ToSecureString(accessTokenSecret));
        }

        private string GetTwitterAccessToken()
        {
            if (twitterAccessCredentials == null)
            {
                LoadTwitterCredentialsFromRegistry();
            }

            return (twitterAccessCredentials.GetTwitterAccessToken() != null ? twitterAccessCredentials.ToString(twitterAccessCredentials.GetTwitterAccessToken()) : null);
        }

        private string GetTwitterAccessTokenSecret()
        {
            if (twitterAccessCredentials == null)
            {
                LoadTwitterCredentialsFromRegistry();
            }

            return (twitterAccessCredentials.GetTwitterAccessTokenSecret() != null ? twitterAccessCredentials.ToString(twitterAccessCredentials.GetTwitterAccessTokenSecret()) : null);
        }

        internal void AuthorizeTwitter(int browser)
        {
            Log("Checking Twitter credentials....");

            if (twitterAccessCredentials == null)
            {
                LoadTwitterCredentialsFromRegistry();
            }

            /* If access token/secret aren't stored, do the workflow for obtaining that.  --Kris */
            if (twitterAccessCredentials.IsAssociated() == false)
            {
                Log("User credentials not stored.  Loading Twitter authorization page....");

                string requestToken = OAuthUtility.GetRequestToken(twitterConsumerKey, twitterConsumerSecret, "oob").Token;
                string authURI = OAuthUtility.BuildAuthorizationUri(requestToken).AbsoluteUri;

                string pin = "";

                /* Open a browser window, navigate to the authorization PIN page, and attempt to extract the PIN automatically for convenience.  --Kris */
                bool pinError = false;
                string pinErrorMessage = "";
                try
                {
                    webDriver = new WebDriver(Main, browser);
                    webDriver.FixtureSetup();
                    webDriver.TestSetUp(authURI);

                    System.Threading.Thread.Sleep(3000);

                    /* Wait for user to login and for PIN page to load.  If not detected after timeoutSeconds seconds, pop it up, anyway.  --Kris */
                    int timeoutSeconds = 30;
                    Log("Waiting for PIN detection.  Please wait until " + Globals.__APPNAME__ + " asks you to enter your PIN (up to " + timeoutSeconds.ToString() + " seconds)....");
                    DateTime start = DateTime.Now;
                    do
                    {
                        try
                        {
                            if (webDriver.GetElementById("oauth_pin") != null)
                            {
                                List<IWebElement> eles = webDriver.GetElementsByTagName("code");
                                foreach (IWebElement element in eles)
                                {
                                    if (element.Text != null && element.Text.Trim().Length >= 5)
                                    {
                                        pin = element.Text.Trim();
                                        break;
                                    }
                                }
                            }

                            System.Threading.Thread.Sleep(1000);
                        }
                        catch (Exception e)
                        {
                            pin = "";
                            pinError = true;
                            pinErrorMessage = e.Message;
                        }
                    } while (pin == "" && DateTime.Now.Subtract(start).Seconds < timeoutSeconds);
                }
                catch (Exception e)
                {
                    Log("Warning:  Error using WebDriver to obtain PIN.  Opening in default browser, instead....");

                    if (browser > 0)
                    {
                        ReportException(e, 
                            "Error using WebDriver to obtain PIN for browser " + Globals.BrowserName(browser) + " : " 
                            + (pinError ? "Unable to extract PIN (" + pinErrorMessage + ")" : "Unable to launch browser") + "."
                        );
                    }
                    
                    System.Diagnostics.Process.Start(authURI);

                    System.Threading.Thread.Sleep(5000);  // After the delay, they'll have to enter the PIN manually.  --Kris
                }

                /* We already have the PIN but we still need the user to confirm.  --Kris */
                TwitPin twitPin = new TwitPin(pin);
                DialogResult res = twitPin.ShowDialog();
                if (res == DialogResult.OK)
                {
                    pin = twitPin.pin;
                }
                else
                {
                    Log("User cancelled PIN input!  Twitter authorization aborted.");
                    return;
                }

                webDriver.FixtureTearDown();
                webDriver = null;

                if (pin == null || pin.Trim() == "")
                {
                    Log("No PIN entered!  Twitter credentials not stored!");
                }
                else
                {
                    OAuthTokenResponse accessToken = OAuthUtility.GetAccessToken(twitterConsumerKey, twitterConsumerSecret, requestToken, pin);

                    if (accessToken != null && accessToken.Token != null && accessToken.TokenSecret != null && accessToken.ScreenName != null)
                    {
                        twitterAccessCredentials.SetTwitter(accessToken.Token, accessToken.TokenSecret, accessToken.ScreenName, accessToken.UserId.ToString());

                        Log("Twitter credentials stored successfully!");
                    }
                    else
                    {
                        Log("Twitter authorization FAILED!  Did you enter the PIN correctly?");
                    }
                }
            }
            else
            {
                Log("Twitter credentials loaded successfully.");
            }
        }

        internal Credentials AuthorizeTwitterWithReturn(int browser)
        {
            AuthorizeTwitter(browser);

            return twitterAccessCredentials;
        }

        public Thread ExecuteFacebookThread(int browser)
        {
            SetExecState(Globals.STATE_TWITTERPIN);

            Thread thread = new Thread(() => ExecuteFacebook(browser));

            Main.LogW("Attempting to start Facebook thread....", false);

            thread.Start();
            while (thread.IsAlive == false) { }

            Main.LogW("Facebook thread started successfully.", false);

            return thread;
        }

        private void ExecuteFacebook(int browser)
        {
            WorkflowFacebook workflowFacebook = new WorkflowFacebook(Main);
            workflowFacebook.ExecuteFacebook(browser);
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

            Main.LogW("Workflow thread started successfully.");

            return thread;
        }

        public void Execute(int browser, Log WorkflowLog)
        {
            try
            {
                //InitLog();
                this.WorkflowLog = WorkflowLog;
                
                this.browser = browser;

                DateTime start = DateTime.Now;
                lastUpdate = start;

                Log("Thread execution initialized.");
                
                SetExecState(Globals.STATE_EXECUTING);

                invited = GetInvitedPeople();  // Get list of people you already invited with this program from the system registry.  --Kris

                /* Load the local recent tweets history.  --Kris */
                LoadTweetsHistory();

                //Main.Invoke(new MethodInvoker(delegate() { Main.Refresh(); }));

                // TEST - This will end up going into the loop below.  --Kris
                //GOTV();


                if (Globals.Config["EnableFacebanking"].Equals("1"))
                {
                    Globals.facebookThread = ExecuteFacebookThread(browser);
                }
                else
                {
                    Log("Facebanking has been disabled.  Facebook workflow skipped.");
                }

                /* Loop until terminated by the user.  --Kris */
                while (Globals.executionState > 0)
                {
                    Log("Beginning workflow....");

                    // Deprecated.  --Kris
                    /*if (Globals.Config["EnableTwitter"].Equals("1"))
                    {
                        // Fight back against the media blackout!  --Kris
                        Twitter();
                    }
                    else
                    {
                        Log("Tweetbanking has been disabled.  Twitter workflow skipped.");
                    }*/

                    /* Update the tweets queue, if enabled.  --Kris */
                    if (Globals.Config["EnableTwitter"].Equals("1"))
                    {
                        UpdateLocalTweetsQueue();
                    }

                    /* Execute any selected campaigns.  --Kris */
                    ExecuteCampaigns();

                    /* Check for updates every 6 hours if auto-update is enabled.  --Kris */
                    if (Globals.Config["AutoUpdate"].Equals("1")
                        && DateTime.Now.Subtract(lastUpdate).TotalHours >= 6)
                    {
                        lastUpdate = DateTime.Now;
                        Main.Invoke(new MethodInvoker(delegate() { Main.CheckForUpdates(true); }));
                        System.Threading.Thread.Sleep(5000);
                    }

                    /* Wait between loops.  May lower it later if we start doing more time-sensitive crap like notifications/etc.  --Kris */
                    Log("Workflow complete!  Waiting " + Globals.__WORKFLOW_WAIT_INTERVAL__.ToString() + " minutes for next run....");
                    
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
            catch (Exception e)
            {
                try
                {
                    Log("ERROR:  Unhandled Exception : " + e.ToString());
                    
                    SetExecState(Globals.STATE_ERROR);

                    ReportException(e, "Unhandled Exception in primary workflow.");

                    Log("Aborting broken workflow....");

                    if (webDriver != null)
                    {
                        webDriver.FixtureTearDown();
                        webDriver = null;
                    }

                    System.Threading.Thread.Sleep(3000);

                    if (Globals.executionState != Globals.STATE_BROKEN)
                    {
                        System.Threading.Thread.Sleep(5000);

                        Log("Spawning new workflow thread....");

                        SetExecState(Globals.STATE_RESTARTING);

                        Globals.thread = ExecuteThread();
                    }
                    else
                    {
                        Log("Broken workflow detected!  Workflow is now disabled for safety reasons.  Please restart " + Globals.__APPNAME__ + " to recover from the error.");
                    }
                }
                catch (Exception ex)
                {
                    try
                    {
                        SetExecState(Globals.STATE_BROKEN);

                        if (Globals.thread != null)
                        {
                            Globals.thread.Abort();
                        }
                        while (Globals.thread.IsAlive) { }

                        Log("Broken workflow detected!  Workflow is now disabled for safety reasons.  Please restart " + Globals.__APPNAME__ + " to recover from the error.");
                    }
                    catch (Exception ex2)
                    {
                        return;
                    }
                }
            }
        }

        private void ReportException(Exception ex, string logMsg = null, bool silent = false)
        {
            try
            {
                if (!(silent))
                {
                    Log("Reporting exception....");
                }

                exceptions.Add(new ExceptionReport(Main, ex, logMsg));
            }
            catch (Exception e)
            {
                if (!(silent))
                {
                    Log("Warning:  Error reporting exception : " + e.ToString(), true, true, true, true, true, true);
                }
            }
        }

        internal void ReportExceptionSilently(Exception ex, string logMsg = null)
        {
            try
            {
                ReportException(ex, logMsg, true);
            }
            catch (Exception)
            { }
        }

        /* Execute our top-level campaigns.  Sanity checks are performed in each respective campaign method.  Parent campaigns will call their child campaigns directly.  --Kris */
        // TODO - Move these campaigns someplace else.  --Kris
        private void ExecuteCampaigns()
        {
            Campaign_RunBernieRun();
        }

        private void Campaign_RunBernieRun()
        {
            if (Globals.CampaignConfigs[Globals.CAMPAIGN_RUNBERNIERUN] == false)
            {
                Log("The #RunBernieRun campaign is disabled.  Skipped.");
                return;
            }

            Log(@"Executing campaign:  #RunBernieRun....");

            Campaign_TweetStillSandersForPres();

            Log(@"Finished executing campaign:  #RunBernieRun.");
        }

        private void Campaign_TweetStillSandersForPres()
        {
            if (Globals.CampaignConfigs[Globals.CAMPAIGN_TWEET_STILLSANDERSFORPRES] == false)
            {
                Log("The campaign to send tweets from /r/StillSandersForPres is disabled.  Skipped.");
                return;
            }
            else if (!(Globals.Config["EnableTwitter"].Equals("1")))
            {
                Log("Unable to send tweets from /r/StillSandersForPres because you have Twitter disabled.  Skipped.");
                return;
            }

            LoadTwitterCredentialsFromRegistry();

            if (twitterAccessCredentials.IsAssociated() == false)
            {
                Log("Warning:  Twitter is enabled but you don't have your Twitter account associated!  Twitter workflow aborted.");
                Log(@"You can link " + Globals.__APPNAME__ + " to your Twitter account under Tools->Settings.");

                return;
            }

            LoadTwitterTokens();

            ConsumeTweetsQueue(Globals.CAMPAIGN_TWEET_STILLSANDERSFORPRES);
        }

        // TODO - Move any Twitter workflow methods to a new dedicated class.  --Kris
        private void Twitter()  // DEPRECATED
        {
            if (!(Globals.Config["EnableTwitter"].Equals("1")))
            {
                return;
            }

            LoadTwitterCredentialsFromRegistry();

            if (twitterAccessCredentials.IsAssociated() == false)
            {
                Log("Warning:  Twitter is enabled but you don't have your Twitter account associated!  Twitter workflow aborted.");
                Log(@"You can link " + Globals.__APPNAME__ + " to your Twitter account under Tools->Settings.");

                return;
            }

            LoadTwitterTokens();

            // Uncomment below for DEBUG.  --Kris
            /*
            TwitterResponse<TwitterStatusCollection> timelineDebug = GetMyTweets();
            foreach (TwitterStatus tweet in timelineDebug.ResponseObject)
            {
                Log("DEBUG:  Tweet:  " + tweet.Text + " (" + tweet.CreatedDate.ToString() + ")");
            }

            Tweet("Test tweet.  Please disregard.");
            */

            /* Load the tweets queue from the specified source(s).  --Kris */
            UpdateLocalTweetsQueue();

            /* If there are any tweets in the queue and we're past the tweet interval, tweet the next entry from it.  --Kris */
            ConsumeTweetsQueue();

            DestroyTwitterTokens();
        }

        // TODO - Find an API call to handle this, as there could be URLs embedded within the text, as well.  --Kris
        private string ComposeTweet(string text, string url = "")
        {
            int twitterCharLimit = 140;
            int maxUrlLength = 25;  // TODO - Query and update this.  Published as 22 for now, so should be able to get away with this for awhile.  --Kris

            int urlLength = (url.Length <= maxUrlLength ? url.Length : maxUrlLength);

            if ((text.Length + 1 + urlLength) <= twitterCharLimit)
            {
                return text + " " + url;
            }
            else
            {
                return (text.Substring(0, (twitterCharLimit - 4 - urlLength)) + "... " + url);
            }
        }

        private DateTime TimestampToDateTime(double timestamp)
        {
            return new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds(timestamp);
        }

        // Note - Regardless of whether appendTweetsQueue is true, this function's return value will consist solely of the tweets from this particular Reddit search without the existing queue.  --Kris
        private List<TweetsQueue> GetTweetsFromReddit(bool appendTweetsQueue = true)
        {
            try
            {
                List<TweetsQueue> res = GetTweetsFromSubreddit("StillSandersForPres", Globals.CAMPAIGN_TWEET_STILLSANDERSFORPRES);
                // TODO - Add more subs when able.  --Kris

                if (appendTweetsQueue)
                {
                    foreach (TweetsQueue entry in res)
                    {
                        bool dup = false;
                        foreach (TweetsQueue globalEntry in tweetsQueue)
                        {
                            if (globalEntry.tweet.Trim().ToLower().Equals(entry.tweet.Trim().ToLower()))
                            {
                                dup = true;
                                break;
                            }
                        }

                        if (dup == false)
                        {
                            tweetsQueue.Add(entry);
                        }
                    }
                }

                return res;
            }
            catch (Exception e)
            {
                ReportException(e, "Exception in GetTweetsFromReddit.");
                return null;
            }
        }

        private List<TweetsQueue> GetTweetsFromSubreddit(string sub, int? campaignId = null)
        {
            try
            {
                if (reddit == null)
                {
                    reddit = new Reddit(false);
                }

                // No need to login to Reddit since all we're doing is a search.  --Kris
                List<RedditPost> redditPosts = SearchSubredditForFlairPosts("Tweet This!", sub, campaignId, "month");

                List<TweetsQueue> res = new List<TweetsQueue>();
                foreach (RedditPost redditPost in redditPosts)
                {
                    res.Add(new TweetsQueue(ComposeTweet(redditPost.GetTitle(), redditPost.GetURL()), "Reddit", @"http://www.reddit.com" + redditPost.permalink, DateTime.Now, redditPost.GetCreated(),
                        @"/u/" + redditPost.GetAuthor(), DateTime.Now, DateTime.Now.AddDays(3), campaignId));
                }

                return res;
            }
            catch (Exception e)
            {
                ReportException(e, "Exception in GetTweetsFromSubreddit.");
                return null;
            }
        }

        /* Tweet the next tweet in the queue.  Just do one at a time in order to prevent spam.  --Kris */
        private void ConsumeTweetsQueue(int? campaignId = null)
        {
            try
            {
                if (!(Globals.Config["EnableTwitter"].Equals("1")))
                {
                    return;
                }

                /* Tweets history is already sorted with the latest entry being the most-recent tweet.  --Kris */
                double r;
                if (Globals.Config.ContainsKey("TweetIntervalMinutes")
                    && Double.TryParse(Globals.Config["TweetIntervalMinutes"], out r)
                    && tweetsHistory != null
                    && tweetsHistory.Count > 0
                    && tweetsHistory[tweetsHistory.Count - 1].GetTweeted() != null
                    && tweetsHistory[tweetsHistory.Count - 1].GetTweeted().Value.AddMinutes(r) > DateTime.Now
                    && Globals.devOverride != true)
                {
                    return;  // If it hasn't been TweetIntervalMinutes minutes since the last tweet, don't do it yet.  --Kris
                }

                Log("Consuming tweets queue....");

                if (tweetsQueue != null && tweetsQueue.Count > 0)
                {
                    List<TweetsQueue> newTweetsQueue = new List<TweetsQueue>();
                    TweetsQueue appendTweetsQueue = null;
                    bool tweeted = false;
                    foreach (TweetsQueue entry in tweetsQueue)
                    {
                        if (tweetsHistory.Where(hEntry => hEntry.tweet.Equals(entry.tweet)).ToList().Count > 0
                            || DateTime.Now >= entry.end)
                        {
                            continue;
                        }

                        /* If a campaign ID is passed, ignore anything in the queue without that ID.  --Kris */
                        if (campaignId != null
                            && entry.GetCampaignId() != campaignId)
                        {
                            continue;
                        }

                        if (!(tweeted)
                            && DateTime.Now >= entry.start)
                        {
                            string statusId;
                            if (Tweet(entry.tweet, out statusId))  // Perform the actual tweet.  --Kris
                            {
                                entry.SetStatusID(statusId);
                                AppendTweetsHistory(entry);
                            }
                            else
                            {
                                Log("Warning:  Unable to tweet from queue.  Will try again later.");

                                entry.IncrementFailures();

                                if (entry.GetFailures() > 1)
                                {
                                    Log("Warning:  Repeated failures on this tweet.  Removed from queue.");
                                }
                                else
                                {
                                    appendTweetsQueue = entry;  // Move it to the bottom of the queue so it won't block others.  TODO - Still comes first?!  Investigate when I have time.  --Kris
                                }
                            }

                            tweeted = true;
                        }
                        else
                        {
                            newTweetsQueue.Add(entry);
                        }
                    }

                    if (appendTweetsQueue != null)
                    {
                        newTweetsQueue.Add(appendTweetsQueue);
                    }

                    tweetsQueue = newTweetsQueue;
                    PersistLocalTweetsQueue();
                }
            }
            catch (Exception e)
            {
                ReportException(e, "Exception in ConsumeTweetsQueue.");
            }
        }

        /* Search a given sub for today's (default) top posts with a given flair.  Should only queue stuff from Reddit same-day to prevent delayed tweet spam.  --Kris */
        private List<RedditPost> SearchSubredditForFlairPosts(string flair, string sub, int? campaignId = null, string t = "day", bool? self = null)
        {
            try
            {
                if (self == null)
                {
                    List<RedditPost> res = new List<RedditPost>();
                    res.AddRange(SearchSubredditForFlairPosts(flair, sub, campaignId, t, false));
                    res.AddRange(SearchSubredditForFlairPosts(flair, sub, campaignId, t, true));

                    return res;
                }
                else
                {
                    return ParseRedditPosts(reddit.Search.search(null, null, "flair:\"" + flair + "\" self:" + (self.Value ? "yes" : "no"), false, "new", null, t, sub), sub, campaignId);
                }
            }
            catch (Exception e)
            {
                ReportException(e, "Exception in SearchSubredditForFlairPosts.");
                return null;
            }
        }

        private List<RedditPost> ParseRedditPosts(dynamic redditObj, string sub = null, int? campaignId = null)
        {
            try
            {
                List<RedditPost> res = new List<RedditPost>();

                try
                {
                    if (redditObj["data"]["children"] == null || redditObj["data"]["children"].Count == 0)
                    {
                        return res;  // No results.  --Kris
                    }
                }
                catch (Exception e)
                {
                    ReportExceptionSilently(e, "Exception handling data children from redditObj in ParseRedditPosts.");
                    return res;
                }

                try
                {
                    int i = 0;
                    foreach (dynamic o in redditObj["data"]["children"])
                    {
                        i++;

                        try
                        {
                            if (o != null
                                && o["data"] != null
                                && o["data"]["title"] != null
                                && o["data"]["subreddit"] != null
                                && o["data"]["url"] != null
                                && o["data"]["permalink"] != null
                                && o["data"]["score"] != null
                                && o["data"]["created"] != null)
                            {
                                /* Sometimes, the Reddit search API returns some results from the wrong sub(s).  This will filter those out.  --Kris */
                                if (sub != null && !(sub.Equals(o["data"]["subreddit"].ToString())))
                                {
                                    continue;
                                }

                                try
                                {
                                    res.Add(new RedditPost((bool) o["data"]["is_self"], o["data"]["title"].ToString(), o["data"]["subreddit"].ToString(), o["data"]["url"].ToString(),
                                        o["data"]["permalink"].ToString(), (int) o["data"]["score"], TimestampToDateTime((double) o["data"]["created"]), o["data"]["author"].ToString(),
                                        (string) o["data"]["selftext"], campaignId));
                                }
                                catch (Exception e)
                                {
                                    Log("Warning:  Error parsing Reddit post : " + e.ToString());

                                    ReportException(e, "Error parsing Reddit post.");
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            try
                            {
                                LogAndReportException(e, "Exception thrown handling redditObj in ParseRedditPosts where o = " + JsonConvert.SerializeObject(o) + ".");
                            }
                            catch (Exception)
                            {
                                try
                                {
                                    LogAndReportException(e, "Exception thrown handling redditObj in ParseRedditPosts where i = " + i.ToString() + "; unable to serialize object o.");
                                }
                                catch (Exception)
                                {
                                    // Just forget it.  No point logging it without any useful information.  --Kris
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    ReportExceptionSilently(e, "Exception iterating through data children for redditObj in ParseRedditPosts.");
                }

                return res;
            }
            catch (Exception e)
            {
                LogAndReportException(e, "Exception in ParseRedditPosts.");

                return null;
            }
        }

        internal bool ValidateBirdieCredentials(string username, string password)
        {
            string passHash = "";
            using (System.Security.Cryptography.SHA512 sha = new System.Security.Cryptography.SHA512Managed())
            {
                byte[] passHashBytes = sha.ComputeHash(Encoding.Default.GetBytes(password));

                for (int i = 0; i < passHashBytes.Length; i++)
                {
                    passHash += passHashBytes[i].ToString("X2");
                }
            }

            bool success = false;
            if (username != null && username.Length > 0 && passHash != null && passHash.Length > 0)
            {
                IRestResponse res = BirdieQuery("/admin/users?username=" + username + "&passwordHash=" + passHash);

                success = (res.StatusCode == System.Net.HttpStatusCode.OK);  // Returns 404 if there's no matching record found.  --Kris
            }

            return success;
        }

        internal bool ValidateBirdieInvitationCode(string invitationCode)
        {
            IRestResponse res = BirdieQuery("/admin/invited?invitationCode=" + invitationCode);

            return (res.StatusCode == System.Net.HttpStatusCode.OK);
        }

        internal bool RegisterBirdieAdmin(string invitationCode, string username, string password, out IRestResponse res)
        {
            res = null;
            if (ValidateBirdieInvitationCode(invitationCode) == false)
            {
                return false;
            }

            Dictionary<string, string> body = new Dictionary<string, string> { 
                                                                                { "username", username }, 
                                                                                { "password", password }, 
                                                                                { "invitationCode", invitationCode } 
            };

            res = BirdieQuery("/admin/users", "POST", null, JsonConvert.SerializeObject(body));

            return (res.StatusCode == System.Net.HttpStatusCode.Created);
        }

        /* Post a tweet.  --Kris */
        private bool Tweet(string tweet, out string statusId)
        {
            try
            {
                statusId = null;

                if (!(Globals.Config["EnableTwitter"].Equals("1")))
                {
                    Log("Warning:  Can't tweet '" + tweet + "' because Twitter is disabled in the settings.");

                    return false;
                }

                if (!(TwitterIsAuthorized()))
                {
                    return false;
                }

                if (tweetsHistory.Where(entry => entry.tweet.Equals(tweet)).ToList().Count > 0)
                {
                    Log("Tweet '" + tweet + "' has already been tweeted recently.  Skipped.");

                    return true;
                }

                LoadTwitterTokens();

#if (DEBUG)
            Log("DEBUG - " + tweet);
            return true;
#endif

                TwitterResponse<TwitterStatus> res = TwitterStatus.Update(twitterTokens, tweet);
                if (res.Result == RequestResult.Success)
                {
                    Log("Tweeted '" + tweet + "' successfully.");
                }
                else
                {
                    Log("ERROR posting tweet '" + tweet + "' : " + res.ErrorMessage);
                }

                /* Get the ID of the tweet.  This is necessary in order to give the user the option to delete ("undo") the tweet later.  Would be nice if they included it in the API result....  --Kris */
                statusId = GetTwitterStatusId(tweet);

                return (res.Result == RequestResult.Success);
            }
            catch (Exception e)
            {
                ReportException(e, "Unable to tweet:  " + tweet);
                statusId = null;
                return false;
            }
        }

        internal string GetTwitterStatusId(string tweet)
        {
            try
            {
                TwitterStatusCollection coll = GetTwitterUserTimeline();
                foreach (TwitterStatus status in coll)
                {
                    // TODO - Strip out all links and compare.  --Kris
                    string checkTweet = (status.Text.LastIndexOf(@"https://t.co/") != -1 ? status.Text.Substring(0, status.Text.LastIndexOf(@"https://t.co/")) : status.Text);
                    if (tweet.IndexOf(checkTweet) != -1)
                    {
                        return status.Id.ToString();
                    }
                }

                return null;
            }
            catch (Exception e)
            {
                ReportException(e, "Exception in GetTwitterStatusId.");
                return null;
            }
        }

        private TwitterStatusCollection GetTwitterUserTimeline()
        {
            try
            {
                if (!(TwitterIsAuthorized()))
                {
                    return null;
                }

                LoadTwitterTokens(true);

                if (twitterTimelineCache == null || twitterTimelineCacheLastRefresh == null
                    || twitterTimelineCacheLastRefresh.Value.AddMinutes(Globals.__TWITTER_TIMELINE_CACHE_SHELF_LIFE__) <= DateTime.Now)
                {
                    twitterTimelineCache = TwitterTimeline.UserTimeline(twitterTokens).ResponseObject;
                    twitterTimelineCacheLastRefresh = DateTime.Now;
                }

                return twitterTimelineCache;
            }
            catch (Exception e)
            {
                ReportException(e, "Exception in GetTwitterUserTimeline.");
                return null;
            }
        }

        internal bool DeleteTweet(string twitterStatusId, bool apiOnly = false)
        {
            TwitterResponse<TwitterStatus> res = null;

            try
            {
                bool doApi;
                if (!(apiOnly))
                {
                    LoadTwitterCredentialsFromRegistry();

                    if (twitterAccessCredentials.IsAssociated() == false)
                    {
                        MessageBox.Show("Unable to delete tweet because that account is no longer associated with " + Globals.__APPNAME__ + @"!");
                        return false;
                    }

                    LoadTwitterTokens();

                    res = TwitterStatus.Delete(twitterTokens, decimal.Parse(twitterStatusId));

                    doApi = (res.Result == RequestResult.Success);
                }
                else
                {
                    doApi = true;
                }

                if (doApi)
                {
                    /* Now that the tweet is deleted from Twitter, update the Birdie API.  --Kris */
                    IRestResponse bRes = BirdieQuery(@"/twitter/tweets", "DELETE", null, JsonConvert.SerializeObject(new Dictionary<string, string> { 
                                                                                                                            { "twitterStatusId", twitterStatusId }, 
                                                                                                                            { "tweetedBy", Globals.appId } 
                                                                                                                        }));

                    if (bRes.StatusCode == System.Net.HttpStatusCode.NoContent)
                    {
                        MessageBox.Show("Tweet deleted successfully!");

                        return true;
                    }
                    else
                    {
                        Exception ex;
                        if (apiOnly)
                        {
                            ex = new Exception("Birdie API query to delete tweet from history failed.  No attempt was made to delete it from Twitter.");
                        }
                        else
                        {
                            ex = new Exception("Tweet deleted successfully from Twitter, but not history.");
                        }

                        try
                        {
                            ex.Data.Add("bRes", JsonConvert.SerializeObject(bRes));
                        }
                        catch (Exception) { }

                        throw ex;
                    }
                }
                else
                {
                    throw new Exception("Error deleting tweet; Twitter API returned non-success response in result : " + JsonConvert.SerializeObject(res));
                }
            }
            catch (Exception e)
            {
                ReportExceptionSilently(e, "Error deleting tweet : " + twitterStatusId);

                MessageBox.Show("Error deleting tweet (" + twitterStatusId + ")!");
                return false;
            }
        }

        /* This function is used for testing Twitter integration.  Not currently used by any production workflows.  --Kris */
        private TwitterResponse<TwitterStatusCollection> GetMyTweets(int count = 20)
        {
            if (!(TwitterIsAuthorized()))
            {
                return null;
            }

            LoadTwitterTokens();

            UserTimelineOptions userTimelineOptions = new UserTimelineOptions();
            userTimelineOptions.APIBaseAddress = "https://api.twitter.com/1.1/";
            userTimelineOptions.Count = count;
            userTimelineOptions.UseSSL = true;
            userTimelineOptions.ScreenName = twitterAccessCredentials.ToString(twitterAccessCredentials.GetTwitterUsername());

            return TwitterTimeline.UserTimeline(twitterTokens, userTimelineOptions);
        }

        private bool LoadTwitterTokens(bool onlyIfNull = false)
        {
            try
            {
                if (!(TwitterIsAuthorized()))
                {
                    return false;
                }

                if (twitterTokens == null || onlyIfNull == false)
                {
                    twitterTokens = new OAuthTokens();
                    twitterTokens.ConsumerKey = twitterConsumerKey;
                    twitterTokens.ConsumerSecret = twitterConsumerSecret;
                    twitterTokens.AccessToken = twitterAccessCredentials.ToString(twitterAccessCredentials.GetTwitterAccessToken());
                    twitterTokens.AccessTokenSecret = twitterAccessCredentials.ToString(twitterAccessCredentials.GetTwitterAccessTokenSecret());
                }

                return true;
            }
            catch (Exception e)
            {
                ReportException(e, "Exception in LoadTwitterTokens.");
                return false;
            }
        }

        private void DestroyTwitterTokens()
        {
            twitterTokens = null;
        }

        private bool TwitterIsAuthorized()
        {
            try
            {
                if (twitterAccessCredentials == null || twitterAccessCredentials.IsAssociated() == false)
                {
                    LoadTwitterCredentialsFromRegistry();
                    if (twitterAccessCredentials.IsAssociated() == false)
                    {
                        return false;
                    }
                }

                return true;
            }
            catch (Exception e)
            {
                ReportException(e, "Exception in TwitterIsAuthorized.");
                return false;
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

                ReportException(e, "Error reading previous invitations from registry.");

                return null;
            }

            return invited;
        }

        protected void Wait(int duration, string reason = "", string unit = "minute")
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

        private void LogAndReportException(Exception e, string text, bool show = true)
        {
            Log(text, show);
            ReportException(e, text);
        }

        private void InitLog()
        {
            WorkflowLog = new Log();
            WorkflowLog.Init(logName);
        }

        protected void Log(string text, bool show = true, bool appendW = true, bool newline = true, bool timestamp = true, bool suppressDups = true, bool breakOnFailure = false)
        {
            try
            {
                if (Main.InvokeRequired)
                {
                    Main.BeginInvoke(
                        new MethodInvoker(
                            delegate() { Log(text, show, appendW, newline, timestamp); }));
                }
                else
                {
                    if (suppressDups == true && text.Equals(lastLogMsg))
                    {
                        return;
                    }
                    else if (Main.outBox.Text.Length >= text.Length
                        && Main.outBox.Text.Substring(Main.outBox.Text.Length - text.Length - 1, text.Length).Equals(text))
                    {
                        return;
                    }

                    lastLogMsg = text;

                    Main.LogW(text, show, appendW, newline, timestamp, logName, WorkflowLog);

                    Main.Refresh();
                }
            }
            catch (Exception e)
            {
                if (breakOnFailure)
                {
                    // To prevent infinite recursion.  --Kris
                    try
                    {
                        SetExecState(Globals.STATE_BROKEN);
                    }
                    catch (Exception)
                    {
                        // Just let it go.  --Kris
                    }
                }
                else
                {
                    ReportException(e, "Exception raised in Workflow Log method.");
                }
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

                //Main.Refresh();
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

        /* Update local and remote total invites.  If you just want to update the total remote invites, pass -1 for x.  --Kris */
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

                //Main.Refresh();
            }

            invitesSent += x;
        }

        /* Update local and remote total tweets.  If you just want to update the total remote tweets, pass -1 for x.  --Kris */
        private void UpdateTweetsCount(int x, int y)
        {
            if (Main.InvokeRequired)
            {
                Main.BeginInvoke(
                    new MethodInvoker(
                        delegate() { UpdateTweetsCount(x, y); }));
            }
            else
            {
                Main.SetTweetsTweeted(x, y);

                //Main.Refresh();
            }
        }

        /* Update displayed number of tweets in the local queue.  --Kris */
        private void UpdateTweetsQueuedCount(int total)
        {
            if (Main.InvokeRequired)
            {
                Main.BeginInvoke(
                    new MethodInvoker(
                        delegate() { UpdateTweetsQueuedCount(total); }));
            }
            else
            {
                Main.SetTweetsQueued(total);

                //Main.Refresh();
            }
        }

        /* Update active and total users.  --Kris */
        private void UpdateActiveUsers(int active, int total)
        {
            if (Main.InvokeRequired)
            {
                Main.BeginInvoke(
                    new MethodInvoker(
                        delegate() { UpdateActiveUsers(active, total); }));
            }
            else
            {
                Main.SetActiveUsers(active, total);

                //Main.Refresh();
            }
        }

        /* After everything's loaded, enable the main form.  --Kris */
        private void EnableMain()
        {
            if (Main.InvokeRequired)
            {
                Main.BeginInvoke(
                    new MethodInvoker(
                        delegate() { EnableMain(); }));
            }
            else
            {
                Main.StartupComplete();
            }
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
