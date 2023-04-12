using RntCar.MongoDBHelper.Model;
using RntCar.SDK.Common;
using MongoDB.Driver.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver;
using System;
using MongoDB.Bson;

namespace RntCar.MongoDBHelper.Repository
{
    public class PriceCalculationSummariesRepository : MongoDBInstance
    {
        public PriceCalculationSummariesRepository(string serverName, string dataBaseName) : base(serverName, dataBaseName)
        {
        }

        public PriceCalculationSummariesRepository(object client, object database) : base(client, database)
        {
        }
        public PriceCalculationSummaryMongoDB getPriceCalculationSummariesSingle()
        {
            var collection = this._database.GetCollection<PriceCalculationSummaryMongoDB>(StaticHelper.GetConfiguration("MongoDBPriceCalculationSummary"));

            return (from e in collection.AsQueryable<PriceCalculationSummaryMongoDB>()
                    select e).FirstOrDefault();


        }
        public IMongoQueryable<PriceCalculationSummaryMongoDB> getPriceCalculationSummaries()
        {
            var collection = this._database.GetCollection<PriceCalculationSummaryMongoDB>(StaticHelper.GetConfiguration("MongoDBPriceCalculationSummary"));

            var query = from e in collection.AsQueryable<PriceCalculationSummaryMongoDB>()
                        select e;
            return query;
            //var v =  getMe();
            //v.Wait();
            //return query.ToList();
        }
        public List<PriceCalculationSummaryMongoDB> getPriceCalculationSummariesByTrackingNumber(string trackingNumber)
        {
            var collection = this._database.GetCollection<PriceCalculationSummaryMongoDB>(StaticHelper.GetConfiguration("MongoDBPriceCalculationSummary"));

            return (from e in collection.AsQueryable<PriceCalculationSummaryMongoDB>()
                    where e.trackingNumber.Equals(trackingNumber)
                    select e).ToList();

        }

        public List<PriceCalculationSummaryMongoDB> getPriceCalculationSummariesWithIsCrmCreatedByTrackingNumber(string trackingNumber, bool isCrmCreated)
        {
            var collection = this._database.GetCollection<PriceCalculationSummaryMongoDB>(StaticHelper.GetConfiguration("MongoDBPriceCalculationSummary"));

            return (from e in collection.AsQueryable<PriceCalculationSummaryMongoDB>()
                    where e.trackingNumber.Equals(trackingNumber) && e.isCrmCreated == isCrmCreated
                    select e).ToList();

        }

        public List<PriceCalculationSummaryMongoDB> getPriceCalculationSummariesByTrackingNumberAndPricingGroup(string trackingNumber, string groupCode)
        {
            var collection = this._database.GetCollection<PriceCalculationSummaryMongoDB>(StaticHelper.GetConfiguration("MongoDBPriceCalculationSummary"));

            return (from e in collection.AsQueryable<PriceCalculationSummaryMongoDB>()
                    where e.trackingNumber.Equals(trackingNumber) &&
                    e.relatedGroupCodeId.Equals(groupCode)
                    select e).ToList();

        }
        public async Task<List<PriceCalculationSummaryMongoDB>> getMe()
        {
            var collection = this._database.GetCollection<PriceCalculationSummaryMongoDB>(StaticHelper.GetConfiguration("MongoDBPriceCalculationSummary"));
            var ret = await collection
                            .Find("{}")
                            .ToListAsync();

            return ret;
        }
        public PriceCalculationSummaryMongoDB getPricesCalculationSummariesByTrackingNumberandGroupCodeandDate(string groupCodeId, string trackingNumber, long priceDateTimeStamp)
        {
            //index_1
            var collection = this._database.GetCollection<PriceCalculationSummaryMongoDB>(StaticHelper.GetConfiguration("MongoDBPriceCalculationSummary"));
            var query = from e in collection.AsQueryable<PriceCalculationSummaryMongoDB>()
                        where e.relatedGroupCodeId.Equals(groupCodeId) && e.trackingNumber.Equals(trackingNumber) &&
                        e.campaignId == Guid.Empty && e.priceDateTimeStamp.Equals(new BsonTimestamp(priceDateTimeStamp))
                        select e;

            return query.FirstOrDefault();

        }

        public List<PriceCalculationSummaryMongoDB> getPricesCalculationSummariesByLessDate(long priceDateTimeStamp)
        {
            //index_1
            var collection = this._database.GetCollection<PriceCalculationSummaryMongoDB>(StaticHelper.GetConfiguration("MongoDBPriceCalculationSummary"));
            var query = from e in collection.AsQueryable<PriceCalculationSummaryMongoDB>()
                        where e.priceDateTimeStamp < (new BsonTimestamp(priceDateTimeStamp))
                        select e;

            return query.ToList();

        }

