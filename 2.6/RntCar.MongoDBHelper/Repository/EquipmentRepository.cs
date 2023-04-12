using RntCar.MongoDBHelper.Model;
using System.Linq;
using MongoDB.Driver;
using System.Collections.Generic;
using System;
using RntCar.ClassLibrary;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.IdGenerators;

namespace RntCar.MongoDBHelper.Repository
{
    public class EquipmentRepository : MongoDBInstance
    {
        public EquipmentRepository(string serverName, string dataBaseName) : base(serverName, dataBaseName)
        {
        }

        public EquipmentRepository(object client, object database) : base(client, database)
        {
        }

        public List<EquipmentDataMongoDB> getCurrentEquipments(Guid pickupBranchId)
        {

            return this._database.GetCollection<EquipmentDataMongoDB>("Equipments")
                           .AsQueryable()
                           .Where(p => p.CurrentBranchId.Equals(pickupBranchId) && p.StateCode.Equals((int)GlobalEnums.StateCode.Active)).ToList();


        }
        public List<EquipmentDataMongoDB> getAllActiveEquipmentsByBranchId(Guid pickupBranchId)
        {
            return this._database.GetCollection<EquipmentDataMongoDB>("Equipments")
                           .AsQueryable()
                           .Where(p => p.StateCode.Equals((int)GlobalEnums.StateCode.Active) &&
                                       p.CurrentBranchId.Equals(pickupBranchId)).ToList();

        }
        //TODO: Daha güzel hale getir. (abdllhcay)
        public List<EquipmentDataMongoDB> getAllActiveEquipmentsByStatusCodes(Guid branchId, List<int> statusCodes)
        {
            List<EquipmentDataMongoDB> result = new List<EquipmentDataMongoDB>();

            statusCodes.ForEach(statusCode =>
            {
                result.AddRange(this._database.GetCollection<EquipmentDataMongoDB>("Equipments")
                          .AsQueryable()
                          .Where(p => p.StateCode.Equals(GlobalEnums.StateCode.Active) &&
                                      p.StatusCode.Equals(statusCode) &&
                                      p.CurrentBranchId.Equals(branchId)).ToList());
            });

            return result;
        }
        public List<EquipmentDataMongoDB> getAllAvailableEquipmentsByBranchId(Guid pickupBranchId)
        {
            return this._database.GetCollection<EquipmentDataMongoDB>("Equipments")
                           .AsQueryable()
                           .Where(p => p.StateCode.Equals(GlobalEnums.StateCode.Active) &&
                                       (p.StatusCode.Equals((int)ClassLibrary._Enums_1033.rnt_equipment_StatusCode.Available) ||
                                       p.StatusCode.Equals((int)ClassLibrary._Enums_1033.rnt_equipment_StatusCode.InFuelFilling) ||
                                       p.StatusCode.Equals((int)ClassLibrary._Enums_1033.rnt_equipment_StatusCode.InWashing)) &&
                                       p.CurrentBranchId.Equals(pickupBranchId)).ToList();

        }
        public EquipmentDataMongoDB getEquipmentByPlateNumber(string plateNumber)
        {
            return this._database.GetCollection<EquipmentDataMongoDB>("Equipments")
                           .AsQueryable()
                           .Where(p => p.PlateNumber.Equals(plateNumber) &&
                                  p.StatusCode.Equals((int)EquipmentEnums.StatusCode.Rental)).FirstOrDefault();

        }
        public EquipmentDataMongoDB getEquipmentByPlateNumberByAnyConditions(string plateNumber)
        {
            return this._database.GetCollection<EquipmentDataMongoDB>("Equipments")
                           .AsQueryable()
                           .Where(p => p.PlateNumber.Equals(plateNumber)).FirstOrDefault();

        }
        public List<EquipmentDataMongoDB> getBranchEquipmentsByGroupCode(Guid pickupBranchId, string groupCodeId)
        {

            return this._database.GetCollection<EquipmentDataMongoDB>("Equipments")
                           .AsQueryable()
                           .Where(p => p.CurrentBranchId.Equals(pickupBranchId) &&
                                  p.GroupCodeInformationId.Equals(groupCodeId) &&
                                  p.StateCode.Equals((int)GlobalEnums.StateCode.Active)).ToList();

        }
        public EquipmentDataMongoDB getEquipmentById(Guid equipmentId)
        {

            return this._database.GetCollection<EquipmentDataMongoDB>("Equipments")
                           .AsQueryable()
                           .Where(p => p.EquipmentId.Equals(equipmentId)).FirstOrDefault();

        }
        public List<EquipmentDataMongoDB> getAllEquipments()
        {
            //TODO
            //add active filterr to currrent branches
            //state field must be send to mongodb for this

            //var filter = Builders<EquipmentDataMongoDB>.Filter.Eq("Name", "FORD FIESTA CUSTOM 8+1 300 L 155 TITANIUM 61 VZ 835");
            //var results = this._database.GetCollection<EquipmentDataMongoDB>("Equipments").Find(filter).ToList();

            return this._database.GetCollection<EquipmentDataMongoDB>("Equipments")
                           .AsQueryable()
                           .ToList();

        }

        public List<FleetReportDataMongoDB> GetEquipmentForFleetReport(long startDate, long endDate)
        {
            return this._database.GetCollection<FleetReportDataMongoDB>("FleetReportData")
                           .AsQueryable()
                            .Where(p => p.publishDate >= startDate && p.publishDate <= endDate).ToList();
        }

        public void BulkDeleteEquipmentForFleetReport(List<FleetReportDataMongoDB> reportDatas)
        {
            foreach (var item in reportDatas)
            {
                var collection = this.getCollection<FleetReportDataMongoDB>("FleetReportData");


                collection.DeleteOne(p => p._id == item._id);
            }
        }
    }


}
