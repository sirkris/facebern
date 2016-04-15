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
        public string abbr;
        public string name;
        public DateTime primaryDate;
        public string primaryType;
        public string primaryAccess;
        public string facebookId;
        public string FTBEventId;

        public States(string abbr, string name, string primaryDateStr, string primaryType, string primaryAccess, string facebookId, string FTBEventId = null)
        {
            this.abbr = abbr;
            this.name = name;
            this.primaryDate = DateTime.ParseExact(primaryDateStr, "yyyy-MM-dd", CultureInfo.InvariantCulture);
            this.primaryType = primaryType;
            this.primaryAccess = primaryAccess;
            this.facebookId = facebookId;
            this.FTBEventId = FTBEventId;
        }

        public States()
        { }
    }
}
