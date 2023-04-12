using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Tooling.Connector;
using RntCar.BusinessLibrary.Handlers;
using RntCar.BusinessLibrary.Repository;
using RntCar.ClassLibrary;
using RntCar.SDK.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RntCar.BusinessLibrary.Business
{
    public class CampaignDetailBL : BusinessHandler
    {
        public CampaignDetailBL(IOrganizationService orgService) : base(orgService)
        {
        }

        public CampaignDetailBL(CrmServiceClient crmServiceClient) : base(crmServiceClient)
        {
        }

        public CampaignDetailBL(GlobalEnums.ConnectionType enums = GlobalEnums.ConnectionType.default_Service, string UserId = "") : base(enums, UserId)
        {
        }

        public CampaignDetailBL(IOrganizationService orgService, CrmServiceClient crmServiceClient) : base(orgService, crmServiceClient)
        {
        }

        public CampaignDetailBL(IOrganizationService orgService, ITracingService tracingService) : base(orgService, tracingService)
        {
        }

        public CampaignDetailBL(IOrganizationService orgService, Guid userId) : base(orgService, userId)
        {
        }

        public CampaignDetailBL(PluginInitializer initializer, IOrganizationService orgService, ITracingService tracingService) : base(initializer, orgService, tracingService)
        {
        }

        public CampaignDetailBL(IOrganizationService orgService, ITracingService tracingService, CrmServiceClient crmServiceClient) : base(orgService, tracingService, crmServiceClient)
        {
        }

        public CampaignDetailBL(IOrganizationService orgService, Guid userId, string orgName) : base(orgService, userId, orgName)
        {
        }

        public CampaignDetailData getCampaignDetail(Guid campaignId, int langId)
        {
            CampaignDetailRepository campaignDetailRepository = new CampaignDetailRepository(this.OrgService);
            var camp = campaignDetailRepository.getCampaignDetailByCampaignId(campaignId);
            if (camp == null)
            {
                return new CampaignDetailData();
            }

            var campTypes = XrmHelper.getEnumsAsOptionSetModelByLangId("rnt_CampaignTypeCode", langId);
            //todo will replace repository class
            CampaignRepository campaignRepository = new CampaignRepository(this.OrgService);
            var campEntity = campaignRepository.getCampaignType();
            if (campEntity == null)
            {
                return new CampaignDetailData();
            }
            return new CampaignDetailData
            {
                campaignContent = camp.GetAttributeValue<string>("rnt_campaigncontent"),
                campaignImageURL = camp.Attributes.Contains("rnt_campaignimageurl") ? camp.GetAttributeValue<string>("rnt_campaignimageurl") : string.Empty,
                campaignMobileImageURL = camp.Attributes.Contains("rnt_campaignmobileimageurl") ? camp.GetAttributeValue<string>("rnt_campaignmobileimageurl") : string.Empty,
                campaignBannerURL = camp.Attributes.Contains("rnt_capmaignbannerurl") ? camp.GetAttributeValue<string>("rnt_capmaignbannerurl") : string.Empty,
                campaignTitle = camp.GetAttributeValue<string>("rnt_campaigntitle"),
                campaignDetailContent = camp.GetAttributeValue<string>("rnt_campaigndetailcontent"),
                campaignDetailImageURL = camp.GetAttributeValue<string>("rnt_campaigndetailimageurl"),
                campaignDetailTitle = camp.GetAttributeValue<string>("rnt_campaigndetailtitle"),
                popupContent = camp.GetAttributeValue<string>("rnt_popupcontent"),
                popupImageURL = camp.GetAttributeValue<string>("rnt_popupimageurl"),
                popupTitle = camp.GetAttributeValue<string>("rnt_popuptitle"),
                campaignType = campEntity.GetAttributeValue<OptionSetValue>("rnt_campaigntypecode").Value,
                campaignTypeName = campTypes.Where(p => p.value.Equals(campEntity.GetAttributeValue<OptionSetValue>("rnt_campaigntypecode").Value)).FirstOrDefault()?.label.removeAlphaNumericCharactersFromString(),
                campaignSeoTitle = camp.GetAttributeValue<string>("rnt_seotitle"),
                campaignSeoDescription = camp.GetAttributeValue<string>("rnt_seodescription"),
                campaignSeoKeyword = camp.GetAttributeValue<string>("rnt_seokeyword"),
            };
        }

        public CampaignDetailData getCampaignBranchList(Guid campaignId, int langId)
        {
            CampaignRepository campaignDetailRepository = new CampaignRepository(this.OrgService);
            var camp = campaignDetailRepository.getCampaignById(campaignId, new string[] { "rnt_branchcode" });
            if (camp == null)
            {
                return new CampaignDetailData();
            }
            
            MultiSelectMappingRepository multiSelectRepository = new MultiSelectMappingRepository(this.OrgService);
            var branchCode = multiSelectRepository.getSelectedItemsGuidListByOptionValueandGivenColumn("rnt_branch",
                camp.Attributes.Contains("rnt_branchcode") ? camp.GetAttributeValue<OptionSetValueCollection>("rnt_branchcode") : null);
            List<string> branchCodeList = new List<string>();
            foreach (var item in branchCode)
            {
                branchCodeList.Add(Convert.ToString(item));
            }
            return new CampaignDetailData
            {
                campaignBranchId = branchCodeList
            };
        }
    }
}
