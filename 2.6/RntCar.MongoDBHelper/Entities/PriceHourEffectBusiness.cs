using MongoDB.Bson;
using MongoDB.Driver;
using RntCar.ClassLibrary;
using RntCar.ClassLibrary.MongoDB;
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
    public class PriceHourEffectBusiness : MongoDBInstance
    {
        public PriceHourEffectBusiness(string serverName, string dataBaseName) : base(serverName, dataBaseName)
        {
        }

        public PriceHourEffectBusiness(object client, object database) : base(client, database)
        {
        }
        public MongoDBResponse createPriceHourEffect(PriceHourEffectData priceHourEffectData)
        {

            var collection = this.getCollection<PriceHourEffectDataMongoDB>(StaticHelper.GetConfiguration("MongoDBPriceEffectCollectionName"));
            var priceEffectMongoDB = new PriceHourEffectDataMongoDB();
            priceEffectMongoDB = priceEffectMongoDB.Map(priceHourEffectData);
            priceEffectMongoDB._id = ObjectId.GenerateNewId();

            var methodName = ErrorLogsHelper.GetCurrentMethod();
            var itemId = priceEffectMongoDB._id.ToString();

            var response = collection.Insert(priceEffectMongoDB, itemId, methodName);
            response.Id = Convert.ToString(priceEffectMongoDB._id);

            return response;
        }
        public bool updatePriceHourEffect(PriceHourEffectData priceHourEffectData, string id)
        {
            var collection = this.getCollection<PriceHourEffectDataMongoDB>(StaticHelper.GetConfiguration("MongoDBPriceEffectCollectionName"));

            var priceEffectMongoDB = new PriceHourEffectDataMongoDB();
            priceEffectMongoDB = priceEffectMongoDB.Map(priceHourEffectData);
            priceEffectMongoDB._id = ObjectId.Parse(id);

            var methodName = ErrorLogsHelper.GetCurrentMethod();
            var itemId = priceEffectMongoDB._id.ToString();

            var filter = Builders<PriceHourEffectDataMongoDB>.Filter.Eq(p => p._id, priceEffectMongoDB._id);
            var response = collection.Replace(priceEffectMongoDB, filter, new UpdateOptions { IsUpsert = false }, itemId, methodName);
            //var response = collection.ReplaceOne(p => p._id == priceEffectMongoDB._id, priceEffectMongoDB, new UpdateOptions { IsUpsert = false });
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
    }
}
