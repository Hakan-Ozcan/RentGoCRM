using RntCar.ClassLibrary;
using RntCar.ClassLibrary._Mobile;
using RntCar.ClassLibrary._Web;
using RntCar.ClassLibrary.MongoDB;
using RntCar.SDK.Common;
using System.Collections.Generic;

namespace RntCar.SDK.Mappers
{
    public class CampaignMapper
    {
        public List<CampaignData_Web> buildWebCampaignData(List<CampaignData> campaignDatas)
        {
            return campaignDatas.ConvertAll(p => new CampaignData_Web
            {
                campaignId = new System.Guid(p.campaignId),
                campaignBeginingDate = p.beginingDate.Value,
                campaignDescription = p.description,
                campaignEndDate = p.endDate.Value,
                campaignName = p.name,
                campaignType = p.campaignType
            });
        }

        public CampaignDetailData_Web buildWebCampaignDetailData(CampaignDetailData campaignDetailData)
        {
            return new CampaignDetailData_Web().Map(campaignDetailData);
        }

        public List<CampaignData_Mobile> buildMobileCampaignData(List<CampaignData> campaignDatas)
        {
            return campaignDatas.ConvertAll(p => new CampaignData_Mobile
            {
                campaignId = new System.Guid(p.campaignId),
                campaignBeginingDate = p.beginingDate.Value,
                campaignDescription = p.description,
                campaignEndDate = p.endDate.Value,
                campaignName = p.name,
                campaignType = p.campaignType
            });
        }

        public CampaignDetailData_Mobile buildMobileCampaignDetailData(CampaignDetailData campaignDetailData)
        {
            // mobile campaigns has to Mobile Image URL and Banner URL
            return string.IsNullOrEmpty(campaignDetailData.campaignBannerURL) || string.IsNullOrEmpty(campaignDetailData.campaignMobileImageURL) ?
                new CampaignDetailData_Mobile().Map(campaignDetailData) : new CampaignDetailData_Mobile();
        }
    }
}
