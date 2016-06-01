using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FaceBERN_
{
    /* Decided to create a class for storing states configuration info, given the data complexity.  --Kris */
    public class States
    {
        public string abbr = null;
        public string name = null;
        public DateTime primaryDate;
        public string primaryType = null;
        public string primaryAccess = null;
        public string facebookId = null;
        public string FTBEventId = null;

        /* The minimum number of "friends of friends" results targetted for this state.  --Kris */
        public int facebookFriendSearchTarget;

        /* How many times to perform the "friends of friends" search for this state if the minimum results target is not met.  --Kris */
        public int facebookFriendSearchPasses;

        public States(string abbr, string name, string primaryDateStr, string primaryType, string primaryAccess,
            int facebookFriendSearchTarget, int facebookFriendSearchPasses, string facebookId, string FTBEventId = null)
        {
            this.abbr = abbr;
            this.name = name;
            this.primaryDate = DateTime.ParseExact(primaryDateStr, "yyyy-MM-dd", CultureInfo.InvariantCulture);
            this.primaryType = primaryType;
            this.primaryAccess = primaryAccess;
            this.facebookFriendSearchTarget = facebookFriendSearchTarget;
            this.facebookFriendSearchPasses = facebookFriendSearchPasses;
            this.facebookId = facebookId;
            this.FTBEventId = FTBEventId;
        }

        public States()
        { }
    }
}
