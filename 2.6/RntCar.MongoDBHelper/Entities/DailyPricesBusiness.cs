using MongoDB.Bson;
using MongoDB.Driver;
using RntCar.ClassLibrary;
using RntCar.ClassLibrary._Enums_1033;
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
    public class DailyPricesBusiness : MongoDBInstance
    {
        public DailyPricesBusiness(string serverName, string dataBaseName) : base(serverName, dataBaseName)
        {
        }
        public DailyPricesBusiness(object client, object database) : base(client, database)
        {
        }

        public void createDailyPrices(ReservationItemData reservationItemData)
        {
            List<PriceCalculationSummaryMongoDB> priceCalculationSummaryMongoDBs = new List<PriceCalculationSummaryMongoDB>();
            try
            {
                if (reservationItemData.PaymentMethod == (int)rnt_PaymentMethodCode.PayBroker &&
                    reservationItemData.billingType == (int)rnt_BillingTypeCode.Corporate)
                {
                    reservationItemData.pricingGroupCodeId = reservationItemData.GroupCodeInformationId;
                }               

                PriceCalculationSummariesRepository priceCalculationSummariesRepository = new PriceCalculationSummariesRepository(this._client, this._database);
                priceCalculationSummaryMongoDBs = priceCalculationSummariesRepository.getPriceCalculationSummariesforDailyPrices(reservationItemData.pricingGroupCodeId, reservationItemData.trackingNumber, reservationItemData.campaignId);

                foreach (var item in priceCalculationSummaryMongoDBs)
                {
                    DailyPriceDataMongoDB dailyPriceDataMongoDB = new DailyPriceDataMongoDB();
                    dailyPriceDataMongoDB = dailyPriceDataMongoDB.Map(item);
                    dailyPriceDataMongoDB.reservationItemId = new Guid(reservationItemData.ReservationItemId);
                    dailyPriceDataMongoDB.reservationItemId_str = reservationItemData.ReservationItemId;
                    if (reservationItemData.PaymentChoice.Value == (int)PaymentEnums.PaymentType.PayNow)
                    {
                        item.totalAmount = item.payNowAmount;
                        dailyPriceDataMongoDB.totalAmount = item.totalAmount;
                    }
                    else
                    {
                        item.totalAmount = item.payLaterAmount;
                        dailyPriceDataMongoDB.totalAmount = item.totalAmount;
                    }
                    dailyPriceDataMongoDB._id = ObjectId.GenerateNewId();

                    PriceCalculationSummariesBusiness priceCalculationSummariesBusiness = new PriceCalculationSummariesBusiness(this._client, this._database);
                    var filter = Builders<PriceCalculationSummaryMongoDB>.Filter.Eq(p => p._id, item._id);
                    var result = priceCalculationSummariesRepository
                                .getCollection<PriceCalculationSummaryMongoDB>(StaticHelper.GetConfiguration("MongoDBPriceCalculationSummary"))
                                .Replace(item, filter, new UpdateOptions { IsUpsert = false }, Convert.ToString(item._id), "");

                    var methodName = ErrorLogsHelper.GetCurrentMethod();
                    var itemId = dailyPriceDataMongoDB._id.ToString();

                    var collection = this.getCollection<DailyPriceDataMongoDB>(StaticHelper.GetConfiguration("MongoDBDailyPrice"));
                    collection.Insert(dailyPriceDataMongoDB, itemId, methodName);

                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public bool deleteDailyPricesByReservationItemId(Guid reservationItemId)
        {
            try
            {
                var collection = this.getCollection<DailyPriceDataMongoDB>(StaticHelper.GetConfiguration("MongoDBDailyPrice"));
                //var g = new Guid(reservationItemId);
                var res = collection.DeleteMany(p => p.reservationItemId == reservationItemId);
                return res.IsAcknowledged;
            }
            catch (Exception ex)
            {
                //todo will implement error mechanism in mongoDB
                throw new Exception(ex.Message);
            }
        }

        public void Test()
        {
            PriceCalculationSummariesRepository priceCalculationSummariesRepository = new PriceCalculationSummariesRepository(this._client, this._database);
            //priceCalculationSummaryMongoDBs = priceCalculationSummariesRepository.getPriceCalculationSummariesforDailyPrices(reservationItemData.pricingGroupCodeId, reservationItemData.trackingNumber, reservationItemData.campaignId);

            //foreach (var item in priceCalculationSummaryMongoDB)
            //{
            //    var filter = Builders<PriceCalculationSummaryMongoDB>.Filter.Eq(p => p._id, item._id);
            //    var result = priceCalculationSummariesRepository
            //                .getCollection<PriceCalculationSummaryMongoDB>(StaticHelper.GetConfiguration("MongoDBPriceCalculationSummary"))
            //                .Replace(item, filter, new UpdateOptions { IsUpsert = false }, Convert.ToString(item._id), "");

            //    var collection = repor.getCollection<PriceCalculationSummaryMongoDB>("PriceCalculationSummaries");


            //    collection.ReplaceOne(p => p._id == item._id, item, new UpdateOptions { IsUpsert = false });

            //}
        }
    }
}
