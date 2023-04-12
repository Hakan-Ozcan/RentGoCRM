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
    public class GroupCodeListPriceBusiness : MongoDBInstance
    {
        public GroupCodeListPriceBusiness(string serverName, string dataBaseName) : base(serverName, dataBaseName)
        {

        }
        public MongoDBResponse CreateGroupCodeListPrice(GroupCodeListPriceData equipmentData)
        {
            var collection = this.getCollection<GroupCodeListPriceDataMongoDB>(StaticHelper.GetConfiguration("MongoDBGroupCodePriceListCollectionName"));

            var groupCodeListPrice = new GroupCodeListPriceDataMongoDB();

            groupCodeListPrice.ShallowCopy(equipmentData);
            groupCodeListPrice._id = ObjectId.GenerateNewId();

            var methodName = ErrorLogsHelper.GetCurrentMethod();
            var itemId = groupCodeListPrice._id.ToString();

            var response = collection.Insert(groupCodeListPrice, itemId, methodName);
            response.Id = Convert.ToString(groupCodeListPrice._id);

            return response;
        }
        public bool UpdateGroupCodeListPrice(GroupCodeListPriceData equipmentData, string id)
        {
            var collection = this.getCollection<GroupCodeListPriceDataMongoDB>(StaticHelper.GetConfiguration("MongoDBGroupCodePriceListCollectionName"));

            var groupCodeListPrice = new GroupCodeListPriceDataMongoDB();

            groupCodeListPrice.ShallowCopy(equipmentData);
            groupCodeListPrice._id = ObjectId.Parse(id);

            var methodName = ErrorLogsHelper.GetCurrentMethod();
            var itemId = groupCodeListPrice._id.ToString();

            var filter = Builders<GroupCodeListPriceDataMongoDB>.Filter.Eq(p => p._id, groupCodeListPrice._id);
            var response = collection.Replace(groupCodeListPrice, filter, new UpdateOptions { IsUpsert = false }, itemId, methodName);
            //var response = collection.ReplaceOne(p => p._id == groupCodeListPrice._id, groupCodeListPrice, new UpdateOptions { IsUpsert = false });

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
