using MongoDB.Bson;
using MongoDB.Driver;
using RntCar.ClassLibrary;
using RntCar.ClassLibrary._Enums_1033;
using RntCar.ClassLibrary._Tablet;
using RntCar.ClassLibrary.MongoDB;
using RntCar.MongoDBHelper.Helper;
using RntCar.MongoDBHelper.Model;
using RntCar.MongoDBHelper.Repository;
using RntCar.SDK.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RntCar.MongoDBHelper.Entities
{
    public class ContractItemBusiness : MongoDBInstance
    {
        public ContractItemBusiness(string serverName, string dataBaseName) : base(serverName, dataBaseName)
        {
        }

        public MongoDBResponse createContractItem(ContractItemData contractItemData)
        {

            var collection = this.getCollection<ContractItemDataMongoDB>(StaticHelper.GetConfiguration("MongoDBContractItemCollectionName"));

            var contractItem = new ContractItemDataMongoDB();
            contractItem = contractItem.Map(contractItemData);
            contractItem._id = ObjectId.GenerateNewId();
            contractItem.DropoffTimeStamp = new BsonTimestamp(contractItemData.dropoffDateTime.Value.converttoTimeStamp());
            contractItem.PickupTimeStamp = new BsonTimestamp(contractItemData.pickupDateTime.Value.converttoTimeStamp());

            var methodName = ErrorLogsHelper.GetCurrentMethod();
            var itemId = contractItem._id.ToString();

            var response = collection.Insert(contractItem, itemId, methodName);
            response.Id = Convert.ToString(contractItem._id);

            if (contractItemData.itemTypeCode == (int)GlobalEnums.ItemTypeCode.Equipment)
            {
                ContractDailyPrices contractDailyPrices = new ContractDailyPrices(this._client, this._database);
                contractDailyPrices.createContractDailyPrices(contractItemData, true);
            }

            return response;
        }

        public bool updateContractItem(ContractItemData contractItemData, string id)
        {
            var collection = this.getCollection<ContractItemDataMongoDB>(StaticHelper.GetConfiguration("MongoDBContractItemCollectionName"));

            var contractItem = new ContractItemDataMongoDB();
            contractItem = contractItem.Map(contractItemData);
            contractItem._id = ObjectId.Parse(id);
            contractItem.DropoffTimeStamp = new BsonTimestamp(contractItemData.dropoffDateTime.Value.converttoTimeStamp());
            contractItem.PickupTimeStamp = new BsonTimestamp(contractItemData.pickupDateTime.Value.converttoTimeStamp());

            var methodName = ErrorLogsHelper.GetCurrentMethod(); ;
            var itemId = contractItem._id.ToString();

            var filter = Builders<ContractItemDataMongoDB>.Filter.Eq(p => p._id, contractItem._id);
            var response = collection.Replace(contractItem, filter, new UpdateOptions { IsUpsert = false }, itemId, methodName);
            //var response = collection.ReplaceOne(p => p._id == contractItem._id, contractItem, new UpdateOptions { IsUpsert = false });
            if (response != null && contractItemData.statecode == (int)GlobalEnums.StateCode.Active)
            {
                if (response.IsAcknowledged && contractItemData.itemTypeCode == (int)GlobalEnums.ItemTypeCode.Equipment)
                {
                    ContractDailyPrices dailyPricesBusiness = new ContractDailyPrices(this._client, this._database);
                    //before delete all daily prices by contractItemId
                    //because always need latest daiky prices for reservation update
                    var r = dailyPricesBusiness.deleteDailyPricesByContractItemId(new Guid(contractItemData.contractItemId));
                    //todo will implement a lofic if deletion in mongodb fails
                    if (r)
                    {
                        ContractDailyPrices contractDailyPrices = new ContractDailyPrices(this._client, this._database);
                        contractDailyPrices.createContractDailyPrices(contractItemData);
                    }

                }
                return false;
            }
            else
            {
                return false;
            }
        }


        public DashboardRentalContractData getRentalContractByPlateNumber(GetContractsByEquipmentParameters getContractsByEquipmentParameters)
        {

            ContractItemRepository contractItemRepository = new ContractItemRepository(this._client, this._database);
            var item = contractItemRepository.getRentalContractsByPlateNumber(getContractsByEquipmentParameters);
            if (item == null)
            {
                return null;
            }
            DashboardRentalContractData contractData = new DashboardRentalContractData
            {
                contractId = item.contractId,
                contractNumber = item.contractNumber,
                dropoffTimestamp = item.DropoffTimeStamp.Value,
                pickupTimestamp = item.PickupTimeStamp.Value,
                statusCode = item.statuscode,
                rentalEquipment = new DashboardRentalEquipment
                {
                    equipmentId = !string.IsNullOrEmpty(item.equipmentId) ? (Guid?)new Guid(item.equipmentId) : null,
                    plateNumber = item.plateNumber
                },
                groupCodeInformation = new DashboardGroupCodeInformation
                {
                    groupCodeId = new Guid(item.groupCodeInformationsId),
                    groupCodeName = item.groupCodeInformationsName
                },
                customer = new DashboardCustomer
                {
                    customerId = new Guid(item.customerId),
                    fullName = item.customerName
                },

            };
            return contractData;
        }
        public List<DashboardContractData> getDashboardWaitingforDeliveryContractsByBranchId(GetContractsByBranchParameters getContractsByBranchParameters)
        {
            ContractItemRepository contractItemRepository = new ContractItemRepository(this._client, this._database);
            var response = contractItemRepository.getWaitingforDeliveryContractsByBranchId(getContractsByBranchParameters);

            List<DashboardContractData> contractDatas = new List<DashboardContractData>();
            for (int i = 0; i < response.Length; i++)
            {
                var item = response[i];

                DashboardContractData contractData = new DashboardContractData
                {
                    pnrNumber = item.pnrNumber,
                    contractId = item.contractId,
                    contractNumber = item.contractNumber,
                    dropoffTimestamp = item.DropoffTimeStamp.Value,
                    pickupTimestamp = item.PickupTimeStamp.Value,
                    statusCode = item.statuscode,
                    groupCodeInformation = new DashboardGroupCodeInformation
                    {
                        groupCodeId = new Guid(item.groupCodeInformationsId),
                        groupCodeName = item.groupCodeInformationsName
                    },
                    customer = new DashboardCustomer
                    {
                        customerId = new Guid(item.customerId),
                        fullName = item.customerName
                    },
                    paymentMethodCode = item.paymentMethod
                };
                contractDatas.Add(contractData);
            }

            return contractDatas;
        }

        public List<DashboardContractData> getDashboardWaitingforDeliveryContractsByBranchId_PayBroker(GetContractsByBranchParameters getContractsByBranchParameters)
        {
            ContractItemRepository contractItemRepository = new ContractItemRepository(this._client, this._database);
            var response = contractItemRepository.getWaitingforDeliveryContractsByBranchId_PayBroker(getContractsByBranchParameters);

            List<DashboardContractData> contractDatas = new List<DashboardContractData>();
            for (int i = 0; i < response.Length; i++)
            {
                var item = response[i];

                DashboardContractData contractData = new DashboardContractData
                {
                    pnrNumber = item.pnrNumber,
                    contractId = item.contractId,
                    contractNumber = item.contractNumber,
                    dropoffTimestamp = item.DropoffTimeStamp.Value,
                    pickupTimestamp = item.pickupDateTimeStamp_Header.Value,
                    statusCode = item.statuscode,
                    groupCodeInformation = new DashboardGroupCodeInformation
                    {
                        groupCodeId = new Guid(item.groupCodeInformationsId),
                        groupCodeName = item.groupCodeInformationsName
                    },
                    customer = new DashboardCustomer
                    {
                        customerId = new Guid(item.customerId),
                        fullName = item.customerName
                    },
                    paymentMethodCode = item.paymentMethod
                };
                contractDatas.Add(contractData);
            }

            return contractDatas;
        }
        public List<DashboardRentalContractData> getDashboardRentalContractsByBranchId(GetContractsByBranchParameters getContractsByBranchParameters)
        {
            ContractItemRepository contractItemRepository = new ContractItemRepository(this._client, this._database);
            var response = contractItemRepository.getRentalContractsByBranchId(getContractsByBranchParameters);

            List<DashboardRentalContractData> contractDatas = new List<DashboardRentalContractData>();
            for (int i = 0; i < response.Length; i++)
            {
                var item = response[i];

                DashboardRentalContractData contractData = new DashboardRentalContractData
                {
                    contractId = item.contractId,
                    pnrNumber = item.pnrNumber,
                    contractNumber = item.contractNumber,
                    dropoffTimestamp = item.DropoffTimeStamp.Value,
                    pickupTimestamp = item.PickupTimeStamp.Value,
                    statusCode = item.statuscode,
                    rentalEquipment = new DashboardRentalEquipment
                    {
                        equipmentId = !string.IsNullOrEmpty(item.equipmentId) ? (Guid?)new Guid(item.equipmentId) : null,
                        plateNumber = item.plateNumber
                    },
                    groupCodeInformation = new DashboardGroupCodeInformation
                    {
                        groupCodeId = new Guid(item.groupCodeInformationsId),
                        groupCodeName = item.groupCodeInformationsName
                    },
                    customer = new DashboardCustomer
                    {
                        customerId = new Guid(item.customerId),
                        fullName = item.customerName
                    },
                    paymentMethodCode = item.paymentMethod

                };
                contractDatas.Add(contractData);
            }

            return contractDatas;
        }

        public GetContractDetailResponse getContractEquipment(string contractId, int statusCode)
        {
            ContractItemRepository contractItemRepository = new ContractItemRepository(this._client, this._database);
            var contractItem = contractItemRepository.getContractItemEquipmentByStatusCode(contractId, statusCode);
            var pickupDateTime = contractItem.PickupTimeStamp.Value;
            if (contractItem.paymentMethod == (int)rnt_PaymentMethodCode.PayBroker)
            {
                var completedItem = contractItemRepository.getContractItemEquipmentCompleted_corporateBillingType(contractId);
                if (completedItem != null)
                {
                    pickupDateTime = completedItem.PickupTimeStamp.Value;
                }
            }
            EquipmentRepository equipmentRepository = new EquipmentRepository(this._client, this._database);
            var equipment = new EquipmentDataMongoDB();
            if (!string.IsNullOrEmpty(contractItem.equipmentId))
            {
                equipment = equipmentRepository.getEquipmentById(new Guid(contractItem.equipmentId));
            }

            return new GetContractDetailResponse
            {
                totalPrice = contractItem.totalAmount,
                pnrNumber = contractItem.pnrNumber,
                contractId = contractId,
                contractNumber = contractItem.contractNumber,
                contractType = contractItem.contractType,
                campaignId = contractItem.campaignId,
                customer = new Customer
                {
                    customerId = new Guid(contractItem.customerId)

                },
                dropoffBranch = new Branch
                {
                    branchId = new Guid(contractItem.dropoffBranchId),
                    branchName = contractItem.dropoffBranchName
                },
                pickupBranch = new Branch
                {
                    branchId = new Guid(contractItem.pickupBranchId),
                    branchName = contractItem.pickupBranchName
                },
                groupCodeInformation = new GroupCodeInformation
                {
                    groupCodeName = contractItem.groupCodeInformationsName,
                    groupCodeId = new Guid(contractItem.groupCodeInformationsId),
                    depositAmount = contractItem.depositAmount
                },
                selectedEquipment = new ClassLibrary._Tablet.EquipmentData
                {
                    hgsNumber = equipment.HGSNumber,
                    equipmentId = !string.IsNullOrEmpty(contractItem.equipmentId) ? new Guid(contractItem.equipmentId) : Guid.Empty
                },
                dropoffTimestamp = contractItem.DropoffTimeStamp.Value,
                pickupTimestamp = pickupDateTime,
                statusCode = contractItem.statuscode
            };
        }

        public List<BranchCount> getWaitingforDeliveryContractReport(GetContractGroupCodeReportParameters getContractGroupCodeReportParameters)
        {
            ContractItemRepository contractItemRepository = new ContractItemRepository(this._client, this._database);

            var response = contractItemRepository.getWaitingforDeliveryContractsByBranchId(new GetContractsByBranchParameters
            {
                branchId = getContractGroupCodeReportParameters.branchId
            });
            var groupList = (from c in response.ToList()
                             group c by new
                             {
                                 c.groupCodeInformationsId,
                                 c.groupCodeInformationsName,

                             } into gcs
                             select new BranchCount()
                             {
                                 countForGroup = gcs.Count(),
                                 groupCodeName = gcs.Key.groupCodeInformationsName,
                                 groupCodeId = gcs.Key.groupCodeInformationsId

                             }).ToList();
            return groupList;
        }

        public ContractDateandBranchData getContractEquipmentWaitingForDeliveryDateandBranchs(string contractId)
        {
            ContractItemRepository contractItemRepository = new ContractItemRepository(this._client, this._database);
            var contractItem = contractItemRepository.getContractItemEquipmentWaitingForDelivery(contractId);

            return new ContractDateandBranchData
            {
                dropoffBranchId = new Guid(contractItem.dropoffBranchId),
                pickupBranchId = new Guid(contractItem.pickupBranchId),
                dropoffDateTime = contractItem.DropoffTimeStamp.Value.converttoDateTime(),
                pickupDateTime = contractItem.PickupTimeStamp.Value.converttoDateTime()
            };
        }

        public ContractDateandBranchData getContractEquipmentRentalDateandBranchs(string contractId)
        {
            ContractItemRepository contractItemRepository = new ContractItemRepository(this._client, this._database);
            var contractItem = contractItemRepository.getContractItemEquipmentRental(contractId);

            return new ContractDateandBranchData
            {
                dropoffBranchId = new Guid(contractItem.dropoffBranchId),
                pickupBranchId = new Guid(contractItem.pickupBranchId),
                dropoffDateTime = contractItem.DropoffTimeStamp.Value.converttoDateTime(),
                pickupDateTime = contractItem.PickupTimeStamp.Value.converttoDateTime()
            };
        }

        public ContractFineAmountResponse getFirstDayPrice(Guid contractItemId)
        {
            ContractDailyPrices contractDailyPrices = new ContractDailyPrices(this._client, this._database);
            var price = contractDailyPrices.getFirstDayPrice(contractItemId);

            if (price == null)
            {
                return new ContractFineAmountResponse
                {
                    //todo mongodb response text mechanism
                    ResponseResult = ClassLibrary.ResponseResult.ReturnError("first day price couldnt found")
                };
            }
            return new ContractFineAmountResponse
            {
                firstDayAmount = price.totalAmount,
                ResponseResult = ClassLibrary.ResponseResult.ReturnSuccess()
            };
        }
    }
}
