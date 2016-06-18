using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FaceBERN_
{
    public class TweetsQueue
    {
        public string tweet;
        public string source;
        public DateTime discovered;
        public DateTime entered;
        public string enteredBy;
        public DateTime start;
        public DateTime end;

        public TweetsQueue(string tweet, string source, DateTime discovered, DateTime entered, string enteredBy, DateTime start, DateTime end)
        {
            this.tweet = tweet;
            this.source = source;
            this.discovered = discovered;
            this.entered = entered;
            this.enteredBy = enteredBy;
            this.start = start;
            this.end = end;
        }

        public TweetsQueue() { }

        public string GetTweet()
        {
            return tweet;
        }

        public void SetTweet(string tweet)
        {
            this.tweet = tweet;
        }

        public string GetSource()
        {
            return source;
        }

        public void SetSource(string source)
        {
            this.source = source;
        }

        public DateTime GetDiscovered()
        {
            return discovered;
        }

        public void SetDiscovered(DateTime discovered)
        {
            this.discovered = discovered;
        }

        public DateTime GetEntered()
        {
            return entered;
        }

        public void SetEntered(DateTime entered)
        {
            this.entered = entered;
        }

        public string GetEnteredBy()
        {
            return enteredBy;
        }

        public void SetEnteredBy(string enteredBy)
        {
            this.enteredBy = enteredBy;
        }

        public DateTime GetStart()
        {
            return start;
        }

        public void SetStart(DateTime start)
        {
            this.start = start;
        }

        public DateTime GetEnd()
        {
            return end;
        }

        public void SetEnd(DateTime end)
        {
            this.end = end;
        }
    }
}
