using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FaceBERN_
{
    public class RedditPost
    {
        public bool self;
        public string title;
        public string url;
        public int score;
        public DateTime created;
        public string author;
        public string selfText = null;

        public RedditPost(bool self, string title, string url, int score, DateTime created, string author, string selfText = null)
        {
            this.self = self;
            this.title = title;
            this.url = url;
            this.score = score;
            this.created = created;
            this.author = author;
            this.selfText = selfText;
        }

        public RedditPost() { }

        public bool GetSelf()
        {
            return self;
        }

        public void SetSelf(bool self)
        {
            this.self = self;
        }

        public string GetTitle()
        {
            return title;
        }

        public void SetTitle(string title)
        {
            this.title = title;
        }

        public string GetURL()
        {
            return url;
        }

        public void SetURL(string url)
        {
            this.url = url;
        }

        public int GetScore()
        {
            return score;
        }

        public void SetScore(int score)
        {
            this.score = score;
        }

        public DateTime GetCreated()
        {
            return created;
        }

        public void SetCreated(DateTime created)
        {
            this.created = created;
        }

        public string GetAuthor()
        {
            return author;
        }

        public void SetAuthor(string author)
        {
            this.author = author;
        }

        public string GetSelfText()
        {
            return selfText;
        }

        public void SetSelfText(string selfText)
        {
            this.selfText = selfText;
        }
    }
}
