using MongoDB.Driver;
using RntCar.ClassLibrary;
using RntCar.MongoDBHelper.Model;
using System.Collections.Generic;
using System.Linq;

namespace RntCar.MongoDBHelper.Repository
{
    public class CorporateCustomerRepository : MongoDBInstance
    {
        public CorporateCustomerRepository(string serverName, string dataBaseName) : base(serverName, dataBaseName)
        {
        }

        public CorporateCustomerRepository(object client, object database) : base(client, database)
        {
        }
        public List<CorporateCustomerDataMongoDB> getCustomers()
        {
            return this._database.GetCollection<CorporateCustomerDataMongoDB>("CorporateCustomers")
                       .AsQueryable()
                       .ToList();

        }
        public RntCar.ClassLibrary.MongoDB.CorporateCustomerData getCustomerById(string corporateCustomerId)
        {
            return this._database.GetCollection<CorporateCustomerDataMongoDB>("CorporateCustomers")
                       .AsQueryable()
                       .Where(p => p.corporateCustomerId.Equals(corporateCustomerId) &&
                              p.statecode == (int)GlobalEnums.StateCode.Active).FirstOrDefault();

        }

        public RntCar.ClassLibrary.MongoDB.CorporateCustomerData getCorporateCustomerByBrokerCode(string brokerCode)
        {
            return this._database.GetCollection<CorporateCustomerDataMongoDB>("CorporateCustomers")
                       .AsQueryable()
                       .Where(p => p.brokerCode.Equals(brokerCode) &&
                              p.statecode == (int)GlobalEnums.StateCode.Active).FirstOrDefault();

        }
    }
}
