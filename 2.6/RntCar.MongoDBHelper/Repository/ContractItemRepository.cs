using MongoDB.Bson;
using MongoDB.Driver;
using RntCar.ClassLibrary;
using RntCar.ClassLibrary._Enums_1033;
using RntCar.ClassLibrary._Tablet;
using RntCar.MongoDBHelper.Model;
using RntCar.SDK.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RntCar.MongoDBHelper.Repository
{
    public class ContractItemRepository : MongoDBInstance
    {
        private string collectionName { get; set; }
        public ContractItemRepository(string serverName, string dataBaseName) : base(serverName, dataBaseName)
        {
            collectionName = StaticHelper.GetConfiguration("MongoDBContractItemCollectionName");
        }

        public ContractItemRepository(object client, object database) : base(client, database)
        {
            collectionName = StaticHelper.GetConfiguration("MongoDBContractItemCollectionName");
        }
        public ContractItemDataMongoDB getContractItemEquipmentRentalByEquipmentId(string equipmentId)
        {
            return this._database.GetCollection<ContractItemDataMongoDB>(collectionName)
                       .AsQueryable()
                       .Where(p => p.itemTypeCode.Equals((int)ContractItemEnums.ItemTypeCode.Equipment) &&
                              p.equipmentId.Equals(equipmentId) &&
                              p.statuscode.Equals((int)ContractItemEnums.StatusCode.Rental)).FirstOrDefault();
        }
        public List<ContractItemDataMongoDB> getContractItems()
        {
            return this._database.GetCollection<ContractItemDataMongoDB>(collectionName)
                           .AsQueryable().ToList();
        }
        public List<ContractItemDataMongoDB> getContractItemsByContractId(string contractId)
        {
            return this._database.GetCollection<ContractItemDataMongoDB>(collectionName)
                       .AsQueryable()
                       .Where(p => p.contractId.Equals(contractId)).ToList();
        }
        public ContractItemDataMongoDB getContractItemById(string contractItemId)
        {
            return this._database.GetCollection<ContractItemDataMongoDB>(collectionName)
                       .AsQueryable()
                       .Where(p => p.contractItemId.Equals(contractItemId)).FirstOrDefault();
        }
        public ContractItemDataMongoDB getContractItemEquipment(string contractId)
        {
            return this._database.GetCollection<ContractItemDataMongoDB>(collectionName)
                       .AsQueryable()
                       .Where(p => p.itemTypeCode.Equals((int)rnt_contractitem_rnt_itemtypecode.Equipment) &&
                              p.contractId.Equals(contractId) &&
                             (p.statuscode.Equals((int)ContractItemEnums.StatusCode.Rental) ||
                              p.statuscode.Equals((int)ContractItemEnums.StatusCode.WaitingforDelivery))).FirstOrDefault();
        }
        public List<ContractItemDataMongoDB> getContractItemsEquipment(string contractId)
        {
            return this._database.GetCollection<ContractItemDataMongoDB>(collectionName)
                       .AsQueryable()
                       .Where(p => p.itemTypeCode.Equals((int)rnt_contractitem_rnt_itemtypecode.Equipment) &&
                              p.contractId.Equals(contractId) &&
                             (p.statuscode.Equals((int)ContractItemEnums.StatusCode.Rental) ||
                              p.statuscode.Equals((int)ContractItemEnums.StatusCode.Completed) ||
                              p.statuscode.Equals((int)ContractItemEnums.StatusCode.WaitingforDelivery))).ToList();
        }
        public ContractItemDataMongoDB getPriceDifference(string contractId)
        {
            return this._database.GetCollection<ContractItemDataMongoDB>(collectionName)
                       .AsQueryable()
                       .Where(p => p.itemTypeCode.Equals((int)rnt_contractitem_rnt_itemtypecode.PriceDifference) &&
                              p.contractId.Equals(contractId) &&
                              p.paymentMethod == (int)rnt_PaymentMethodCode.PayBroker).FirstOrDefault();
        }
        public ContractItemDataMongoDB getContractItemEquipmentCompleted_corporateBillingType(string contractId)
        {
            return this._database.GetCollection<ContractItemDataMongoDB>(collectionName)
                       .AsQueryable()
                       .Where(p => p.itemTypeCode.Equals((int)rnt_contractitem_rnt_itemtypecode.Equipment) &&
                              p.contractId.Equals(contractId) &&
                              p.billingType.Equals((int)rnt_BillingTypeCode.Corporate) &&
                              p.statuscode.Equals((int)ContractItemEnums.StatusCode.Completed)).FirstOrDefault();
        }
        public ContractItemDataMongoDB getContractItemEquipmentWaitingForDelivery(string contractId)
        {
            return this._database.GetCollection<ContractItemDataMongoDB>(collectionName)
                       .AsQueryable()
                       .Where(p => p.itemTypeCode.Equals((int)ContractItemEnums.ItemTypeCode.Equipment) &&
                              p.contractId.Equals(contractId) &&
                              p.statuscode.Equals((int)ContractItemEnums.StatusCode.WaitingforDelivery)).FirstOrDefault();
        }
        public ContractItemDataMongoDB getContractItemEquipmentRental(string contractId)
        {
            return this._database.GetCollection<ContractItemDataMongoDB>(collectionName)
                       .AsQueryable()
                       .Where(p => p.itemTypeCode.Equals((int)rnt_contractitem_rnt_itemtypecode.Equipment) &&
                              p.contractId.Equals(contractId) &&
                              p.statuscode.Equals((int)ContractItemEnums.StatusCode.Rental)).FirstOrDefault();
        }
        public ContractItemDataMongoDB getContractItemEquipmentByStatusCode(string contractId, int statusCode)
        {
            return this._database.GetCollection<ContractItemDataMongoDB>(collectionName)
                       .AsQueryable()
                       .Where(p => p.itemTypeCode.Equals((int)ContractItemEnums.ItemTypeCode.Equipment) &&
                              p.contractId.Equals(contractId) &&
                              p.statuscode.Equals(statusCode)).FirstOrDefault();
        }
        public ContractItemDataMongoDB[] getWaitingforDeliveryContractsByBranchId(GetContractsByBranchParameters getContractsByBranchParameters)
        {
            var yesterday = DateTime.UtcNow.AddDays(-1);
            var theDayBefore = DateTime.UtcNow.AddDays(-2);
            var today = DateTime.UtcNow;
            var tomorrow = today.AddDays(1);

            var query = this._database.GetCollection<ContractItemDataMongoDB>(collectionName)
                       .AsQueryable()
                       .Where(p => p.pickupBranchId.Equals(getContractsByBranchParameters.branchId) &&
                              p.statuscode.Equals((int)ContractItemEnums.StatusCode.WaitingforDelivery) &&
                              p.itemTypeCode.Equals((int)ContractItemEnums.ItemTypeCode.Equipment))
                       .ToArray();
            query = query.Where(p => p.PickupTimeStamp.Value.converttoDateTime().Day == today.Day ||
                                     p.PickupTimeStamp.Value.converttoDateTime().Day == tomorrow.Day ||
                                    p.PickupTimeStamp.Value.converttoDateTime().Day == yesterday.Day ||
                                    p.PickupTimeStamp.Value.converttoDateTime().Day == theDayBefore.Day).ToArray();

            query = query.Where(p => p.PickupTimeStamp.Value.converttoDateTime().Month == today.Month ||
                                    p.PickupTimeStamp.Value.converttoDateTime().Month == tomorrow.Month ||
                                    p.PickupTimeStamp.Value.converttoDateTime().Month == yesterday.Month ||
                                    p.PickupTimeStamp.Value.converttoDateTime().Day == theDayBefore.Month).ToArray();

            query = query.Where(p => p.PickupTimeStamp.Value.converttoDateTime().Year == today.Year ||
                                    p.PickupTimeStamp.Value.converttoDateTime().Year == tomorrow.Year ||
                                    p.PickupTimeStamp.Value.converttoDateTime().Year == yesterday.Year ||
                                    p.PickupTimeStamp.Value.converttoDateTime().Day == theDayBefore.Year).ToArray();
            return query;

        }
        public ContractItemDataMongoDB[] getWaitingforDeliveryContractsByBranchId_PayBroker(GetContractsByBranchParameters getContractsByBranchParameters)
        {
            var yesterday = DateTime.UtcNow.AddDays(-1);
            var today = DateTime.UtcNow;
            var tomorrow = today.AddDays(1);

            var query = this._database.GetCollection<ContractItemDataMongoDB>(collectionName)
                       .AsQueryable()
                       .Where(p => p.pickupBranchId.Equals(getContractsByBranchParameters.branchId) &&
                              p.statuscode.Equals((int)ContractItemEnums.StatusCode.WaitingforDelivery) &&
                              p.itemTypeCode.Equals((int)ContractItemEnums.ItemTypeCode.Equipment) &&
                              p.paymentMethod == (int)rnt_PaymentMethodCode.PayBroker)
                       .ToArray();
            query = query.Where(p => p.pickupDateTimeStamp_Header.Value.converttoDateTime().Day == today.Day ||
                                     p.pickupDateTimeStamp_Header.Value.converttoDateTime().Day == tomorrow.Day ||
                                     p.pickupDateTimeStamp_Header.Value.converttoDateTime().Day == yesterday.Day).ToArray();

            query = query.Where(p => p.pickupDateTimeStamp_Header.Value.converttoDateTime().Month == today.Month ||
                                     p.pickupDateTimeStamp_Header.Value.converttoDateTime().Month == tomorrow.Month ||
                                     p.pickupDateTimeStamp_Header.Value.converttoDateTime().Month == yesterday.Month).ToArray();

            query = query.Where(p => p.pickupDateTimeStamp_Header.Value.converttoDateTime().Year == today.Year ||
                                     p.pickupDateTimeStamp_Header.Value.converttoDateTime().Year == tomorrow.Year ||
                                     p.pickupDateTimeStamp_Header.Value.converttoDateTime().Year == yesterday.Year).ToArray();
            return query;

        }
        public ContractItemDataMongoDB[] getRentalContractsByBranchId(GetContractsByBranchParameters getContractsByBranchParameters)
        {
            var equipmentCollectionName = StaticHelper.GetConfiguration("MongoDBEquipmentCollectionName");

            var yesterday = DateTime.UtcNow.AddDays(-1);
            var today = DateTime.UtcNow;
            var tomorrow = today.AddDays(1);
            var contractItemCollection = this._database.GetCollection<ContractItemDataMongoDB>(collectionName).AsQueryable();
            var equipmentCollection = this._database.GetCollection<EquipmentDataMongoDB>(equipmentCollectionName).AsQueryable();

            var query = (from p in contractItemCollection.AsQueryable()
                         join e in equipmentCollection.AsQueryable() on p.equipmentId equals e.EquipmentId into equipment
                         select new ContractItemDataMongoDB
                         {
                             contractId = p.contractId,
                             plateNumber = equipment.ElementAt(0).PlateNumber,
                             statuscode = p.statuscode,
                             itemTypeCode = p.itemTypeCode,
                             contractNumber = p.contractNumber,
                             customerId = p.customerId,
                             customerName = p.customerName,
                             pickupBranchId = p.pickupBranchId,
                             DropoffTimeStamp = p.DropoffTimeStamp,
                             dropoffBranchId = p.dropoffBranchId,
                             equipmentId = p.equipmentId,
                             equipmentName = p.equipmentName,
                             groupCodeInformationsId = p.groupCodeInformationsId,
                             groupCodeInformationsName = p.groupCodeInformationsName,
                             PickupTimeStamp = p.PickupTimeStamp,
                             pnrNumber = p.pnrNumber
                         }).Where(p => p.dropoffBranchId.Equals(getContractsByBranchParameters.branchId) &&
                                  p.statuscode.Equals((int)ContractItemEnums.StatusCode.Rental) &&
                                  p.itemTypeCode.Equals((int)ContractItemEnums.ItemTypeCode.Equipment))
                                 .ToArray();

            query = query.Where(p => p.DropoffTimeStamp.Value.converttoDateTime().Day == today.Day ||
                                p.DropoffTimeStamp.Value.converttoDateTime().Day == tomorrow.Day ||
                                p.DropoffTimeStamp.Value.converttoDateTime().Day == yesterday.Day).ToArray();

            query = query.Where(p => p.DropoffTimeStamp.Value.converttoDateTime().Month == today.Month ||
                                   p.DropoffTimeStamp.Value.converttoDateTime().Month == tomorrow.Month ||
                                   p.DropoffTimeStamp.Value.converttoDateTime().Month == yesterday.Month).ToArray();

            query = query.Where(p => p.DropoffTimeStamp.Value.converttoDateTime().Year == today.Year ||
                                    p.DropoffTimeStamp.Value.converttoDateTime().Year == tomorrow.Year ||
                                    p.DropoffTimeStamp.Value.converttoDateTime().Year == yesterday.Year).ToArray();
            return query;

        }

        public ContractItemDataMongoDB getRentalContractsByPlateNumber(GetContractsByEquipmentParameters getContractsByEquipmentParameters)
        {
            var today = DateTime.UtcNow;
            var tomorrow = today.AddDays(1);
            var contractItemCollection = this._database.GetCollection<ContractItemDataMongoDB>(collectionName).AsQueryable();

            EquipmentRepository equipmentRepository = new EquipmentRepository(this._client, this._database);
            var equipment = equipmentRepository.getEquipmentByPlateNumber(getContractsByEquipmentParameters.plateNumber);

            if (equipment == null)
            {
                return null;
            }
            var contract = this.getContractItemEquipmentRentalByEquipmentId(equipment.EquipmentId);
            contract.plateNumber = equipment.PlateNumber;
            return contract;
        }



        public List<ContractItemDataMongoDB> getAvailableContracts(DateTime queryPickupDate,
                                                                        Guid queryPickupBranchId)
        {
            var pickupTimeStamp = new BsonTimestamp(queryPickupDate.converttoTimeStamp());
            //index_2
            var collection = this._database.GetCollection<ContractItemDataMongoDB>(collectionName)
                            .AsQueryable()
                            .Where(p => p.DropoffTimeStamp <= pickupTimeStamp &&
                                   p.dropoffBranchId.Equals(Convert.ToString(queryPickupBranchId)) &&
                                   p.pickupBranchId != Convert.ToString(queryPickupBranchId) &&
                                   p.itemTypeCode.Equals(1) &&
                                   (p.statuscode.Equals((int)ContractItemEnums.StatusCode.Rental) ||
                                    p.statuscode.Equals((int)ContractItemEnums.StatusCode.WaitingforDelivery))).ToList();

            return collection;

        }

        public List<ContractItemDataMongoDB> getExcludedContracts(DateTime queryPickupDate,
                                                                     DateTime queryDropOffTime,
                                                                     Guid queryPickupBranchId)
        {
            var allReservations = new List<ContractItemDataMongoDB>();
            Int32 queryPickupTimeStamp = queryPickupDate.converttoTimeStamp();
            Int32 queryDropoffTimeStamp = queryDropOffTime.converttoTimeStamp();

            var queryPickupBsonTimeStamp = new BsonTimestamp(queryPickupTimeStamp);
            var queryDropoffBsonTimeStamp = new BsonTimestamp(queryDropoffTimeStamp);

            //status code --> 1 new
            //sample -->
            //Query1 --> index for taht query can be found named --> ExcluededReservation_Query2
            //query is 10 october to 20 october
            //reservation 18 october to 22 october
            //index_3
            var collectionpickOff = this._database.GetCollection<ContractItemDataMongoDB>(collectionName)
                            .AsQueryable()
                            .Where(p => queryPickupBsonTimeStamp <= p.PickupTimeStamp &&
                                   p.PickupTimeStamp <= queryDropoffBsonTimeStamp &&
                                   p.DropoffTimeStamp > queryDropoffBsonTimeStamp &&
                                   p.pickupBranchId.Equals(Convert.ToString(queryPickupBranchId)) &&
                                   p.itemTypeCode.Equals((int)GlobalEnums.ItemTypeCode.Equipment) &&
                                   (p.statuscode.Equals((int)ContractItemEnums.StatusCode.WaitingforDelivery) ||
                                    p.statuscode.Equals((int)ContractItemEnums.StatusCode.Rental))).ToList();
            allReservations.AddRange(collectionpickOff);
            //sample -->
            //Query2 --> index for taht query can be found named --> ExcluededReservation_Query2
            //query is 10 october to 20 october
            //reservation 8 october to 12 october
            //index_4
            var collectionDropOff = this._database.GetCollection<ContractItemDataMongoDB>(collectionName)
                           .AsQueryable()
                           .Where(p => queryPickupBsonTimeStamp < p.DropoffTimeStamp &&
                                  p.DropoffTimeStamp <= queryDropoffBsonTimeStamp &&
                                  p.pickupBranchId.Equals(Convert.ToString(queryPickupBranchId)) &&
                                  p.itemTypeCode.Equals((int)GlobalEnums.ItemTypeCode.Equipment) &&
                                  (p.statuscode.Equals((int)ContractItemEnums.StatusCode.WaitingforDelivery) ||
                                   p.statuscode.Equals((int)ContractItemEnums.StatusCode.Rental))).ToList();

            allReservations.AddRange(collectionDropOff);

            //sample -->
            //query is 10 october to 20 october
            //reservation 8 october to 22 october
            var collectionBoth = this._database.GetCollection<ContractItemDataMongoDB>(collectionName)
                           .AsQueryable()
                           .Where(p => queryPickupBsonTimeStamp > p.PickupTimeStamp &&
                                  queryDropoffBsonTimeStamp < p.DropoffTimeStamp &&
                                  p.pickupBranchId.Equals(Convert.ToString(queryPickupBranchId)) &&
                                  p.itemTypeCode.Equals((int)GlobalEnums.ItemTypeCode.Equipment) &&
                                  (p.statuscode.Equals((int)ContractItemEnums.StatusCode.WaitingforDelivery) ||
                                   p.statuscode.Equals((int)ContractItemEnums.StatusCode.Rental))).ToList();

            allReservations.AddRange(collectionBoth);
            //sample -->
            //query is 10 october to 20 october
            //reservation 12 october to 16 october
            var collectionDropOff1 = this._database.GetCollection<ContractItemDataMongoDB>(collectionName)
                           .AsQueryable()
                           .Where(p => queryPickupBsonTimeStamp <= p.PickupTimeStamp &&
                                      p.PickupTimeStamp <= queryDropoffBsonTimeStamp &&
                                      queryPickupBsonTimeStamp <= p.DropoffTimeStamp &&
                                      p.DropoffTimeStamp <= queryDropoffBsonTimeStamp &&
                                      p.pickupBranchId.Equals(Convert.ToString(queryPickupBranchId)) &&
                                      p.itemTypeCode.Equals((int)GlobalEnums.ItemTypeCode.Equipment) &&
                                      (p.statuscode.Equals((int)ContractItemEnums.StatusCode.WaitingforDelivery) ||
                                       p.statuscode.Equals((int)ContractItemEnums.StatusCode.Rental))).ToList();
            allReservations.AddRange(collectionDropOff1);
            //res 9.12.2018 21:30 --> 10.12.2018 21:30 
            //query 9.12.2018 21:45 --> 10.12.2018 21:45 
            //var collectionDropOff2 = this._database.GetCollection<ReservationItemDataMongoDB>("ReservationItems")
            //             .AsQueryable()
            //             .Where(p => queryPickupBsonTimeStamp >= p.PickupTimeStamp &&
            //                         queryPickupBsonTimeStamp <= p.DropoffTimeStamp &&
            //                         queryDropoffBsonTimeStamp >= p.DropoffTimeStamp &&
            //                         p.PickupBranchId.Equals(Convert.ToString(queryDropoffBranchId)) &&
            //                         p.ItemTypeCode.Equals(1) &&
            //                         p.StateCode.Equals(0)).ToList();

            //allReservations.AddRange(collectionBoth);

            return allReservations;
        }
        public ContractItemDataMongoDB getContractItemByAdditionalProductId(string contractId, string additionalProductId)
        {
            var collection = this._database.GetCollection<ContractItemDataMongoDB>(collectionName).
                                AsQueryable().
                                Where(p => p.statecode.Equals((int)GlobalEnums.StateCode.Active) &&
                                           p.itemTypeCode.Equals((int)ContractItemEnums.ItemTypeCode.AdditionalProduct) &&
                                           p.contractId.Equals(contractId) &&
                                           p.additionalProductId.Equals(additionalProductId)).FirstOrDefault();
            return collection;
        }

        public List<ContractItemDataMongoDB> getAllAvailableContractsByBranchId(Guid branchId)
        {
            var collection = this._database.GetCollection<ContractItemDataMongoDB>(collectionName)
                            .AsQueryable()
                            .Where(p => p.pickupBranchId.Equals(Convert.ToString(branchId)) &&
                                        p.itemTypeCode.Equals((int)ContractItemEnums.ItemTypeCode.Equipment) &&
                                        (p.statuscode.Equals((int)ContractItemEnums.StatusCode.Rental) ||
                                        p.statuscode.Equals((int)ContractItemEnums.StatusCode.WaitingforDelivery))).ToList();

            return collection;
        }
    }
}
