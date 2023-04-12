using RntCar.MongoDBHelper.Model;
using System;
using MongoDB.Driver;
using System.Linq;
using MongoDB.Bson;
using RntCar.SDK.Common;

namespace RntCar.MongoDBHelper.Repository
{
    public class CurrencyRepository : MongoDBInstance
    {
        public CurrencyRepository(string serverName, string dataBaseName) : base(serverName, dataBaseName)
        {
        }

        public CurrencyRepository(object client, object database) : base(client, database)
        {
        }
        public CurrencyDataMongoDB GetCurrency(Guid currencyId)
        {
            return this._database.GetCollection<CurrencyDataMongoDB>("currency")
                       .AsQueryable()
                       .Where(p => p._id  == new ObjectId(currencyId.convertGuidToMongoDBId())).FirstOrDefault();
        }
    }
}
