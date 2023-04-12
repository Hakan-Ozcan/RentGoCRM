using MongoDB.Bson;
using MongoDB.Driver;
using RntCar.MongoDBHelper.Helper;
using RntCar.MongoDBHelper.Model;
using RntCar.SDK.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.MongoDBHelper.Entities
{
    public class PriceCalculationSummariesBusiness : MongoDBInstance
    {
        public PriceCalculationSummariesBusiness(string serverName, string dataBaseName) : base(serverName, dataBaseName)
        {
        }

        public PriceCalculationSummariesBusiness(object client, object database) : base(client, database)
        {
        }

        public void createPriceCalculationSummary(PriceResponse priceResponse, string trackingNumber)
        {
            try
            {
                var timeStamp = priceResponse.relatedDay.converttoTimeStamp();
                var priceDayTimeStamp = new BsonTimestamp(timeStamp);

                PriceCalculationSummaryMongoDB priceCalculationSummaryMongoDB = new PriceCalculationSummaryMongoDB
                {
                    ID = Guid.NewGuid().ToString(),
                    _id = ObjectId.GenerateNewId(),
                    priceDate = priceResponse.relatedDay,
                    trackingNumber = trackingNumber,
                    userId = string.Empty,//todo will handle from parameter
                    userEntityLogicalName = string.Empty,//todo will handle from parameter
                    priceDateTimeStamp = priceDayTimeStamp,
                    selectedPriceListId = priceResponse.selectedPriceList.PriceListId,
                    totalAmount = priceResponse.totalAmount,
                    payLaterAmount = priceResponse.payLaterAmount,
                    payNowAmount = priceResponse.payNowAmount,
                    selectedGroupCodePriceListId = priceResponse.selectedGroupCodeListPrice.GroupCodeListPriceId,
                    selectedGroupCodeAmount = priceResponse.selectedGroupCodeListPrice.ListPrice,
                    selectedAvailabilityPriceListId = priceResponse.selectedAvailabilityPriceList?.AvailabilityPriceListId,
                    selectedAvailabilityPriceRate = priceResponse.selectedAvailabilityPriceList != null ? priceResponse.selectedAvailabilityPriceList.PriceChangeRate : decimal.Zero,
                    relatedGroupCodeId = priceResponse.selectedGroupCodeListPrice?.GroupCodeInformationId,
                    relatedGroupCodeName = priceResponse.selectedGroupCodeListPrice?.GroupCodeInformationName,
                    availabilityRate = priceResponse.availibilityRatio,
                    priceAfterAvailabilityFactor = priceResponse.priceAfterAvailabilityFactor,
                    priceAfterChannelFactor = priceResponse.priceAfterChannelFactor,
                    priceAfterCustomerFactor = priceResponse.priceAfterCustomerFactor,
                    priceAfterSpecialDaysFactor = priceResponse.priceAfterSpecialDaysFactor,
                    priceAfterWeekDaysFactor = priceResponse.priceAfterWeekDaysFactor,
                    priceAfterBranchFactor = priceResponse.priceAfterBranchFactor,
                    priceAfterBranch2Factor = priceResponse.priceAfterBranch2Factor,
                    payLaterWithoutTickDayAmount = priceResponse.payLaterWithoutTickDayAmount,
                    priceAfterEqualityFactor = priceResponse.priceAfterEqualityFactor,
                    payNowWithoutTickDayAmount = priceResponse.payNowWithoutTickDayAmount,
                    isTickDay = priceResponse.isTickDay,
                    priceAfterPayMethodPayLater = priceResponse.priceAfterPayMethodPayLater,
                    priceAfterPayMethodPayNow = priceResponse.priceAfterPayMethodPayNow
                };
                if (priceResponse.campaignId.HasValue)
                    priceCalculationSummaryMongoDB.campaignId = priceResponse.campaignId.Value;

                var methodName = ErrorLogsHelper.GetCurrentMethod();
                var itemId = priceCalculationSummaryMongoDB._id.ToString();

                var collection = this.getCollection<PriceCalculationSummaryMongoDB>(StaticHelper.GetConfiguration("MongoDBPriceCalculationSummary"));
                collection.Insert(priceCalculationSummaryMongoDB, itemId, methodName);

            }
            catch
            {
                //todo will implement error mechanism in mongoDB
            }
        }

        public void createPriceCalculationSummaryFromExisting(PriceCalculationSummaryMongoDB priceCalculationSummaryMongoDB)
        {
            try
            {
                var methodName = ErrorLogsHelper.GetCurrentMethod();
                var itemId = priceCalculationSummaryMongoDB._id.ToString();
                priceCalculationSummaryMongoDB.ID = Guid.NewGuid().ToString();
                var collection = this.getCollection<PriceCalculationSummaryMongoDB>(StaticHelper.GetConfiguration("MongoDBPriceCalculationSummary"));
                collection.Insert(priceCalculationSummaryMongoDB, itemId, methodName);

            }
            catch
            {
                //todo will implement error mechanism in mongoDB
            }
        }
        public void createPriceCalculationSummaryFromDailyPrices(DailyPriceDataMongoDB dailyPriceDataMongoDB)
        {
            try
            {
                var methodName = ErrorLogsHelper.GetCurrentMethod();
                var priceCalculationSummaryMongoDB = new PriceCalculationSummaryMongoDB();
                //copy reservationItemData to ReservationItemDataMongoDB
                priceCalculationSummaryMongoDB = priceCalculationSummaryMongoDB.Map(dailyPriceDataMongoDB);
                priceCalculationSummaryMongoDB.ID = Guid.NewGuid().ToString();
                var itemId = priceCalculationSummaryMongoDB._id.ToString();

                var collection = this.getCollection<PriceCalculationSummaryMongoDB>(StaticHelper.GetConfiguration("MongoDBPriceCalculationSummary"));
                collection.Insert(priceCalculationSummaryMongoDB, itemId, methodName);

            }
            catch
            {
                //todo will implement error mechanism in mongoDB
            }
        }
        public void createPriceCalculationSummaryFromContractDailyPrices(ContractDailyPriceDataMongoDB contractDailyPriceDataMongoDB)
        {
            try
            {
                var methodName = ErrorLogsHelper.GetCurrentMethod();
                var priceCalculationSummaryMongoDB = new PriceCalculationSummaryMongoDB();
                //copy reservationItemData to ReservationItemDataMongoDB
                priceCalculationSummaryMongoDB = priceCalculationSummaryMongoDB.Map(contractDailyPriceDataMongoDB);
                priceCalculationSummaryMongoDB.ID = Guid.NewGuid().ToString();
                var itemId = priceCalculationSummaryMongoDB._id.ToString();

                var collection = this.getCollection<PriceCalculationSummaryMongoDB>(StaticHelper.GetConfiguration("MongoDBPriceCalculationSummary"));
                collection.Insert(priceCalculationSummaryMongoDB, itemId, methodName);

            }
            catch
            {
                //todo will implement error mechanism in mongoDB
            }
        }
        public void bulkCreatePriceCalculationSummaries(List<PriceCalculationSummaryMongoDB> priceCalculationSummaryMongoDB)
        {
            try
            {
                foreach (var item in priceCalculationSummaryMongoDB)
                {
                    item.ID = Guid.NewGuid().ToString();
                }
                var collection = this.getCollection<PriceCalculationSummaryMongoDB>(StaticHelper.GetConfiguration("MongoDBPriceCalculationSummary"));
                collection.InsertMany(priceCalculationSummaryMongoDB);
            }
            catch (Exception ex)
            {
                //todo will implement error mechanism in mongoDB
            }
        }

        public void bulkMovePriceCalculationSummaries(List<PriceCalculationSummaryMongoDB> priceCalculationSummaryMongoDB, string tableName)
        {
            try
            {
                foreach (var item in priceCalculationSummaryMongoDB)
                {
                    item.ID = Guid.NewGuid().ToString();
                }
                var collection = this.getCollection<PriceCalculationSummaryMongoDB>(tableName);
                collection.InsertMany(priceCalculationSummaryMongoDB);
            }
            catch (Exception ex)
            {
                //todo will implement error mechanism in mongoDB
            }
        }

        public bool bulkUpdatePriceCalculationSummaries(List<PriceCalculationSummaryMongoDB> priceCalculationSummaryMongoDB)
        {
            foreach (var item in priceCalculationSummaryMongoDB)
            {
                var collection = this.getCollection<PriceCalculationSummaryMongoDB>("PriceCalculationSummaries");


                collection.ReplaceOne(p => p._id == item._id, item, new UpdateOptions { IsUpsert = false });
            }
            return true;
        }

        public PriceCalculationSummaryMongoDB buildPriceCalculationSummaryObjectfromExisting(PriceCalculationSummaryMongoDB priceCalculationSummaryMongoDB,
                                                                   Guid campaignId,
                                                                   decimal payNowAmount,
                                                                   decimal payLaterAmount)
        {
            var PriceCalculationSummaryMongoDB = new PriceCalculationSummaryMongoDB();
            //copy reservationItemData to ReservationItemDataMongoDB
            PriceCalculationSummaryMongoDB = PriceCalculationSummaryMongoDB.Map(priceCalculationSummaryMongoDB);
            PriceCalculationSummaryMongoDB._id = ObjectId.GenerateNewId();

            PriceCalculationSummaryMongoDB.campaignId = campaignId;
            PriceCalculationSummaryMongoDB.payLaterAmount = payLaterAmount;
            PriceCalculationSummaryMongoDB.payNowAmount = payNowAmount;
            PriceCalculationSummaryMongoDB.ID = Guid.NewGuid().ToString();
            PriceCalculationSummaryMongoDB._id = ObjectId.GenerateNewId(DateTime.Now.converttoTimeStamp());
            return PriceCalculationSummaryMongoDB;
        }

        public void addPriceCalculationSummaryWithSourceGroupCodeId(List<PriceCalculationSummaryMongoDB> priceCalculationList, string trackingNumber, string groupCodeId, string groupCodeName)
        {
            try
            {
                foreach (var priceCalculation in priceCalculationList)
                {
                    var timeStamp = priceCalculation.priceDate.converttoTimeStamp();

                    var priceDayTimeStamp = new BsonTimestamp(timeStamp);

                    PriceCalculationSummaryMongoDB priceCalculationSummaryMongoDB = new PriceCalculationSummaryMongoDB
                    {
                        ID = Guid.NewGuid().ToString(),
                        _id = ObjectId.GenerateNewId(),
                        priceDate = priceCalculation.priceDate,
                        trackingNumber = trackingNumber,
                        userId = string.Empty,//todo will handle from parameter
                        userEntityLogicalName = string.Empty,//todo will handle from parameter
                        priceDateTimeStamp = priceDayTimeStamp,
                        selectedPriceListId = priceCalculation.selectedPriceListId,
                        totalAmount = priceCalculation.totalAmount,
                        payLaterAmount = priceCalculation.payLaterAmount,
                        payNowAmount = priceCalculation.payNowAmount,
                        selectedGroupCodePriceListId = priceCalculation.selectedGroupCodePriceListId,
                        selectedGroupCodeAmount = priceCalculation.selectedGroupCodeAmount,
                        selectedAvailabilityPriceListId = priceCalculation.selectedAvailabilityPriceListId,
                        selectedAvailabilityPriceRate = priceCalculation.selectedAvailabilityPriceRate,
                        relatedGroupCodeId = groupCodeId,
                        relatedGroupCodeName = groupCodeName,
                        availabilityRate = priceCalculation.availabilityRate,
                        priceAfterAvailabilityFactor = priceCalculation.priceAfterAvailabilityFactor,
                        priceAfterChannelFactor = priceCalculation.priceAfterChannelFactor,
                        priceAfterCustomerFactor = priceCalculation.priceAfterCustomerFactor,
                        priceAfterSpecialDaysFactor = priceCalculation.priceAfterSpecialDaysFactor,
                        priceAfterWeekDaysFactor = priceCalculation.priceAfterWeekDaysFactor,
                        priceAfterBranchFactor = priceCalculation.priceAfterBranchFactor,
                        priceAfterBranch2Factor = priceCalculation.priceAfterBranch2Factor,
                        payLaterWithoutTickDayAmount = priceCalculation.payLaterWithoutTickDayAmount,
                        priceAfterEqualityFactor = priceCalculation.priceAfterEqualityFactor,
                        payNowWithoutTickDayAmount = priceCalculation.payNowWithoutTickDayAmount,
                        isTickDay = priceCalculation.isTickDay,
                        priceAfterPayMethodPayLater = priceCalculation.priceAfterPayMethodPayLater,
                        priceAfterPayMethodPayNow = priceCalculation.priceAfterPayMethodPayNow
                    };
                    if (priceCalculation.campaignId != Guid.Empty)
                        priceCalculationSummaryMongoDB.campaignId = priceCalculation.campaignId;

                    var methodName = ErrorLogsHelper.GetCurrentMethod();
                    var itemId = priceCalculationSummaryMongoDB._id.ToString();

                    var collection = this.getCollection<PriceCalculationSummaryMongoDB>(StaticHelper.GetConfiguration("MongoDBPriceCalculationSummary"));
                    collection.Insert(priceCalculationSummaryMongoDB, itemId, methodName);
                }

            }
            catch
            {
                //todo will implement error mechanism in mongoDB
            }
        }

        public bool bulkDeletePriceCalculationSummaries(List<PriceCalculationSummaryMongoDB> priceCalculationSummaryMongoDB)
        {
            foreach (var item in priceCalculationSummaryMongoDB)
            {
                var collection = this.getCollection<PriceCalculationSummaryMongoDB>("PriceCalculationSummaries");


                collection.DeleteOne(p => p._id == item._id);
            }
            return true;
        }

    }
}
