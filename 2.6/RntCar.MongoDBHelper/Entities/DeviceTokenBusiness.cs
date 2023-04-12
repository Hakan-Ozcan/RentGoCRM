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
    public class DeviceTokenBusiness : MongoDBInstance
    {
        public DeviceTokenBusiness(string serverName, string dataBaseName) : base(serverName, dataBaseName)
        {
        }

        public DeviceTokenBusiness(object client, object database) : base(client, database)
        {
        }

        public MongoDBResponse CreateDeviceToken(DeviceTokenData deviceTokenData)
        {
            var collection = this.getCollection<DeviceTokenDataMongoDB>("DeviceTokens");

            var deviceToken = new DeviceTokenDataMongoDB();
            deviceToken = deviceToken.Map(deviceTokenData);
            deviceToken._id = ObjectId.GenerateNewId();

            var methodName = ErrorLogsHelper.GetCurrentMethod();
            var itemId = deviceToken._id.ToString();

            var response = collection.Insert(deviceToken, itemId, methodName);
            response.Id = itemId;

            return response;
        }

        public MongoDBResponse UpdateDeviceToken(DeviceTokenData deviceTokenData, string id)
        {
            var collection = this.getCollection<DeviceTokenDataMongoDB>("DeviceTokens");

            var deviceToken = new DeviceTokenDataMongoDB();
            deviceToken = deviceToken.Map(deviceTokenData);
            deviceToken._id = ObjectId.Parse(id);

            var methodName = ErrorLogsHelper.GetCurrentMethod();

            var filter = Builders<DeviceTokenDataMongoDB>.Filter.Eq(p => p._id, deviceToken._id);
            var response = collection.Replace(deviceToken, filter, new UpdateOptions { IsUpsert = false }, id, methodName);

            if (response != null && !response.IsAcknowledged)
            {
                MongoDBResponse.ReturnError("Updating failed!");
            }

            return MongoDBResponse.ReturnSuccessWithId(id);
        }
    }
}
