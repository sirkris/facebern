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
        public string sourceURI;
        public DateTime discovered;
        public DateTime entered;
        public string enteredBy;
        public DateTime start;
        public DateTime end;
        public int? campaignId = null;
        public int tid = 0;
        public DateTime? tweeted = null;

        public string tweetedBy = null;  // Only used for reporting new tweet to Birdie API.  --Kris

        public TweetsQueue(string tweet, string source, string sourceURI, DateTime discovered, DateTime entered, string enteredBy, DateTime start, DateTime end, 
            int? campaignId = null, int tid = 0, DateTime? tweeted = null)
        {
            this.tweet = tweet;
            this.source = source;
            this.sourceURI = sourceURI;
            this.discovered = discovered;
            this.entered = entered;
            this.enteredBy = enteredBy;
            this.start = start;
            this.end = end;
            this.campaignId = campaignId;
            this.tid = tid;
            this.tweeted = tweeted;
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

        public string GetSourceURI()
        {
            return sourceURI;
        }

        public void SetSourceURI(string sourceURI)
        {
            this.sourceURI = sourceURI;
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

        public int? GetCampaignId()
        {
            return campaignId;
        }

        public void SetCampaignId(int? campaignId)
        {
            this.campaignId = campaignId;
        }

        public DateTime GetEnd()
        {
            return end;
        }

        public void SetEnd(DateTime end)
        {
            this.end = end;
        }

        public int GetTID()
        {
            return tid;
        }

        public void SetTID(int tid)
        {
            this.tid = tid;
        }

        public DateTime? GetTweeted()
        {
            return tweeted;
        }

        public void SetTweeted(DateTime? tweeted)
        {
            this.tweeted = tweeted;
        }

        public string GetTweetedBy()
        {
            return tweetedBy;
        }

        public void SetTweetedBy(string tweetedBy)
        {
            this.tweetedBy = tweetedBy;
        }
    }
}
