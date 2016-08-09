using csReddit;
using Microsoft.Win32;
using Newtonsoft.Json;
using OpenQA.Selenium;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Twitterizer;

namespace FaceBERN_
{
    public class WorkflowTwitter : Workflow
    {
        private string logName = "WorkflowTwitter";
        public Log WorkflowTwitterLog;

        private string lastLogMsg = null;

        private List<ExceptionReport> exceptions;

        private Form1 Main;

        public WorkflowTwitter(Form1 Main)
            : base(Main)
        {
            this.Main = Main;
        }

        public void ExecuteTwitter(int browser)
        {
            try
            {
                Log("Commencing Twitter workflow....");

                if (!(Globals.Config["EnableTwitter"].Equals("1")))
                {
                    Log("Twitter is disabled.  Twitter workflow aborted.");
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

                /* Load the local recent tweets history.  --Kris */
                LoadTweetsHistory();

                /* Loop until terminated by the user.  --Kris */
                while (Globals.executionState > 0)
                {
                    /* Update the tweets queue.  --Kris */
                    UpdateLocalTweetsQueue();

                    /* Iterate through the campaigns and execute.  Each campaign will contain its own sanity checks (including checking if it's enabled).  --Kris */
                    IEnumerable<Type> types = Assembly.GetExecutingAssembly().GetTypes().Where(t => t != null && t.Namespace != null && t.Namespace.StartsWith("FaceBERN_.Campaigns"));
                    foreach (Type t in types)
                    {
                        if (t.Name.Equals("Generic"))
                        {
                            continue;
                        }
                        
                        Log("Executing Twitter for campaign:  " + t.Name);

                        try
                        {
                            dynamic instance = Activator.CreateInstance(t, null, this, true);
                            instance.ExecuteTwitter();
                        }
                        catch (Exception e)
                        {
                            ReportException(e, "Unable to execute campaign!  Skipped.");
                        }
                    }

                    /* Wait between loops.  May lower it later if we start doing more time-sensitive crap like notifications/etc.  --Kris */
                    Log("Twitter workflow complete!  Waiting " + Globals.__WORKFLOW_TWITTER_WAIT_INTERVAL__.ToString() + " minutes for next run....");

                    System.Threading.Thread.Sleep(Globals.__WORKFLOW_TWITTER_WAIT_INTERVAL__ * 60 * 1000);
                }
            }
            catch (ThreadAbortException)
            {
                Log("Twitter thread execution aborted.");

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
                    Log("ERROR:  Unhandled Exception in Twitter thread : " + e.ToString());

                    SetExecState(Globals.STATE_ERROR);

                    ReportException(e, "Unhandled Exception in Twitter workflow.");

                    Log("Aborting broken Twitter workflow....");

                    if (webDriver != null)
                    {
                        webDriver.FixtureTearDown();
                        webDriver = null;
                    }

                    System.Threading.Thread.Sleep(3000);

                    if (Globals.executionState != Globals.STATE_BROKEN)
                    {
                        System.Threading.Thread.Sleep(5000);

                        Log("Restarting Twitter thread....");

                        SetExecState(Globals.STATE_RESTARTING);

                        Globals.twitterThread = ExecuteTwitterThread(browser);
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

                        if (Globals.twitterThread != null)
                        {
                            Globals.twitterThread.Abort();
                        }
                        while (Globals.twitterThread.IsAlive) { }

                        Log("Broken workflow detected!  Workflow is now disabled for safety reasons.  Please restart " + Globals.__APPNAME__ + " to recover from the error.");
                    }
                    catch (Exception ex2)
                    {
                        return;
                    }
                }
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

                localTweetsQueue = JsonConvert.DeserializeObject<List<TweetsQueue>>((string)twitterKey.GetValue("tweetsQueue", new List<TweetsQueue>()));

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
                for (int i = 0; i < tweetsQueue.Count; i++)
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
                            o["enteredBy"].ToString(), DateTime.Parse(o["start"].ToString()), DateTime.Parse(o["end"].ToString()), (int?)(o["campaignId"] != null ? o["campaignId"] : null),
                            (o["tid"] != null ? (int)o["tid"] : 0), (o["tweetedAt"] != null ? DateTime.Parse(o["tweetedAt"].ToString()) : null), (string)o["twitterStatusId"]));
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
                    (string)twitterKey.GetValue("tweetsHistory", JsonConvert.SerializeObject(new List<TweetsQueue>()), RegistryValueOptions.None)
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
        internal void ConsumeTweetsQueue(int? campaignId = null)
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
                                    res.Add(new RedditPost((bool)o["data"]["is_self"], o["data"]["title"].ToString(), o["data"]["subreddit"].ToString(), o["data"]["url"].ToString(),
                                        o["data"]["permalink"].ToString(), (int)o["data"]["score"], TimestampToDateTime((double)o["data"]["created"]), o["data"]["author"].ToString(),
                                        (string)o["data"]["selftext"], campaignId));
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

        /* Update the number of tweets in the queue displayed on the main form.  --Kris */
        internal void UpdateLocalTweetsQueueCount()
        {
            UpdateLocalTweetsQueue(true);
            GetTweetsQueue();

            UpdateTweetsQueuedCount(tweetsQueue.Count);
        }

        /* Get the number of tweets tweeted by everyone.  --Kris */
        internal void UpdateRemoteTweetsCount()
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
                Main.SetExecState(state, logName, WorkflowTwitterLog);
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

        private void LogAndReportException(Exception e, string text, bool show = true)
        {
            Log(text, show);
            ReportException(e, text);
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
                Log("Twitter execution terminated successfully.");

                WorkflowTwitterLog.Save();

                Main.buttonStart_ToStart();
                Main.Ready(logName);

                Main.Refresh();
            }
        }
    }
}
