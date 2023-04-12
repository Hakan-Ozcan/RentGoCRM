using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Tooling.Connector;
using RntCar.BusinessLibrary.Handlers;
using RntCar.BusinessLibrary.Repository;
using RntCar.ClassLibrary;
using RntCar.SDK.Common;
using System;
using System.Collections.Generic;

namespace RntCar.BusinessLibrary.Business
{
    public class BannerBL : BusinessHandler
    {
        public BannerBL(IOrganizationService orgService) : base(orgService)
        {
        }

        public BannerBL(CrmServiceClient crmServiceClient) : base(crmServiceClient)
        {
        }

        public BannerBL(GlobalEnums.ConnectionType enums = GlobalEnums.ConnectionType.default_Service, string UserId = "") : base(enums, UserId)
        {
        }

        public BannerBL(IOrganizationService orgService, CrmServiceClient crmServiceClient) : base(orgService, crmServiceClient)
        {
        }

        public BannerBL(IOrganizationService orgService, ITracingService tracingService) : base(orgService, tracingService)
        {
        }

        public BannerBL(IOrganizationService orgService, Guid userId) : base(orgService, userId)
        {
        }

        public BannerBL(PluginInitializer initializer, IOrganizationService orgService, ITracingService tracingService) : base(initializer, orgService, tracingService)
        {
        }

        public BannerBL(IOrganizationService orgService, ITracingService tracingService, CrmServiceClient crmServiceClient) : base(orgService, tracingService, crmServiceClient)
        {
        }

        public BannerBL(IOrganizationService orgService, Guid userId, string orgName) : base(orgService, userId, orgName)
        {
        }

        public List<BannerData> getCampaignBanners(Guid campaignId)
        {
            BannerRepository bannerRepository = new BannerRepository(this.OrgService, this.CrmServiceClient);
            var campBanners = bannerRepository.getCampaignBanners(campaignId);

            List<BannerData> bannerDatas = new List<BannerData>();
            foreach (var item in campBanners)
            {
                bannerDatas.Add(new BannerData
                {
                    bannerURL  = item.GetAttributeValue<string>("rnt_bannerurl"),
                    order = item.GetAttributeValue<int>("rnt_order"),
                });
            }
            return bannerDatas;
        }
        public List<BannerData> getBanners()
        {
            BannerRepository bannerRepository = new BannerRepository(this.OrgService, this.CrmServiceClient);
            var campBanners = bannerRepository.getBanners();

            List<BannerData> bannerDatas = new List<BannerData>();
            foreach (var item in campBanners)
            {
                bannerDatas.Add(new BannerData
                {
                    bannerURL = item.GetAttributeValue<string>("rnt_bannerurl"),
                    order = item.GetAttributeValue<int>("rnt_order"),
                    bannerId = item.Id,
                    bannerName = item.GetAttributeValue<string>("rnt_name"),
                });
            }
            return bannerDatas;
        }
    }
}
