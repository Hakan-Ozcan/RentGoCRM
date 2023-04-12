using Microsoft.Xrm.Sdk;
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
    public class GroupCodeInformationBL : BusinessHandler
    {
       
        public GroupCodeInformationBL(IOrganizationService orgService) : base(orgService)
        {
        }

        public GroupCodeInformationBL(IOrganizationService orgService, CrmServiceClient crmServiceClient) : base(orgService, crmServiceClient)
        {
        }

        public GroupCodeInformationBL(IOrganizationService orgService, ITracingService tracingService, CrmServiceClient crmServiceClient) : base(orgService, tracingService, crmServiceClient)
        {
        }

        public GroupCodeInformationBL(CrmServiceClient crmServiceClient) : base(crmServiceClient)
        {
        }
        public List<GroupCodeInformationDetailData> getAllGroupCodesInformationDetailData(int langId)
        {
            GroupCodeInformationRepository repository = new GroupCodeInformationRepository(this.OrgService, this.CrmServiceClient);
            var result = repository.getAllGroupCodeInformations();

            List<GroupCodeInformationDetailData> data = new List<GroupCodeInformationDetailData>();

            var gearBoxes = XrmHelper.getEnumsAsOptionSetModelByLangId("rnt_groupcodeinformations_rnt_gearboxcode", langId);
            var fuelTypes = XrmHelper.getEnumsAsOptionSetModelByLangId("rnt_FuelTypeCode", langId);
            var colorNames = XrmHelper.getEnumsAsOptionSetModelByLangId("rnt_color", langId);

            foreach (var item in result)
            {
                GroupCodeInformationDetailData groupCodeInformationDetailData = new GroupCodeInformationDetailData
                {
                    deposit = item.Attributes.Contains("rnt_deposit") ? (decimal?)item.GetAttributeValue<decimal>("rnt_deposit") : null,
                    findeks = item.Attributes.Contains("rnt_findeks") ? (int?)item.GetAttributeValue<int>("rnt_findeks") : null,
                    carModelImage = item.GetAttributeValue<string>("rnt_image"),
                    minimumAge = item.Attributes.Contains("rnt_minimumage") ? (int?)item.GetAttributeValue<int>("rnt_minimumage") : null,
                    minimumDriverLicense = item.Attributes.Contains("rnt_minimumdriverlicence") ? (int?)item.GetAttributeValue<int>("rnt_minimumdriverlicence") : null,
                    segment = item.Attributes.Contains("rnt_segment") ? (int?)item.GetAttributeValue<OptionSetValue>("rnt_segment").Value : null,
                    showRoomBrandId = item.Attributes.Contains("rnt_showroombrandid") ? (Guid?)(item.GetAttributeValue<EntityReference>("rnt_showroombrandid").Id) : null,
                    showRoomBrandName = item.Attributes.Contains("rnt_showroombrandid") ? item.GetAttributeValue<EntityReference>("rnt_showroombrandid").Name : null,
                    showRoomModelId = item.Attributes.Contains("rnt_showroommodelid") ? (Guid?)(item.GetAttributeValue<EntityReference>("rnt_showroommodelid").Id) : null,
                    showRoomModelName = item.Attributes.Contains("rnt_showroommodelid") ? item.GetAttributeValue<EntityReference>("rnt_showroommodelid").Name : null,
                    youngDriverAge = item.Attributes.Contains("rnt_youngdriverage") ? (int?)item.GetAttributeValue<int>("rnt_youngdriverage") : null,
                    youngDriverMinimumLicense = item.Attributes.Contains("rnt_youngdriverlicence") ? (int?)item.GetAttributeValue<int>("rnt_youngdriverlicence") : null,
                    groupCodeInformationId = item.Id,
                    groupCodeInformationName = item.GetAttributeValue<string>("rnt_name"),
                    gearboxcodeName = gearBoxes.Where(p => p.value.Equals(item.GetAttributeValue<OptionSetValue>("rnt_gearboxcode").Value)).FirstOrDefault()?.label,
                    fueltypecodeName = fuelTypes.Where(p => p.value.Equals(item.GetAttributeValue<OptionSetValue>("rnt_fueltypecode").Value)).FirstOrDefault()?.label,
                    fueltypecode = item.GetAttributeValue<OptionSetValue>("rnt_fueltypecode").Value,
                    gearboxcode = item.GetAttributeValue<OptionSetValue>("rnt_gearboxcode").Value,
                    image = item.GetAttributeValue<string>("rnt_image"),
                    isDoubleCard = item.GetAttributeValue<bool>("rnt_doublecreditcard"),
                    groupCodeDescription = item.GetAttributeValue<string>("rnt_groupcodedefinition"),
                    webImageURL  = item.Attributes.Contains("rnt_webimageurl") ? item.GetAttributeValue<string>("rnt_webimageurl") : string.Empty,
                    colorName = colorNames.Where(p => p.value.Equals(item.GetAttributeValue<OptionSetValue>("rnt_color")?.Value)).FirstOrDefault()?.label.removeAlphaNumericCharactersFromString(),
                    engineVolume = item.Attributes.Contains("rnt_enginevolumeid") ? item.GetAttributeValue<EntityReference>("rnt_enginevolumeid").Name : string.Empty,
                    SIPPCode =  item.Attributes.Contains("rnt_sippcode") ? item.GetAttributeValue<string>("rnt_sippcode") : string.Empty
                };
                data.Add(groupCodeInformationDetailData);
            }
            return data;
        }
        public List<GroupCodeInformationDetailData> getGroupCodesInformationDetail(List<string> ids)
        {
            GroupCodeInformationRepository repository = new GroupCodeInformationRepository(this.OrgService);
            var result = repository.getGroupCodeInformationByGivenIds(ids);

            List<GroupCodeInformationDetailData> data = new List<GroupCodeInformationDetailData>();
            foreach (var item in result)
            {
                GroupCodeInformationDetailData groupCodeInformationDetailData = new GroupCodeInformationDetailData
                {
                    deposit = item.Attributes.Contains("rnt_deposit") ? (decimal?)item.GetAttributeValue<decimal>("rnt_deposit") : null,
                    findeks = item.Attributes.Contains("rnt_findeks") ? (int?)item.GetAttributeValue<int>("rnt_findeks") : null,
                    carModelImage = item.GetAttributeValue<string>("rnt_image"),
                    minimumAge = item.Attributes.Contains("rnt_minimumage") ? (int?)item.GetAttributeValue<int>("rnt_minimumage") : null,
                    minimumDriverLicense = item.Attributes.Contains("rnt_minimumdriverlicence") ? (int?)item.GetAttributeValue<int>("rnt_minimumdriverlicence") : null,
                    segment = item.Attributes.Contains("rnt_segment") ? (int?)item.GetAttributeValue<OptionSetValue>("rnt_segment").Value : null,
                    showRoomBrandId = item.Attributes.Contains("rnt_showroombrandid") ? (Guid?)(item.GetAttributeValue<EntityReference>("rnt_showroombrandid").Id) : null,
                    showRoomBrandName = item.Attributes.Contains("rnt_showroombrandid") ? item.GetAttributeValue<EntityReference>("rnt_showroombrandid").Name : null,
                    showRoomModelId = item.Attributes.Contains("rnt_showroommodelid") ? (Guid?)(item.GetAttributeValue<EntityReference>("rnt_showroommodelid").Id) : null,
                    showRoomModelName = item.Attributes.Contains("rnt_showroommodelid") ? item.GetAttributeValue<EntityReference>("rnt_showroommodelid").Name : null,
                    youngDriverAge = item.Attributes.Contains("rnt_youngdriverage") ? (int?)item.GetAttributeValue<int>("rnt_youngdriverage") : null,
                    youngDriverMinimumLicense = item.Attributes.Contains("rnt_youngdriverlicence") ? (int?)item.GetAttributeValue<int>("rnt_youngdriverlicence") : null,
                    groupCodeInformationId = item.Id,
                    groupCodeInformationName = item.GetAttributeValue<string>("rnt_name"),
                };
                data.Add(groupCodeInformationDetailData);
            }
            return data;
        }

        public GroupCodeInformationDetailData getGroupCodesInformationDetailById(Guid groupCodeInformationId)
        {
            GroupCodeInformationRepository groupCodeInformationRepository = new GroupCodeInformationRepository(this.OrgService);
            var groupCodeData = groupCodeInformationRepository.getGroupCodeInformationById(groupCodeInformationId);

            return new GroupCodeInformationDetailData
            {
                deposit = groupCodeData.Attributes.Contains("rnt_deposit") ? (decimal?)groupCodeData.GetAttributeValue<decimal>("rnt_deposit") : null,
                findeks = groupCodeData.Attributes.Contains("rnt_findeks") ? (int?)groupCodeData.GetAttributeValue<int>("rnt_findeks") : null,
                carModelImage = groupCodeData.GetAttributeValue<string>("rnt_image"),
                minimumAge = groupCodeData.Attributes.Contains("rnt_minimumage") ? (int?)groupCodeData.GetAttributeValue<int>("rnt_minimumage") : null,
                minimumDriverLicense = groupCodeData.Attributes.Contains("rnt_minimumdriverlicence") ? (int?)groupCodeData.GetAttributeValue<int>("rnt_minimumdriverlicence") : null,
                segment = groupCodeData.Attributes.Contains("rnt_segment") ? (int?)groupCodeData.GetAttributeValue<OptionSetValue>("rnt_segment").Value : null,
                showRoomBrandId = groupCodeData.Attributes.Contains("rnt_showroombrandid") ? (Guid?)(groupCodeData.GetAttributeValue<EntityReference>("rnt_showroombrandid").Id) : null,
                showRoomBrandName = groupCodeData.Attributes.Contains("rnt_showroombrandid") ? groupCodeData.GetAttributeValue<EntityReference>("rnt_showroombrandid").Name : null,
                showRoomModelId = groupCodeData.Attributes.Contains("rnt_showroommodelid") ? (Guid?)(groupCodeData.GetAttributeValue<EntityReference>("rnt_showroommodelid").Id) : null,
                showRoomModelName = groupCodeData.Attributes.Contains("rnt_showroommodelid") ? groupCodeData.GetAttributeValue<EntityReference>("rnt_showroommodelid").Name : null,
                youngDriverAge = groupCodeData.Attributes.Contains("rnt_youngdriverage") ? (int?)groupCodeData.GetAttributeValue<int>("rnt_youngdriverage") : null,
                youngDriverMinimumLicense = groupCodeData.Attributes.Contains("rnt_youngdriverlicence") ? (int?)groupCodeData.GetAttributeValue<int>("rnt_youngdriverlicence") : null,
                groupCodeInformationId = groupCodeData.Id,
                groupCodeInformationName = groupCodeData.GetAttributeValue<string>("rnt_name"),
                gearboxcodeName = groupCodeData.FormattedValues["rnt_gearboxcode"],
                fueltypecodeName = groupCodeData.FormattedValues["rnt_fueltypecode"],
                image = groupCodeData.GetAttributeValue<string>("rnt_image"),
                stateCode = groupCodeData.GetAttributeValue<OptionSetValue>("statecode").Value,
            };
        }

        public GroupCodeInformationDetailDataForDocument getGroupCodeInformationDetailForDocument(Guid groupCodeInformationId)
        {
            GroupCodeInformationRepository repository = new GroupCodeInformationRepository(this.OrgService);
            var item = repository.getGroupCodeInformationForDocument(Convert.ToString(groupCodeInformationId));
            var _upgradeGroupCodes = item.GetAttributeValue<OptionSetValueCollection>("rnt_upgradegroupcodes"); 

            MultiSelectMappingRepository multiSelectMappingRepository = new MultiSelectMappingRepository(this.OrgService); 
            var _upgradeGroupCodesList = multiSelectMappingRepository.getSelectedItemsGuidListByOptionValueandGivenColumn("rnt_groupcode", _upgradeGroupCodes);
            
            GroupCodeInformationDetailDataForDocument groupCodeInformationDetailData = new GroupCodeInformationDetailDataForDocument
            {
                doubleCreditCard = item.GetAttributeValue<bool>("rnt_doublecreditcard"),
                findeks = item.GetAttributeValue<int>("rnt_findeks"),
                minimumAge = item.GetAttributeValue<int>("rnt_minimumage"),
                minimumDriverLicense = item.GetAttributeValue<int>("rnt_minimumdriverlicence"),
                youngDriverAge = item.GetAttributeValue<int>("rnt_youngdriverage"),
                youngDriverLicense = item.GetAttributeValue<int>("rnt_youngdriverlicence"),
                overKilometerPrice = item.GetAttributeValue<Money>("rnt_overkilometerprice").Value,
                depositAmount = item.GetAttributeValue<decimal>("rnt_deposit"),
                segmentCode = item.GetAttributeValue<OptionSetValue>("rnt_segment").Value,
                upgradeGroupCodes = _upgradeGroupCodesList, 

            };
            return groupCodeInformationDetailData;
        }

        public List<GroupCodeImageData> GetAllGroupCodeImages()
        {
            GroupCodeInformationRepository repository = new GroupCodeInformationRepository(this.OrgService);
            var result = repository.getAllGroupCodeImages();

            List<GroupCodeImageData> data = new List<GroupCodeImageData>();
            foreach (var item in result)
            {
                GroupCodeImageData groupCodeImage = new GroupCodeImageData() 
                {
                    groupCodeImageId = item.Id,
                    imageUrl = item.GetAttributeValue<string>("rnt_name"),
                    groupCodeInformationId = item.GetAttributeValue<EntityReference>("rnt_groupcodeinformationid").Id,
                    groupCodeInformationName = item.GetAttributeValue<EntityReference>("rnt_groupcodeinformationid").Name
                };
                data.Add(groupCodeImage);
            }

            return data;
        }

    }
}
