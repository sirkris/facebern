using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FaceBERN_.Campaigns
{
    public abstract class Generic : Campaign
    {
        internal WorkflowFacebook workflowFacebook = null;
        internal WorkflowTwitter workflowTwitter = null;

        public Generic(WorkflowFacebook workflowFacebook, WorkflowTwitter workflowTwitter, bool refresh = true)
        {
            if (refresh)
            {
                RefreshCampaignData();
            }

            this.workflowFacebook = workflowFacebook;
            this.workflowTwitter = workflowTwitter;
        }
        
        public virtual bool ExecuteFacebook()
        {
            return true;
        }

        public virtual bool ExecuteTwitter()
        {
            return true;
        }
    }
}
