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
    public class OneWayFeeBusiness : MongoDBInstance
    {
        public OneWayFeeBusiness(string serverName, string dataBaseName) : base(serverName, dataBaseName)
        {
        }
        public MongoDBResponse CreateOneWayFee(OneWayFeeData oneWayFeeData)
        {
            var collection = this.getCollection<OneWayFeeDataMongoDB>(StaticHelper.GetConfiguration("MongoDBOneWayFeeCollectionName"));

            var oneWayFee = new OneWayFeeDataMongoDB();
            oneWayFee = oneWayFee.Map(oneWayFeeData);
            oneWayFee._id = ObjectId.GenerateNewId();
            
            var methodName = ErrorLogsHelper.GetCurrentMethod();
            var itemId = oneWayFee._id.ToString();

            var response = collection.Insert(oneWayFee, itemId, methodName);
            response.Id = Convert.ToString(oneWayFee._id);

            return response;
        }
        public bool UpdateOneWayFee(OneWayFeeData oneWayFeeData, string id)
        {
            var collection = this.getCollection<OneWayFeeDataMongoDB>(StaticHelper.GetConfiguration("MongoDBOneWayFeeCollectionName"));

            var oneWayFee = new OneWayFeeDataMongoDB();
            oneWayFee = oneWayFee.Map(oneWayFeeData);
            //oneWayFee.ShallowCopy(oneWayFeeData);
            oneWayFee._id = ObjectId.Parse(id);
            
            var methodName = ErrorLogsHelper.GetCurrentMethod();
            var itemId = oneWayFee._id.ToString();

            var filter = Builders<OneWayFeeDataMongoDB>.Filter.Eq(p => p._id, oneWayFee._id);
            var response = collection.Replace(oneWayFee, filter, new UpdateOptions { IsUpsert = false }, itemId, methodName);
            //var response = collection.ReplaceOne(p => p._id == oneWayFee._id, oneWayFee, new UpdateOptions { IsUpsert = false });

            if (response != null)
            {
                if (response.IsAcknowledged)
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
