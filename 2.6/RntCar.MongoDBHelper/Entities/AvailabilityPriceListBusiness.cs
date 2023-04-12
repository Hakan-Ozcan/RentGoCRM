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
    public class AvailabilityPriceListBusiness : MongoDBInstance
    {
        public AvailabilityPriceListBusiness(string serverName, string dataBaseName) : base(serverName, dataBaseName)
        {
        }

        public MongoDBResponse CreateAvailabilityPriceList(AvailabilityPriceListData availabilityPriceListData)
        {
            var collection = this.getCollection<AvailabilityPriceListDataMongoDB>(StaticHelper.GetConfiguration("MongoDBAvailabilityPriceListListCollectionName"));

            var availabilityPriceList = new AvailabilityPriceListDataMongoDB();

            availabilityPriceList.ShallowCopy(availabilityPriceListData);
            availabilityPriceList._id = ObjectId.GenerateNewId();

            var methodName = ErrorLogsHelper.GetCurrentMethod();
            var itemId = availabilityPriceList._id.ToString();

            var response = collection.Insert(availabilityPriceList, itemId, methodName);
            response.Id = Convert.ToString(availabilityPriceList._id);

            return response;
        }
        public bool UpdateAvailabilityPriceList(AvailabilityPriceListData availabilityPriceListData, string id)
        {
            var collection = this.getCollection<AvailabilityPriceListDataMongoDB>(StaticHelper.GetConfiguration("MongoDBAvailabilityPriceListListCollectionName"));

            var availabilityPriceList = new AvailabilityPriceListDataMongoDB();

            availabilityPriceList.ShallowCopy(availabilityPriceListData);
            availabilityPriceList._id = ObjectId.Parse(id);

            var methodName = ErrorLogsHelper.GetCurrentMethod();
            var itemId = availabilityPriceList._id.ToString();

            var filter = Builders<AvailabilityPriceListDataMongoDB>.Filter.Eq(p => p._id, availabilityPriceList._id);
            var response = collection.Replace(availabilityPriceList, filter, new UpdateOptions { IsUpsert = false }, itemId, methodName);
            //var response = collection.ReplaceOne(p => p._id == availabilityPriceList._id, availabilityPriceList, new UpdateOptions { IsUpsert = false });

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
