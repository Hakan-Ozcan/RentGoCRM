using MongoDB.Bson;
using MongoDB.Driver;
using RntCar.MongoDBHelper.Helper;
using RntCar.MongoDBHelper.Model;
using RntCar.SDK.Common;
using System;

namespace RntCar.MongoDBHelper.Entities
{
    public class CurrencyBusiness : MongoDBInstance
    {
        private string collectionName { get; set; }

        public CurrencyBusiness(string serverName, string dataBaseName) : base(serverName, dataBaseName)
        {
            collectionName = "currency";
        }

        public CurrencyBusiness(object client, object database) : base(client, database)
        {
            collectionName = "currency";

        }

        public void upsertCurrency(CurrencyDataMongoDB currencyDataMongoDB, Guid id)
        {
            var collection = this.getCollection<CurrencyDataMongoDB>(collectionName);
            var methodName = ErrorLogsHelper.GetCurrentMethod();
            currencyDataMongoDB._id = new ObjectId(id.convertGuidToMongoDBId());
            var filter = Builders<CurrencyDataMongoDB>.Filter.Eq(p => p._id, new ObjectId(id.convertGuidToMongoDBId()));
            var response = collection.Replace(currencyDataMongoDB, filter, new UpdateOptions { IsUpsert = true }, id.ToString(), methodName);
        }
    }
}
