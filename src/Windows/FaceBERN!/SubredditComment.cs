using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FaceBERN_
{
    public class SubredditComment
    {
        public string id36;
        public string postPermalink;
        public string subreddit;
        public string username;
        public int score;
        public bool distinguished;
        public bool stickied;
        public DateTime created;

        public SubredditComment(string id36, string postPermalink, string subreddit, string username, int score, bool distinguished, bool stickied, DateTime created)
        {
            this.id36 = id36;
            this.postPermalink = postPermalink;
            this.subreddit = subreddit;
            this.username = username;
            this.score = score;
            this.distinguished = distinguished;
            this.stickied = stickied;
            this.created = created;
        }
    }
}
