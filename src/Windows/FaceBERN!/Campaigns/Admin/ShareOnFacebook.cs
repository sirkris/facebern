using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FaceBERN_.Campaigns.Admin
{
    public class ShareOnFacebook : Generic
    {
        public ShareOnFacebook(WorkflowFacebook workflowFacebook, WorkflowTwitter workflowTwitter, Workflow workflow, bool refresh = true)
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
    }
}
