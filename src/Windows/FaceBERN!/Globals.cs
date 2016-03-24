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
        public static string __VERSION__        = @"1.0.0.a";

        /* File paths.  --Kris */
        public static string ConfigDir          = @"config";
        public static string MainINI            = @"FaceBERN!.ini";
        public static string StatesINIDir       = @"states";

        /* How long to wait for an action element to appear before dying.  --Kris */
        public static int __TIMEOUT__ = 3;  // Seconds

        /*
         * -- END GLOBAL SETTINGS --
         */

        /* Global containers.  --Kris */
        public static Dictionary<string, string> Config;
        public static Process process = null;
        public static Thread thread = null;
        public static int executionState = -2;

        /* Global singletons.  --Kris */
        public static INI sINI;
        public static Log MainLog;

        /* Execution states.  --Kris */
        public const int STATE_INITIALIZING = -2;
        public const int STATE_BROKEN = -1;
        public const int STATE_READY = 0;
        public const int STATE_VALIDATING = 1;
        public const int STATE_WAITING = 2;
        public const int STATE_EXECUTING = 3;

        /* Bitwise constants for browser usage.  --Kris */
        public const int FIREFOX = 2;
        public const int IE = 4;
        public const int CHROME = 8;
        public const int SIMPLE = 16;

        /* Nothing says "nonsequitur" quite like "salt".  --Kris */
        public static Random rand = new Random();

        /* List browser names indexed by constant.  --Kris */
        public static string[] BrowserNames()
        {
            string[] names = new string[99];

            names[FIREFOX] = "Firefox";
            names[IE] = "Internet Explorer";
            names[CHROME] = "Chrome";
            names[SIMPLE] = "SimpleBrowser";

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
            names[SIMPLE] = "SimpleBrowser";

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
            consts["simplebrowser"] = SIMPLE;

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
