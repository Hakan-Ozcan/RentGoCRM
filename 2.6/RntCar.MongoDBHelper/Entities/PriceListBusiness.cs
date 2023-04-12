using MongoDB.Bson;
using MongoDB.Driver;
using RntCar.ClassLibrary;
using RntCar.ClassLibrary.MongoDB;
using RntCar.MongoDBHelper.Helper;
using RntCar.MongoDBHelper.Model;
using RntCar.SDK.Common;
using System;

namespace RntCar.MongoDBHelper.Entities
{
    public class PriceListBusiness : MongoDBInstance
    {
        public PriceListBusiness(string serverName, string dataBaseName) : base(serverName, dataBaseName)
        {
        }
        public MongoDBResponse CreatePriceList(PriceListData priceListData)
        {
            var collection = this.getCollection<PriceListDataMongoDB>(StaticHelper.GetConfiguration("MongoDBPriceListCollectionName"));

            var priceList = new PriceListDataMongoDB();
            priceList = priceList.Map(priceListData);
            priceList._id = ObjectId.GenerateNewId();
            
            var methodName = ErrorLogsHelper.GetCurrentMethod();
            var itemId = priceList._id.ToString();

            var response = collection.Insert(priceList, itemId, methodName);
            response.Id = Convert.ToString(priceList._id);

            return response;
        }
        public bool UpdatePriceList(PriceListData priceListData, string id)
        {
            var collection = this.getCollection<PriceListDataMongoDB>(StaticHelper.GetConfiguration("MongoDBPriceListCollectionName"));

            var priceList = new PriceListDataMongoDB();
            priceList = priceList.Map(priceListData);
            priceList._id = ObjectId.Parse(id);

            var methodName = ErrorLogsHelper.GetCurrentMethod();
            var itemId = priceList._id.ToString();

            var filter = Builders<PriceListDataMongoDB>.Filter.Eq(p => p._id, priceList._id);
            var response = collection.Replace(priceList, filter, new UpdateOptions { IsUpsert = false }, itemId, methodName);
            //var response = collection.ReplaceOne(p => p._id == priceList._id, priceList, new UpdateOptions { IsUpsert = false });

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
