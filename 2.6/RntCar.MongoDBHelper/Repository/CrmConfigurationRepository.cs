using MongoDB.Driver;
using RntCar.ClassLibrary;
using RntCar.MongoDBHelper.Model;
using System.Linq;

namespace RntCar.MongoDBHelper.Repository
{
    public class CrmConfigurationRepository : MongoDBInstance
    {
        public CrmConfigurationRepository(string serverName, string dataBaseName) : base(serverName, dataBaseName)
        {
        }

        public CrmConfigurationRepository(object client, object database) : base(client, database)
        {
        }

        public CrmConfigurationDataMongoDB getCrmConfigurationByKey(string key)
        {
            return this._database.GetCollection<CrmConfigurationDataMongoDB>("CrmConfigurations")
                       .AsQueryable()
                       .Where(p => p.name == key && p.statecode.Equals((int)GlobalEnums.StateCode.Active)).FirstOrDefault();
        }
    }
}
