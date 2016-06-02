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

        /* File paths.  --Kris */
        public static string ConfigDir          = @"config";
        public static string MainINI            = @"FaceBERN!.ini";
        public static string StatesINIDir       = @"states";

        /* How long to wait for an action element to appear before dying.  --Kris */
        public static int __TIMEOUT__ = 3;  // Seconds

        /* How long to wait between each browser iteration.  --Kris */
        public static int __BROWSE_DELAY__ = 2;  // Seconds

        /* How long to wait between checks for having received the feelthebern.events friend request.  --Kris */
        public static int __FTB_REQUEST_ACCESS_WAIT_INTERVAL__ = 60;  // Minutes

        /* How long to wait between each iteration of the main workflow loop.  --Kris */
        public static int __WORKFLOW_WAIT_INTERVAL__ = 5;  // Minutes

        /* How long to wait between each iteration fo the main InterCom loop.  --Kris */
        public static int __INTERCOM_WAIT_INTERVAL__ = 5;  // Minutes

        /*
         * -- END GLOBAL SETTINGS --
         */

        /* Global containers.  --Kris */
        public static Dictionary<string, string> Config;
        public static Process process = null;
        public static Thread thread = null;
        public static int executionState = -2;
        public static List<string> bernieFacebookIDs;
        public static bool devOverride = false;  // Enable by holding down the Shift key while clicking Start.  This will force GOTV for all states, regardless of dates or recent prior checks.  --Kris

        /* Global state configs container.  Individual states setup in Form1.SetStateDefaults.  --Kris */
        public static Dictionary<string, States> StateConfigs;

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

        /* Browser constants.  --Kris */
        public const int FIREFOX_HEADLESS = -1;  // Open a headless browser instance using NHtmlUnit.  Not working with Facebook.
        public const int FIREFOX_HIDDEN = 0;  // Hide using AutoIt.
        public const int FIREFOX_WINDOWED = 1;  // Open in a visible, maximized Firefox browser window.
        public const int CHROME = 2;  // In case we ever decide to support Chrome in the future.
        public const int IE = 3;  // Doubtful, but what the hell.

        /* Progress bar function constants.  --Kris */
        public const int PROGRESSBAR_HIDDEN = -2;
        public const int PROGRESSBAR_MARQUEE = -1;
        public const int PROGRESSBAR_CONTINUOUS = 0;

        /* Application ID.  Used for API calls.  It should be set to null here.  --Kris */
        public static string appId = null;

        /* Nothing says "nonsequitur" quite like "salt".  --Kris */
        public static Random rand = new Random();

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

        /* List browser names indexed by constant.  --Kris */
        public static string[] BrowserNames()
        {
            string[] names = new string[99];

            names[FIREFOX_WINDOWED] = "Firefox";
            names[IE] = "Internet Explorer";
            names[CHROME] = "Chrome";
            names[FIREFOX_HEADLESS] = "Awesomium";

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

            names[FIREFOX_WINDOWED] = "Mozilla Firefox";
            names[IE] = "Windows Internet Explorer";
            names[CHROME] = "Google Chrome";
            //names[FIREFOX_HEADLESS] = "Mozilla Firefox";
            names[FIREFOX_HIDDEN] = "Mozilla Firefox";

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

            consts["firefox"] = FIREFOX_WINDOWED;
            consts["ie"] = IE;
            consts["internet explorer"] = IE;
            consts["chrome"] = CHROME;
            consts["awesomium"] = FIREFOX_HEADLESS;

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
