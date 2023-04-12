using MongoDB.Bson;
using MongoDB.Driver;
using RntCar.ClassLibrary;
using RntCar.ClassLibrary._Enums_1033;
using RntCar.ClassLibrary.MongoDB;
using RntCar.ClassLibrary.odata;
using RntCar.MongoDBHelper.Helper;
using RntCar.MongoDBHelper.Model;
using RntCar.MongoDBHelper.Repository;
using RntCar.SDK.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RntCar.MongoDBHelper.Entities
{
    public class ContractDailyPrices : MongoDBInstance
    {
        private string collectionName { get; set; }
        public ContractDailyPrices(string serverName, string dataBaseName) : base(serverName, dataBaseName)
        {
            collectionName = StaticHelper.GetConfiguration("MongoDBContractDailyPrice");
        }
        public ContractDailyPrices(object client, object database) : base(client, database)
        {
            collectionName = StaticHelper.GetConfiguration("MongoDBContractDailyPrice");

        }
        public List<ContractDailyPriceDataMongoDB> getDailyPricesByContractItemId(Guid contractItemId)
        {
            var collection = this._database.GetCollection<ContractDailyPriceDataMongoDB>(collectionName);
            var query = (from e in collection.AsQueryable<ContractDailyPriceDataMongoDB>()
                         where e.contractItemId.Equals(contractItemId)
                         select e).ToList();

            return query.OrderBy(p => p.priceDateTimeStamp.Value).ToList();
        }
        public ContractDailyPriceDataMongoDB getFirstDayPrice(Guid contractItemId)
        {
            var collection = this._database.GetCollection<ContractDailyPriceDataMongoDB>(collectionName);
            var query = (from e in collection.AsQueryable<ContractDailyPriceDataMongoDB>()
                         where e.contractItemId.Equals(contractItemId)
                         select e).ToList();
            return query.OrderBy(p => p.priceDateTimeStamp.Value).FirstOrDefault();

        }
        public void createContractDailyPrices(ContractItemData contractItemData, bool applyCurrencyEffect = false)
        {
            List<PriceCalculationSummaryMongoDB> priceCalculationSummaryMongoDBs = new List<PriceCalculationSummaryMongoDB>();
            try
            {
                if (contractItemData.paymentMethod == (int)rnt_PaymentMethodCode.PayBroker &&
                    contractItemData.billingType == (int)rnt_BillingTypeCode.Corporate)
                {
                    contractItemData.pricingGroupCodeId = contractItemData.groupCodeInformationsId;
                }

                PriceCalculationSummariesRepository priceCalculationSummariesRepository = new PriceCalculationSummariesRepository(this._client, this._database);
                priceCalculationSummaryMongoDBs = priceCalculationSummariesRepository.getPriceCalculationSummariesforDailyPrices(contractItemData.pricingGroupCodeId,
                                                                                                                                 contractItemData.trackingNumber,
                                                                                                                                 contractItemData.campaignId);
                if (applyCurrencyEffect)
                {
                    //upgrade/upsell ve araç değişiminde currency hesaplamaması için
                    ContractItemRepository contractItemRepository = new ContractItemRepository(this._client, this._database);
                    var contractItems = contractItemRepository.getContractItemsByContractId(contractItemData.contractId);
                    applyCurrencyEffect = contractItems.Count > 1 ? false : true;
                }

               

                foreach (var item in priceCalculationSummaryMongoDBs)
                {
                    ContractDailyPriceDataMongoDB dailyPriceDataMongoDB = new ContractDailyPriceDataMongoDB();
                    dailyPriceDataMongoDB = dailyPriceDataMongoDB.Map(item);
                    dailyPriceDataMongoDB._id = ObjectId.GenerateNewId();
                    dailyPriceDataMongoDB.contractItemId = new Guid(contractItemData.contractItemId);
                    dailyPriceDataMongoDB.contractItemId_str = contractItemData.contractItemId.ToString();
                    if (contractItemData.paymentChoice == (int)PaymentEnums.PaymentType.PayNow)
                    {
                        item.totalAmount = item.payNowAmount;
                        dailyPriceDataMongoDB.totalAmount = item.totalAmount;
                        if (applyCurrencyEffect && contractItemData.changeCurrency)
                        {
                            item.totalAmount = item.totalAmount / contractItemData.exchangeRate;
                            item.payNowAmount = item.payNowAmount / contractItemData.exchangeRate;
                            item.payNowWithoutTickDayAmount = item.payNowWithoutTickDayAmount / contractItemData.exchangeRate;

                            dailyPriceDataMongoDB.totalAmount = dailyPriceDataMongoDB.totalAmount / contractItemData.exchangeRate;
                        }

                    }
                    else
                    {
                        item.totalAmount = item.payLaterAmount;
                        dailyPriceDataMongoDB.totalAmount = item.totalAmount;

                        if (applyCurrencyEffect && contractItemData.changeCurrency)
                        {
                            item.totalAmount = item.totalAmount / contractItemData.exchangeRate;
                            item.payLaterAmount = item.payLaterAmount / contractItemData.exchangeRate;
                            item.payNowWithoutTickDayAmount = item.payNowWithoutTickDayAmount / contractItemData.exchangeRate;

                            dailyPriceDataMongoDB.totalAmount = dailyPriceDataMongoDB.totalAmount / contractItemData.exchangeRate;
                        }

                    }
                    if (contractItemData.paymentMethod == (int)rnt_PaymentMethodCode.PayBroker &&
                        contractItemData.totalAmount == decimal.Zero)
                    {
                        dailyPriceDataMongoDB.totalAmount = decimal.Zero;
                    }
                    PriceCalculationSummariesBusiness priceCalculationSummariesBusiness = new PriceCalculationSummariesBusiness(this._client, this._database);
                    var filter = Builders<PriceCalculationSummaryMongoDB>.Filter.Eq(p => p._id, item._id);
                    var result = priceCalculationSummariesRepository
                                .getCollection<PriceCalculationSummaryMongoDB>(StaticHelper.GetConfiguration("MongoDBPriceCalculationSummary"))
                                .Replace(item, filter, new UpdateOptions { IsUpsert = false }, Convert.ToString(item._id), "");

                    var methodName = ErrorLogsHelper.GetCurrentMethod();
                    var itemId = dailyPriceDataMongoDB._id.ToString();

                    var collection = this.getCollection<ContractDailyPriceDataMongoDB>(collectionName);
                    collection.Insert(dailyPriceDataMongoDB, itemId, methodName);
                    //MongoDBDailyPrice
                }
            }
            catch
            {
                //todo will implement error mechanism in mongoDB
            }
        }
        public bool deleteDailyPricesByContractItemId(Guid contractItemId)
        {
            try
            {
                var collection = this.getCollection<ContractDailyPriceDataMongoDB>(collectionName);
                //var g = new Guid(reservationItemId);
                var res = collection.DeleteMany(p => p.contractItemId == contractItemId);
                return res.IsAcknowledged;
            }
            catch
            {
                //todo will implement error mechanism in mongoDB
                return false;
            }
        }

        public bool deleteContractDailyPricesByTrackingNumberAndGroupCodeId(string trackingNumber, string groupCodeId)
        {
            try
            {
                var collection = this.getCollection<ContractDailyPriceDataMongoDB>(collectionName);

                var res = collection.DeleteMany(s => s.trackingNumber.Equals(trackingNumber)
                                                && s.relatedGroupCodeId.Equals(groupCodeId));
                return res.IsAcknowledged;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool createContractDailyPricesFromGivenList(List<DailyPrice> dailyPrices)
        {
            try
            {
                var collection = this.getCollection<ContractDailyPriceDataMongoDB>(collectionName);
                List<ContractDailyPriceDataMongoDB> dailyPriceDatalistMongoDB = new List<ContractDailyPriceDataMongoDB>();
                foreach (var item in dailyPrices)
                {
                    ContractDailyPriceDataMongoDB dailyPriceDataMongoDB = new ContractDailyPriceDataMongoDB();
                    dailyPriceDataMongoDB = dailyPriceDataMongoDB.Map(item);
                    dailyPriceDataMongoDB._id = ObjectId.GenerateNewId();
                    dailyPriceDataMongoDB.contractItemId = item.contractItemId;
                    dailyPriceDataMongoDB.contractItemId_str = item.contractItemId.ToString();
                    dailyPriceDataMongoDB.totalAmount = item.payNowAmount;
                    dailyPriceDataMongoDB.priceAfterBranchFactor = item.priceAfterBranchFactor;
                    dailyPriceDataMongoDB.priceAfterAvailabilityFactor = item.priceAfterAvailabilityFactor;
                    dailyPriceDataMongoDB.priceAfterCustomerFactor = item.priceAfterCustomerFactor;
                    dailyPriceDataMongoDB.priceAfterChannelFactor = item.priceAfterChannelFactor;
                    dailyPriceDataMongoDB.priceAfterSpecialDaysFactor = item.priceAfterSpecialDaysFactor;
                    dailyPriceDataMongoDB.priceAfterWeekDaysFactor = item.priceAfterWeekDaysFactor;
                    dailyPriceDataMongoDB.payNowAmount = item.payNowAmount;
                    dailyPriceDataMongoDB.payLaterAmount = item.payLaterAmount;
                    dailyPriceDataMongoDB.ID = Guid.NewGuid().ToString();
                    dailyPriceDatalistMongoDB.Add(dailyPriceDataMongoDB);
                }
                collection.InsertMany(dailyPriceDatalistMongoDB);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private static readonly DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);


        private static long ConvertToTimestamp(DateTime value)
        {
            TimeSpan elapsedTime = value - Epoch;
            return (long)elapsedTime.TotalSeconds;
        }
    }
}