        public List<PriceCalculationSummaryMongoDB> getPriceCalculationSummariesforDailyPrices(string groupCodeId, string trackingNumber, Guid? campaignId)
        {
            var items = new List<PriceCalculationSummaryMongoDB>();
            if (campaignId.HasValue)
            {
                items = this.getPricesCalculationSummariesByTrackingNumberCampaignIdandGroupCode(groupCodeId, trackingNumber, campaignId.Value);

            }
            else
            {
                items = this.getPricesCalculationSummariesByTrackingNumberandGroupCode(groupCodeId, trackingNumber);
            }

            return items;


        }

        public List<PriceCalculationSummaryMongoDB> checkDailyPricesForMainCampaign(List<PriceCalculationSummaryMongoDB> items,
                                                                                    string trackingNumber,
                                                                                    string groupCodeId)
        {
            if (items.Sum(p => p.totalAmount) == decimal.Zero && items.Count == 0)
            {
                MongoDBInstance mongoDBInstanceCamp = new MongoDBInstance(StaticHelper.GetConfiguration("MongoDBHostName"),
                                                                        StaticHelper.GetConfiguration("MongoDBDatabaseName"));


                var collection2 = mongoDBInstanceCamp.getCollection<CampaignAvailabilityMongoDB>("AvailabilityCampaings");

                var data = collection2.AsQueryable().Where(p => p.trackingNumber == trackingNumber &&
                                                                p.groupCodedId == groupCodeId).FirstOrDefault();

                if (data != null)
                {
                    return this.getPricesCalculationSummariesByTrackingNumberCampaignIdandGroupCode(groupCodeId, trackingNumber, new Guid(data.campaignId));
                }
            }
            return items;
        }
        public List<PriceCalculationSummaryMongoDB> getPricesCalculationSummariesByTrackingNumberandGroupCode(string groupCodeId, string trackingNumber)
        {
            //index_1
            var collection = this._database.GetCollection<PriceCalculationSummaryMongoDB>(StaticHelper.GetConfiguration("MongoDBPriceCalculationSummary"));
            var query = from e in collection.AsQueryable<PriceCalculationSummaryMongoDB>()
                        where e.relatedGroupCodeId.Equals(groupCodeId) && e.trackingNumber.Equals(trackingNumber) &&
                        e.campaignId == Guid.Empty
                        select e;

            return query.ToList();

        }
        public List<PriceCalculationSummaryMongoDB> getPricesCalculationSummariesByTrackingNumberCampaignIdandGroupCode(string groupCodeId, string trackingNumber, Guid campaignId)
        {
            //index_1
            var collection = this._database.GetCollection<PriceCalculationSummaryMongoDB>(StaticHelper.GetConfiguration("MongoDBPriceCalculationSummary"));
            var query = from e in collection.AsQueryable<PriceCalculationSummaryMongoDB>()
                        where e.relatedGroupCodeId.Equals(groupCodeId) &&
                              e.trackingNumber.Equals(trackingNumber) &&
                              (e.campaignId == campaignId || e.campaignId == StaticHelper.dummyCampaignId)
                        select e;
            var newQuery = query.ToList();
            //because it can retrieve another campaing dummy data
            //if we have the same values for one day need to remove the dummy one
            var grouped = newQuery
                         .GroupBy(n => n.priceDateTimeStamp)
                         .Select(c => new { Key = c.Key, total = c.Count() })
                         .Where(p => p.total > 1)
                         .Select(p => p.Key)
                         .ToList();

            var duplicateDates = newQuery
                                .Where(p => grouped.Contains(p.priceDateTimeStamp))
                                .ToList();

            var willDuplicateItems = duplicateDates.Where(p => p.campaignId.Equals(StaticHelper.dummyCampaignId)).ToList();
            var finalList = newQuery.Except(willDuplicateItems).ToList();

            return finalList;
        }

        public List<PriceCalculationSummaryMongoDB> getPricesCalculationSummariesIsNotCreated(int beforeHour)
        {
            var collection = this._database.GetCollection<PriceCalculationSummaryMongoDB>(StaticHelper.GetConfiguration("MongoDBPriceCalculationSummary"));
            var query = (from e in collection.AsQueryable<PriceCalculationSummaryMongoDB>()
                         where e.isCrmCreated == false && e.createdOn < DateTime.UtcNow.AddHours(beforeHour)
                         select e).Take(10000);

            return query.ToList();

        }

        public List<PriceCalculationSummaryMongoDB> getPricesCalculationSummariesArchiveByDate(DateTime LastPriceDate)
        {
            var collection = this._database.GetCollection<PriceCalculationSummaryMongoDB>(StaticHelper.GetConfiguration("MongoDBPriceCalculationSummaryArchive"));
            var query = from e in collection.AsQueryable<PriceCalculationSummaryMongoDB>()
                        where e.createdOn <= LastPriceDate
                        select e;

            return query.ToList();

        }
    }
}
