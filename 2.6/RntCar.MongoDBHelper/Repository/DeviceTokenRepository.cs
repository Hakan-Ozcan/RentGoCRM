using MongoDB.Driver;
using RntCar.ClassLibrary;
using RntCar.MongoDBHelper.Model;
using RntCar.SDK.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.MongoDBHelper.Repository
{
    public class DeviceTokenRepository : MongoDBInstance
    {
        private string collectionName { get; set; }
        public DeviceTokenRepository(string serverName, string dataBaseName) : base(serverName, dataBaseName)
        {
            collectionName = StaticHelper.GetConfiguration("MongoDBDeviceTokenCollectionName");
        }

        public DeviceTokenRepository(object client, object database) : base(client, database)
        {
            collectionName = StaticHelper.GetConfiguration("MongoDBDeviceTokenCollectionName");
        }

        public DeviceTokenDataMongoDB GetDeviceTokenDataByDeviceToken(string deviceToken)
        {
            return this._database.GetCollection<DeviceTokenDataMongoDB>("DeviceTokens").AsQueryable()
               .Where(q => q.token.Equals(deviceToken)).FirstOrDefault();
        }
    }
}
