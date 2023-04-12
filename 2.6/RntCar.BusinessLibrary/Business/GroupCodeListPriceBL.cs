using Microsoft.Xrm.Sdk;
using Newtonsoft.Json;
using RestSharp;
using RntCar.BusinessLibrary.Handlers;
using RntCar.BusinessLibrary.Repository;
using RntCar.ClassLibrary;
using RntCar.ClassLibrary.GroupCodeList.Validation;
using RntCar.ClassLibrary.MongoDB;
using RntCar.SDK.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.BusinessLibrary.Business
{
    public class GroupCodeListPriceBL : BusinessHandler
    {
        #region CONSTRUCTORS
        public GroupCodeListPriceBL(IOrganizationService orgService) : base(orgService)
        {
        }
        public GroupCodeListPriceBL(IOrganizationService orgService, ITracingService tracingService) : base(orgService, tracingService)
        {
        }
        #endregion

        #region METHODS
        public MongoDBResponse CreateGroupCodeListPriceInMongoDB(Entity entity)
        {
            Entity groupCodeListPrice = entity;

            ConfigurationBL configurationBL = new ConfigurationBL(this.OrgService);

            var responseUrl = configurationBL.GetConfigurationByName("MongoDBWepAPIURL");

            RestSharpHelper restSharpHelper = new RestSharpHelper(responseUrl, "CreateGroupCodeListPriceInMongoDB", Method.POST);

            var groupCodeListPriceData = this.BuildMongoDBGroupCodeListData(groupCodeListPrice);
            this.TracingService.Trace("after build class");
            restSharpHelper.AddJsonParameter<GroupCodeListPriceData>(groupCodeListPriceData);

            var responseGroupCodeListPrice = restSharpHelper.Execute<MongoDBResponse>();

            if (!responseGroupCodeListPrice.Result)
            {
                return MongoDBResponse.ReturnError(responseGroupCodeListPrice.ExceptionDetail);
            }

            return responseGroupCodeListPrice;
        }
        public MongoDBResponse UpdateGroupCodeListPriceInMongoDB(Entity entity)
        {
            Entity groupCodeListPrice = entity;

            ConfigurationBL configurationBL = new ConfigurationBL(this.OrgService);

            var responseUrl = configurationBL.GetConfigurationByName("MongoDBWepAPIURL");

            RestSharpHelper restSharpHelper = new RestSharpHelper(responseUrl, "UpdateGroupCodeListPriceInMongoDB", Method.POST);

            var groupCodeListPriceData = this.BuildMongoDBGroupCodeListData(groupCodeListPrice);
            this.TracingService.Trace("after build class");
            restSharpHelper.AddJsonParameter<GroupCodeListPriceData>(groupCodeListPriceData);
            restSharpHelper.AddQueryParameter("id", entity.GetAttributeValue<string>("rnt_mongodbid"));

            var responseGroupCodeListPrice = restSharpHelper.Execute<MongoDBResponse>();

            if (!responseGroupCodeListPrice.Result)
            {
                return MongoDBResponse.ReturnError(responseGroupCodeListPrice.ExceptionDetail);
            }

            return responseGroupCodeListPrice;
        }

        public GroupCodeListPriceData BuildMongoDBGroupCodeListData(Entity groupCodeListPrice)
        {

            GroupCodeListPriceData groupCodeListPriceData = new GroupCodeListPriceData
            {
                GroupCodeListPriceId = Convert.ToString(groupCodeListPrice.Id),
                Name = groupCodeListPrice.GetAttributeValue<string>("rnt_name"),
                CreatedBy = Convert.ToString(groupCodeListPrice.GetAttributeValue<EntityReference>("createdby").Id),
                ModifiedBy = Convert.ToString(groupCodeListPrice.GetAttributeValue<EntityReference>("modifiedby").Id),
                CreatedOn = groupCodeListPrice.GetAttributeValue<DateTime>("createdon"),
                ModifiedOn = groupCodeListPrice.GetAttributeValue<DateTime>("modifiedon"),
                GroupCodeInformationId = Convert.ToString(groupCodeListPrice.GetAttributeValue<EntityReference>("rnt_groupcodeid").Id),
                GroupCodeInformationName = groupCodeListPrice.GetAttributeValue<EntityReference>("rnt_groupcodeid").Name,
                ListPrice = groupCodeListPrice.Attributes.Contains("rnt_listprice") ?
                             groupCodeListPrice.GetAttributeValue<Money>("rnt_listprice").Value :
                             decimal.Zero,
                MinimumDay = groupCodeListPrice.GetAttributeValue<int>("rnt_minimumday"),
                MaximumDay = groupCodeListPrice.GetAttributeValue<int>("rnt_maximumday"),
                PriceListId = Convert.ToString(groupCodeListPrice.GetAttributeValue<EntityReference>("rnt_pricelistid").Id),
                PriceListName = groupCodeListPrice.GetAttributeValue<EntityReference>("rnt_pricelistid").Name,
                Status = groupCodeListPrice.GetAttributeValue<OptionSetValue>("statuscode").Value,
                State = groupCodeListPrice.GetAttributeValue<OptionSetValue>("statecode").Value,

            };
            return groupCodeListPriceData;
        }

        public void createGroupCodeListPriceFromPriceCalculation(List<GroupCodeRelationData> groupCodesData, List<GroupCodeListPriceRelationData> groupCodeListPricesData, Guid priceListId)
        {
            Entity e = new Entity("rnt_listprice");

            //create listprice for each groupCodes data
            foreach (var item in groupCodesData)
            {
                foreach (var groupCodeListPrices in groupCodeListPricesData)
                {
                    if (item.rnt_price.HasValue)
                    {
                        e.Attributes["rnt_name"] = item.rnt_name + " Grubu " + groupCodeListPrices.rnt_minimumday + " - " + groupCodeListPrices.rnt_maximumday + " Gün Fiyat";
                        e.Attributes["rnt_groupcodeid"] = new EntityReference("rnt_groupcodeinformations", new Guid(item.rnt_groupcodeinformationsId));
                        e.Attributes["rnt_pricelistid"] = new EntityReference("rnt_pricelist", priceListId);
                        e.Attributes["rnt_minimumday"] = groupCodeListPrices.rnt_minimumday;
                        e.Attributes["rnt_maximumday"] = groupCodeListPrices.rnt_maximumday;
                        e.Attributes["rnt_listprice"] = new Money(item.rnt_price.Value - (item.rnt_price.Value * (groupCodeListPrices.rnt_ratio / 100)));
                        e["rnt_mongodbintegrationtrigger"] = StaticHelper.RandomDigits(10);
                        this.OrgService.Create(e);
                    }
                }
            }

        }

        // Tolga AYKURT - 11.03.2019
        public bool ValidateGroupCodeListForCreate(GroupCodeListValidationInput groupCodeListValidationInput, out string validationMessage)
        {
            var groupCodeListRepository = new GroupCodeListRepository(OrgService);
            var groupCodeLists = groupCodeListRepository.GetGroupCodeLists(groupCodeListValidationInput);
            validationMessage = null;
            bool isOK = true;
            XrmHelper xrmHelper;

            isOK = ValidateBeginDateEndDate(groupCodeListValidationInput.MinDay, groupCodeListValidationInput.MaxDay);

            if (isOK == false)
            {
                xrmHelper = new XrmHelper(OrgService);
                validationMessage = xrmHelper.GetXmlTagContent(groupCodeListValidationInput.InitiatingUserId, "MinMaxDayValidation");
            }

            if (groupCodeLists != null && groupCodeLists.Entities.Count > 0 && isOK == true)
            {
                xrmHelper = new XrmHelper(OrgService);
                validationMessage = string.Format(xrmHelper.GetXmlTagContent(groupCodeListValidationInput.InitiatingUserId, "GroupCodeListValidation"), groupCodeLists.Entities[0].GetAttributeValue<string>("rnt_name"));
                isOK = false;
            }

            return isOK;
        }

        // Tolga AYKURT - 11.03.2019
        public bool ValidateGroupCodeListForUpdate(GroupCodeListValidationInput groupCodeListValidationInput, Guid groupCodeListId, out string validationMessage)
        {
            var groupCodeListRepository = new GroupCodeListRepository(OrgService);
            var groupCodeLists = groupCodeListRepository.GetGroupCodeLists(groupCodeListValidationInput);
            validationMessage = null;
            bool isOK = true;

            XrmHelper xrmHelper;

            isOK = ValidateBeginDateEndDate(groupCodeListValidationInput.MinDay, groupCodeListValidationInput.MaxDay);

            if (isOK == false)
            {
                xrmHelper = new XrmHelper(OrgService);
                validationMessage = xrmHelper.GetXmlTagContent(groupCodeListValidationInput.InitiatingUserId, "MinMaxDayValidation");
            }

            if (groupCodeLists != null && groupCodeLists.Entities.Count > 0 && isOK == true)
            {
                var groupCodeListEntity = groupCodeLists.Entities.Where(grp => grp.Id.Equals(groupCodeListId)).FirstOrDefault();

                if (groupCodeListEntity != null && groupCodeListEntity.Id != default(Guid))
                {
                    groupCodeLists.Entities.Remove(groupCodeListEntity);
                }

                if (groupCodeLists.Entities.Count > 0)
                {
                    xrmHelper = new XrmHelper(OrgService);
                    validationMessage = string.Format(xrmHelper.GetXmlTagContent(groupCodeListValidationInput.InitiatingUserId, "GroupCodeListValidation"), groupCodeLists.Entities[0].GetAttributeValue<string>("rnt_name"));
                    isOK = false;
                }
            }

            return isOK;
        }
        #endregion

        #region HELPER METHODS
        // Tolga AYKURT - 14.03.2029
        private bool ValidateBeginDateEndDate(int minDay, int maxDay)
        {
            return maxDay >= minDay;
        }
        #endregion
    }
}
