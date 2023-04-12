using RntCar.ClassLibrary;
using RntCar.ClassLibrary.MongoDB;
using RntCar.MongoDBHelper.Model;
using RntCar.SDK.Common;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using RntCar.MongoDBHelper.Helper;

namespace RntCar.MongoDBHelper.Entities
{
    public class EquipmentAvailabilityBusiness : MongoDBInstance
    {
        private IMongoCollection<EquipmentAvailabilityDataMongoDB> collection;
        private bool isRecordsDeleted = false;
        public EquipmentAvailabilityBusiness(string serverName, string dataBaseName, string[] args) : base(serverName, dataBaseName)
        {
            collection = this.getCollection<EquipmentAvailabilityDataMongoDB>(StaticHelper.GetConfiguration("MongoDBEquipmentAvailabilityCollectionName"));

            var publishDate = DateTime.Now.Date;
            if (args.Length != 0)
            {
                publishDate = DateTime.Parse(args[0]);
            }

            isRecordsDeleted = deleteSameData(publishDate.Ticks);
        }

        private bool deleteSameData(long date)
        {
            try
            {
                collection.DeleteMany(p => p.PublishDate == date);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public MongoDBResponse createEquipmentAvailability(EquipmentAvailabilityData equipmentAvailabilityData)
        {
            if (isRecordsDeleted)
            {
                var equipmentAvailabilityItem = new EquipmentAvailabilityDataMongoDB();
                equipmentAvailabilityItem = equipmentAvailabilityItem.Map(equipmentAvailabilityData);
                equipmentAvailabilityItem._id = ObjectId.GenerateNewId();

                var response = collection.Insert(equipmentAvailabilityItem, Convert.ToString(equipmentAvailabilityItem._id), ErrorLogsHelper.GetCurrentMethod());

                return response;
            }

            return MongoDBResponse.ReturnError("Cannot delete same data");
        }
    }
}
