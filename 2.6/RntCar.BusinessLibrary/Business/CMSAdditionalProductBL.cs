using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Tooling.Connector;
using RntCar.BusinessLibrary.Handlers;
using RntCar.BusinessLibrary.Repository;
using RntCar.ClassLibrary;
using RntCar.SDK.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.BusinessLibrary.Business
{
    public class CMSAdditionalProductBL : BusinessHandler
    {
        public CMSAdditionalProductBL(IOrganizationService orgService) : base(orgService)
        {
        }

        public CMSAdditionalProductBL(CrmServiceClient crmServiceClient) : base(crmServiceClient)
        {
        }

        public CMSAdditionalProductBL(GlobalEnums.ConnectionType enums = GlobalEnums.ConnectionType.default_Service, string UserId = "") : base(enums, UserId)
        {
        }

        public CMSAdditionalProductBL(IOrganizationService orgService, CrmServiceClient crmServiceClient) : base(orgService, crmServiceClient)
        {
        }

        public CMSAdditionalProductBL(IOrganizationService orgService, ITracingService tracingService) : base(orgService, tracingService)
        {
        }

        public CMSAdditionalProductBL(IOrganizationService orgService, Guid userId) : base(orgService, userId)
        {
        }

        public CMSAdditionalProductBL(PluginInitializer initializer, IOrganizationService orgService, ITracingService tracingService) : base(initializer, orgService, tracingService)
        {
        }

        public CMSAdditionalProductBL(IOrganizationService orgService, ITracingService tracingService, CrmServiceClient crmServiceClient) : base(orgService, tracingService, crmServiceClient)
        {
        }

        public CMSAdditionalProductBL(IOrganizationService orgService, Guid userId, string orgName) : base(orgService, userId, orgName)
        {
        }

        public List<CMSAdditionalProductData> GetCMSAdditionalProducts()
        {
            CMSAdditionalProductRepository cmsAdditionalProductRepository = new CMSAdditionalProductRepository(this.OrgService, this.CrmServiceClient);
            var products = cmsAdditionalProductRepository.GetCMSAdditionalProducts();

            List<CMSAdditionalProductData> cmsAdditionalProductDatas = new List<CMSAdditionalProductData>();
            foreach (var item in products)
            {
                cmsAdditionalProductDatas.Add(new CMSAdditionalProductData
                {
                    additionalProductId = item.GetAttributeValue<EntityReference>("rnt_additionalproductid").Id,
                    detailContent = item.GetAttributeValue<string>("rnt_detailcontent"),
                    detailImageURL = item.GetAttributeValue<string>("rnt_detailimageurl"),
                    previewImageURL = item.GetAttributeValue<string>("rnt_previewimageurl"),
                    previewText = item.GetAttributeValue<string>("rnt_previewtext"),
                    seoDescription = item.GetAttributeValue<string>("rnt_seodescription"),
                    seoKeyword = item.GetAttributeValue<string>("rnt_seokeyword"),
                    seoTitle = item.GetAttributeValue<string>("rnt_seotitle"),
                });
            }

            return cmsAdditionalProductDatas;
        }
    }
}
