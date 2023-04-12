using RntCar.MongoDBHelper.Model;
using RntCar.SDK.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Driver;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.MongoDBHelper.Repository
{
    public class AvailabilityQueryAnalyzerRepository : MongoDBInstance
    {
        public AvailabilityQueryAnalyzerRepository(string serverName, string dataBaseName) : base(serverName, dataBaseName)
        {
        }

        public AvailabilityQueryAnalyzerRepository(object client, object database) : base(client, database)
        {
        }
        public List<AvailabilityQueryAnalyzer> getAllAvailabilityQueryAnalyzer()
        {
            //get active price list
            return this._database.GetCollection<AvailabilityQueryAnalyzer>("AvailabilityQueryAnalyzer")
                           .AsQueryable()
                           .ToList();
                          

        }
    }
}
