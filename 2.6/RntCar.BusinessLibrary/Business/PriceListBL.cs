using Microsoft.Xrm.Sdk;
using Newtonsoft.Json;
using RestSharp;
using RntCar.BusinessLibrary.Handlers;
using RntCar.BusinessLibrary.Repository;
using RntCar.ClassLibrary;
using RntCar.ClassLibrary.MongoDB;
using RntCar.ClassLibrary.PriceList.Validation;
using RntCar.SDK.Common;
using System;
using System.Linq;

namespace RntCar.BusinessLibrary.Business
{
    public class PriceFactorValidation : BusinessHandler
    {
        #region CONSTRUCTORS
        public PriceFactorValidation(IOrganizationService orgService) : base(orgService)
        {
        }
        public PriceFactorValidation(IOrganizationService orgService, ITracingService tracingService) : base(orgService, tracingService)
        {
        }
        #endregion

        #region METHODS
        public MongoDBResponse ExecutePriceListActionInMongoDB(Entity entity, string messageName)
        {
            try
            {

                OrganizationRequest request = new OrganizationRequest("rnt_ExecutePriceListInMongoDB");
                request["MessageName"] = messageName;
                request["PriceListEntity"] = entity;

                this.TracingService.Trace("calling action");

                var response = this.OrgService.Execute(request);

                var result = Convert.ToString(response.Results["ExecutionResult"]);
                var mongodbId = Convert.ToString(response.Results["ID"]);
                this.TracingService.Trace("result" + result);
                this.TracingService.Trace("mongodbId" + mongodbId);

                if (!string.IsNullOrEmpty(result))
                {
                    return MongoDBResponse.ReturnError(result);
                }

                return MongoDBResponse.ReturnSuccessWithId(mongodbId);

            }
            catch (Exception ex)
            {
                return MongoDBResponse.ReturnError(ex.Message);
            }
        }


        public MongoDBResponse CreatePriceListInMongoDB(Entity entity)
        {
            Entity priceList = entity;

            ConfigurationBL configurationBL = new ConfigurationBL(this.OrgService);

            var responseUrl = configurationBL.GetConfigurationByName("MongoDBWepAPIURL");

            RestSharpHelper restSharpHelper = new RestSharpHelper(responseUrl, "CreatePriceListInMongoDB", Method.POST);

            var priceListData = this.BuildMongoDBPriceListData(priceList);
            restSharpHelper.AddJsonParameter<PriceListData>(priceListData);

            var responsePrice = restSharpHelper.Execute<MongoDBResponse>();
            this.TracingService.Trace("response" + JsonConvert.SerializeObject(responsePrice));
            if (!responsePrice.Result)
            {
                return MongoDBResponse.ReturnError(responsePrice.ExceptionDetail);
            }

            return responsePrice;
        }

        public MongoDBResponse UpdatePriceListInMongoDB(Entity entity)
        {

            Entity priceList = entity;
            //todo null check            
            //first get the webapi url from crm config
            ConfigurationBL configurationBL = new ConfigurationBL(this.OrgService);
            var responseUrl = configurationBL.GetConfigurationByName("MongoDBWepAPIURL");
            //request initilization
            RestSharpHelper helper = new RestSharpHelper(responseUrl, "UpdatePriceListInMongoDB", Method.POST);
            //prepare parameters
            var reservationItemData = this.BuildMongoDBPriceListData(priceList);
            //add parameter to body 
            helper.AddJsonParameter<PriceListData>(reservationItemData);
            helper.AddQueryParameter("id", priceList.GetAttributeValue<string>("rnt_mongodbid"));

            var responsePriceList = helper.Execute<MongoDBResponse>();
            //check response
            if (!responsePriceList.Result)
            {
                return MongoDBResponse.ReturnError(responsePriceList.ExceptionDetail);
            }

            return responsePriceList;
        }

