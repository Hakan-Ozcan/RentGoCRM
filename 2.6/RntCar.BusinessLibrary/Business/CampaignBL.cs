using Microsoft.Xrm.Sdk;
using Newtonsoft.Json;
using RntCar.BusinessLibrary.Handlers;
using RntCar.BusinessLibrary.Repository;
using RntCar.ClassLibrary;
using RntCar.ClassLibrary.MongoDB;
using RntCar.SDK.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RntCar.BusinessLibrary.Business
{
    public class CampaignBL : BusinessHandler
    {
        #region CONSTRUCTORS
        public CampaignBL(IOrganizationService orgService) : base(orgService)
        {
        }

        public CampaignBL(IOrganizationService orgService, ITracingService tracingService) : base(orgService, tracingService)
        {
        }
        #endregion

        #region METHODS
        public MongoDBResponse createCampaignMongoDB(Entity campaign)
        {
            ConfigurationBL configurationBL = new ConfigurationBL(this.OrgService);
            var responseUrl = configurationBL.GetConfigurationByName("MongoDBWepAPIURL");

            RestSharpHelper restSharpHelper = new RestSharpHelper(responseUrl, "CreateCampaignInMongoDB", RestSharp.Method.POST);

            var campaignData = this.buildCampaignData(campaign);

            restSharpHelper.AddJsonParameter<CampaignData>(campaignData);

            var response = restSharpHelper.Execute<MongoDBResponse>();
            if (!response.Result)
            {
                return MongoDBResponse.ReturnError(response.ExceptionDetail);
            }

            return response;
        }

        public MongoDBResponse updateCampaignMongoDB(Entity campaign)
        {
            ConfigurationBL configurationBL = new ConfigurationBL(this.OrgService);
            var responseUrl = configurationBL.GetConfigurationByName("MongoDBWepAPIURL");

            RestSharpHelper restSharpHelper = new RestSharpHelper(responseUrl, "UpdateCampaignInMongoDB", RestSharp.Method.POST);

            var campaignData = this.buildCampaignData(campaign);

            restSharpHelper.AddJsonParameter<CampaignData>(campaignData);
            restSharpHelper.AddQueryParameter("id", campaign.GetAttributeValue<string>("rnt_mongodbid"));

            var response = restSharpHelper.Execute<MongoDBResponse>();
            if (!response.Result)
            {
                return MongoDBResponse.ReturnError(response.ExceptionDetail);
            }
            return response;
        }

        private CampaignData buildCampaignData(Entity campaign)
        {
            MultiSelectMappingRepository multiSelectRepository = new MultiSelectMappingRepository(this.OrgService);
            var branchCode = multiSelectRepository.getSelectedItemsGuidListByOptionValueandGivenColumn("rnt_branch",
                campaign.Attributes.Contains("rnt_branchcode") ? campaign.GetAttributeValue<OptionSetValueCollection>("rnt_branchcode") : null);

            var groupCode = multiSelectRepository.getSelectedItemsGuidListByOptionValueandGivenColumn("rnt_groupcode",
                campaign.Attributes.Contains("rnt_groupcode") ? campaign.GetAttributeValue<OptionSetValueCollection>("rnt_groupcode") : null);

            var additionalProductCode = multiSelectRepository.getSelectedItemsGuidListByOptionValueandGivenColumn("rnt_additionalproduct",
                campaign.Attributes.Contains("rnt_additionalproductcode") ? campaign.GetAttributeValue<OptionSetValueCollection>("rnt_additionalproductcode") : null);

            CampaignGroupCodePricesRepository campaignGroupCodePricesRepository = new CampaignGroupCodePricesRepository(this.OrgService);
            var res = campaignGroupCodePricesRepository.getCampaignGroupCodePricesByCampaignId(campaign.Id);

            List<CampaignGroupCodePrices> prices = new List<CampaignGroupCodePrices>();
            foreach (var item in res)
            {
                CampaignGroupCodePrices p = new CampaignGroupCodePrices
                {
                    groupcodeId = item.GetAttributeValue<EntityReference>("rnt_groupcodeinformationid").Id.ToString(),
                    groupcodeName = item.GetAttributeValue<EntityReference>("rnt_groupcodeinformationid").Name,
                    paynowPrice = item.GetAttributeValue<Money>("rnt_price").Value,
                    paylaterPrice = item.GetAttributeValue<Money>("rnt_paylaterprice").Value,
                };
                prices.Add(p);
            }
            CampaignData campaignData = new CampaignData
            {
                groupCodePrices = prices,
                campaignId = Convert.ToString(campaign.Id),
                description = campaign.Attributes.Contains("rnt_campaigndescription") ? campaign.GetAttributeValue<string>("rnt_campaigndescription") : string.Empty,
                name = campaign.Attributes.Contains("rnt_name") ? campaign.GetAttributeValue<string>("rnt_name") : string.Empty,
                productType = campaign.Attributes.Contains("rnt_producttypecode") ? campaign.GetAttributeValue<OptionSetValue>("rnt_producttypecode").Value : 0,
                campaignType = campaign.Attributes.Contains("rnt_campaigntypecode") ? campaign.GetAttributeValue<OptionSetValue>("rnt_campaigntypecode").Value : 0,
                priceEffect = campaign.Attributes.Contains("rnt_priceeffect") ? campaign.GetAttributeValue<OptionSetValue>("rnt_priceeffect").Value : 0,
                beginingDate = campaign.Attributes.Contains("rnt_beginingdate") ? campaign.GetAttributeValue<DateTime>("rnt_beginingdate") : (DateTime?)null,
                endDate = campaign.Attributes.Contains("rnt_enddate") ? campaign.GetAttributeValue<DateTime>("rnt_enddate") : (DateTime?)null,
                minReservationDay = campaign.Attributes.Contains("rnt_minimumreservationday") ? campaign.GetAttributeValue<int>("rnt_minimumreservationday") : 0,
                maxReservationDay = campaign.Attributes.Contains("rnt_maximumreservationday") ? campaign.GetAttributeValue<int>("rnt_maximumreservationday") : 0,
                reservationTypeCode = campaign.Attributes.Contains("rnt_reservationtype") ?
                                      JsonConvert.SerializeObject(campaign.GetAttributeValue<OptionSetValueCollection>("rnt_reservationtype").Select(p => p.Value)) :
                                      string.Empty,
                reservatinChannelCode = campaign.Attributes.Contains("rnt_reservationchannelcode") ?
                                        JsonConvert.SerializeObject(campaign.GetAttributeValue<OptionSetValueCollection>("rnt_reservationchannelcode").Select(p => p.Value)) :
                                        string.Empty,
                branchCode = JsonConvert.SerializeObject(branchCode),
                groupCode = JsonConvert.SerializeObject(groupCode),
                additionalProductCode = JsonConvert.SerializeObject(additionalProductCode),
                payNowDailyPrice = campaign.Attributes.Contains("rnt_paynowdailyprice") ? campaign.GetAttributeValue<Money>("rnt_paynowdailyprice").Value : 0,
                payLaterDailyPrice = campaign.Attributes.Contains("rnt_paylaterdailyprice") ? campaign.GetAttributeValue<Money>("rnt_paylaterdailyprice").Value : 0,
                additionalProductDailyPrice = campaign.Attributes.Contains("rnt_additionalproductdailyprice") ? campaign.GetAttributeValue<Money>("rnt_additionalproductdailyprice").Value : 0,
                payNowDiscountRatio = campaign.Attributes.Contains("rnt_paynowdiscountratio") ? campaign.GetAttributeValue<decimal>("rnt_paynowdiscountratio") : 0,
                payLaterDiscountRatio = campaign.Attributes.Contains("rnt_paylaterdiscountratio") ? campaign.GetAttributeValue<decimal>("rnt_paylaterdiscountratio") : 0,
                additionalProductDiscountRatio = campaign.Attributes.Contains("rnt_additionalproductdiscount") ? campaign.GetAttributeValue<decimal>("rnt_additionalproductdiscount") : 0,
                createdby = Convert.ToString(campaign.GetAttributeValue<EntityReference>("createdby").Id),
                modifiedby = Convert.ToString(campaign.GetAttributeValue<EntityReference>("modifiedby").Id),
                createdon = campaign.GetAttributeValue<DateTime>("createdon"),
                modifiedon = campaign.GetAttributeValue<DateTime>("modifiedon"),
                statecode = campaign.GetAttributeValue<OptionSetValue>("statecode").Value,
                statuscode = campaign.GetAttributeValue<OptionSetValue>("statuscode").Value
            };

            return campaignData;
        }


        public CalculatedCampaignPricesResponse GetCalculatedCampaignPrices(CampaignParameters campaignParameters)
        {
            var response = new CalculatedCampaignPricesResponse();
            response.ResponseResult.Result = true;
            response.CalculatedCampaignPrices = new List<CalculatedCampaignPrice>();

            var currentCampaignParameters = campaignParameters;

            try
            {
                campaignParameters.beginingDate = campaignParameters.beginingDate.AddMilliseconds(-campaignParameters.beginingDate.Millisecond);
                campaignParameters.endDate = campaignParameters.endDate.AddMilliseconds(-campaignParameters.endDate.Millisecond);
                this.Trace("campaignParameters" + JsonConvert.SerializeObject(campaignParameters));

                var configurationBL = new ConfigurationBL(this.OrgService);
                var clientUrl = configurationBL.GetConfigurationByName("MongoDBWepAPIURL");
                this.Trace("mongodb url" + clientUrl);
                var getPriceCalculationSummaries = new RestSharpHelper(clientUrl, "createCampaignPrices", RestSharp.Method.POST);

                var CreateCampaignPricesRequest = new CreateCampaignPricesRequest
                {
                    campaignParameters = campaignParameters
                };
                getPriceCalculationSummaries.AddJsonParameter<CreateCampaignPricesRequest>(new CreateCampaignPricesRequest()
                {
                    campaignParameters = campaignParameters

                });
                var priceCalculationSummariesInMongoDB = getPriceCalculationSummaries.Execute<CreateCampaignPricesResponse>();
                response.CalculatedCampaignPrices = priceCalculationSummariesInMongoDB.calculatedCampaignPrices;
                this.Trace("campaign response is" + JsonConvert.SerializeObject(response));

            }
            catch (Exception ex)
            {
                response.ResponseResult = ResponseResult.ReturnError(ex.Message);
            }

            return response;
        }


        #endregion
    }
}
