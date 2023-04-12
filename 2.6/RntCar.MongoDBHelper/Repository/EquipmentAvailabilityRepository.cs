using MongoDB.Driver;
using RntCar.ClassLibrary;
using RntCar.MongoDBHelper.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.MongoDBHelper.Repository
{
    public class EquipmentAvailabilityRepository : MongoDBInstance
    {
        public EquipmentAvailabilityRepository(string serverName, string dataBaseName) : base(serverName, dataBaseName)
        {
        }

        public List<EquipmentAvailabilityDataMongoDB> getEquipmentAvailabilityByDate(long startDate, long endDate)
        {
            return this._database.GetCollection<EquipmentAvailabilityDataMongoDB>("EquipmentAvailability")
                .AsQueryable()
                .Where(p => p.PublishDate >= startDate && p.PublishDate <= endDate).ToList();
        }

        public List<EquipmentAvailabilityDataMongoDB> getEquipmentAvailability()
        {
            return this._database.GetCollection<EquipmentAvailabilityDataMongoDB>("EquipmentAvailability")
                .AsQueryable()
                .ToList();
        }
    }
}
