using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FaceBERN_
{
    public class SubredditUser
    {
        public string subreddit;
        public string username;

        public bool? isModerator = null;
        public int selfPosts = 0;
        public int linkPosts = 0;
        public int comments = 0;
        public int? averageSelfPostScore = 0;
        public int? averageLinkPostScore = 0;
        public int? averageCommentScore = 0;
        public int totalSelfPostScore = 0;
        public int totalLinkPostScore = 0;
        public int totalCommentScore = 0;
        public int? totalRedditPostKarma = null;
        public int? totalRedditCommentKarma = null;
        public DateTime? firstPost = null;
        public DateTime? firstComment = null;
        public DateTime? lastPost = null;
        public DateTime? lastComment = null;

        public DateTime lastUpdated;

        public SubredditUser(string subreddit, string username, bool? isModerator = null, int selfPosts = 0, int linkPosts = 0, int comments = 0, int totalSelfPostScore = 0, int totalLinkPostScore = 0, 
                            int totalCommentScore = 0, DateTime? firstPost = null, DateTime? firstComment = null, DateTime? lastPost = null, DateTime? lastComment = null, 
                            int? averageSelfPostScore = 0, int? averageLinkPostScore = 0, int? averageCommentScore = 0, int? totalRedditPostKarma = null, int? totalRedditCommentKarma = null)
        {
            this.subreddit = subreddit;
            this.username = username;
            this.isModerator = isModerator;
            this.selfPosts = selfPosts;
            this.linkPosts = linkPosts;
            this.comments = comments;
            this.averageSelfPostScore = averageSelfPostScore;
            this.averageLinkPostScore = averageLinkPostScore;
            this.averageCommentScore = averageCommentScore;
            this.totalSelfPostScore = totalSelfPostScore;
            this.totalLinkPostScore = totalLinkPostScore;
            this.totalCommentScore = totalCommentScore;
            this.firstPost = firstPost;
            this.firstComment = firstComment;
            this.lastPost = lastPost;
            this.lastComment = lastComment;
            this.totalRedditPostKarma = totalRedditPostKarma;
            this.totalRedditCommentKarma = totalRedditCommentKarma;

            this.lastUpdated = DateTime.Now;
        }

        public bool absorb(SubredditUser subUser)
        {
            if (!(this.username.Equals(subUser.username)))
            {
                return false;
            }

            this.isModerator = (subUser.isModerator == true ? true : this.isModerator);

            this.selfPosts += subUser.selfPosts;
            this.linkPosts += subUser.linkPosts;
            this.comments += subUser.comments;

            // Average scores are handled at the API level for better accuracy.  --Kris

            this.totalSelfPostScore += subUser.totalSelfPostScore;
            this.totalLinkPostScore += subUser.totalLinkPostScore;
            this.totalCommentScore += subUser.totalCommentScore;

            this.firstPost = (this.firstPost.Value.Subtract(subUser.firstPost.Value).TotalSeconds > 0 ? subUser.firstPost : this.firstPost);
            this.firstComment = (this.firstComment.Value.Subtract(subUser.firstComment.Value).TotalSeconds > 0 ? subUser.firstComment : this.firstComment);
            this.lastPost = (this.lastPost.Value.Subtract(subUser.lastPost.Value).TotalSeconds > 0 ? subUser.lastPost : this.lastPost);
            this.lastComment = (this.lastComment.Value.Subtract(subUser.lastComment.Value).TotalSeconds > 0 ? subUser.lastComment : this.lastComment);

            this.lastUpdated = DateTime.Now;

            return true;
        }
    }
}
