using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FaceBERN_
{
    public class FacebookUser
    {
        public string fbUserId = null;
        public string name = null;
        public string fbId = null;
        public DateTime lastInvited = new DateTime();
        public string lastInvitedBy = null;
        public string stateAbbr = null;
        public string eventId = null;

        public FacebookUser() { }
    }
}
