using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FaceBERN_
{
    public class Person
    {
        public string name;
        public string facebookInternalID;
        public string facebookID;
        public string stateAbbr;
        public bool bernieSupporter;
        public string facebookEventID;

        public string lastGOTVInvite;  // Ticks.  --Kris

        internal Person(string name, string facebookID, string stateAbbr, bool bernieSupporter, string facebookInternalID = null, string facebookEventID = null)
        {
            this.name = name;
            this.facebookID = facebookID;
            this.stateAbbr = stateAbbr;
            this.bernieSupporter = bernieSupporter;
            this.facebookInternalID = facebookInternalID;
            this.facebookEventID = facebookEventID;

            this.lastGOTVInvite = null;
        }

        internal Person() { }

        internal string getName(bool sanitize = true)
        {
            if (sanitize)
            {
                return Regex.Replace(this.name, "(\\(.*\\))", "").Replace("  ", " ").Trim();
            }
            else
            {
                return this.name;
            }
        }

        internal void setName(string name)
        {
            this.name = name;
        }

        internal string getFacebookID()
        {
            return this.facebookID;
        }

        internal void setFacebookID(string facebookID)
        {
            this.facebookID = facebookID;
        }

        internal string getFacebookEventID()
        {
            return this.facebookID;
        }

        internal void setFacebookEventID(string facebookID)
        {
            this.facebookID = facebookID;
        }

        internal string getFacebookInternalID()
        {
            return this.facebookInternalID;
        }

        internal void setFacebookInternalID(string facebookInternalID)
        {
            this.facebookInternalID = facebookInternalID;
        }

        internal string getStateAbbr()
        {
            return this.stateAbbr;
        }

        internal void setStateAbbr(string stateAbbr)
        {
            this.stateAbbr = stateAbbr;
        }

        internal bool getBernieSupporter()
        {
            return this.bernieSupporter;
        }

        internal void setBernieSupporter(bool bernieSupporter)
        {
            this.bernieSupporter = bernieSupporter;
        }

        internal bool isBernieSupporter()
        {
            return getBernieSupporter();
        }

        internal string getLastGOTVInvite()
        {
            return this.lastGOTVInvite;
        }

        internal DateTime getLastGOTVInviteAsDateTime()
        {
            return new DateTime(long.Parse(this.lastGOTVInvite));
        }

        internal void setLastGOTVInvite(string ticks)
        {
            this.lastGOTVInvite = ticks;
        }

        internal void setLastGOTVInvite(DateTime lastInvite)
        {
            this.lastGOTVInvite = lastInvite.Ticks.ToString();
        }
    }
}
