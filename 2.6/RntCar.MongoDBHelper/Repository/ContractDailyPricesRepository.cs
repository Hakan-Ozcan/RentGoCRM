using MongoDB.Bson;
using MongoDB.Driver;
using RntCar.MongoDBHelper.Model;
using RntCar.SDK.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RntCar.MongoDBHelper.Repository
{
    public class ContractDailyPricesRepository : MongoDBInstance
    {
        private string collectionName { get; set; }
        public ContractDailyPricesRepository(string serverName, string dataBaseName) : base(serverName, dataBaseName)
        {
            collectionName = StaticHelper.GetConfiguration("MongoDBContractDailyPrice");
        }
        public ContractDailyPricesRepository(object client, object database) : base(client, database)
        {
            collectionName = StaticHelper.GetConfiguration("MongoDBContractDailyPrice");
        }
        public List<ContractDailyPriceDataMongoDB> getContractDailyPricesTrackingNumber(string trackingNumber)
        {
            var collection = this._database.GetCollection<ContractDailyPriceDataMongoDB>(collectionName);
            var query = (from e in collection.AsQueryable<ContractDailyPriceDataMongoDB>()
                         where e.trackingNumber.Equals(trackingNumber)
                         select e).ToList();

            return query.OrderBy(p => p.priceDateTimeStamp.Value).ToList();
        }
        public ContractDailyPriceDataMongoDB getContractDailyPricesByTrackingNumberByPriceDateTimeStamp(string trackingNumber, long priceDateTimeStamp)
        {
            var collection = this._database.GetCollection<ContractDailyPriceDataMongoDB>(collectionName);
            var query = from e in collection.AsQueryable<ContractDailyPriceDataMongoDB>()
                        where e.trackingNumber.Equals(trackingNumber) &&
                        e.priceDateTimeStamp.Equals(new BsonTimestamp(priceDateTimeStamp))
                        select e;
            return query.FirstOrDefault();
        }
        public List<ContractDailyPriceDataMongoDB> getContractDailyPricesByReservationItemId_str(Guid reservationItemId)
        {
            var collection = this._database.GetCollection<ContractDailyPriceDataMongoDB>(collectionName);
            var query = (from e in collection.AsQueryable<ContractDailyPriceDataMongoDB>()
                         where e.contractItemId_str.Equals(reservationItemId.ToString())
                         select e).ToList();

            return query.ToList();
        }
        public List<ContractDailyPriceDataMongoDB> getContractDailyPrices()
        {
            var collection = this._database.GetCollection<ContractDailyPriceDataMongoDB>(collectionName);
            var query = (from e in collection.AsQueryable<ContractDailyPriceDataMongoDB>()                         
                         select e).ToList();

            return query.OrderBy(p => p.priceDateTimeStamp.Value).ToList();
        }
        public List<ContractDailyPriceDataMongoDB> getContractDailyPricesById(ObjectId id)
        {
            var collection = this._database.GetCollection<ContractDailyPriceDataMongoDB>(collectionName);
            var query = (from e in collection.AsQueryable<ContractDailyPriceDataMongoDB>()
                         where e._id == id
                         select e).ToList();

            return query.OrderBy(p => p.priceDateTimeStamp.Value).ToList();
        }
    }
}
