using MongoDB.Driver;
using RntCar.MongoDBHelper.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RntCar.MongoDBHelper.Repository
{
    public class EquipmentAvailabilityOldRepository : MongoDBInstance
    {
        public EquipmentAvailabilityOldRepository(string serverName, string dataBaseName) : base(serverName, dataBaseName)
        {
        }

        public EquipmentAvailabilityOldRepository(object client, object database) : base(client, database)
        {
        }

        public List<EquipmentAvailabilityOldDataMongoDB> getEquipmentByDate(DateTime startDate ,  DateTime publishDate)
        {
            //todo add statecode == 0
            return this._database.GetCollection<EquipmentAvailabilityOldDataMongoDB>("oldequipmentdata")
                           .AsQueryable()
                           .Where(p => p.publishDate <= publishDate &&
                                       p.publishDate >= startDate).ToList();

        }
    }
}
