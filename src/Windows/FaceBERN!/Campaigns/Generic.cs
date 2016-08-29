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
        internal Workflow workflow = null;

        public Generic(WorkflowFacebook workflowFacebook, WorkflowTwitter workflowTwitter, Workflow workflow, bool refresh = true)
        {
            if (refresh)
            {
                RefreshCampaignData();
            }

            this.workflowFacebook = workflowFacebook;
            this.workflowTwitter = workflowTwitter;
            this.workflow = workflow;
        }
        
        /* Execution to be performed by the Facebook workflow thread.  --Kris */
        public virtual bool ExecuteFacebook()
        {
            return true;
        }

        /* Execution to be performed by the Twitter workflow thread.  --Kris */
        public virtual bool ExecuteTwitter()
        {
            return true;
        }

        /* Execution to be performed by the primary workflow thread.  --Kris */
        public virtual bool ExecuteWorkflow()
        {
            return true;
        }
    }
}
