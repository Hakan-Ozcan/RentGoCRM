using MongoDB.Driver;
using RntCar.ClassLibrary;
using RntCar.MongoDBHelper.Model;
using System.Linq;

namespace RntCar.MongoDBHelper.Repository
{
    public class OneWayFeeRepository : MongoDBInstance
    {
        public OneWayFeeRepository(string serverName, string dataBaseName) : base(serverName, dataBaseName)
        {
        }

        public OneWayFeeRepository(object client, object database) : base(client, database)
        {
        }

        public OneWayFeeDataMongoDB getOneWayFee(string pickupBranchId, string dropoffBranchId)
        {
            return this._database.GetCollection<OneWayFeeDataMongoDB>("OneWayFees")
                   .AsQueryable()
                   .Where(p => p.PickUpBranchId == pickupBranchId.Replace("{","").Replace("}","") &&
                               p.DropoffBranchId == dropoffBranchId.Replace("{", "").Replace("}", "") &&
                               p.StateCode.Equals((int)GlobalEnums.StateCode.Active)).FirstOrDefault();
        }
       
    }
}
