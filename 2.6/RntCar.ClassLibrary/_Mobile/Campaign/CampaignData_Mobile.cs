using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary._Mobile
{
    public class CampaignData_Mobile
    {
        public Guid campaignId { get; set; }
        public string campaignName { get; set; }
        public string campaignDescription { get; set; }
        public DateTime campaignBeginingDate { get; set; }
        public DateTime campaignEndDate { get; set; }
        public string campaignImageURL { get; set; }
        public string campaignMobileImageURL { get; set; }
        public string campaignBannerURL { get; set; }
        public string campaignTerms { get; set; }
        public int campaignType { get; set; }
    }
}
