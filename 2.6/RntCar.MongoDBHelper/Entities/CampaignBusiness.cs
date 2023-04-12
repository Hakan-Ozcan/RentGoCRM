using MongoDB.Bson;
using MongoDB.Driver;
using RntCar.ClassLibrary;
using RntCar.ClassLibrary.MongoDB;
using RntCar.MongoDBHelper.Helper;
using RntCar.MongoDBHelper.Model;
using RntCar.MongoDBHelper.Repository;
using RntCar.SDK.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RntCar.MongoDBHelper.Entities
{
    public class CampaignBusiness : MongoDBInstance
    {
        #region CONSTRUCTORS
        public CampaignBusiness(string serverName, string dataBaseName) : base(serverName, dataBaseName)
        {
        }
        #endregion

        #region METHODS
        public MongoDBResponse createCampaign(CampaignData campaignData)
        {
            var collection = this.getCollection<CampaignDataMongoDB>(StaticHelper.GetConfiguration("MongoDBCampaignCollectionName"));

            var campaign = new CampaignDataMongoDB();           
            campaign = campaign.Map(campaignData);
            campaign.BeginingDateTimeStamp = new BsonTimestamp(campaignData.beginingDate.Value.converttoTimeStamp());
            campaign.EndDateTimeStamp = new BsonTimestamp(campaignData.endDate.Value.converttoTimeStamp());
            campaign._id = ObjectId.GenerateNewId();
            
            var methodName = ErrorLogsHelper.GetCurrentMethod();
            var itemId = campaign._id.ToString();

            var response = collection.Insert(campaign, itemId, methodName);
            response.Id = Convert.ToString(campaign._id);

            return response;
        }

        public bool updateCampaign(CampaignData campaignData, string id)
        {
            var collection = this.getCollection<CampaignDataMongoDB>(StaticHelper.GetConfiguration("MongoDBCampaignCollectionName"));

            var campaign = new CampaignDataMongoDB();
            campaign = campaign.Map(campaignData);
            campaign._id = ObjectId.Parse(id);
            campaign.BeginingDateTimeStamp = new BsonTimestamp(campaignData.beginingDate.Value.converttoTimeStamp());
            campaign.EndDateTimeStamp = new BsonTimestamp(campaignData.endDate.Value.converttoTimeStamp());
            
            var methodName = ErrorLogsHelper.GetCurrentMethod();
            var itemId = campaign._id.ToString();

            var filter = Builders<CampaignDataMongoDB>.Filter.Eq(p => p._id, campaign._id);
            var response = collection.Replace(campaign, filter, new UpdateOptions { IsUpsert = false }, itemId, methodName);
            //var response = collection.ReplaceOne(p => p._id == campaign._id, campaign, new UpdateOptions { IsUpsert = false });

            if (response != null)
            {
                if (!response.IsAcknowledged)
                {
                    return true;
                }
                return false;
            }
            else
            {
                return false;
            }
        }

        public CreateCampaignPricesResponse calculateCampaignPrices(CreateCampaignPricesRequest createCampaignPricesRequest,string campaignId)
        {
            //*****************IMPORTANT**************** Naming Misunderstanding//
            //--> createCampaignPricesRequest.campaignParameters.beginingDate equals pickupdatetime
            //-->  createCampaignPricesRequest.campaignParameters.endDate equals dropoffdatetime
            var repo = new PriceCalculationSummariesRepository(StaticHelper.GetConfiguration("MongoDBHostName"), StaticHelper.GetConfiguration("MongoDBDatabaseName"));
            var priceCalculationSummaries = repo.getPricesCalculationSummariesByTrackingNumberandGroupCode(createCampaignPricesRequest.campaignParameters.groupCodeInformationId,
                                                                                                           createCampaignPricesRequest.campaignParameters.calculatedPricesTrackingNumber);
            //*****************IMPORTANT**************** Naming Misunderstanding//

            List<PriceCalculationSummaryData> priceCalculationSummaryDatas = new List<PriceCalculationSummaryData>();
            List<PriceCalculationSummaryMongoDB> priceCalculationSummaryMongoDBs = new List<PriceCalculationSummaryMongoDB>();
            var response = new List<CalculatedCampaignPrice>();

            var totalDays = CommonHelper.calculateTotalDurationInDays(createCampaignPricesRequest.campaignParameters.beginingDate,
                                                                      createCampaignPricesRequest.campaignParameters.endDate);
            var reservationChannelCode = createCampaignPricesRequest.campaignParameters.reservationChannelCode;
            var branchId = createCampaignPricesRequest.campaignParameters.branchId;
            var groupCodeInformationId = createCampaignPricesRequest.campaignParameters.groupCodeInformationId;

            Dictionary<Guid, List<long>> campaignData = new Dictionary<Guid, List<long>>();

            foreach (var summary in priceCalculationSummaries)
            {
                var campaignRepository = new CampaignRepository(StaticHelper.GetConfiguration("MongoDBHostName"), StaticHelper.GetConfiguration("MongoDBDatabaseName"));
                //todo check for double totaldays later
                var campaigns = campaignRepository.GetCampaign(summary.priceDateTimeStamp,
                                                               Convert.ToInt32(totalDays),
                                                               reservationChannelCode,
                                                               branchId,
                                                               createCampaignPricesRequest.campaignParameters.customerType,
                                                               groupCodeInformationId,
                                                               campaignId);

                if(campaigns.Count == 0)
                {
                    return new CreateCampaignPricesResponse
                    {
                        ResponseResult = ResponseResult.ReturnError("campaign not found")
                    };
                }
                //check intersect days for campaigns
                //reservation 10-15
                //campaign 12-16
                //reservation price days --> 10 11 12 13 14
                //campaign price days --> 12 13 14 15
                // so logic is --> kesişen ve kesişmeyen tarihler için pricecalculationsummaries tablosuna campaign id'si dolu olan kayıtlar atılır
                // kesişen tarihlerin fiyatı kampanyalı fiyat olarak atanır
                //kesişmeyen tarihlerin fiyatı kullanabilirlikten gelen fiyat olacaktır
                foreach (var item in campaigns)
                {
                    List<long> internalDates = new List<long>();
                    campaignData.TryGetValue(new Guid(item.campaignId), out internalDates);
                    if (internalDates == null)
                    {
                        internalDates = new List<long>();
                        internalDates.Add(summary.priceDateTimeStamp.Value);
                        campaignData.Add(new Guid(item.campaignId), internalDates);
                    }
                    else
                    {
                        internalDates.Add(summary.priceDateTimeStamp.Value);
                        campaignData[new Guid(item.campaignId)] = internalDates;
                    }
                }

            }
            //iterate for campaigns
            foreach (var campItem in campaignData)
            {
                var campaignRepository = new CampaignRepository(StaticHelper.GetConfiguration("MongoDBHostName"), StaticHelper.GetConfiguration("MongoDBDatabaseName"));
                var campaign = campaignRepository.getCampaingById(campItem.Key.ToString());
                var camp = new CampaignHelper().buildCampaignData(campaign);

                var payNowPrice = decimal.Zero;
                var payLaterPrice = decimal.Zero;

                for (DateTime t = createCampaignPricesRequest.campaignParameters.beginingDate; t < createCampaignPricesRequest.campaignParameters.endDate; t += TimeSpan.FromDays(1))
                {
                    var standartDailyPrice = priceCalculationSummaries.Where(p => p.priceDateTimeStamp == new BsonTimestamp(t.converttoTimeStamp())).FirstOrDefault();

                    var r = campItem.Value.Where(ps => ps == t.converttoTimeStamp()).ToList();
                    if (r.Count == 0)
                    {
                        payNowPrice += standartDailyPrice.payNowAmount;
                        payLaterPrice += standartDailyPrice.payLaterAmount;

                        PriceCalculationSummariesBusiness _priceCalculationSummariesBusiness = new PriceCalculationSummariesBusiness(StaticHelper.GetConfiguration("MongoDBHostName"), StaticHelper.GetConfiguration("MongoDBDatabaseName"));
                        var campaingSummaryPrice = _priceCalculationSummariesBusiness.buildPriceCalculationSummaryObjectfromExisting(standartDailyPrice,
                                                                                                                                     StaticHelper.dummyCampaignId,
                                                                                                                                     standartDailyPrice.payNowAmount,
                                                                                                                                     standartDailyPrice.payLaterAmount);
                        priceCalculationSummaryMongoDBs.Add(campaingSummaryPrice);
                    }
                    else
                    {

                        var price = new PriceBusiness().CalculateCampaignPrice(
                                    camp,
                                    standartDailyPrice.payNowAmount,
                                    standartDailyPrice.payLaterAmount,
                                    standartDailyPrice.isTickDay);

                        payNowPrice += price.payNowDailyPrice.Value;
                        payLaterPrice += price.payLaterDailyPrice.Value;

                        //standartDailyPrice.totalAmount = price.payLaterDailyPrice.Value; 
                        PriceCalculationSummariesBusiness _priceCalculationSummariesBusiness = new PriceCalculationSummariesBusiness(StaticHelper.GetConfiguration("MongoDBHostName"), StaticHelper.GetConfiguration("MongoDBDatabaseName"));
                        var campaingSummaryPrice = _priceCalculationSummariesBusiness.buildPriceCalculationSummaryObjectfromExisting(standartDailyPrice,
                                                                                                                                     campItem.Key,
                                                                                                                                     price.payNowDailyPrice.Value,
                                                                                                                                     price.payLaterDailyPrice.Value);
                        priceCalculationSummaryMongoDBs.Add(campaingSummaryPrice);
                    }
                }
                response.Add(new CalculatedCampaignPrice
                {
                    CampaignInfo = camp,
                    payLaterDailyPrice = payLaterPrice,
                    payNowDailyPrice = payNowPrice
                });
            }
            if (priceCalculationSummaryMongoDBs.Count > 0)
            {
                //now create daily prices for campaigns in price calculation summaries
                PriceCalculationSummariesBusiness priceCalculationSummariesBusiness = new PriceCalculationSummariesBusiness(StaticHelper.GetConfiguration("MongoDBHostName"), StaticHelper.GetConfiguration("MongoDBDatabaseName"));
                priceCalculationSummariesBusiness.bulkCreatePriceCalculationSummaries(priceCalculationSummaryMongoDBs);
            }

            return new CreateCampaignPricesResponse
            {
                ResponseResult = ResponseResult.ReturnSuccess(),
                calculatedCampaignPrices = response
            };
        }
        #endregion
    }
}
