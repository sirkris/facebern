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

            return ExecuteThreads();
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
                }
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
