using Microsoft.Xrm.Sdk;
using Newtonsoft.Json;
using RestSharp;
using RntCar.BusinessLibrary.Handlers;
using RntCar.BusinessLibrary.Repository;
using RntCar.ClassLibrary;
using RntCar.ClassLibrary.MongoDB;
using RntCar.SDK.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.BusinessLibrary.Business
{
    public class AvailabilityPriceListBL : BusinessHandler
    {
        #region CONSTRUCTORS
        public AvailabilityPriceListBL(IOrganizationService orgService) : base(orgService)
        {
        }
        #endregion

        #region METHODS
        public AvailabilityPriceListBL(IOrganizationService orgService, ITracingService tracingService) : base(orgService, tracingService)
        {
        }
        public MongoDBResponse CreateAvailabilityPriceListInMongoDB(Entity entity)
        {
            Entity availabilityPriceList = entity;

            ConfigurationBL configurationBL = new ConfigurationBL(this.OrgService);

            var responseUrl = configurationBL.GetConfigurationByName("MongoDBWepAPIURL");

            RestSharpHelper restSharpHelper = new RestSharpHelper(responseUrl, "CreateAvailabilityPriceListInMongoDB", Method.POST);

            var availabilityPriceListData = this.BuildMongoDBAvailabilityPriceListData(availabilityPriceList);
            this.TracingService.Trace("after build class");
            restSharpHelper.AddJsonParameter<AvailabilityPriceListData>(availabilityPriceListData);

            var responseGroupCodeListPrice = restSharpHelper.Execute<MongoDBResponse>();

            if (!responseGroupCodeListPrice.Result)
            {
                return MongoDBResponse.ReturnError(responseGroupCodeListPrice.ExceptionDetail);
            }

            return responseGroupCodeListPrice;
        }
        public MongoDBResponse UpdateAvailabilityPriceListInMongoDB(Entity entity)
        {
            Entity availabilityPriceList = entity;

            ConfigurationBL configurationBL = new ConfigurationBL(this.OrgService);

            var responseUrl = configurationBL.GetConfigurationByName("MongoDBWepAPIURL");

            RestSharpHelper restSharpHelper = new RestSharpHelper(responseUrl, "UpdateAvailabilityPriceListInMongoDB", Method.POST);

            var availabilityPriceListData = this.BuildMongoDBAvailabilityPriceListData(availabilityPriceList);
            this.Trace("after build class");
            restSharpHelper.AddJsonParameter<AvailabilityPriceListData>(availabilityPriceListData);
            restSharpHelper.AddQueryParameter("id", entity.GetAttributeValue<string>("rnt_mongodbid"));

            var responseGroupCodeListPrice = restSharpHelper.Execute<MongoDBResponse>();

            if (!responseGroupCodeListPrice.Result)
            {
                return MongoDBResponse.ReturnError(responseGroupCodeListPrice.ExceptionDetail);
            }

            return responseGroupCodeListPrice;
        }
        public AvailabilityPriceListData BuildMongoDBAvailabilityPriceListData(Entity availabilityPriceList)
        {

            AvailabilityPriceListData groupCodeListPriceData = new AvailabilityPriceListData
            {
                AvailabilityPriceListId = Convert.ToString(availabilityPriceList.Id),
                Name = availabilityPriceList.GetAttributeValue<string>("rnt_name"),
                CreatedBy = Convert.ToString(availabilityPriceList.GetAttributeValue<EntityReference>("createdby").Id),
                ModifiedBy = Convert.ToString(availabilityPriceList.GetAttributeValue<EntityReference>("modifiedby").Id),
                CreatedOn = availabilityPriceList.GetAttributeValue<DateTime>("createdon"),
                ModifiedOn = availabilityPriceList.GetAttributeValue<DateTime>("modifiedon"),
                MinimumAvailability = availabilityPriceList.GetAttributeValue<int>("rnt_minimumavailability"),
                MaximumAvailability = availabilityPriceList.GetAttributeValue<int>("rnt_maximumavailability"),
                PriceListId = Convert.ToString(availabilityPriceList.GetAttributeValue<EntityReference>("rnt_pricelistid").Id),
                PriceListName = availabilityPriceList.GetAttributeValue<EntityReference>("rnt_pricelistid").Name,
                StateCode = availabilityPriceList.GetAttributeValue<OptionSetValue>("statecode").Value,
                StatusCode = availabilityPriceList.GetAttributeValue<OptionSetValue>("statuscode").Value,
                PriceChangeRate = availabilityPriceList.GetAttributeValue<decimal>("rnt_pricechangerate"),
                groupCodeId = availabilityPriceList.Contains("rnt_groupcodeid") ? availabilityPriceList.GetAttributeValue<EntityReference>("rnt_groupcodeid").Id : Guid.Empty,
                groupCodeName = availabilityPriceList.Contains("rnt_groupcodeid")?  availabilityPriceList.GetAttributeValue<EntityReference>("rnt_groupcodeid").Name : null
            };
            return groupCodeListPriceData;
        }

        public void createAvailabilityPriceListFromPriceCalculation(List<AvailabilityPriceListRelationData> availabilityPriceListData, Guid priceListId)
        {
            Entity e = new Entity("rnt_availabilitypricelist");

            foreach (var item in availabilityPriceListData)
            {
                e.Attributes["rnt_name"] = item.rnt_name;
                e.Attributes["rnt_maximumavailability"] = item.rnt_maximumavailability;
                e.Attributes["rnt_minimumavailability"] = item.rnt_minimumavailability;
                e.Attributes["rnt_pricechangerate"] = item.rnt_ratio;
                e.Attributes["rnt_pricelistid"] = new EntityReference("rnt_pricelist", priceListId);
                e["rnt_mongodbintegrationtrigger"] = StaticHelper.RandomDigits(10);
                this.OrgService.Create(e);
            }
        }

        // Tolga AYKURT - 11.03.2019
        public bool ValidateAvailabilityPriceListForCreate(AvailabilityPriceListValidationInput availabilityPriceListValidationInput, out string validationMessage)
        {
            var repo = new AvailabilityPriceListRepository(OrgService);
            var records = repo.GetAvailabilityPriceList(availabilityPriceListValidationInput);
            bool isOK = true;
            validationMessage = null;
            XrmHelper xrmHelper;

            isOK = MinMaxValidation(availabilityPriceListValidationInput.MinimumAvailability, availabilityPriceListValidationInput.MaximumAvailability);

            if(isOK == false)
            {
                xrmHelper = new XrmHelper(OrgService);
                validationMessage = xrmHelper.GetXmlTagContent(availabilityPriceListValidationInput.InitiatingUserId, "MinMaxAvailabilityValidation");
            }

            if (records != null && records.Entities.Count > 0 && isOK == true)
            {
                isOK = false;
                xrmHelper = new XrmHelper(OrgService);
                validationMessage = string.Format(xrmHelper.GetXmlTagContent(availabilityPriceListValidationInput.InitiatingUserId, "AvailabilityPriceListValidation"), records.Entities[0].GetAttributeValue<string>("rnt_name"));
            }

            return isOK;
        }

        // Tolga AYKURT - 11.03.2019
        public bool ValidateAvailabilityPriceListForUpdate(AvailabilityPriceListValidationInput availabilityPriceListValidationInput,  Guid recordId, out string validationMessage)
        {
            var repo = new AvailabilityPriceListRepository(OrgService);
            var records = repo.GetAvailabilityPriceList(availabilityPriceListValidationInput);
            bool isOK = true;
            validationMessage = null;
            XrmHelper xrmHelper;

            isOK = MinMaxValidation(availabilityPriceListValidationInput.MinimumAvailability, availabilityPriceListValidationInput.MaximumAvailability);

            if (isOK == false)
            {
                xrmHelper = new XrmHelper(OrgService);
                validationMessage = xrmHelper.GetXmlTagContent(availabilityPriceListValidationInput.InitiatingUserId, "AvailabilityPriceListValidation");
            }

            if (records != null && records.Entities.Count > 0 && isOK == true)
            {
                var recordEntity = records.Entities.Where(rc => rc.Id.Equals(recordId)).FirstOrDefault();

                if (recordEntity != null && recordEntity.Id != default(Guid))
                {
                    records.Entities.Remove(recordEntity);

                    if(records.Entities.Count > 0)
                    {
                        isOK = false;
                        xrmHelper = new XrmHelper(OrgService);
                        validationMessage = string.Format(xrmHelper.GetXmlTagContent(availabilityPriceListValidationInput.InitiatingUserId, "AvailabilityPriceListValidation"), records.Entities[0].GetAttributeValue<string>("rnt_name"));
                    }
                }
            }

            return isOK;
        }
        #endregion

        #region HELPER METHODS
        // Tolga AYKURT - 14.03.2019
        private bool MinMaxValidation(int minAvailability, int maxAvailability)
        {
            return maxAvailability >= minAvailability;
        }
        #endregion
    }
}
