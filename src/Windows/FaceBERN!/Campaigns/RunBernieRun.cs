using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FaceBERN_.Campaigns
{
    public class RunBernieRun : Generic
    {
        public RunBernieRun(WorkflowFacebook workflowFacebook, WorkflowTwitter workflowTwitter, bool refresh = true)
            : base(workflowFacebook, workflowTwitter, refresh)
        { }
        
        public override bool ExecuteFacebook()
        {
            return true;
        }

        public override bool ExecuteTwitter()
        {
            if (Globals.CampaignConfigs[Globals.CAMPAIGN_TWEET_STILLSANDERSFORPRES] == false)
            {
                workflowTwitter.Log("The campaign to send tweets from /r/StillSandersForPres is disabled.  Skipped.");
                return false;
            }

            workflowTwitter.ConsumeTweetsQueue(Globals.CAMPAIGN_TWEET_STILLSANDERSFORPRES);

            return true;
        }
    }
}
