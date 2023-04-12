using MongoDB.Bson;
using MongoDB.Driver;
using RntCar.ClassLibrary;
using RntCar.ClassLibrary.MongoDB;
using RntCar.ClassLibrary.Report;
using RntCar.MongoDBHelper.Helper;
using RntCar.MongoDBHelper.Model;
using RntCar.SDK.Common;
using System;

namespace RntCar.MongoDBHelper.Entities
{
    public class EquipmentBusiness : MongoDBInstance
    {
        public EquipmentBusiness(string serverName, string dataBaseName) : base(serverName, dataBaseName)
        {
        }

        public MongoDBResponse CreateEquipment(EquipmentData equipmentData)
        {
            var collection = this.getCollection<EquipmentDataMongoDB>(StaticHelper.GetConfiguration("MongoDBEquipmentCollectionName"));

            var equipment = new EquipmentDataMongoDB();

            equipment.ShallowCopy(equipmentData);
            equipment._id = ObjectId.GenerateNewId();

            var methodName = ErrorLogsHelper.GetCurrentMethod();
            var itemId = equipment._id.ToString();

            var response = collection.Insert(equipment, itemId, methodName);
            response.Id = Convert.ToString(equipment._id);

            return response;
        }
        public bool UpdateEquipment(EquipmentData equipmentData, string id)
        {
            var collection = this.getCollection<EquipmentDataMongoDB>(StaticHelper.GetConfiguration("MongoDBEquipmentCollectionName"));

            var equipment = new EquipmentDataMongoDB();

            equipment.ShallowCopy(equipmentData);
            equipment._id = ObjectId.Parse(id);

            var methodName = ErrorLogsHelper.GetCurrentMethod();
            var itemId = equipment._id.ToString();

            var filter = Builders<EquipmentDataMongoDB>.Filter.Eq(p => p._id, equipment._id);
            var response = collection.Replace(equipment, filter, new UpdateOptions { IsUpsert = false }, itemId, methodName);
            //var response = collection.ReplaceOne(p => p._id == equipment._id, equipment, new UpdateOptions { IsUpsert = false });

            if (response != null)
            {
                if (response.IsAcknowledged)
                {
                    return true;
                }
                return false;
            }
            else
            {
                return false;
            }
        }

        public MongoDBResponse createFleetReportData(FleetReportData fleetReportData)
        {
            var collection = this.getCollection<FleetReportDataMongoDB>(StaticHelper.GetConfiguration("MongoDBFleetReportCollectionName"));

            var fleetReportItem = new FleetReportDataMongoDB();
            fleetReportItem = fleetReportItem.Map(fleetReportData);
            fleetReportItem._id = ObjectId.GenerateNewId();

            var response = collection.Insert(fleetReportItem, Convert.ToString(fleetReportItem._id), ErrorLogsHelper.GetCurrentMethod());

            return response;

        }
    }
}
