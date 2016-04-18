using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FaceBERN_
{
    internal class Person
    {
        private string name;
        private string facebookID;
        private string stateAbbr;
        private bool bernieSupporter;

        internal Person(string name, string facebookID, string stateAbbr, bool bernieSupporter)
        {
            this.name = name;
            this.facebookID = facebookID;
            this.stateAbbr = stateAbbr;
            this.bernieSupporter = bernieSupporter;
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
    }
}
