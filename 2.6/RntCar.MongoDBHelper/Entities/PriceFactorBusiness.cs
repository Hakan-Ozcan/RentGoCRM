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
    public class PriceFactorBusiness : MongoDBInstance
    {
        public PriceFactorBusiness(string serverName, string dataBaseName) : base(serverName, dataBaseName)
        {
        }

        public MongoDBResponse CreatePriceFactor(PriceFactorData priceFactorData)
        {
            var collection = this.getCollection<PriceFactorDataMongoDB>(StaticHelper.GetConfiguration("MongoDBPriceFactorCollectionName"));

            var priceFactor = new PriceFactorDataMongoDB();
            priceFactor = priceFactor.Map(priceFactorData);
            priceFactor._id = ObjectId.GenerateNewId();
            if (priceFactorData.beginDate.HasValue)
                priceFactor.beginDateTimeStamp = new BsonTimestamp(priceFactorData.beginDate.Value.converttoTimeStamp());
            if (priceFactorData.endDate.HasValue)
                priceFactor.endDateTimeStamp = new BsonTimestamp(priceFactorData.endDate.Value.converttoTimeStamp());

            var methodName = ErrorLogsHelper.GetCurrentMethod();
            var itemId = priceFactor._id.ToString();

            var response = collection.Insert(priceFactor, itemId, methodName);
            response.Id = Convert.ToString(priceFactor._id);

            return response;
        }
        public bool UpdatePriceFactor(PriceFactorData priceFactorData, string id)
        {
            var collection = this.getCollection<PriceFactorDataMongoDB>(StaticHelper.GetConfiguration("MongoDBPriceFactorCollectionName"));

            var priceFactor = new PriceFactorDataMongoDB();
            priceFactor = priceFactor.Map(priceFactorData);
            if (priceFactorData.beginDate.HasValue)
                priceFactor.beginDateTimeStamp = new BsonTimestamp(priceFactorData.beginDate.Value.converttoTimeStamp());
            if (priceFactorData.endDate.HasValue)
                priceFactor.endDateTimeStamp = new BsonTimestamp(priceFactorData.endDate.Value.converttoTimeStamp());

            priceFactor._id = ObjectId.Parse(id);
            
            var methodName = ErrorLogsHelper.GetCurrentMethod();
            var itemId = priceFactor._id.ToString();

            var filter = Builders<PriceFactorDataMongoDB>.Filter.Eq(p => p._id, priceFactor._id);
            var response = collection.Replace(priceFactor, filter, new UpdateOptions { IsUpsert = false }, itemId, methodName);
            //var response = collection.ReplaceOne(p => p._id == priceFactor._id, priceFactor, new UpdateOptions { IsUpsert = false });

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
