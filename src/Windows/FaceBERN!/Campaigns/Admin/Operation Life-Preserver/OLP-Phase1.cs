using Newtonsoft.Json;
using OpenQA.Selenium;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FaceBERN_.Campaigns.Admin.Operation_Life_Preserver
{
    public class OLP_Phase1 : OperationLifePreserver
    {
        public OLP_Phase1(WorkflowFacebook workflowFacebook, WorkflowTwitter workflowTwitter, Workflow workflow, bool refresh = true)
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
                || Globals.CampaignConfigs[Globals.CAMPAIGN_OLP_PHASE1] == false)
            {
                return false;
            }

            int phase = 1;  // Use phase 0 to disable when the campaign is complete.  --Kris
            if (phase > 0)
            {
                workflow.Log("Commencing Operation Life-Preserver:  Phase " + phase.ToString());
            }

            List<RedditPost> s4pPosts = null;
            switch (phase)
            {
                case 0:
                    return true;
                case 1:
                    // Note - The below won't work due to limitations in the Reddit API.  So instead, we'll do this the old-fashioned way....  --Kris
                    //s4pPosts = workflow.SearchSubredditForPosts("SandersForPresident", Globals.CAMPAIGN_OPERATION_LIFE_PRESERVER);

                    /* Search one day at a time to get around result limits.  We'll see if cloudsearch is up to the task.  --Kris */
                    string src;
                    string[] results = new string[99999];
                    int dayNum = 0;
                    foreach (DateTime day in IterateDays(new DateTime(2016, 01, 30), new DateTime(2015, 3, 1)))
                    {
                        dayNum++;
                        int delayMinutes = (dayNum % 30 == 0 ? 15 : (dayNum % 3) + 1);

                        workflow.Log("Operation Life-Preserver:  Acquiring S4P posts for " + day.ToString("MM/dd/yyyy") + "....");

                        /* Get the timestamps for the Reddit cloudsearch.  --Kris */
                        string start = workflow.DateTimeToTimestamp(day).ToString();
                        string end = workflow.DateTimeToTimestamp(day.AddDays(1)).ToString();

                        /* Build the query URL.  Basically, we're searching for all S4P posts that occurred on this day.  --Kris */
                        string sub = "SandersForPresident";
                        string baseUrl = @"https://www.reddit.com/r/" + sub + @"/search?";
                        string url = baseUrl + @"q=%28filter+timestamp+" + start + @".." + end + @"%29&restrict_sr=true&sort=top&t=all&limit=100&syntax=cloudsearch";

                        WebDriver webDriver = new WebDriver(workflow.Main, workflow.browser, false);  // I want the browser to be visible for this so I can monitor.  --Kris
                        webDriver.FixtureSetup();
                        webDriver.TestSetUp(url);

                        // TODO - Do(?)/while loop that iterates for each results page until the next button is gone.  Do manual testing of end-of-results page first to ensure correct trigger.  --Kris
                        List<Dictionary<string, string>> payload = new List<Dictionary<string, string>>();
                        int page = 0;
                        do
                        {
                            page++;

                            workflow.Log("Operation Life-Preserver:  Processing results page " + page.ToString() + " for " + day.ToString("MM/dd/yyyy") + "....");

                            src = webDriver.GetPageSource();
                            results = src.Split(new string[] { "div class=\" search-result " }, StringSplitOptions.RemoveEmptyEntries);

                            for (int i = 1; i < results.Length; i++)
                            {
                                if (results[i] != null)
                                {
                                    /* Scrape the permalink.  --Kris */
                                    string r = results[i];
                                    string tag = "<a href=\"";
                                    string permalink = r.Substring(r.IndexOf(tag) + tag.Length, r.Substring(r.IndexOf(tag) + tag.Length).IndexOf("\""));

                                    if (permalink.IndexOf(@"?") != -1)
                                    {
                                        permalink = permalink.Substring(0, permalink.IndexOf(@"?"));
                                    }

                                    /* Scrape the flair title.  --Kris */
                                    /*
                                    tag = "title=\"";
                                    string flairTitle = r.Substring(r.IndexOf(tag) + tag.Length);
                                    flairTitle = flairTitle.Substring(0, flairTitle.IndexOf("\""));
                                    */

                                    /* Scrape the post title.  --Kris */
                                    tag = "class=\"search-title may-blank\">";
                                    string title = r.Substring(r.IndexOf(tag) + tag.Length);
                                    title = title.Substring(0, title.IndexOf(@"</a>"));

                                    /* Scrape the author.  --Kris */
                                    tag = "<a href=\"https://www.reddit.com/user/";
                                    string author = r.Substring(r.IndexOf(tag) + tag.Length);
                                    author = author.Substring(0, author.IndexOf("\""));

                                    /* Scrape the link.  --Kris */
                                    tag = "class=\"search-link may-blank\">";
                                    string link = permalink;
                                    bool self = (r.IndexOf(tag) == -1);
                                    if (r.IndexOf(tag) != -1)
                                    {
                                        link = r.Substring(r.IndexOf(tag) + tag.Length);
                                        link = link.Substring(0, link.IndexOf(@"</a>"));
                                    }

                                    /* Scrape the timestamp.  --Kris */
                                    tag = "datetime=\"";
                                    string timestamp = r.Substring(r.IndexOf(tag) + tag.Length);
                                    timestamp = timestamp.Substring(0, timestamp.IndexOf("\""));

                                    DateTime datetime;
                                    try
                                    {
                                        datetime = Convert.ToDateTime(timestamp);
                                    }
                                    catch (Exception e)
                                    {
                                        e.Data.Add("timestamp", timestamp);
                                        workflow.LogAndReportException(e, "Unable to parse Reddit timestamp in OperationLifePreserver.");

                                        datetime = day;
                                    }

                                    /* Scrape the number of comments.  --Kris */
                                    tag = "class=\"search-comments may-blank\">";
                                    string commentsStr = r.Substring(r.IndexOf(tag) + tag.Length);
                                    commentsStr = commentsStr.Substring(0, commentsStr.IndexOf(@"</a>"));
                                    if (commentsStr.IndexOf("comments") != -1)
                                    {
                                        commentsStr = commentsStr.Substring(0, commentsStr.IndexOf("comments")).Trim();
                                    }
                                    else if (commentsStr.IndexOf("comment") != -1)
                                    {
                                        commentsStr = commentsStr.Substring(0, commentsStr.IndexOf("comment")).Trim();
                                    }

                                    int comments = 0;
                                    try
                                    {
                                        comments = int.Parse(commentsStr, NumberStyles.Any);
                                    }
                                    catch (Exception e)
                                    {
                                        string salt = "1234567890";
                                        string newC = "";
                                        for (int ii = 0; ii < commentsStr.Length; ii++)
                                        {
                                            if (salt.IndexOf(commentsStr.Substring(ii, 1)) != -1)
                                            {
                                                newC += commentsStr.Substring(ii, 1);
                                            }
                                        }

                                        try
                                        {
                                            comments = int.Parse(newC);
                                        }
                                        catch (Exception ex)
                                        {
                                            e.Data.Add("commentsStr", commentsStr);
                                            e.Data.Add("newC", newC);
                                            e.Data.Add("fullResult", r);
                                            e.Data.Add("retryException", ex);

                                            workflow.LogAndReportException(e, "Unable to parse comments value in OperationLifePreserver.");
                                        }
                                    }

                                    commentsStr = comments.ToString();

                                    /* Scrape the post score.  --Kris */
                                    tag = "</span><span class=\"search-score\">";
                                    string scoreStr = r.Substring(r.IndexOf(tag) + tag.Length);
                                    scoreStr = scoreStr.Substring(0, scoreStr.IndexOf(@"</span>"));
                                    if (scoreStr.IndexOf("points") != -1)
                                    {
                                        scoreStr = scoreStr.Substring(0, scoreStr.IndexOf("points")).Trim();
                                    }
                                    else if (scoreStr.IndexOf("point") != -1)
                                    {
                                        scoreStr = scoreStr.Substring(0, scoreStr.IndexOf("point")).Trim();
                                    }

                                    int score = 0;
                                    try
                                    {
                                        score = int.Parse(scoreStr, NumberStyles.Any);
                                    }
                                    catch (Exception e)
                                    {
                                        string salt = "1234567890";
                                        string newScore = "";
                                        for (int ii = 0; ii < scoreStr.Length; ii++)
                                        {
                                            if (salt.IndexOf(scoreStr.Substring(ii, 1)) != -1)
                                            {
                                                newScore += scoreStr.Substring(ii, 1);
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
                                            e.Data.Add("fullResult", r);
                                            e.Data.Add("retryException", ex);

                                            workflow.LogAndReportException(e, "Unable to parse score value in OperationLifePreserver.");
                                        }
                                    }

                                    scoreStr = score.ToString();

                                    /* Add to the Birdie POST queue.  --Kris */
                                    Dictionary<string, string> body = new Dictionary<string, string>();

                                    body.Add("subreddit", sub);
                                    body.Add("permalink", permalink);
                                    body.Add("username", author);
                                    body.Add("title", title);
                                    body.Add("url", link);
                                    body.Add("score", scoreStr);
                                    body.Add("comments", commentsStr);
                                    body.Add("self", (self == true ? "1" : "0"));
                                    body.Add("created", timestamp);

                                    payload.Add(body);
                                }
                            }

                            workflow.Log("Operation Life-Preserver:  Processing of results page " + page.ToString() + " complete!  Checking for additional results page....");

                            webDriver.ScrollToBottom(0, 500, 5);

                            bool leave = false;
                            int retry = 5;
                            do
                            {
                                try
                                {
                                    IWebElement element = webDriver.GetElementByCSSSelector("a[rel='nofollow next']", -1, true);
                                    webDriver.ClickElement(element, false, false, true);

                                    break;
                                }
                                catch (NoSuchElementException)
                                {
                                    leave = true;

                                    break;
                                }
                                catch (Exception e)
                                {
                                    retry--;

                                    e.Data.Add("day", day);
                                    e.Data.Add("payload", payload);

                                    if (retry > 0)
                                    {
                                        workflow.LogAndReportException(e, "Exception trying to click on \"next >\" button on Reddit.  Retrying....");
                                    }
                                    else
                                    {
                                        workflow.LogAndReportException(e, "Unable to click \"next >\" button on Reddit.  Skipping to next day.");
                                    }
                                }
                            } while (retry > 0);

                            if (leave == true)
                            {
                                break;
                            }
                        } while (page <= 50);

                        webDriver.FixtureTearDown();
                        webDriver = null;

                        /* Report our bounty for this day to the Birdie API.  --Kris */
                        workflow.Log("Retrieved " + payload.Count.ToString() + " posts from /r/" + sub + " for " + day.ToString("MM/dd/yyyy") + " successfully.  Reporting to Birdie API....");

                        IRestResponse res = workflow.BirdieQuery("/admin/subPosts", "POST", null, JsonConvert.SerializeObject(payload));

                        if (res.StatusCode == System.Net.HttpStatusCode.Created)
                        {
                            workflow.Log("Reddit posts reported to Birdie API successfully!");
                        }
                        else
                        {
                            try
                            {
                                Exception e = new Exception("Error updating Birdie API with Phase 1 payload in OperationLifePreserver!");
                                e.Data.Add("payload", payload);
                                e.Data.Add("statusCode", res.StatusCode);
                                e.Data.Add("statusDescription", res.StatusDescription);
                                e.Data.Add("content", res.Content);
                                e.Data.Add("day", day);

                                throw e;
                            }
                            catch (Exception e)
                            {
                                workflow.LogAndReportException(e, "Error updating Birdie API with Phase 1 payload in OperationLifePreserver.");
                            }
                        }

                        workflow.Log("Operation Life-Preserver:  Day " + day.ToString("MM/dd/yyyy") + " processed successfully!  Waiting " + delayMinutes.ToString() + " minutes for next iteration....");

                        System.Threading.Thread.Sleep(1000);

                        System.Threading.Thread.Sleep(delayMinutes * 60 * 1000);  // Wait delayMinutes minutes between each day.  Task should take a few days to complete, give or take.  --Kris
                    }

                    workflow.Log("Operation Life-Preserver:  Phase 1 search complete!");

                    break;
            }

            return true;
        }
    }
}
