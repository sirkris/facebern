using csReddit;
using Newtonsoft.Json;
using OpenQA.Selenium;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FaceBERN_.Campaigns.Admin.Operation_Life_Preserver
{
    public class OLP_Phase2 : OperationLifePreserver
    {
        private List<Thread> threads;

        private int secondsInterval = 5;

        public OLP_Phase2(WorkflowFacebook workflowFacebook, WorkflowTwitter workflowTwitter, Workflow workflow, bool refresh = true)
            : base(workflowFacebook, workflowTwitter, workflow, refresh)
        { }

        public override bool ExecuteFacebook()
        {
            return true;
        }

        public override bool ExecuteTwitter()
        {
            return true;
        }

        public override bool ExecuteWorkflow()
        {
            if (Globals.CampaignConfigs[Globals.CAMPAIGN_OPERATION_LIFE_PRESERVER] == false
                || Globals.CampaignConfigs[Globals.CAMPAIGN_OLP_PHASE2] == false)
            {
                return false;
            }

            return ExecuteThreads(1);  // TODO - Change back to default of 3 after initial testing.  --Kris
        }

        private bool ExecuteThreads(int threadCount = 3, int year = 2016)
        {
            try
            {
                int delaySeconds = (threadCount * secondsInterval);
                threads = new List<Thread>();
                for (int i = 1; i <= threadCount; i++)
                {
                    Thread thread = new Thread(() => ExecuteThreadWorkflow(i, threadCount, year));

                    thread.IsBackground = true;

                    workflow.Log("Attempting to start OLP-Phase2 thread #" + i.ToString() + "....", false);

                    thread.Start();
                    while (thread.IsAlive == false) { }

                    workflow.Log("OLP-Phase2 thread #" + i.ToString() + " started successfully.");

                    threads.Add(thread);

                    System.Threading.Thread.Sleep(delaySeconds * 1000);
                }

                while (true)
                {
                    bool stop = true;
                    foreach (Thread thread in threads)
                    {
                        if (thread.IsAlive)
                        {
                            stop = false;
                            break;
                        }
                    }

                    if (stop)
                    {
                        break;
                    }
                }

                workflow.Log("Operation Life-Preserver Phase 2 completed successfully!");
            }
            catch (ThreadAbortException)
            {
                foreach (Thread thread in threads)
                {
                    thread.Abort();
                }

                workflow.Log("Primary workflow aborted successfully in OLP-Phase2.");
            }
            catch (Exception e)
            {
                workflow.LogAndReportException(e, "Exception in OLP-Phase2.ExecuteThreads.  One or more threads may not have been launched successfully.");

                return false;
            }

            return true;
        }

        private bool ExecuteAPIThread(List<SubredditComment> comments, List<SubredditUser> users)
        {
            try
            {
                    Thread thread = new Thread(() => ExecuteAPIWorkflow(comments, users));

                    thread.IsBackground = false;

                    workflow.Log("Attempting to start OLP-Phase2 Birdie API thread....", false);

                    thread.Start();
                    while (thread.IsAlive == false) { }

                    workflow.Log("OLP-Phase2 Birdie API thread started successfully.");
            }
            catch (ThreadAbortException)
            {
                foreach (Thread thread in threads)
                {
                    thread.Abort();
                }

                workflow.Log("Birdie API thread aborted successfully in OLP-Phase2.");
            }
            catch (Exception e)
            {
                workflow.LogAndReportException(e, "Exception in OLP-Phase2.ExecuteAPIThread.");

                return false;
            }

            return true;
        }

        private void ExecuteAPIWorkflow(List<SubredditComment> comments, List<SubredditUser> users)
        {
            // TODO - Report to the Birdie API.  --Kris
        }

        private void ExecuteThreadWorkflow(int threadNum, int threadCount, int year)
        {
            try
            {
                string logPrefix = "OLP-Phase2 thread #" + threadNum.ToString() + ":  ";

                workflow.Log(logPrefix + "Executing....");

                /* Scan posts where 12 % month == threadNum for the given year.  Basically, divides the months evenly between threads.  --Kris */
                List<SubredditComment> comments = new List<SubredditComment>();
                List<SubredditUser> users = new List<SubredditUser>();
                for (int i = threadNum; i <= 12; i += threadCount)
                {
                    //i = 3;  // Uncomment for DEBUG.  --Kris
                    IRestResponse res = workflow.BirdieQuery(@"/admin/subPosts?subreddit=SandersForPresident&processed=0&month=" + i.ToString() + @"&year=" + year.ToString());

                    if (res == null || res.StatusCode != System.Net.HttpStatusCode.OK)
                    {
                        try
                        {
                            Exception e = new Exception("Birdie API returned non-200 response in OLP-Phase2.ExecuteThreadWorkflow!");

                            e.Data.Add("threadNum", threadNum);
                            e.Data.Add("threadCount", threadCount);
                            e.Data.Add("year", year);

                            throw e;
                        }
                        catch (Exception e)
                        {
                            workflow.ReportException(e, "Birdie API returned non-200 response in OLP-Phase2.ExecuteThreadWorkflow.");

                            System.Threading.Thread.Sleep(10000);

                            continue;
                        }
                    }

                    WebDriver webDriver = new WebDriver(workflow.Main, workflow.browser, false);  // I want the browser to be visible for this so I can monitor.  --Kris
                    webDriver.FixtureSetup();
                    
                    List<RedditPost> posts = BirdieToRedditPosts(JsonConvert.DeserializeObject(res.Content));
                    int p = 0;
                    foreach (RedditPost post in posts)
                    {
                        p++;

                        /* Doesn't work; Reddit API is stripping-out too many comments from morecomments.  --Kris */
                        /*
                        // TODO - Query Reddit API for each post then scrape its comments and users data.  Create API endpoints and database table for comments.  --Kris
                        string id36 = GetPostID36(post);
                        if (workflow.reddit == null)
                        {
                            workflow.reddit = new Reddit(false);
                        }
                        id36 = "43r8z7";
                        dynamic rRes = workflow.reddit.Listings.comments(id36, "SandersForPresident");

                        List<SubredditComment> comments = null;
                        List<SubredditUser> users = ParseRedditPostCommentsAndUsers(rRes, post, out comments);
                        */
                        //post.permalink = "https://www.reddit.com/r/SandersForPresident/comments/34epu3/reddit_i_am_running_for_president_of_the_united/";
                        /* Uncomment below for DEBUG.  --Kris */
                        /*
                        if (!(post.permalink.Contains("4bl35p")))
                        {
                            continue;
                        }
                        */
                        
                        /* Scrape Reddit using the webdriver.  --Kris */
                        webDriver.TestSetUp("https://www.reddit.com" + post.permalink + @"?limit=500");

                        webDriver.ScrollToBottom(10000);

                        //List<IWebElement> eles = webDriver.GetElementsByTagNameAndAttribute("a", "onclick", "return morechildren(this,", 0, true);
                        List<IWebElement> eles = null;
                        int limit = 1000;
                        do
                        {
                            eles = webDriver.GetElementsByLinkText("load more comments", true);
                            if (eles != null && eles.Count > 0)
                            {
                                foreach (IWebElement ele in eles)
                                {
                                    try
                                    {
                                        webDriver.ClickElement(ele, false, false, false, false);

                                        System.Threading.Thread.Sleep(2100 * threadCount);
                                    }
                                    catch (Exception)
                                    {
                                        continue;
                                    }
                                }
                            }

                            System.Threading.Thread.Sleep(3000 * threadCount);

                            limit--;
                        } while (eles != null && eles.Count > 0 && limit > 0);

                        /* Once all the comments are loaded, parse them from the HTML.  --Kris */
                        string src = webDriver.GetPageSource();
                        
                        string[] entries = src.Split(new string[] { "class=\"midcol unvoted\"" }, StringSplitOptions.RemoveEmptyEntries);
                        for (int ii = 2; ii < entries.Length; ii++)
                        {
                            string entry = null;
                            try
                            {
                                entry = entries[ii];
                                if (entry == null || !(entry.Contains("tagline")))
                                {
                                    continue;
                                }

                                string tag = "<p class=\"tagline\">";
                                string tagline = entry.Substring(entry.IndexOf(tag) + tag.Length);

                                tag = "<a href=\"https://www.reddit.com/user/";
                                string user = tagline.Substring(tagline.IndexOf(tag) + tag.Length);
                                user = user.Substring(0, user.IndexOf("\""));

                                bool distinguished = tagline.Contains("moderator");
                                bool isModerator = distinguished;

                                bool stickied = tagline.Contains("stickied");

                                tag = "datetime=\"";
                                string created = tagline.Substring(tagline.IndexOf(tag) + tag.Length);
                                created = created.Substring(0, created.IndexOf("\""));

                                tag = "<span class=\"score likes\">";
                                string scoreStr = tagline.Substring(tagline.IndexOf(tag) + tag.Length);
                                try
                                {
                                    scoreStr = scoreStr.Substring(0, scoreStr.IndexOf(" points"));
                                }
                                catch (Exception)
                                {
                                    string salt = "1234567890-";
                                    string newScoreStr = "";
                                    for (int iii = 0; iii < scoreStr.Length; iii++)
                                    {
                                        if (salt.Contains(scoreStr.Substring(iii, 1)))
                                        {
                                            newScoreStr += scoreStr.Substring(iii, 1);
                                            break;
                                        }
                                    }

                                    scoreStr = newScoreStr;
                                }

                                int score = 0;
                                try
                                {
                                    score = int.Parse(scoreStr, NumberStyles.Any);
                                }
                                catch (Exception e)
                                {
                                    string salt = "1234567890-";
                                    string newScore = "";
                                    for (int iii = 0; iii < scoreStr.Length; iii++)
                                    {
                                        if (salt.IndexOf(scoreStr.Substring(iii, 1)) != -1)
                                        {
                                            newScore += scoreStr.Substring(iii, 1);
                                        }
                                    }

                                    try
                                    {
                                        score = int.Parse(newScore);
                                    }
                                    catch (Exception ex)
                                    {
                                        e.Data.Add("scoreStr", scoreStr);
                                        e.Data.Add("newScore", newScore);
                                        e.Data.Add("fullResult", entry);
                                        e.Data.Add("retryException", ex);

                                        workflow.LogAndReportException(e, "Unable to parse score value in OLP-Phase2.");
                                    }
                                }

                                scoreStr = score.ToString();

                                string id36 = null;
                                if (tagline.Contains(post.permalink))
                                {
                                    id36 = tagline.Substring(tagline.IndexOf(post.permalink) + post.permalink.Length);
                                    id36 = id36.Substring(0, id36.IndexOf("\""));
                                }

                                tag = "<span class=\"flair";
                                string flair = null;
                                try
                                {
                                    if (tagline.Contains(tag))
                                    {
                                        flair = tagline.Substring(tagline.IndexOf(tag) + tag.Length);
                                        flair = flair.Substring(flair.IndexOf(">") + 1);
                                        flair = flair.Substring(0, flair.IndexOf("</span"));
                                    }
                                }
                                catch (Exception)
                                { }

                                string stateAbbr = null;
                                string rankFlair = null;
                                foreach (KeyValuePair<string, States> state in Globals.StateConfigs)
                                {
                                    if (state.Key.Equals("DA"))
                                    {
                                        continue;
                                    }

                                    if (flair != null
                                        && (flair.Contains(state.Key) || flair.Contains(state.Value.name)))
                                    {
                                        stateAbbr = state.Key;
                                        break;
                                    }
                                }

                                List<string> ranks = new List<string>
                                {
                                    "Cadet", 
                                    "Private", 
                                    "Private First Class", 
                                    "Lance Corporal", 
                                    "Corporal", 
                                    "Sergeant", 
                                    "Staff Sergeant", 
                                    "Gunnery Sergeant", 
                                    "Master Sergeant", 
                                    "First Sergeant", 
                                    "Master Gunnery Sergeant", 
                                    "Sergeant Major", 
                                    "Second Lieutenant", 
                                    "First Lieutenant", 
                                    "Captain", 
                                    "Major", 
                                    "Lieutenant Colonel", 
                                    "Colonel", 
                                    "Brigadier General", 
                                    "Major General", 
                                    "Lieutenant General", 
                                    "General", 
                                    "Field Marshal", 
                                    "Marshal", 
                                    "General of the Force" 
                                };

                                // Iterate longer strings first to eliminate false-positives (i.e. "Sergeant" firing when it should be "Master Sergeant").  --Kris
                                System.Linq.IOrderedEnumerable<string> ranksSorted = from element in ranks 
                                        orderby element.Length descending 
                                        select element;

                                foreach (string rank in ranksSorted)
                                {
                                    if (flair != null && flair.Contains(rank))
                                    {
                                        rankFlair = rank;
                                        break;
                                    }
                                }

                                comments.Add(new SubredditComment(id36, post.permalink, "SandersForPresident", user, score, distinguished, stickied, DateTime.Parse(created)));

                                int selfPostScore = 0;
                                int linkPostScore = 0;
                                int selfPosts = 0;
                                int linkPosts = 0;
                                if (post.GetAuthor().Equals(user))
                                {
                                    if (post.GetSelf() == true)
                                    {
                                        selfPostScore = post.GetScore();
                                        selfPosts++;
                                    }
                                    else
                                    {
                                        linkPostScore = post.GetScore();
                                        linkPosts++;
                                    }
                                }

                                bool userFound = false;
                                for (int iii = 0; iii < users.Count; iii++)
                                {
                                    if (users[iii].username.Equals(user))
                                    {
                                        users[iii].totalLinkPostScore += linkPostScore;
                                        users[iii].totalSelfPostScore += selfPostScore;
                                        users[iii].totalCommentScore += score;

                                        if (users[iii].firstComment == null
                                            || DateTime.Parse(created) < users[iii].firstComment.Value)
                                        {
                                            users[iii].firstComment = DateTime.Parse(created);
                                        }

                                        if (users[iii].lastComment == null
                                            || DateTime.Parse(created) > users[iii].lastComment.Value)
                                        {
                                            users[iii].lastComment = DateTime.Parse(created);
                                        }

                                        if (post.GetAuthor().Equals(user))
                                        {
                                            if (users[iii].firstPost == null
                                                || post.GetCreated() < users[iii].firstPost.Value)
                                            {
                                                users[iii].firstPost = post.GetCreated();
                                            }

                                            if (users[iii].lastPost == null
                                                || post.GetCreated() > users[iii].lastPost.Value)
                                            {
                                                users[iii].lastPost = post.GetCreated();
                                            }

                                            if (post.self)
                                            {
                                                users[iii].selfPosts++;
                                            }
                                            else
                                            {
                                                users[iii].linkPosts++;
                                            }
                                        }

                                        users[iii].comments++;

                                        userFound = true;
                                        break;
                                    }
                                }

                                if (userFound == false)
                                {
                                    users.Add(new SubredditUser("SandersForPresident", user, isModerator, selfPosts, linkPosts, 1, selfPostScore, linkPostScore, score,
                                                                (post.GetAuthor().Equals(user) ? (DateTime?)DateTime.Parse(created) : null), DateTime.Parse(created),
                                                                (post.GetAuthor().Equals(user) ? (DateTime?)DateTime.Parse(created) : null), DateTime.Parse(created),
                                                                null, null, null, null, null, flair, stateAbbr, rankFlair));
                                }
                            }
                            catch (Exception e)
                            {
                                e.Data.Add("entry", entry);
                                e.Data.Add("post", "");
                                e.Data.Add("p", p);
                                e.Data.Add("threadNum", threadNum);
                                e.Data.Add("threadCount", threadCount);
                                e.Data.Add("i", i);
                                e.Data.Add("year", year);

                                workflow.LogAndReportException(e, "Exception parsing post comments in OLP-Phase2.");

                                continue;
                            }
                        }

                        // TODO - Add subreddit comments and users endpoints to Birdie API.  --Kris
                        
                        if (users.Count >= 1000
                            || comments.Count >= 1000)
                        {
                            if (ExecuteAPIThread(comments, users))
                            {
                                comments = new List<SubredditComment>();
                                users = new List<SubredditUser>();
                            }
                        }
                    }

                    webDriver.FixtureTearDown();

                    if (users.Count > 0
                        || comments.Count > 0)
                    {
                        ExecuteAPIThread(comments, users);
                    }
                }
            }
            catch (ThreadAbortException)
            {
                workflow.Log("OLP-Phase2 thread #" + threadNum.ToString() + " terminated successfully!");
            }
            catch (Exception e)
            {
                workflow.LogAndReportException(e, "Unhandled exception in OLP-Phase2 workflow.");
            }
        }

        private string GetPostID36(RedditPost post)
        {
            string id36 = post.GetPermalink().Substring(32);
            return id36.Substring(0, id36.IndexOf(@"/"));
        }

        public string GetPostFullname(RedditPost post)
        {
            return "t3_" + GetPostID36(post);
        }

        private List<SubredditUser> ParseRedditMoreComments(dynamic redditObj, dynamic o, RedditPost post, out List<SubredditComment> comments, int i, string sub = null)
        {
            comments = null;
            List<string> commentIds = null;
            try
            {
                if (o["data"]["replies"]["data"]["children"][0]["data"]["children"] != null)
                {
                    commentIds = new List<string>();
                    foreach (string commentId in o["data"]["replies"]["data"]["children"][0]["data"]["children"])
                    {
                        commentIds.Add(commentId);
                    }

                    dynamic rRes = null;
                    rRes = workflow.reddit.LinksAndComments.morechildren(string.Join(@",", commentIds.ToArray()), GetPostFullname(post), "top");
                    if (rRes != null)
                    {
                        foreach (dynamic resO in rRes)
                        {
                            i = i;
                        }
                    }

                    return null;  // TODO
                }
                else
                {
                    return null;
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        private SubredditUser ParseRedditCommentObj(dynamic redditObj, dynamic o, RedditPost post, out List<SubredditComment> comments, int i, string sub = null)
        {
            SubredditUser subUser = null;
            comments = new List<SubredditComment>();

            try
            {
                if (o != null
                    && o["data"] != null
                    && o["data"]["author"] != null
                    && o["data"]["id"] != null
                    && o["data"]["score"] != null
                    && o["data"]["stickied"] != null
                    && o["data"]["created"] != null)
                {
                    /* If you're seeing results from the wrong sub, make sure you're passing true for restrict_sr to the Reddit API.  --Kris */
                    if (sub != null && !(sub.Equals(o["data"]["subreddit"].ToString())))
                    {
                        try
                        {
                            Exception e = new Exception("Result from wrong subreddit returned by Reddit API!");

                            e.Data.Add("o", o);
                            e.Data.Add("redditObj", redditObj);
                            e.Data.Add("sub", sub);
                            e.Data.Add("campaignId", campaignId);

                            throw e;
                        }
                        catch (Exception e)
                        {
                            workflow.ReportExceptionSilently(e, "Result from wrong subreddit returned by Reddit API.");
                        }

                        return null;
                    }

                    try
                    {
                        bool distinguished;
                        bool stickied;
                        try
                        {
                            distinguished = (o["data"]["distinguished"] != null ? Convert.ToBoolean((bool) o["data"]["distinguished"]) : false);
                        }
                        catch (Exception)
                        {
                            distinguished = false;
                        }
                        try
                        {
                            stickied = (o["data"]["stickied"] != null ? Convert.ToBoolean((bool) o["data"]["stickied"]) : false);
                        }
                        catch (Exception)
                        {
                            stickied = false;
                        }
                        comments.Add(new SubredditComment((string) o["data"]["id"], (string) post.GetPermalink(), (string) o["data"]["subreddit"], (string) o["data"]["author"],
                                    (int) o["data"]["score"], (bool) distinguished, (bool) stickied,
                                    (DateTime) workflow.TimestampToDateTime((int) o["data"]["created"])));

                        /* Collect user data for this thread.  Local combining happens below.  Combining with totals/averages from other post threads will be handled at the API level.  --Kris */
                        subUser = new SubredditUser((string) o["data"]["subreddit"], (string) o["data"]["author"], null,
                                                                (int) (post.GetSelf() == true && post.GetAuthor().Equals(o["data"]["author"]) ? 1 : 0),
                                                                (int) (post.GetSelf() == false && post.GetAuthor().Equals(o["data"]["author"]) ? 1 : 0),
                                                                (int) 1, (int) (post.GetSelf() == true && post.GetAuthor().Equals(o["data"]["author"]) ? post.GetScore() : 0),
                                                                (int) (post.GetSelf() == false && post.GetAuthor().Equals(o["data"]["author"]) ? post.GetScore() : 0),
                                                                (int) o["data"]["score"],
                                                                (post.GetAuthor().Equals(o["data"]["author"]) ? (DateTime?) workflow.TimestampToDateTime((int) o["data"]["created"]) : null),
                                                                (DateTime) workflow.TimestampToDateTime((int) o["data"]["created"]),
                                                                (post.GetAuthor().Equals(o["data"]["author"]) ? (DateTime?) workflow.TimestampToDateTime((int) o["data"]["created"]) : null),
                                                                (DateTime) workflow.TimestampToDateTime((int) o["data"]["created"]));

                    }
                    catch (Exception e)
                    {
                        workflow.Log("Warning:  Error parsing Reddit post : " + e.ToString());

                        workflow.ReportException(e, "Error parsing Reddit post.");
                    }
                }
            }
            catch (Exception e)
            {
                try
                {
                    workflow.LogAndReportException(e, "Exception thrown handling redditObj in OLP-Phase2.ParseRedditCommentObj where o = " + JsonConvert.SerializeObject(o) + ".");
                }
                catch (Exception)
                {
                    try
                    {
                        workflow.LogAndReportException(e, "Exception thrown handling redditObj in OLP-Phase2.ParseRedditCommentObj where i = " + i.ToString() + "; unable to serialize object o.");
                    }
                    catch (Exception)
                    {
                        // Just forget it.  No point logging it without any useful information.  --Kris
                    }
                }
            }

            return subUser;
        }

        private List<SubredditUser> ParseRedditPostCommentsAndUsers(dynamic redditObj, RedditPost post, out List<SubredditComment> comments, string sub = null)
        {
            comments = null;
            try
            {
                redditObj = redditObj[1];
            }
            catch (Exception)
            {
                return null;
            }
            
            try
            {
                List<SubredditUser> res = new List<SubredditUser>();
                comments = new List<SubredditComment>();

                try
                {
                    if (redditObj["data"]["children"] == null || redditObj["data"]["children"].Count == 0)
                    {
                        return res;  // No results.  --Kris
                    }
                }
                catch (Exception e)
                {
                    workflow.ReportExceptionSilently(e, "Exception handling data children from redditObj in OLP-Phase2.ParseRedditPostCommentsAndUsers.");
                    return res;
                }
                // rRes[1]["data"]["children"][increment]["data"]["replies"]["data"]["children"][0]["data"]["children"]
                try
                {
                    int i = 0;
                    foreach (dynamic o in redditObj["data"]["children"])
                    {
                        i++;
                        List<SubredditComment> subComments = null;
                        List<SubredditUser> subUsers = new List<SubredditUser> { ParseRedditCommentObj(redditObj, o, post, out subComments, i, sub) };

                        comments.AddRange(subComments);

                        subComments = new List<SubredditComment>();
                        List<SubredditUser> moreSubUsers = new List<SubredditUser>();
                        moreSubUsers = ParseRedditMoreComments(redditObj, o, post, out subComments, i, sub);

                        if (moreSubUsers != null)
                        {
                            subUsers.AddRange(moreSubUsers);
                        }

                        foreach (SubredditUser subUser in subUsers)
                        {
                            bool dup = false;
                            for (int ii = 0; ii < res.Count; ii++)
                            {
                                if (res[ii].subreddit.Equals(subUser.username)
                                    && res[ii].username.Equals(subUser.username))
                                {
                                    res[ii].absorb(subUser);
                                    dup = true;

                                    break;
                                }
                            }

                            if (dup == false)
                            {
                                res.Add(subUser);
                            }
                        }
                    }

                    return res;
                }
                catch (Exception e)
                {
                    workflow.LogAndReportException(e, "Exception in OLP-Phase2.ParseRedditPostCommentsAndUsers.");

                    return null;
                }
            }
            catch (Exception e)
            {
                workflow.LogAndReportException(e, "Uncaught exception in OLP-Phase2.ParseRedditPostCommentsAndUsers.");

                return null;
            }
        }

        private List<RedditPost> BirdieToRedditPosts(dynamic deserializedJSON)
        {
            List<RedditPost> posts = new List<RedditPost>();
            foreach (dynamic o in deserializedJSON)
            {
                if (o != null
                    && o["subreddit"] != null
                    && o["permalink"] != null
                    && o["username"] != null
                    && o["title"] != null
                    && o["url"] != null
                    && o["score"] != null
                    && o["comments"] != null
                    && o["self"] != null
                    && o["created"] != null)
                {
                    RedditPost post = new RedditPost((bool)((string)o["self"]).Equals("1"), (string)o["title"], (string)o["subreddit"], (string)o["url"], (string)o["permalink"],
                                                    (int)o["score"], (DateTime)o["created"], (string)o["username"]);

                    posts.Add(post);
                }
            }

            return posts;
        }
    }
}
