using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FaceBERN_.Campaigns.Admin
{
    public class ShareOnFacebook : FaceBERN_.Campaign
    {
        public ShareOnFacebook(bool refresh = true)
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
