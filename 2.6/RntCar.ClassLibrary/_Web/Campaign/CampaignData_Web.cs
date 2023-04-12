using System;

namespace RntCar.ClassLibrary._Web
{
    public class CampaignData_Web
    {
        public Guid campaignId { get; set; }
        public string campaignName { get; set; }
        public string campaignDescription { get; set; }
        public DateTime campaignBeginingDate { get; set; }
        public DateTime campaignEndDate { get; set; }
        public string campaignImageURL { get; set; }
        public int campaignType { get; set; }
        public int order { get; set; }
    }
}
