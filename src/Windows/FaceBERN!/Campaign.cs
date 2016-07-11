using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FaceBERN_
{
    public class Campaign
    {
        public int campaignId;
        public string campaignTitle;
        public string createdByAdminUsername;
        public DateTime createdAt;
        public DateTime start;
        public bool enabled;
        public bool requiresFacebook;
        public bool requiresTwitter;

        public bool userSelected;  // Whether or not the user checked the box for this campaign in Settings.  --Kris
        public bool approvedByDefault;

        public string campaignDescription = null;
        public string campaignURL = null;
        public int? parentCampaignId = null;
        public DateTime? end = null;

        public Campaign(int campaignId, string campaignTitle, string createdByAdminUsername, DateTime createdAt, DateTime start, bool enabled, bool requiresFacebook, bool requiresTwitter, bool userSelected, 
                        bool approvedByDefault, string campaignDescription = null, string campaignURL = null, int? parentCampaignId = null, DateTime? end = null)
        {
            this.campaignId = campaignId;
            this.campaignTitle = campaignTitle;
            this.createdByAdminUsername = createdByAdminUsername;
            this.createdAt = createdAt;
            this.start = start;
            this.enabled = enabled;
            this.requiresFacebook = requiresFacebook;
            this.requiresTwitter = requiresTwitter;

            this.userSelected = userSelected;
            this.approvedByDefault = approvedByDefault;

            this.campaignDescription = campaignDescription;
            this.campaignURL = campaignURL;
            this.parentCampaignId = parentCampaignId;
            this.end = end;
        }

        public Campaign() { }

        public int GetCampaignId()
        {
            return campaignId;
        }

        public void SetCampaignId(int campaignId)
        {
            this.campaignId = campaignId;
        }

        public string GetCampaignTitle()
        {
            return campaignTitle;
        }

        public void SetCampaignTitle(string campaignTitle)
        {
            this.campaignTitle = campaignTitle;
        }

        public string GetCreatedByAdminUsername()
        {
            return createdByAdminUsername;
        }

        public void SetCreatedByAdminUsername(string createdByAdminUsername)
        {
            this.createdByAdminUsername = createdByAdminUsername;
        }

        public DateTime GetCreatedAt()
        {
            return createdAt;
        }

        public void SetCreatedAt(DateTime createdAt)
        {
            this.createdAt = createdAt;
        }

        public DateTime GetStart()
        {
            return start;
        }

        public void SetStart(DateTime start)
        {
            this.start = start;
        }

        public string GetCampaignDescription()
        {
            return campaignDescription;
        }

        public void SetCampaignDescription(string campaignDescription)
        {
            this.campaignDescription = campaignDescription;
        }

        public string GetCampaignURL()
        {
            return campaignURL;
        }

        public void SetCampaignURL(string campaignURL)
        {
            this.campaignURL = campaignURL;
        }

        public int? GetParentCampaignId()
        {
            return parentCampaignId;
        }

        public void SetParentCampaignId(int? parentCampaignId)
        {
            this.parentCampaignId = parentCampaignId;
        }

        public DateTime? GetEnd()
        {
            return end;
        }

        public void SetEnd(DateTime? end)
        {
            this.end = end;
        }
    }
}
