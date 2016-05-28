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
        public string facebookID;
        public string stateAbbr;
        public bool bernieSupporter;

        public string lastGOTVInvite;  // Ticks.  --Kris

        internal Person(string name, string facebookID, string stateAbbr, bool bernieSupporter)
        {
            this.name = name;
            this.facebookID = facebookID;
            this.stateAbbr = stateAbbr;
            this.bernieSupporter = bernieSupporter;

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
