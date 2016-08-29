using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FaceBERN_
{
    public static class Globals
    {
        /*
         * -- BEGIN GLOBAL SETTINGS --
         */

        /* The version.  Please adhere to Git versioning procedures.  --Kris */
        public static string __VERSION__        = @"1.0.0.b";

        /* The name of the application.  --Kris */
        public static string __APPNAME__        = @"Birdie";
        //public static string __APPNAME__      = @"FaceBERN!";  // Deprecated.  --Kris

        /* File paths.  --Kris */
        public static string ConfigDir          = @"config";
        public static string MainINI            = __APPNAME__ + @".ini";
        public static string CampaignsINI       = @"campaigns.ini";
        public static string StatesINIDir       = @"states";

        /* The Birdie API protocol/host.  --Kris */
        public static string BirdieHost         = @"http://birdie.freeddns.org";

        /* How long to wait for an action element to appear before dying.  --Kris */
        public static int __TIMEOUT__ = 3;  // Seconds

        /* How long to wait between each browser iteration.  --Kris */
        public static int __BROWSE_DELAY__ = 2;  // Seconds

        /* How long to wait between checks for having received the feelthebern.events friend request.  --Kris */
        public static int __FTB_REQUEST_ACCESS_WAIT_INTERVAL__ = 60;  // Minutes

        /* How long to wait between each iteration of the main workflow loop.  --Kris */
        public static int __WORKFLOW_WAIT_INTERVAL__ = 60;  // Minutes

        /* How long to wait between each iteration of the Facebook workflow loop.  --Kris */
        public static int __WORKFLOW_FACEBOOK_WAIT_INTERVAL__ = 60;  // Minutes

        /* How long to wait between each iteration of the Twitter workflow loop.  --Kris */
        public static int __WORKFLOW_TWITTER_WAIT_INTERVAL__ = 5;  // Minutes

        /* How long to wait between each iteration of the main InterCom loop.  --Kris */
        public static int __INTERCOM_WAIT_INTERVAL__ = 4;  // Minutes

        /* How old the Twitter user timeline cache can get before it needs to be refreshed.  --Kris */
        public static int __TWITTER_TIMELINE_CACHE_SHELF_LIFE__ = 2;  // Minutes

        /*
         * -- END GLOBAL SETTINGS --
         */

        /* Global containers.  --Kris */
        public static Dictionary<string, string> Config;
        public static Process process = null;
        public static Thread thread = null;
        public static Thread facebookThread = null;
        public static Thread twitterThread = null;
        public static int executionState = -2;
        public static List<string> bernieFacebookIDs;
        public static bool devOverride = false;  // Enable by holding down the Shift key while clicking Start.  This will force GOTV for all states, regardless of dates or recent prior checks.  --Kris
        public static bool requestedFTBInvite = false;
        public static List<Campaign> campaigns = null;

        /* Global state configs container.  Individual states setup in Form1.SetStateDefaults.  --Kris */
        public static Dictionary<string, States> StateConfigs;

        /* Global campaign configs container used to persist local settings.  The userSelected property corresponds to whether or not the user checked the box for a given campaign.  --Kris */
        public static Dictionary<int, bool> CampaignConfigs = null;

        /* Global singletons.  --Kris */
        public static INI sINI;
        public static Log MainLog;
        public static Log WorkflowLog;

        /* Execution states (any state > 0 means that the Workflow thread is running).  --Kris */
        public const int STATE_INITIALIZING = -3;  // Default state.
        public const int STATE_ERROR = -2;  // A recoverable error has occurred.
        public const int STATE_BROKEN = -1;  // An unrecoverable error has occurred.
        public const int STATE_READY = 0;  // Application is running but execution is either paused or not started yet (essentially the same thing).
        public const int STATE_VALIDATING = 1;  // Running sanity checks prior to a state change.
        public const int STATE_WAITING = 2;  // Sitting idle until some action needs to be taken (then switches to executing state).
        public const int STATE_SLEEPING = 3;  // Sitting idle because the end-user restricted execution to another timeframe (then switches to waiting state).
        public const int STATE_EXECUTING = 4;  // Doing the actual work.
        public const int STATE_STOPPING = 5;  // Stop button has been clicked.
        public const int STATE_RESTARTING = 6;  // Workflow thread is being re-created, most likely due to an error.
        public const int STATE_TWITTERPIN = 7;  // User is associating or de-associating their Twitter account (via Tools->Settings or post-install setup).

        /* Browser constants.  --Kris */
        public const int FIREFOX_HEADLESS = -1;  // Open a headless browser instance using NHtmlUnit.  Not working with Facebook.
        public const int NO_BROWSER = 0;  // User has not selected a web browser.
        public const int CHROME = 1;  // Open in a Chrome browser window.
        public const int FIREFOX = 2;  // Open in a Firefox browser window.
        public const int IE = 3;  // Open in an Internet Explorer window.
        public const int EDGE = 4;  // Open in an Edge browser window.
        public const int OPERA = 5;  // Open in an Opera browser window.
        public const int PHANTOMJS = 6;  // In case someone ever writes a driver that actually works in .NET.

        /* Progress bar function constants.  --Kris */
        public const int PROGRESSBAR_HIDDEN = -2;
        public const int PROGRESSBAR_MARQUEE = -1;
        public const int PROGRESSBAR_CONTINUOUS = 0;

        /* Campaign ID constants.  --Kris */
        public const int CAMPAIGN_RUNBERNIERUN = 1;
        public const int CAMPAIGN_TWEET_STILLSANDERSFORPRES = 2;
        public const int CAMPAIGN_TWEET_SANDERSFORPRESIDENT = 3;
        public const int CAMPAIGN_TWEET_POLITICALREVOLUTION = 4;
        public const int CAMPAIGN_MANAGE_FACEBOOK_GROUPS    = 5;
        public const int CAMPAIGN_OPERATION_LIFE_PRESERVER  = 6;
        public const int CAMPAIGN_OLP_PHASE1                = 7;
        public const int CAMPAIGN_OLP_PHASE2                = 8;

        /* Application ID.  Used for API calls.  It should be set to null here.  --Kris */
        public static string appId = null;

        /* Nothing says "nonsequitur" quite like "salt".  --Kris */
        public static Random rand = new Random();

        /* Get a campaign by ID.  --Kris */
        public static Campaign GetCampaignById(int campaignId)
        {
            foreach (Campaign campaign in campaigns)
            {
                if (campaign.campaignId == campaignId)
                {
                    return campaign;
                }
            }

            return null;
        }

        /* Set a campaign.  --Kris */
        public static void SetCampaign(Campaign setCampaign)
        {
            List<Campaign> newCampaigns = new List<Campaign>();
            bool found = false;
            foreach (Campaign campaign in campaigns)
            {
                if (campaign.campaignId == setCampaign.campaignId)
                {
                    newCampaigns.Add(setCampaign);
                    found = true;
                }
                else
                {
                    newCampaigns.Add(campaign);
                }
            }

            if (found == false)
            {
                newCampaigns.Add(setCampaign);
            }

            campaigns = newCampaigns;
        }

        /* Map log names to their respective objects (DEPRECATED).  --Kris */
        public static Log getLogObj(string logName)
        {
            Log log;
            switch (logName)
            {
                default:
                    return null;
                case "FaceBERN!":
                    log = MainLog;
                    break;
                case "Workflow":
                    log = WorkflowLog;
                    break;
            }

            /*if (log == null)
            {
                log = new Log();
                log.Init(logName);
            }*/

            return log;
        }

        /* Campaign ID to logged name mappings.  --Kris */
        public static string[] CampaignNames()
        {
            string[] names = new string[32767];

            names[CAMPAIGN_RUNBERNIERUN] = @"#RunBernieRun";
            names[CAMPAIGN_TWEET_POLITICALREVOLUTION] = @"Tweet from /r/Political_Revolution";
            names[CAMPAIGN_TWEET_SANDERSFORPRESIDENT] = @"Tweet from /r/SandersForPresident";
            names[CAMPAIGN_TWEET_STILLSANDERSFORPRES] = @"#RunBernieRun::Tweet from /r/StillSandersForPres";

            return names;
        }

        /* Get the string logged name for a given campaign constant.  --Kris */
        public static string CampaignName(int? campaignId)
        {
            if (campaignId == null)
            {
                return null;
            }
            else
            {
                string[] names = CampaignNames();

                return names[campaignId.Value];
            }
        }

        /* List browser names indexed by constant.  --Kris */
        public static string[] BrowserNames()
        {
            string[] names = new string[99];

            names[FIREFOX] = "Firefox";
            names[IE] = "Internet Explorer";
            names[CHROME] = "Chrome";
            names[EDGE] = "Edge";
            names[OPERA] = "Opera";
            names[PHANTOMJS] = "PhantomJS";
            names[NO_BROWSER] = "(none)";

            return names;
        }

        /* Get the string browser name for a given constant.  --Kris */
        public static string BrowserName(int browser)
        {
            string[] names = new string[99];

            names = BrowserNames();

            return names[browser];
        }

        /* List browser names as they appear in Windows.  --Kris */
        public static string[] BrowserPIDNames()
        {
            string[] names = new string[99];

            names[FIREFOX] = "Mozilla Firefox";
            names[IE] = "Windows Internet Explorer";
            names[CHROME] = "Google Chrome";
            names[EDGE] = "Microsoft Edge";  // TODO - Verify.
            names[OPERA] = "Opera";  // TODO - Verify.
            names[PHANTOMJS] = "PhantomJS";  // TODO - Verify.
            names[NO_BROWSER] = null;

            return names;
        }

        /* Get the string browser name (window title).  --Kris */
        public static string BrowserPIDName(int browser)
        {
            string[] names = new string[99];

            names = BrowserPIDNames();

            return names[browser];
        }

        /* List browser constants indexed by string.  --Kris */
        public static Dictionary<string, Int32> BrowserConsts()
        {
            Dictionary<string, Int32> consts = new Dictionary<string, Int32>();

            consts["firefox"] = FIREFOX;
            consts["ie"] = IE;
            consts["internet explorer"] = IE;
            consts["chrome"] = CHROME;
            consts["edge"] = EDGE;
            consts["opera"] = OPERA;
            consts["phantomjs"] = PHANTOMJS;

            return consts;
        }

        /* Get the integer browser constant for a given string.  --Kris */
        public static int BrowserConst(string browsername)
        {
            Dictionary<string, Int32> consts = new Dictionary<string, Int32>();

            consts = BrowserConsts();

            return consts[browsername.ToLower()];
        }

    }
}
