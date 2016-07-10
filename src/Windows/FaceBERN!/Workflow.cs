using AutoIt;
using csReddit;
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
        private Reddit reddit;
        private int browser = 0;
        private bool ftbFriended = false;

        public long invitesSent = 0;  // Tracked for current session only.  --Kris

        public List<Person> invited;
        public List<string> tweets;

        private WebDriver webDriver = null;

        private RestClient restClient;

        private int remoteInvitesSent = 0;

        private List<Person> remoteUpdateQueue = null;

        private Random rand;

        private string lastLogMsg = null;

        private string twitterConsumerKey = "NB74pt5RpC7QuszjGy8qy7rju";
        private string twitterConsumerSecret = "ifP1aw4ggRjkCktFEnU2T0zS2HA0XxlCpzjb601SMRo7U4HoNR";
        private Credentials twitterAccessCredentials = null;
        private OAuthTokens twitterTokens = null;

        private List<TweetsQueue> tweetsQueue = null;
        private List<TweetsQueue> tweetsHistory = null;

        private List<ExceptionReport> exceptions;

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
            restClient = new RestClient("http://birdie.freeddns.org");

            reddit = new Reddit(false);
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
            DateTime start = DateTime.Now;

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

            /* The main InterCom loop.  --Kris */
            int i = 0;
            while (true)
            {
                /* Send a keep-alive to the Birdie API.  --Kris */
                KeepAlive();

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

                /* Update our local count for both active users and total users.  --Kris */
                UpdateRemoteUsers();

                System.Threading.Thread.Sleep(Globals.__INTERCOM_WAIT_INTERVAL__ * 60 * 1000);

                i++;
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

        private List<TweetsQueue> BirdieToTweetsQueue(dynamic deserializedJSON, bool overwrite = true)
        {
            if (overwrite || tweetsQueue == null)
            {
                tweetsQueue = new List<TweetsQueue>();
            }

            foreach (dynamic o in deserializedJSON)
            {
                if (o != null
                    && o["tweet"] != null
                    && o["entered"] != null
                    && o["enteredBy"] != null
                    && o["start"] != null
                    && o["end"] != null)
                {
                    tweetsQueue.Add(new TweetsQueue(o["tweet"].ToString(), "Birdie", DateTime.Now, DateTime.Parse(o["entered"].ToString()),
                        o["enteredBy"].ToString(), DateTime.Parse(o["start"].ToString()), DateTime.Parse(o["end"].ToString()), (o["campaignId"] != null ? o["campaignId"].ToString() : null),
                        (o["tid"] != null ? (int)o["tid"] : 0)));
                }
            }

            return tweetsQueue;
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

        /* Update the local cache of this client's tweets queue.  Just doing a straight-up replace since that'll clean-out any expired/disabled entries.  --Kris */
        private void UpdateLocalTweetsQueue()
        {
            if (!(Globals.Config["EnableTwitter"].Equals("1")))
            {
                return;
            }

            Log("Updating tweets queue....");

            /* Get any tweets in the Birdie API queue.  --Kris */
            IRestResponse res = BirdieQuery(@"/twitter/tweetsQueue?showActiveOnly&showQueueFor=" + GetAppID(), "GET");

            tweetsQueue = BirdieToTweetsQueue(JsonConvert.DeserializeObject(res.Content));

            /* Get any tweets from Reddit, if enabled.  --Kris */
            GetTweetsFromReddit();

            PersistLocalTweetsQueue();
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
        private void AppendTweetsHistory(List<TweetsQueue> entries)
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

            ReportNewTweets(entries);
        }

        private void AppendTweetsHistory(TweetsQueue entry)
        {
            AppendTweetsHistory(new List<TweetsQueue> { entry });
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

        /* Load recent tweets history.  --Kris */
        private List<TweetsQueue> LoadTweetsHistory()
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
            IRestResponse res = BirdieQuery("/clients/" + GetAppID() + "/keepAlive", "PUT");

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
            Log("Commencing Twitter authorization workflow....");

            AuthorizeTwitter(browser);

            Log("Twitter authorization complete!");

            Ready();
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
                DateTime lastUpdate = start;

                Log("Thread execution initialized.");
                
                SetExecState(Globals.STATE_EXECUTING);

                invited = GetInvitedPeople();  // Get list of people you already invited with this program from the system registry.  --Kris

                /* Load the local recent tweets history.  --Kris */
                LoadTweetsHistory();

                //Main.Invoke(new MethodInvoker(delegate() { Main.Refresh(); }));

                // TEST - This will end up going into the loop below.  --Kris
                //GOTV();


                /* Loop until terminated by the user.  --Kris */
                while (Globals.executionState > 0)
                {
                    Log("Beginning workflow....");

                    if (Globals.Config["EnableFacebanking"].Equals("1"))
                    {
                        /* Get-out-the-vote!  --Kris */
                        GOTV();
                    }
                    else
                    {
                        Log("Facebanking has been disabled.  GOTV skipped.");
                    }

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

                    /* Check for updates every 24 hours if auto-update is enabled.  --Kris */
                    if (Globals.Config["AutoUpdate"].Equals("1")
                        && DateTime.Now.Subtract(lastUpdate).TotalDays >= 1)
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

        private void ReportException(Exception ex, string logMsg = null)
        {
            try
            {
                Log("Reporting exception....");

                exceptions.Add(new ExceptionReport(Main, ex, logMsg));
            }
            catch (Exception e)
            {
                Log("Warning:  Error reporting exception : " + e.ToString());
            }
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

        private List<TweetsQueue> GetTweetsFromSubreddit(string sub, int? campaignId = null)
        {
            if (reddit == null)
            {
                reddit = new Reddit(false);
            }

            // No need to login to Reddit since all we're doing is a search.  --Kris
            List<RedditPost> redditPosts = SearchSubredditForFlairPosts("Tweet This!", sub, "all");

            List<TweetsQueue> res = new List<TweetsQueue>();
            foreach (RedditPost redditPost in redditPosts)
            {
                res.Add(new TweetsQueue(ComposeTweet(redditPost.GetTitle(), redditPost.GetURL()), "Reddit", DateTime.Now, redditPost.GetCreated(),
                    @"/u/" + redditPost.GetAuthor(), redditPost.GetCreated(), redditPost.GetCreated().AddDays(3), campaignId));
            }

            return res;
        }

        /* Tweet the next tweet in the queue.  Just do one at a time in order to prevent spam.  --Kris */
        private void ConsumeTweetsQueue(int? campaignId = null)
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
                && tweetsHistory[tweetsHistory.Count - 1].GetTweeted().Value.AddMinutes(r) > DateTime.Now)
            {
                return;  // If it hasn't been TweetIntervalMinutes minutes since the last tweet, don't do it yet.  --Kris
            }

            Log("Consuming tweets queue....");

            if (tweetsQueue != null && tweetsQueue.Count > 0)
            {
                List<TweetsQueue> newTweetsQueue = new List<TweetsQueue>();
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
                        if (Tweet(entry.tweet))  // Perform the actual tweet.  --Kris
                        {
                            AppendTweetsHistory(entry);
                        }
                        else
                        {
                            Log("Warning:  Unable to tweet from queue.  Will try again later.");

                            newTweetsQueue.Add(entry);
                        }

                        tweeted = true;
                    }
                    else
                    {
                        newTweetsQueue.Add(entry);
                    }
                }

                tweetsQueue = newTweetsQueue;
                PersistLocalTweetsQueue();
            }
        }

        /* Search a given sub for today's (default) top posts with a given flair.  Should only queue stuff from Reddit same-day to prevent delayed tweet spam.  --Kris */
        private List<RedditPost> SearchSubredditForFlairPosts(string flair, string sub, string t = "day", bool? self = null)
        {
            if (self == null)
            {
                List<RedditPost> res = new List<RedditPost>();
                res.AddRange(SearchSubredditForFlairPosts(flair, sub, t, false));
                res.AddRange(SearchSubredditForFlairPosts(flair, sub, t, true));

                return res;
            }
            else
            {
                return ParseRedditPosts(reddit.Search.search(null, null, "flair:\"" + flair + "\" self:" + (self.Value ? "yes" : "no"), false, "new", null, t, sub));
            }
        }

        private List<RedditPost> ParseRedditPosts(dynamic redditObj)
        {
            List<RedditPost> res = new List<RedditPost>();

            if (redditObj["data"]["children"] == null || redditObj["data"]["children"].Count == 0)
            {
                return res;  // No results.  --Kris
            }

            foreach (dynamic o in redditObj["data"]["children"])
            {
                if (o != null
                    && o["data"] != null
                    && o["data"]["title"] != null
                    && o["data"]["url"] != null
                    && o["data"]["score"] != null 
                    && o["data"]["created"] != null)
                {
                    try
                    {
                        res.Add(new RedditPost((bool) o["data"]["is_self"], o["data"]["title"].ToString(), o["data"]["url"].ToString(),
                            (int) o["data"]["score"], TimestampToDateTime((double) o["data"]["created"]), o["data"]["author"].ToString(), (string) o["data"]["selftext"]));
                    }
                    catch (Exception e)
                    {
                        Log("Warning:  Error parsing Reddit post : " + e.ToString());

                        ReportException(e, "Error parsing Reddit post.");
                    }
                }
            }

            return res;
        }

        /* Post a tweet.  --Kris */
        private bool Tweet(string tweet)
        {
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
            Log("DEBUG - " + tweet);
            return true;
            TwitterResponse<TwitterStatus> res = TwitterStatus.Update(twitterTokens, tweet);
            if (res.Result == RequestResult.Success)
            {
                Log("Tweeted '" + tweet + "' successfully.");
            }
            else
            {
                Log("ERROR posting tweet '" + tweet + "' : " + res.ErrorMessage);
            }
            
            return (res.Result == RequestResult.Success);
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

        private bool LoadTwitterTokens()
        {
            if (!(TwitterIsAuthorized()))
            {
                return false;
            }

            twitterTokens = new OAuthTokens();
            twitterTokens.ConsumerKey = twitterConsumerKey;
            twitterTokens.ConsumerSecret = twitterConsumerSecret;
            twitterTokens.AccessToken = twitterAccessCredentials.ToString(twitterAccessCredentials.GetTwitterAccessToken());
            twitterTokens.AccessTokenSecret = twitterAccessCredentials.ToString(twitterAccessCredentials.GetTwitterAccessTokenSecret());

            return true;
        }

        private void DestroyTwitterTokens()
        {
            twitterTokens = null;
        }

        private bool TwitterIsAuthorized()
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

        /* Messy check to see if we'll need to open a browser window for GOTV on this iteration.  Will replace later when Facebook has been moved to its own class (TODO).  --Kris */
        private bool GOTVNeeded()
        {
            foreach (KeyValuePair<string, States> state in Globals.StateConfigs.OrderBy(s => s.Value.primaryDate))
            {
                if (Globals.executionState == Globals.STATE_STOPPING || Main.stop)
                {
                    Log("Thread stop received.  Workflow aborted.");
                    return false;
                }

                if (GOTVNeeded(state.Value))
                {
                    return true;
                }
            }

            return false;
        }

        /* Check to see if GOTV should be executed for a given state.  --Kris */
        private bool GOTVNeeded(States state, bool log = false)
        {
            /* Skip if user disabled GOTV for this state.  --Kris */
            if (state.enableGOTV == false)
            {
                Log("GOTV not enabled for " + state.name + ".  Skipped.", log);
            }

            /* Determine if it's time for GOTV in this state.  --Kris */
            // TODO - Yes, I know this logic is overly broad and simplistic.  We can expand upon it and enable per-state configurations later.  --Kris
            RegistryKey softwareKey = Registry.CurrentUser.OpenSubKey("Software", true);
            RegistryKey appKey = softwareKey.CreateSubKey("FaceBERN!");
            RegistryKey GOTVKey = appKey.CreateSubKey("GOTV");
            RegistryKey stateKey = GOTVKey.CreateSubKey(state.abbr);

            string lastGOTVDaysBack = stateKey.GetValue("LastGOTVDaysBack", "", RegistryValueOptions.None).ToString();
            int last = (lastGOTVDaysBack != "" ? Int32.Parse(lastGOTVDaysBack) : -1);

            if (state.primaryDate >= DateTime.Now
                && state.enableGOTV == true 
                && state.primaryDate.Subtract(DateTime.Today).TotalDays >= 0
                && ((last - Math.Floor(state.primaryDate.Subtract(DateTime.Now).TotalDays)) >= state.GOTVWaitInterval) || Globals.devOverride == true)
            {
                return true;
            }

            return false;
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

            if (!(GOTVNeeded()))
            {
                Log("No GOTV is needed at this time.");

                return;
            }

            /* Initialize the Selenium WebDriver.  --Kris */
            webDriver = new WebDriver(Main, browser, (Globals.Config["HideFacebookBrowser"].Equals("1")));

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
            //string[] defaultGOTVDaysBack = Globals.Config["DefaultGOTVDaysBack"].Split(new char[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries);  // DEPRECATED.
            foreach (KeyValuePair<string, States> state in Globals.StateConfigs.OrderBy(s => s.Value.primaryDate))
            {
                if (Globals.executionState == Globals.STATE_STOPPING || Main.stop)
                {
                    Log("Thread stop received.  Workflow aborted.");
                    return;
                }

                if (GOTVNeeded(state.Value, true))
                {
                    // DEBUG - Uncomment below if you'd like to force-test a single state.  --Kris
                    /*
                    if (!(state.Key.Equals("NM")))
                    {
                        continue;
                    }
                    */

                    Log("Checking GOTV for " + state.Value.name + "....");

                    if (state.Value.FTBEventId == null && !(Globals.Config["UseFTBEvents"].Equals("1")))
                    {
                        Log("There is no feelthebern.events event on record for " + state.Key + ".  Skipped.");

                        continue;
                    }

                    RegistryKey stateKey = GOTVKey.CreateSubKey(state.Key);

                    SetProgressBar(Globals.PROGRESSBAR_MARQUEE);

                    /* Retrieve friends of friends who like Bernie Sanders and live in this state.  --Kris */
                    //GetFacebookBernieSupporters("124955570892789");  // DEBUG
                    List<Person> friends = GetFacebookFriendsOfFriends(state.Key);

                    // TODO - Add direct friends who like Bernie Sanders and live in this state.  --Kris

                    if (friends == null || friends.Count == 0)
                    {
                        Log("You have no friends or friends of friends in " + state.Key + " who like Bernie Sanders.  Skipped.");
                    }
                    else
                    {
                        ExecuteGOTV(ref friends, ref stateKey, state.Value);
                    }

                    SetProgressBar(Globals.PROGRESSBAR_HIDDEN);

                    stateKey.Close();

                    break;
                }
                else
                {
                    Log("No GOTV needed for " + state.Key + " at this time.");
                }
            }

            try
            {
                GOTVKey.SetValue("LastCheck", DateTime.Now.Ticks.ToString(), RegistryValueKind.String);
            }
            catch (IOException e)
            {
                Log("Warning:  Error updating last GOTV check : " + e.Message);

                ReportException(e, "Error updating last GOTV check.");
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

        private void ExecuteGOTV(ref List<Person> friends, ref RegistryKey stateKey, States state)
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
                        InviteToEventSidebar(ref friends, state.abbr);
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

                ReportException(e, "Error updating last GOTV for state : " + state.name);
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
                Credentials credentials = new Credentials(true);

                SecureString u = credentials.GetFacebookUsername();  // Load encrypted username if stored in registry.  --Kris
                SecureString p = credentials.GetFacebookPassword();  // Load encrypted password if stored in registry.  --Kris
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

                    Thread.Sleep(3);  // Give the state a chance to unready itself.  Better safe than sorry.  --Kris
                    webDriver.WaitForPageLoad();

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
                    profileURL = webDriver.GetAttribute(ele, "href");
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

                            ReportException(e, "Error storing Facebook profile URL.");
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

                    ReportException(e, "Unable to create event for state '" + stateAbbr + "' after retries.");

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

                string invitedJSON = (string) GOTVKey.GetValue("invitedJSON", null);
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

                    ReportException(e, "Error storing updated invitations record.");
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

                System.Threading.Thread.Sleep(250);

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
            if (searchBox == null)
            {
                return null;
            }

            try
            {
                webDriver.TypeText(searchBox, OpenQA.Selenium.Keys.Control + "a");
                webDriver.TypeText(searchBox, friend.getName());
            }
            catch (Exception e)
            {
                // Whatever the problem is, just refresh and try again.  StaleElementReferenceExceptions are handled in the WebDriver class.  --Kris
                return null;
            }

            System.Threading.Thread.Sleep(250 * delayMultiplier);

            /* This is NOT intended as a spam tool.  These delays are necessary to keep Facebook's automated spam checks from throwing a false positive and blocking the user.  --Kris */
            int i = 3;
            List<IWebElement> res = new List<IWebElement>();
            do
            {
                System.Threading.Thread.Sleep(250 * delayMultiplier);

                res = webDriver.GetElementsByTagNameAndAttribute("li", "aria-label", friend.getName());

                i--;
            } while ((res == null || res.Count == 0) && i > 0);

            return res;
        }

        /* Must already be on the event page with the invite sidebar!  --Kris */
        private void InviteToEventSidebar(ref List<Person> friends, string stateAbbr = null)
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
            int ratelimitCount = 0;
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

                    int r = 3;
                    List<IWebElement> res;
                    do
                    {
                        res = LoadFriendInEventSearchBox(friend, searchBox);
                        if (res == null)
                        {
                            webDriver.Refresh();
                            System.Threading.Thread.Sleep(3000);
                            searchBox = webDriver.GetElementByTagNameAndAttribute("input", "aria-label", "Add friends to this event");
                        }

                        r--;
                    } while (res == null && r > 0);

                    if (res == null || res.Count == 0)
                    {
                        /* Retry once; a bit more slowly, this time.  Sometimes it just doesn't load correctly or quickly enough in the browser.  --Kris */
                        //res = LoadFriendInEventSearchBox(friend, searchBox, 2);  // This doesn't seem to ever help.  Just makes it take forever.  --Kris
                    }

                    if (res == null || res.Count == 0)
                    {
                        Log("Unable to add " + friend.getName() + " to invite list.");
                    }
                    else if (res.Count == 1)
                    {
                        if (webDriver.GetAttribute(res[0], "class") != null && webDriver.GetAttribute(res[0], "class").Contains("nonInvitable"))
                        {
                            Log("Facebook user " + friend.getName() + " has already been invited.  Skipped.");

                            System.Threading.Thread.Sleep(250);
                        }
                        else
                        {
                            webDriver.ClickElement(res[0]);

                            IWebElement ele;
                            int ii = 10;
                            do
                            {
                                System.Threading.Thread.Sleep(250);

                                ele = webDriver.GetElementById("event_invite_feedback");

                                ii--;
                            } while (ele == null && ii > 0);

                            /* To prevent stale element exceptions.  --Kris */
                            bool stale = false;
                            string msg = null;
                            try
                            {
                                msg = webDriver.GetText(ele);
                            }
                            catch (StaleElementReferenceException e)
                            {
                                stale = true;
                            }

                            if (stale)
                            {
                                System.Threading.Thread.Sleep(500);

                                ele = webDriver.GetElementById("event_invite_feedback");

                                stale = false;
                                try
                                {
                                    msg = webDriver.GetText(ele);
                                }
                                catch (StaleElementReferenceException e)
                                {
                                    Log("Unable to determine whether " + friend.getName() + " was added to invite list.");

                                    stale = true;
                                }
                            }

                            if (!stale)
                            {
                                try
                                {
                                    if (ele != null && webDriver.GetText(ele) == null && webDriver.GetAttribute(ele, "innerHTML") != null)
                                    {
                                        msg = webDriver.GetAttribute(ele, "innerHTML");
                                    }
                                }
                                catch (StaleElementReferenceException e)
                                {
                                    ele = null;
                                }

                                // Facebook keeps switching-up the attribute on us, so we'll just assume it's good for now if the message can't be found (i.e. is null).  --Kris
                                if (ele != null && (msg == null || msg.Contains(" was invited.")))
                                {
                                    Log("Added " + friend.getName() + " to invite list.");

                                    i++;
                                    ratelimitCount++;

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
                    }
                    else
                    {
                        Log("Multiple people with the same name found.  The feature to handle that hasn't been written yet.  Skipped.");
                    }

                    /* We don't want to trigger any false positives from the spam algos.  --Kris */
                    if (ratelimitCount % 1000 == 0 && ratelimitCount > 1)
                    {
                        Wait(15, "for 1000-interval ratelimit");
                        ratelimitCount++;
                    }
                    if (ratelimitCount % 100 == 0 && ratelimitCount > 1)
                    {
                        Wait(3, "for 100-interval ratelimit");
                        ratelimitCount++;
                    }
                    else if (ratelimitCount % 50 == 0 && ratelimitCount > 1)
                    {
                        Wait(2, "for 50-interval ratelimit");
                        ratelimitCount++;
                    }
                    else if (ratelimitCount % 25 == 0 && ratelimitCount > 1)
                    {
                        Wait(1, "for 25-interval ratelimit");
                        ratelimitCount++;
                    }
                    else if (ratelimitCount % 5 == 0 && ratelimitCount > 1)
                    {
                        Wait(3, "for 5-interval ratelimit", "second");
                        ratelimitCount++;
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

                ReportException(e, "Error storing updated invitations record.");
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

        /* Graph search disappeared but I'm not about to be deterred.  Game on, motherfuckers.  --Kris */
        private Dictionary<string, List<Person>> GetFacebookBernieSupporters(string bernieFacebookId = null)
        {
            Dictionary<string, List<Person>> res = new Dictionary<string, List<Person>>();  // People without known states are listed under "GA" (Earth); will use "US" if we know they're in this country.  --Kris

            if (bernieFacebookId == null)
            {
                // TODO - Cycle through both Bernie Facebook IDs simultaneously; i.e. open each set in a separate window via a separate thread, then join them together when done.  --Kris
                Log("Multi-ID search not yet implemented.  Search cancelled.");

                return res;
            }

            Log("Searching for Bernie Sanders supporters and gathering results (this will take a long time)....");

            SetProgressBar(Globals.PROGRESSBAR_MARQUEE);

            string URL = "https://www.facebook.com/search";

            URL += "/" + bernieFacebookId + "/likers";

            /* Navigate to the search page.  --Kris */
            webDriver.GoToUrl(URL);

            /* Keep scrolling to the bottom until all results have been loaded.  --Kris */
            webDriver.ScrollToBottom("Preparing to load results....", "Loading results (set $i/$L max sets)....", "Finished loading results!", 100000);  // Each "set" is a scroll.  --Kris

            /* Scrape the results from the page source.  Seems to average just under a second for processing each result set.  --Kris */
            string[] resRaw = new string[999999];
            resRaw = webDriver.GetPageSource(browser).Split(new string[] { "<div class=\"_glj\">" }, StringSplitOptions.RemoveEmptyEntries);

            Log("Parsing search results....  This will probably take awhile....");

            SetProgressBar(Globals.PROGRESSBAR_CONTINUOUS);

            int increment = 10;
            if (resRaw.Length >= 500)
            {
                increment = 100;
            }

            int i = 0;
            int r = 0;
            foreach (string entry in resRaw)
            {
                if (i == 0)
                {
                    i++;
                    continue;
                }

                SetProgressBar((int) Math.Round((decimal) (((double) i / resRaw.Length) * 100), 0, MidpointRounding.AwayFromZero));

                if (i % increment == 0)
                {
                    Log("Processed " + i.ToString() + " Facebook search results....");
                    System.Threading.Thread.Sleep(100);
                }

                i++;
                
                // TODO - Find a more comprehensive HTML parsing library (NOT RegEx!) when I have more time.  This sloppy solution will work, for now.  --Kris
                string livesInLine = null;
                string nameLine = null;
                foreach (string subEntry in entry.Split(new string[] { "</a></div></div>" }, StringSplitOptions.RemoveEmptyEntries))
                {
                    if (subEntry.IndexOf("Lives in") != -1)
                    {
                        livesInLine = subEntry;
                    }
                    else if (subEntry.IndexOf("https://www.facebook.com/") != -1 
                        && nameLine == null)
                    {
                        nameLine = subEntry;
                    }
                }

                if (nameLine == null)
                {
                    continue;
                }

                /* Parse the Facebook User ID.  --Kris */
                string link = nameLine.Substring((nameLine.IndexOf("https://www.facebook.com/") + "https://www.facebook.com/".Length));

                string userId = link.Substring(0, link.IndexOf(@">"));
                if (userId == null || userId.ToLower().Equals("profile.php"))
                {
                    continue;
                }
                
                string name = nameLine.Substring((nameLine.IndexOf(userId) + userId.Length));

                if (userId.IndexOf(@"?") != -1)
                {
                    userId = userId.Substring(0, userId.IndexOf(@"?"));
                }

                /* Parse the person's name.  --Kris */
                name = Regex.Replace(name.Substring(name.IndexOf("<div")), @"<[^>]+>", "").Trim();  // Fuck it, it's gotta be RegEx for this part, at least.  --Kris

                /* Parse the person's state, if specified.  --Kris */
                string livesIn = null;
                if (livesInLine != null)
                {
                    livesIn = Regex.Replace(livesInLine, @"<[^>]+>", "").Trim();

                    livesIn = livesIn.Substring(livesIn.LastIndexOf(" ") + 1).Trim();
                }

                /* Attempt to match the state with one of our State entries.  --Kris */
                string stateAbbr = "GA";  // Code I made-up for Earth since ISO doesn't seem to have an one (EA is taken already).  Not all search results are U.S.-based.  --Kris
                if (livesIn != null)
                {
                    foreach (KeyValuePair<string, States> state in Globals.StateConfigs)
                    {
                        if (state.Value.name.ToLower().Equals(livesIn.ToLower()))
                        {
                            stateAbbr = state.Key;
                            break;
                        }
                    }
                }

                /* Append the result set.  --Kris */
                Person person = new Person();
                person.setBernieSupporter(true);
                person.setFacebookID(userId);
                person.setName(name);
                person.setStateAbbr(stateAbbr);

                if (!(res.ContainsKey(stateAbbr)))
                {
                    res.Add(stateAbbr, new List<Person>());
                }
                res[stateAbbr].Add(person);

                r++;
            }

            Log("Search processing complete!  Retrieved " + r.ToString() + " Facebook users who like Bernie Sanders.");

            SetProgressBar(100);

            System.Threading.Thread.Sleep(5000);

            SetProgressBar(Globals.PROGRESSBAR_HIDDEN);

            return res;
        }

        private List<Person> GetFacebookFriendsOfFriends(string stateAbbr = null, bool bernieSupportersOnly = true, int pass = 1)
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

            // Temporary override URL; the intersect one has stopped working.  --Kris
            /*
            URL = @"https://www.facebook.com/search/people/?q=friends%20of%20my%20friends%20who%20live%20in%20";
            URL += Globals.StateConfigs[stateAbbr].name.ToLower();
            URL += @"%20and%20like%20bernie%20sanders";
             * */
            // End override.  --Kris

            Log(logBaseMsg + logMsg + "....");

            /* Navigate to the search page.  --Kris */
            webDriver.GoToUrl(URL);

            /* Keep scrolling to the bottom until all results have been loaded.  --Kris */
            webDriver.ScrollToBottom("Preparing to load results....", "Loading results (set $i/$L max sets)....", "Finished loading results!");  // Each "set" is a scroll.  --Kris

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

            if (res.Count < Globals.StateConfigs[stateAbbr].facebookFriendSearchTarget
                && pass < Globals.StateConfigs[stateAbbr].facebookFriendSearchPasses)
            {
                Log("Results target not met.  Re-performing search to supplement results (pass " + (pass + 1).ToString() + @" / " + Globals.StateConfigs[stateAbbr].facebookFriendSearchPasses + ")....");

                res.AddRange(GetFacebookFriendsOfFriends(stateAbbr, bernieSupportersOnly, (pass + 1)));
                //res = res.Distinct<Person>().ToList<Person>();

                List<Person> resClean = new List<Person>();
                foreach (Person person in res)
                {
                    bool dup = false;
                    foreach (Person person2 in resClean)
                    {
                        if (person2.facebookID.Equals(person.facebookID))
                        {
                            dup = true;
                            break;
                        }
                    }

                    if (!dup)
                    {
                        resClean.Add(person);
                    }
                }
                res = resClean;
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

            if (Globals.requestedFTBInvite == true)
            {
                Log("FTB invitation already requested.  Skipped.");
                return;
            }

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

                WebDriver webDriver2 = new WebDriver(Main, browser);

                webDriver2.FixtureSetup();
                webDriver2.TestSetUp("http://www.feelthebern.events");

                webDriver2.TypeInXPath(@"/html/body/div[2]/input", profileURL);
                webDriver2.ClickOnXPath(@"/html/body/div[2]/button");

                System.Threading.Thread.Sleep(5000);

                Log("Request sent.");

                webDriver2.FixtureTearDown();

                appKey.SetValue("requestedFTBInvite", "1", RegistryValueKind.String);
            }
            else
            {
                SetExecState(Globals.STATE_ERROR);

                Log("Unable to request feelthebern.events invitation due to unknown profile URL!");
            }

            facebookKey.Close();
            appKey.Close();
            softwareKey.Close();

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

        private void Log(string text, bool show = true, bool appendW = true, bool newline = true, bool timestamp = true, bool suppressDups = true)
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

                    lastLogMsg = text;

                    Main.LogW(text, show, appendW, newline, timestamp, logName, WorkflowLog);

                    Main.Refresh();
                }
            }
            catch (Exception e)
            {
                ReportException(e, "Exception raised in Workflow Log method.");
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
