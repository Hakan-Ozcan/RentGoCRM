using MongoDB.Driver;
using RntCar.ClassLibrary.MongoDB;
using RntCar.ClassLibrary.MongoDB.BusinessClosure;
using RntCar.MongoDBHelper.Helper;
using RntCar.SDK.Common;
using System;

namespace RntCar.MongoDBHelper.Entities
{
    public class BusinessClosureBusiness : MongoDBInstance
    {
        public BusinessClosureBusiness(string serverName, string dataBaseName) : base(serverName, dataBaseName)
        {
        }

        public BusinessClosureBusiness(object client, object database) : base(client, database)
        {
        }

        public MongoDBResponse CreateBusinessClosure(BusinessClosureData businessClosureData)
        {
            var collection = this.getCollection<BusinessClosureData>(StaticHelper.GetConfiguration("MongoDBBusinessClosureCollectionName"));
            var methodName = ErrorLogsHelper.GetCurrentMethod();
            var response = collection.Insert(businessClosureData, businessClosureData.businessClosureId, methodName);
            response.Id = Convert.ToString(businessClosureData.businessClosureId);

            return response;
        }
        public bool UpdateBusinessClosure(BusinessClosureData businessClosureData, string id)
        {
            var collection = this.getCollection<BusinessClosureData>(StaticHelper.GetConfiguration("MongoDBBusinessClosureCollectionName"));
            var methodName = ErrorLogsHelper.GetCurrentMethod();
            var filter = Builders<BusinessClosureData>.Filter.Eq(p => p.businessClosureId, id);
            ReplaceOneResult response = collection.Replace(businessClosureData, filter, new UpdateOptions { IsUpsert = false }, id, methodName);

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
