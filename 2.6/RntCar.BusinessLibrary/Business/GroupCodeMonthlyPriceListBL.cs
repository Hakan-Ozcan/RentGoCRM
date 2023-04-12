using Microsoft.Xrm.Sdk;
using Newtonsoft.Json;
using RestSharp;
using RntCar.BusinessLibrary.Handlers;
using RntCar.ClassLibrary.MongoDB;
using RntCar.ClassLibrary.MonthlyGroupCodePriceList;
using RntCar.SDK.Common;
using System;

namespace RntCar.BusinessLibrary.Business
{
    public class GroupCodeMonthlyPriceListBL : BusinessHandler
    {
        public GroupCodeMonthlyPriceListBL(IOrganizationService orgService) : base(orgService)
        {
        }

        public GroupCodeMonthlyPriceListBL(IOrganizationService orgService, ITracingService tracingService) : base(orgService, tracingService)
        {
        }
        public MongoDBResponse createGroupCodeMonthlyPriceListInMongoDB(Entity monthlyPriceList)
        {
            ConfigurationBL configurationBL = new ConfigurationBL(this.OrgService);

            var responseUrl = configurationBL.GetConfigurationByName("MongoDBWepAPIURL");
            RestSharpHelper restSharpHelper = new RestSharpHelper(responseUrl, "CreateMonthlyGroupCodePriceListInMongoDB", Method.POST);
            var monthlyPriceListData = this.buildMonthlyGroupCodePriceListData(monthlyPriceList);

            restSharpHelper.AddJsonParameter<MonthlyGroupCodePriceListData>(monthlyPriceListData);

            var response = restSharpHelper.Execute<MongoDBResponse>();

            if (!response.Result)
            {
                return MongoDBResponse.ReturnError(response.ExceptionDetail);
            }

            Entity e = new Entity("rnt_monthlygrouppricelist");
            e.Id = monthlyPriceList.Id;
            e["rnt_mongodbcreatedon"] = DateTime.Now;
            this.OrgService.Update(e);
            return response;
        }
        public MongoDBResponse updateGroupCodeMonthlyPriceListInMongoDB(Entity monthlyPriceList)
        {
            ConfigurationBL configurationBL = new ConfigurationBL(this.OrgService);

            var responseUrl = configurationBL.GetConfigurationByName("MongoDBWepAPIURL");
            RestSharpHelper restSharpHelper = new RestSharpHelper(responseUrl, "UpdateMonthlyGroupCodePriceListInMongoDB", Method.POST);
            var monthlyPriceListData = this.buildMonthlyGroupCodePriceListData(monthlyPriceList);

            restSharpHelper.AddJsonParameter<MonthlyGroupCodePriceListData>(monthlyPriceListData);
            restSharpHelper.AddQueryParameter("id", Convert.ToString(monthlyPriceList.Id));

            var response = restSharpHelper.Execute<MongoDBResponse>();

            if (!response.Result)
            {
                return MongoDBResponse.ReturnError(response.ExceptionDetail);
            }

            Entity e = new Entity("rnt_monthlygrouppricelist");
            e.Id = monthlyPriceList.Id;
            e["rnt_mongodbmodifiedon"] = DateTime.Now;
            this.OrgService.Update(e);
            return response;
        }
        private MonthlyGroupCodePriceListData buildMonthlyGroupCodePriceListData(Entity monthlyGroupCodePriceList)
        {
            MonthlyGroupCodePriceListData monthlyGroupCodePriceListData = new MonthlyGroupCodePriceListData
            {
                name = monthlyGroupCodePriceList.GetAttributeValue<string>("rnt_name"),
                amount = monthlyGroupCodePriceList.GetAttributeValue<Money>("rnt_amount").Value,
                groupCodeId = Convert.ToString(monthlyGroupCodePriceList.GetAttributeValue<EntityReference>("rnt_groupcodeid").Id),
                groupCodeName = monthlyGroupCodePriceList.GetAttributeValue<EntityReference>("rnt_groupcodeid").Name,
                month = monthlyGroupCodePriceList.GetAttributeValue<OptionSetValue>("rnt_month").Value,
                monthlyGroupCodeListId = Convert.ToString(monthlyGroupCodePriceList.Id),
                monthlyPriceListId = Convert.ToString(monthlyGroupCodePriceList.GetAttributeValue<EntityReference>("rnt_monthlypricelistid").Id),
                monthlyPriceListName = monthlyGroupCodePriceList.GetAttributeValue<EntityReference>("rnt_monthlypricelistid").Name,
                stateCode = monthlyGroupCodePriceList.GetAttributeValue<OptionSetValue>("statecode").Value,
                statusCode = monthlyGroupCodePriceList.GetAttributeValue<OptionSetValue>("statuscode").Value
            };
            this.Trace(JsonConvert.SerializeObject(monthlyGroupCodePriceListData));
            return monthlyGroupCodePriceListData;
        }
    }
}
