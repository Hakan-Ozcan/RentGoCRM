using RntCar.MongoDBHelper.Model;
using RntCar.SDK.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.MongoDBHelper.Entities
{
    public class AvailabilityQueryAnalyzerBusiness : MongoDBInstance
    {
        public AvailabilityQueryAnalyzerBusiness(string serverName, string dataBaseName) : base(serverName, dataBaseName)
        {
        }

        public void createAvailabilityQueryAnalyzer(AvailabilityQueryAnalyzer availabilityQueryAnalyzer)
        {
            var collection = this.getCollection<AvailabilityQueryAnalyzer>(StaticHelper.GetConfiguration("MongoDBAvailabilityQueryAnalyzer"));
           collection.Insert(availabilityQueryAnalyzer);
        }
    }
}
