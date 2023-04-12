using System.Collections.Generic;

namespace RntCar.ClassLibrary._Web
{
    public class CampaignDetailData_Web
    {
        public string campaignContent { get; set; }
        public string campaignImageURL { get; set; }
        public string campaignTitle { get; set; }
        public string campaignDetailContent { get; set; }
        public string campaignDetailImageURL { get; set; }
        public string campaignDetailTitle { get; set; }
        public string popupContent { get; set; }
        public string popupImageURL { get; set; }
        public string popupTitle { get; set; }
        public int campaignType { get; set; }
        public string campaignTypeName { get; set; }
        public List<string> campaignBranchId { get; set; }
        public string campaignSeoTitle { get; set; }
        public string campaignSeoDescription { get; set; }
        public string campaignSeoKeyword { get; set; }
    }
}
