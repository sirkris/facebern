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

                    System.Threading.Thread.Sleep(delaySeconds * 1000);
                }
            }
            catch (Exception e)
            {
                workflow.LogAndReportException(e, "Exception in OLP-Phase2.ExecuteThreads.  One or more threads may not have been launched successfully.");

                return false;
            }

            return true;
        }

        private void ExecuteThreadWorkflow(int threadNum, int threadCount, int year)
        {
            string logPrefix = "OLP-Phase2 thread #" + threadNum.ToString() + ":  ";

            workflow.Log(logPrefix + "Executing....");

            /* Scan posts where 12 % month == threadNum for the given year.  Basically, divides the months evenly between threads.  --Kris */
            for (int i = threadNum; i <= 12; i += threadCount)
            {
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

                List<RedditPost> posts = BirdieToRedditPosts(JsonConvert.DeserializeObject(res.Content));
                foreach (RedditPost post in posts)
                {
                    // TODO - Query Reddit API for each post then scrape its comments and users data.  Create API endpoints and database table for comments.  --Kris
                    string id36 = post.GetPermalink().Substring(32);
                    id36 = id36.Substring(0, id36.IndexOf(@"/"));

                    if (workflow.reddit == null)
                    {
                        workflow.reddit = new Reddit(false);
                    }

                    dynamic rRes = workflow.reddit.Listings.comments(id36, "SandersForPresident");

                    List<SubredditComment> comments = null;
                    List<SubredditUser> users = ParseRedditPostCommentsAndUsers(rRes, post, out comments);

                    // TODO - Add subreddit comments and users endpoints to Birdie API.  --Kris
                    // TODO - Report to the Birdie API.  --Kris
                }
            }
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

                try
                {
                    int i = 0;
                    foreach (dynamic o in redditObj["data"]["children"])
                    {
                        i++;

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

                                    continue;
                                }

                                try
                                {
                                    comments.Add(new SubredditComment((string) o["data"]["id"], (string) post.GetPermalink(), (string) o["data"]["subreddit"], (string) o["data"]["author"],
                                                (int) o["data"]["score"], (o["data"]["distinguished"] != null ? Convert.ToBoolean((bool) o["data"]["distinguished"]) : false), 
                                                (o["data"]["stickied"] != null ? Convert.ToBoolean((bool) o["data"]["stickied"]) : false), 
                                                (DateTime) workflow.TimestampToDateTime((int) o["data"]["created"])));
                                    
                                    /* Collect user data for this thread.  Local combining happens below.  Combining with totals/averages from other post threads will be handled at the API level.  --Kris */
                                    SubredditUser subUser = new SubredditUser((string) o["data"]["subreddit"], (string) o["data"]["author"], null,
                                                                            (int) (post.GetSelf() == true && post.GetAuthor().Equals(o["data"]["author"]) ? 1 : 0),
                                                                            (int) (post.GetSelf() == false && post.GetAuthor().Equals(o["data"]["author"]) ? 1 : 0),
                                                                            (int) 1, (int) (post.GetSelf() == true && post.GetAuthor().Equals(o["data"]["author"]) ? post.GetScore() : 0),
                                                                            (int) (post.GetSelf() == false && post.GetAuthor().Equals(o["data"]["author"]) ? post.GetScore() : 0),
                                                                            (int) o["data"]["score"],
                                                                            (post.GetAuthor().Equals(o["data"]["author"]) ? (DateTime?) workflow.TimestampToDateTime((int) o["data"]["created"]) : null),
                                                                            (DateTime) workflow.TimestampToDateTime((int) o["data"]["created"]),
                                                                            (post.GetAuthor().Equals(o["data"]["author"]) ? (DateTime?) workflow.TimestampToDateTime((int) o["data"]["created"]) : null),
                                                                            (DateTime) workflow.TimestampToDateTime((int) o["data"]["created"]));

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
                                workflow.LogAndReportException(e, "Exception thrown handling redditObj in OLP-Phase2.ParseRedditPostCommentsAndUsers where o = " + JsonConvert.SerializeObject(o) + ".");
                            }
                            catch (Exception)
                            {
                                try
                                {
                                    workflow.LogAndReportException(e, "Exception thrown handling redditObj in OLP-Phase2.ParseRedditPostCommentsAndUsers where i = " + i.ToString() + "; unable to serialize object o.");
                                }
                                catch (Exception)
                                {
                                    // Just forget it.  No point logging it without any useful information.  --Kris
                                }
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
                    RedditPost post = new RedditPost((bool) ((string) o["self"]).Equals("1"), (string) o["title"], (string) o["subreddit"], (string) o["url"], (string) o["permalink"],
                                                    (int) o["score"], (DateTime) o["created"], (string) o["username"]);

                    posts.Add(post);
                }
            }

            return posts;
        }
    }
}
