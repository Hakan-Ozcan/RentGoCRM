using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using RntCar.ClassLibrary;
using RntCar.ClassLibrary._Enums_1033;
using RntCar.MongoDBHelper.Model;
using RntCar.SDK.Common;

namespace RntCar.MongoDBHelper.Repository
{
    public class TransferRepository : MongoDBInstance
    {
        public TransferRepository(string serverName, string dataBaseName) : base(serverName, dataBaseName)
        {
        }
        public TransferRepository(object client, object database) : base(client, database)
        {
        }
        public List<TransferDataMongoDB> getAllTransfers()
        {
            return this._database.GetCollection<TransferDataMongoDB>("Transfers")
                        .AsQueryable()
                        .ToList();
        }
        public TransferDataMongoDB getTransferById(Guid transferId)
        {
            return this._database.GetCollection<TransferDataMongoDB>("Transfers")
               .AsQueryable()
               .Where(p => p.transferId.Equals(transferId)).FirstOrDefault();
        }
        public List<TransferDataMongoDB> getDeliveryTransfersByBranch(Guid branchId)
        {
            return this._database.GetCollection<TransferDataMongoDB>("Transfers")
                        .AsQueryable()
                        .Where(item => item.pickupBranchId.Equals(branchId) &&
                        item.statuscode.Equals(ClassLibrary._Enums_1033.rnt_transfer_StatusCode.WaitingForDelivery) &&
                        item.statecode.Equals((int)GlobalEnums.StateCode.Active))
                        .ToList();
        }
        public List<TransferDataMongoDB> getAvailableTransfers(DateTime queryPickupDate,
                                                               Guid queryPickupBranchId)
        {
            var pickupTimeStamp = queryPickupDate.converttoTimeStamp();
            var collection = this._database.GetCollection<TransferDataMongoDB>("Transfers")
                .AsQueryable()
                .Where(p => p.estimatedDropoffDateTimeStamp <= pickupTimeStamp &&
                       p.dropoffBranchId.Equals(Convert.ToString(queryPickupBranchId)) &&
                       p.pickupBranchId != Convert.ToString(queryPickupBranchId) &&
                       (p.statuscode == (int)ClassLibrary._Enums_1033.rnt_transfer_StatusCode.WaitingForDelivery ||
                       p.statuscode == (int)ClassLibrary._Enums_1033.rnt_transfer_StatusCode.Transferred)).ToList();

            return collection;
        }
        public List<TransferDataMongoDB> getReturnTransfersByBranch(Guid branchId)
        {
            return this._database.GetCollection<TransferDataMongoDB>("Transfers")
                        .AsQueryable()
                        .Where(item => item.dropoffBranchId.Equals(branchId) &&
                                       item.statuscode.Equals(ClassLibrary._Enums_1033.rnt_transfer_StatusCode.Transferred) &&
                                       item.statecode.Equals((int)GlobalEnums.StateCode.Active))
                        .ToList();
        }
        public List<TransferDataMongoDB> getExcludeTransfers(DateTime queryPickupDate,
                                                             DateTime queryDropOffTime,
                                                             Guid queryPickupBranchId)
        {
            List<TransferDataMongoDB> allTransfers = new List<TransferDataMongoDB>();
            Int32 queryPickupTimeStamp = queryPickupDate.converttoTimeStamp();
            Int32 queryDropoffTimeStamp = queryDropOffTime.converttoTimeStamp();

            //var queryPickupBsonTimeStamp = new BsonTimestamp(queryPickupTimeStamp);
            //var queryDropoffBsonTimeStamp = new BsonTimestamp(queryDropoffTimeStamp);

            var collectionPickup = this._database.GetCollection<TransferDataMongoDB>("Transfers")
                                    .AsQueryable()
                                    .Where(p =>
                                    queryPickupTimeStamp <= p.estimatedPickupDateTimeStamp &&
                                    p.estimatedPickupDateTimeStamp <= queryDropoffTimeStamp &&
                                    p.estimatedDropoffDateTimeStamp > queryDropoffTimeStamp &&
                                    p.pickupBranchId.Equals(Convert.ToString(queryPickupBranchId)) &&
                                    p.statuscode != (int)ClassLibrary._Enums_1033.rnt_transfer_StatusCode.Completed &&
                                    p.statecode == 0).ToList();
            allTransfers.AddRange(collectionPickup);

            var collectionDropOff = this._database.GetCollection<TransferDataMongoDB>("Transfers")
               .AsQueryable()
               .Where(p => queryPickupTimeStamp < p.estimatedDropoffDateTimeStamp &&
                      p.estimatedPickupDateTimeStamp <= queryDropoffTimeStamp &&
                      p.pickupBranchId.Equals(Convert.ToString(queryPickupBranchId)) &&
                      p.statuscode != (int)ClassLibrary._Enums_1033.rnt_transfer_StatusCode.Completed &&
                                    p.statecode == 0).ToList();

            allTransfers.AddRange(collectionDropOff);

            var collectionBoth = this._database.GetCollection<TransferDataMongoDB>("Transfers")
               .AsQueryable()
               .Where(p => queryPickupTimeStamp > p.estimatedPickupDateTimeStamp &&
                      queryDropoffTimeStamp < p.estimatedDropoffDateTimeStamp &&
                      p.pickupBranchId.Equals(Convert.ToString(queryPickupBranchId)) &&
                      p.statuscode != (int)ClassLibrary._Enums_1033.rnt_transfer_StatusCode.Completed &&
                                    p.statecode == 0).ToList();

            allTransfers.AddRange(collectionBoth);

            var collectionDropOff1 = this._database.GetCollection<TransferDataMongoDB>("Transfers")
               .AsQueryable()
               .Where(p => queryPickupTimeStamp <= p.estimatedPickupDateTimeStamp &&
                          p.estimatedPickupDateTimeStamp <= queryDropoffTimeStamp &&
                          queryPickupTimeStamp <= p.estimatedDropoffDateTimeStamp &&
                          p.estimatedDropoffDateTimeStamp <= queryDropoffTimeStamp &&
                          p.pickupBranchId.Equals(Convert.ToString(queryPickupBranchId)) &&
                          p.statuscode != (int)ClassLibrary._Enums_1033.rnt_transfer_StatusCode.Completed &&
                                    p.statecode == 0).ToList();
            allTransfers.AddRange(collectionDropOff1);

            return allTransfers;
        }
        public TransferDataMongoDB getTransferDataByEquipmentId(Guid equipmentId)
        {
            return this._database.GetCollection<TransferDataMongoDB>("Transfers")
                        .AsQueryable()
                        .Where(item => item.statecode.Equals((int)GlobalEnums.StateCode.Active) &&
                                       item.equipmentId.Equals(equipmentId) &&
                                       item.statuscode.Equals((int)ClassLibrary._Enums_1033.rnt_transfer_StatusCode.Transferred))
                        .FirstOrDefault();
        }

        public TransferDataMongoDB getTransferDataByEquipmentId_Status(Guid equipmentId)
        {
            return this._database.GetCollection<TransferDataMongoDB>("Transfers")
                        .AsQueryable()
                        .Where(item => item.statecode.Equals((int)GlobalEnums.StateCode.Active) &&
                                       item.equipmentId.Equals(equipmentId) &&
                                       (item.statuscode.Equals((int)rnt_transfer_StatusCode.Transferred) ||
                                        item.statuscode.Equals((int)rnt_transfer_StatusCode.WaitingForDelivery)))
                        .FirstOrDefault();
        }
    }
}
