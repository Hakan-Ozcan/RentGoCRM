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
    public class ShowRoomProductBL : BusinessHandler
    {
        public ShowRoomProductBL(IOrganizationService orgService) : base(orgService)
        {
        }

        public ShowRoomProductBL(CrmServiceClient crmServiceClient) : base(crmServiceClient)
        {
        }

        public ShowRoomProductBL(GlobalEnums.ConnectionType enums = GlobalEnums.ConnectionType.default_Service, string UserId = "") : base(enums, UserId)
        {
        }

        public ShowRoomProductBL(IOrganizationService orgService, CrmServiceClient crmServiceClient) : base(orgService, crmServiceClient)
        {
        }

        public ShowRoomProductBL(IOrganizationService orgService, ITracingService tracingService) : base(orgService, tracingService)
        {
        }

        public ShowRoomProductBL(IOrganizationService orgService, Guid userId) : base(orgService, userId)
        {
        }

        public ShowRoomProductBL(PluginInitializer initializer, IOrganizationService orgService, ITracingService tracingService) : base(initializer, orgService, tracingService)
        {
        }

        public ShowRoomProductBL(IOrganizationService orgService, ITracingService tracingService, CrmServiceClient crmServiceClient) : base(orgService, tracingService, crmServiceClient)
        {
        }

        public ShowRoomProductBL(IOrganizationService orgService, Guid userId, string orgName) : base(orgService, userId, orgName)
        {
        }

        public List<ShowRoomProductData> getActiveShowRoomProducts(int langId)
        {
            TranslationBL translationBL = new TranslationBL(this.OrgService);
            ShowRoomProductRepository showRoomProductRepository = new ShowRoomProductRepository(this.OrgService);
            var products = showRoomProductRepository.getActiveShowRoomProduct();

            List<ShowRoomProductData> showRoomProductDatas = new List<ShowRoomProductData>();

            var bodyTypes = XrmHelper.getEnumsAsOptionSetModelByLangId("rnt_BodyTypeCode", langId);
            var fuelTypes = XrmHelper.getEnumsAsOptionSetModelByLangId("rnt_FuelTypeCode", langId); 
            var gearBoxes = XrmHelper.getEnumsAsOptionSetModelByLangId("rnt_Gearbox", langId); 
            var numberOfDoors = XrmHelper.getEnumsAsOptionSetModelByLangId("rnt_NumberofDoor", langId); 
            var numberOfPersons = XrmHelper.getEnumsAsOptionSetModelByLangId("rnt_NumberofPersonCode", langId);
            var colorNames = XrmHelper.getEnumsAsOptionSetModelByLangId("rnt_color", langId);
            foreach (var item in products)
            {
                showRoomProductDatas.Add(new ShowRoomProductData
                {
                    showRoomProductId = item.GetAttributeValue<Guid>("rnt_showroomproductid"),
                    webURL = item.GetAttributeValue<string>("rnt_weburl"),
                    name = item.GetAttributeValue<string>("rnt_name"),
                    abs = item.GetAttributeValue<bool>("rnt_abs"),
                    airbag = item.GetAttributeValue<bool>("rnt_airbag"),
                    bodyType = item.GetAttributeValue<OptionSetValue>("rnt_bodytypecode").Value,
                    bodyTypeName = bodyTypes.Where(p => p.value.Equals(item.GetAttributeValue<OptionSetValue>("rnt_bodytypecode").Value)).FirstOrDefault()?.label,
                    carbonEmission = decimal.Round(item.GetAttributeValue<decimal>("rnt_carbonemission"), 2, MidpointRounding.AwayFromZero),
                    cruiseControl = item.GetAttributeValue<bool>("rnt_cruisecontrol"),
                    engineVolume = item.GetAttributeValue<EntityReference>("rnt_enginevolumeid").Name,
                    fuelConsumption = decimal.Round(item.GetAttributeValue<decimal>("rnt_fuelconsumption"), 2, MidpointRounding.AwayFromZero),
                    fuelType = item.GetAttributeValue<OptionSetValue>("rnt_fueltypecode").Value,
                    fuelTypeName = fuelTypes.Where(p => p.value.Equals(item.GetAttributeValue<OptionSetValue>("rnt_fueltypecode").Value)).FirstOrDefault()?.label.removeAlphaNumericCharactersFromString(),
                    gearbox = item.GetAttributeValue<OptionSetValue>("rnt_gearboxcode").Value,
                    gearboxName = gearBoxes.Where(p => p.value.Equals(item.GetAttributeValue<OptionSetValue>("rnt_gearboxcode").Value)).FirstOrDefault()?.label.removeAlphaNumericCharactersFromString(),
                    groupCodeInformationId = item.GetAttributeValue<EntityReference>("rnt_groupcodeid").Id,
                    numberofDoors = item.GetAttributeValue<OptionSetValue>("rnt_numberofdoorscode").Value,
                    numberofDoorsName = numberOfDoors.Where(p => p.value.Equals(item.GetAttributeValue<OptionSetValue>("rnt_numberofdoorscode").Value)).FirstOrDefault()?.label.removeAlphaNumericCharactersFromString(),
                    numberofPerson = item.GetAttributeValue<OptionSetValue>("rnt_numberofperson").Value,
                    numberofPersonName = numberOfPersons.Where(p => p.value.Equals(item.GetAttributeValue<OptionSetValue>("rnt_numberofperson").Value)).FirstOrDefault()?.label.removeAlphaNumericCharactersFromString(),
                    productDescription = item.GetAttributeValue<string>("rnt_description"),
                    seoDescription = item.GetAttributeValue<string>("rnt_seodescription"),
                    seoKeyword = item.GetAttributeValue<string>("rnt_seokeyword"),
                    seoTitle = item.GetAttributeValue<string>("rnt_seotitle"),
                    trunkVolume = item.GetAttributeValue<int>("rnt_trunkvolume"),
                    model = item.GetAttributeValue<string>("rnt_model"),
                    isMaster = item.Attributes.Contains("rnt_ismaster") ? item.GetAttributeValue<bool>("rnt_ismaster") : false,
                    description = item.GetAttributeValue<string>("rnt_description"),
                    colorName = colorNames.Where(p => p.value.Equals(item.GetAttributeValue<OptionSetValue>("rnt_color")?.Value)).FirstOrDefault()?.label.removeAlphaNumericCharactersFromString()
                });

            }
            return showRoomProductDatas;
        }

    }
}