        public Guid createPriceListFromPriceCalculation(PriceListRelationData priceListParameter)
        {
            Entity e = new Entity("rnt_pricelist");
            e.Attributes["rnt_name"] = priceListParameter.rnt_begindate.ToString("dd/MM/yyyy") + " - " + priceListParameter.rnt_enddate.ToString("dd/MM/yyyy") + " - " + priceListParameter.rnt_name;
            e.Attributes["rnt_pricecodeid"] = new EntityReference("rnt_pricecode", priceListParameter.rnt_pricecodeid);
            e.Attributes["rnt_pricetypecode"] = new OptionSetValue(priceListParameter.rnt_pricetype);
            e.Attributes["rnt_begindate"] = priceListParameter.rnt_begindate;
            e.Attributes["rnt_enddate"] = priceListParameter.rnt_enddate;
            e.Attributes["rnt_mongodbintegrationtrigger"] = StaticHelper.RandomDigits(10);
            return this.OrgService.Create(e);
        }
        internal PriceListData BuildMongoDBPriceListData(Entity priceList)
        {
            CurrencyRepository currencyRepository = new CurrencyRepository(this.OrgService);
            var code = currencyRepository.getCurrencyCode(priceList.GetAttributeValue<EntityReference>("transactioncurrencyid").Id);

            PriceListData priceListData = new PriceListData
            {
                BeginDate = priceList.GetAttributeValue<DateTime>("rnt_begindate"),
                EndDate = priceList.GetAttributeValue<DateTime>("rnt_enddate"),
                PriceListName = priceList.GetAttributeValue<string>("rnt_name"),
                Status = priceList.GetAttributeValue<OptionSetValue>("statuscode").Value,
                State = priceList.GetAttributeValue<OptionSetValue>("statecode").Value,
                PriceType = priceList.GetAttributeValue<OptionSetValue>("rnt_pricetypecode").Value,
                PriceListId = priceList.Id.ToString(),
                PriceCodeId = Convert.ToString(priceList.GetAttributeValue<EntityReference>("rnt_pricecodeid").Id),
                transactionCurrencyId = priceList.GetAttributeValue<EntityReference>("transactioncurrencyid").Id,
                transactionCurrencyName = priceList.GetAttributeValue<EntityReference>("transactioncurrencyid").Name,
                currencyCode = code
            };
            return priceListData;
        }
        /// <summary>
        /// PriceList ile ilgili validasyonları create mesajı için yürütür.
        /// </summary>
        /// <param name="priceListValidationInput"></param>
        /// <returns>Verilen parametreler validasyondan geçemezlerse hata mesajı dönülür.</returns>
        public string PriceValidationForCreate(PriceListValidationInput priceListValidationInput)
        {
            var xrmHelper = new XrmHelper(OrgService);
            string errorMessage = string.Empty;
            string nameOfExistingPriceListName = null;
            bool isOK = true;

            if (DateValidation(priceListValidationInput.BeginDate, priceListValidationInput.EndDate) == false)
            {
                isOK = false;
                errorMessage = xrmHelper.GetXmlTagContent(priceListValidationInput.PluginInitializerUserId, "BeginDateDontBeGreaterThanEndDate");
            }

            if (IsExistPriceList(
                    priceListValidationInput.BeginDate,
                    priceListValidationInput.EndDate,
                    priceListValidationInput.PriceListTypeCode,
                    priceListValidationInput.PriceCodeId,
                    out nameOfExistingPriceListName) == true &&
                isOK == true)
            {
                errorMessage = String.Format(xrmHelper.GetXmlTagContent(priceListValidationInput.PluginInitializerUserId, "ExistPriceListBetweenDates"), nameOfExistingPriceListName);
            }

            return errorMessage;
        }

        // Tolga AYKURT -11.03.2019
        /// <summary>
        /// PriceList ile ilgili validasyonları update mesajı için yürütür.
        /// </summary>
        /// <param name="priceListValidationInput"></param>
        /// <param name="recordId"></param>
        /// <returns></returns>
        public string PriceValidationForUpdate(PriceListValidationInput priceListValidationInput, Guid recordId)
        {
            var xrmHelper = new XrmHelper(OrgService);
            string errorMessage = string.Empty;
            string nameOfExistingPriceListName = null;
            bool isOK = true;

            if (DateValidation(priceListValidationInput.BeginDate, priceListValidationInput.EndDate) == false)
            {
                isOK = false;
                errorMessage = xrmHelper.GetXmlTagContent(priceListValidationInput.PluginInitializerUserId, "BeginDateDontBeGreaterThanEndDate");
            }

            if (IsExistPriceListForUpdate(
                    priceListValidationInput.BeginDate,
                    priceListValidationInput.EndDate,
                    priceListValidationInput.PriceListTypeCode,
                    priceListValidationInput.PriceCodeId,
                    recordId,
                    out nameOfExistingPriceListName) == true &&
                isOK == true)
            {
                errorMessage = String.Format(xrmHelper.GetXmlTagContent(priceListValidationInput.PluginInitializerUserId, "ExistPriceListBetweenDates"), nameOfExistingPriceListName);
            }

            return errorMessage;
        }
        #endregion

        #region HELPER METHODS
        // Tolga AYKURT - 07.03.2019
        private bool IsExistPriceList(DateTime beginDate, DateTime endDate, int priceTypeCode, Guid priceCodeId, out string nameOfExistingPriceList)
        {
            var priceListRepository = new PriceListRepository(OrgService);
            var priceLists = priceListRepository.GetPriceLists(beginDate, endDate, priceTypeCode, priceCodeId);
            nameOfExistingPriceList = string.Empty;

            if (priceLists != null && priceLists.Entities.Count > 0)
                nameOfExistingPriceList = priceLists.Entities[0].GetAttributeValue<string>("rnt_name");

            return priceLists != null && priceLists.Entities.Count > 0;
        }

        // Tolga AYKURT - 07.03.2019
        private bool IsExistPriceListForUpdate(DateTime beginDate, DateTime endDate, int priceTypeCode, Guid priceCodeId, Guid recordId, out string nameOfExistingPriceList)
        {
            var priceListRepository = new PriceListRepository(OrgService);
            var priceLists = priceListRepository.GetPriceLists(beginDate, endDate, priceTypeCode, priceCodeId);
            nameOfExistingPriceList = string.Empty;

            if (priceLists != null && priceLists.Entities.Count > 0)
            {
                var priceListEntity = priceLists.Entities.Where(pl => pl.Id.Equals(recordId)).FirstOrDefault();

                if (priceListEntity != null) priceLists.Entities.Remove(priceListEntity);

                if (priceLists.Entities.Count > 0)
                    nameOfExistingPriceList = priceLists.Entities[0].GetAttributeValue<string>("rnt_name");
            }

            return priceLists != null && priceLists.Entities.Count > 0;
        }

        // Tolga AYKURT - 07.03.2019
        private bool DateValidation(DateTime beginDate, DateTime endDate)
        {
            return endDate > beginDate;
        }
        #endregion
    }
}
