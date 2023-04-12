using Microsoft.Xrm.Sdk;
using Newtonsoft.Json;
using RestSharp;
using RntCar.BusinessLibrary.Handlers;
using RntCar.BusinessLibrary.Repository;
using RntCar.ClassLibrary.MongoDB;
using RntCar.ClassLibrary.MonthlyPriceList;
using RntCar.SDK.Common;
using System;

namespace RntCar.BusinessLibrary.Business
{
    public class MonthlyPriceListBL : BusinessHandler
    {
        public MonthlyPriceListBL(IOrganizationService orgService) : base(orgService)
        {
        }

        public MonthlyPriceListBL(IOrganizationService orgService, ITracingService tracingService) : base(orgService, tracingService)
        {
        }
        public MongoDBResponse createMonthlyPriceListInMongoDB(Entity monthlyPriceList)
        {
            ConfigurationBL configurationBL = new ConfigurationBL(this.OrgService);

            var responseUrl = configurationBL.GetConfigurationByName("MongoDBWepAPIURL");
            RestSharpHelper restSharpHelper = new RestSharpHelper(responseUrl, "CreateMonthlyPriceListInMongoDB", Method.POST);
            var monthlyPriceListData = this.buildMonthlyPriceListData(monthlyPriceList);

            restSharpHelper.AddJsonParameter<MonthlyPriceListData>(monthlyPriceListData);

            var response = restSharpHelper.Execute<MongoDBResponse>();

            if (!response.Result)
            {
                return MongoDBResponse.ReturnError(response.ExceptionDetail);
            }

            Entity e = new Entity("rnt_monthlypricelist");
            e.Id = monthlyPriceList.Id;
            e["rnt_mongodbcreatedon"] = DateTime.Now;
            this.OrgService.Update(e);
            return response;
        }
        public MongoDBResponse updateMonthlyPriceListInMongoDB(Entity monthlyPriceList)
        {
            ConfigurationBL configurationBL = new ConfigurationBL(this.OrgService);

            var responseUrl = configurationBL.GetConfigurationByName("MongoDBWepAPIURL");
            RestSharpHelper restSharpHelper = new RestSharpHelper(responseUrl, "UpdateMonthlyPriceListInMongoDB", Method.POST);
            var monthlyPriceListData = this.buildMonthlyPriceListData(monthlyPriceList);

            restSharpHelper.AddJsonParameter<MonthlyPriceListData>(monthlyPriceListData);
            restSharpHelper.AddQueryParameter("id", Convert.ToString(monthlyPriceList.Id));

            var response = restSharpHelper.Execute<MongoDBResponse>();

            if (!response.Result)
            {
                return MongoDBResponse.ReturnError(response.ExceptionDetail);
            }

            Entity e = new Entity("rnt_monthlypricelist");
            e.Id = monthlyPriceList.Id;
            e["rnt_mongodbmodifiedon"] = DateTime.Now;
            this.OrgService.Update(e);
            return response;
        }
        private MonthlyPriceListData buildMonthlyPriceListData(Entity monthlyPriceList)
        {

            CurrencyRepository currencyRepository = new CurrencyRepository(this.OrgService);
            var symbol = currencyRepository.getCurrencyCode(monthlyPriceList.GetAttributeValue<EntityReference>("transactioncurrencyid").Id);
            MonthlyPriceListData monthlyPriceListData = new MonthlyPriceListData
            {
                transactionCurrencyId = monthlyPriceList.GetAttributeValue<EntityReference>("transactioncurrencyid").Id.ToString(),
                transactionCurrencyName = monthlyPriceList.GetAttributeValue<EntityReference>("transactioncurrencyid").Id.ToString(),
                currencyCode = symbol,
                name = monthlyPriceList.GetAttributeValue<string>("rnt_name"),
                priceType = monthlyPriceList.GetAttributeValue<OptionSetValue>("rnt_pricetype").Value,
                statusCode = monthlyPriceList.GetAttributeValue<OptionSetValue>("statuscode").Value,
                stateCode = monthlyPriceList.GetAttributeValue<OptionSetValue>("statecode").Value,
                monthlyPriceListId = Convert.ToString(monthlyPriceList.Id),
                priceCodeId = monthlyPriceList.Contains("rnt_pricecodeid") ?
                              Convert.ToString(monthlyPriceList.GetAttributeValue<EntityReference>("rnt_pricecodeid").Id) :
                              null,
                priceCodeName = monthlyPriceList.Contains("rnt_pricecodeid") ?
                              Convert.ToString(monthlyPriceList.GetAttributeValue<EntityReference>("rnt_pricecodeid").Name) :
                              null,

            };
            this.Trace(JsonConvert.SerializeObject(monthlyPriceListData));
            return monthlyPriceListData;
        }
    }
}
