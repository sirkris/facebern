using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FaceBERN_.Campaigns
{
    public class RunBernieRun : FaceBERN_.Campaign
    {
        public RunBernieRun(bool refresh = true)
        {
            if (refresh)
            {
                RefreshCampaignData();
            }
        }
        
        public bool ExecuteFacebook()
        {
            return true;
        }

        public bool ExecuteTwitter()
        {
            return true;
        }
    }
}
