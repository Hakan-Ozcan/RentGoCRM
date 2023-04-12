using MongoDB.Bson;
using MongoDB.Driver;
using RntCar.ClassLibrary.MongoDB;
using RntCar.MongoDBHelper.Helper;
using RntCar.MongoDBHelper.Model;
using RntCar.MongoDBHelper.Repository;
using RntCar.SDK.Common;
using System;
using System.Linq;

namespace RntCar.MongoDBHelper.Entities
{
    public class CrmConfigurationBusiness : MongoDBInstance
    {
        private string _serverName;
        private string _dataBaseName;

        public CrmConfigurationBusiness(string serverName, string dataBaseName) : base(serverName, dataBaseName)
        {
            _serverName = serverName;
            _dataBaseName = dataBaseName;
        }

        public MongoDBResponse createConfiguration(CrmConfigurationData crmConfigurationData, string id)
        {
            var collection = this.getCollection<CrmConfigurationDataMongoDB>(StaticHelper.GetConfiguration("MongoDBCrmConfigurationCollectionName"));

            var crmConfiguration = new CrmConfigurationDataMongoDB();

            crmConfiguration = crmConfiguration.Map(crmConfigurationData);
            crmConfiguration._id = new ObjectId(new Guid(id).ToByteArray().Take(12).ToArray());

            var methodName = ErrorLogsHelper.GetCurrentMethod();
            var itemId = crmConfiguration._id.ToString();

            var response = collection.Insert(crmConfiguration, itemId, methodName);
            response.Id = Convert.ToString(crmConfiguration._id);

            return response;
        }

        public bool updateConfiguration(CrmConfigurationData crmConfigurationData, string id)
        {
            var collection = this.getCollection<CrmConfigurationDataMongoDB>(StaticHelper.GetConfiguration("MongoDBCrmConfigurationCollectionName"));

            var crmConfiguration = new CrmConfigurationDataMongoDB();
            crmConfiguration = crmConfiguration.Map(crmConfigurationData);
            crmConfiguration._id = new ObjectId(new Guid(id).ToByteArray().Take(12).ToArray());

            var methodName = ErrorLogsHelper.GetCurrentMethod();
            var itemId = crmConfiguration._id.ToString();

            var filter = Builders<CrmConfigurationDataMongoDB>.Filter.Eq(p => p._id, crmConfiguration._id);
            var response = collection.Replace(crmConfiguration, filter, new UpdateOptions { IsUpsert = false }, itemId, methodName);

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

        public CrmConfigurationData getCrmConfigurationByKey(string key)
        {
            CrmConfigurationRepository crmConfigurationRepository = new CrmConfigurationRepository(_serverName, _dataBaseName);
            return crmConfigurationRepository.getCrmConfigurationByKey(key);
        }
        public T getCrmConfigurationByKey<T>(string key)
        {
            CrmConfigurationRepository crmConfigurationRepository = new CrmConfigurationRepository(_serverName, _dataBaseName);
            var r = crmConfigurationRepository.getCrmConfigurationByKey(key);
            return (T)Convert.ChangeType(r.value, typeof(T));           
        }
    }
}
