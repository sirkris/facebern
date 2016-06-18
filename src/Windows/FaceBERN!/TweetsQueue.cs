using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FaceBERN_
{
    internal class TweetsQueue
    {
        internal string tweet;
        internal string source;
        internal DateTime discovered;
        internal DateTime entered;
        internal string enteredBy;
        internal DateTime start;
        internal DateTime end;

        internal TweetsQueue(string tweet, string source, DateTime discovered, DateTime entered, string enteredBy, DateTime start, DateTime end)
        {
            this.tweet = tweet;
            this.source = source;
            this.discovered = discovered;
            this.entered = entered;
            this.enteredBy = enteredBy;
            this.start = start;
            this.end = end;
        }

        internal TweetsQueue() { }

        internal string GetTweet()
        {
            return tweet;
        }

        internal void SetTweet(string tweet)
        {
            this.tweet = tweet;
        }

        internal string GetSource()
        {
            return source;
        }

        internal void SetSource(string source)
        {
            this.source = source;
        }

        internal DateTime GetDiscovered()
        {
            return discovered;
        }

        internal void SetDiscovered(DateTime discovered)
        {
            this.discovered = discovered;
        }

        internal DateTime GetEntered()
        {
            return entered;
        }

        internal void SetEntered(DateTime entered)
        {
            this.entered = entered;
        }

        internal string GetEnteredBy()
        {
            return enteredBy;
        }

        internal void SetEnteredBy(string enteredBy)
        {
            this.enteredBy = enteredBy;
        }

        internal DateTime GetStart()
        {
            return start;
        }

        internal void SetStart(DateTime start)
        {
            this.start = start;
        }

        internal DateTime GetEnd()
        {
            return end;
        }

        internal void SetEnd(DateTime end)
        {
            this.end = end;
        }
    }
}
