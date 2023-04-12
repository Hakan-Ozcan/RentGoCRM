using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using RestSharp;
using RntCar.BusinessLibrary.Handlers;
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
    public class PriceCalculationSummariesBL : BusinessHandler
    {
        public PriceCalculationSummariesBL(IOrganizationService orgService) : base(orgService)
        {
        }

        public PriceCalculationSummariesBL(IOrganizationService orgService, ITracingService tracingService) : base(orgService, tracingService)
        {
        }

        public bool GetPriceCalculationSummaries(Guid contractItemId, Guid groupCodeInformationId, Guid campaignId, string trackingNumber, int totalDuration, Money totalAmount)
        {
            bool isSuccess = false;


            List<PriceCalculationSummaryData> responsePriceCalculationSummaries = GetPriceCalculationSummariesList(groupCodeInformationId, campaignId, trackingNumber);

            if (responsePriceCalculationSummaries.Count == totalDuration)
            {
                foreach (var item in responsePriceCalculationSummaries)
                {
                    item.payLaterAmount = totalAmount.Value / totalDuration;
                    item.payNowAmount = totalAmount.Value / totalDuration;
                    item.totalAmount = totalAmount.Value / totalDuration;
                }
                isSuccess = BulkUpdatePriceCalculationSummariesList(responsePriceCalculationSummaries);
            }
            else if (responsePriceCalculationSummaries.Count < totalDuration && campaignId != Guid.Empty)
            {
                List<PriceCalculationSummaryData> responsePriceCalculationSummariesWithOutCampaign = GetPriceCalculationSummariesList(groupCodeInformationId, Guid.Empty, trackingNumber);
                foreach (var item in responsePriceCalculationSummariesWithOutCampaign)
                {
                    item.campaignId = campaignId;
                }
                isSuccess = BulkUpdatePriceCalculationSummariesList(responsePriceCalculationSummariesWithOutCampaign);
            }

            return isSuccess;
        }

        private List<PriceCalculationSummaryData> GetPriceCalculationSummariesList(Guid groupCodeInformationId, Guid campaignId, string trackingNumber)
        {
            ConfigurationBL configurationBL = new ConfigurationBL(this.OrgService);
            var responseUrl = configurationBL.GetConfigurationByName("MongoDBWepAPIURL");
            //request initilization
            RestSharpHelper helper = new RestSharpHelper(responseUrl, "getPriceCalculationSummaries", Method.POST);

            GetPriceCalculationSummariesRequest getPriceCalculationSummariesRequest = new GetPriceCalculationSummariesRequest();
            getPriceCalculationSummariesRequest.trackingNumber = trackingNumber;
            getPriceCalculationSummariesRequest.groupCodeInformationId = Convert.ToString(groupCodeInformationId);
            if (campaignId != Guid.Empty)
            {
                getPriceCalculationSummariesRequest.campaignId = Convert.ToString(campaignId);
            }

            helper.AddJsonParameter<GetPriceCalculationSummariesRequest>(getPriceCalculationSummariesRequest);

            var responsePriceCalculationSummaries = helper.Execute<List<PriceCalculationSummaryData>>();
            return responsePriceCalculationSummaries;
        }

        private bool BulkUpdatePriceCalculationSummariesList(List<PriceCalculationSummaryData> priceCalculationSummaryDatas)
        {
            ConfigurationBL configurationBL = new ConfigurationBL(this.OrgService);
            var responseUrl = configurationBL.GetConfigurationByName("MongoDBWepAPIURL");
            //request initilization
            RestSharpHelper helper = new RestSharpHelper(responseUrl, "updatePriceCalculationSummaries", Method.POST);


            helper.AddJsonParameter<List<PriceCalculationSummaryData>>(priceCalculationSummaryDatas);

            var responsePriceCalculationSummaries = helper.Execute<MongoDBResponse>();
            return responsePriceCalculationSummaries.Result;
        }

        public bool CheckPriceCalculationSummaries(Guid groupCodeInformationId, Guid campaignId, string trackingNumber, int totalDuration, Money totalAmount)
        {
            bool isSuccess = false;


            List<PriceCalculationSummaryData> responsePriceCalculationSummaries = GetPriceCalculationSummariesList(groupCodeInformationId, campaignId, trackingNumber);

            if (responsePriceCalculationSummaries.Count == totalDuration)
            {
                decimal totalAmontMongo = 0;

                foreach (var item in responsePriceCalculationSummaries)
                {
                    totalAmontMongo += item.totalAmount;
                }

                if (totalAmontMongo == totalAmount.Value)
                {
                    isSuccess = true;
                }
            }

            return isSuccess;
        }


        private void T(Guid contractItemId)
        {
            ContractItemBL contractItemBL = new ContractItemBL(this.OrgService);
            Entity contractItem = this.OrgService.Retrieve("rnt_contractitem", contractItemId, new ColumnSet(true));
            var contractItemData = contractItemBL.buildMongoDBContractItemData(contractItem);

            ConfigurationBL configurationBL = new ConfigurationBL(this.OrgService);
            var responseUrl = configurationBL.GetConfigurationByName("MongoDBWepAPIURL");
            //request initilization
            RestSharpHelper helper = new RestSharpHelper(responseUrl, "CreateContractItemInMongoDB", Method.POST);
            helper.AddJsonParameter<ContractItemData>(contractItemData);
        }
    }
}
