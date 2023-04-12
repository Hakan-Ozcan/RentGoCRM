using MongoDB.Bson;
using RntCar.MongoDBHelper.Model;

namespace RntCar.MongoDBHelper.Entities
{
    public class BrokerAvailabilityLogBusiness : MongoDBInstance
    {
        public BrokerAvailabilityLogBusiness(string serverName, string dataBaseName) : base(serverName, dataBaseName)
        {
        }

        public BrokerAvailabilityLogBusiness(object client, object database) : base(client, database)
        {
        }
        public void createBrokerAvailabilityLog(BrokerAvailabilityLog brokerAvailabilityLog)
        {
            var collection = this.getCollection<BrokerAvailabilityLog>("brokeravailabilitylogs");          
            brokerAvailabilityLog._id = ObjectId.GenerateNewId();            

            var response = collection.Insert(brokerAvailabilityLog, "", "");
        }
    }
}
