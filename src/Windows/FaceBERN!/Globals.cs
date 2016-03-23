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

        /* Execution states.  --Kris */
        public const int STATE_INITIALIZING = -2;
        public const int STATE_BROKEN = -1;
        public const int STATE_READY = 0;
        public const int STATE_VALIDATING = 1;
        public const int STATE_WAITING = 2;
        public const int STATE_EXECUTING = 3;

        /* Nothing says "nonsequitur" quite like "salt".  --Kris */
        public static Random rand = new Random();
    }
}
