using MongoDB.Driver;
using RntCar.ClassLibrary.MongoDB.BusinessClosure;
using RntCar.SDK.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RntCar.MongoDBHelper.Repository
{
    public class BusinessClosureRepository : MongoDBInstance
    {
        public BusinessClosureRepository(string serverName, string dataBaseName) : base(serverName, dataBaseName)
        {
        }

        public BusinessClosureRepository(object client, object database) : base(client, database)
        {
        }

        public List<BusinessClosureDataMongoDB> getBusinessClosureByDate(string pickupBranchId , DateTime pickupDateTime , DateTime dropoffDateTime)
        {

            var res = this._database.GetCollection<BusinessClosureDataMongoDB>(StaticHelper.GetConfiguration("MongoDBBusinessClosureCollectionName"))
                       .AsQueryable()
                       .Where(p => p.branchValues.Contains(pickupBranchId) &&
                       p.statecode == 0).ToList();
            //res = res.Where(p => pickupDateTime.converttoTimeStamp().IsBetween(Convert.ToInt32(p.beginDateTimestamp), Convert.ToInt32(p.endDateTimestamp))).ToList();
            //res = res.Where(p => dropoffDateTime.converttoTimeStamp().IsBetween(Convert.ToInt32(p.beginDateTimestamp), Convert.ToInt32(p.endDateTimestamp))).ToList();
            
            res = res.Where(p => 
                            pickupDateTime.converttoTimeStamp().IsBetween(Convert.ToInt32(p.beginDateTimestamp), Convert.ToInt32(p.endDateTimestamp)) ||
                            dropoffDateTime.converttoTimeStamp().IsBetween(Convert.ToInt32(p.beginDateTimestamp), Convert.ToInt32(p.endDateTimestamp))
                            ).ToList();
            return res;
        }
    }
}
