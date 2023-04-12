using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Tooling.Connector;
using RntCar.BusinessLibrary.Handlers;
using RntCar.BusinessLibrary.Repository;
using RntCar.ClassLibrary;
using RntCar.ClassLibrary._Tablet;
using RntCar.SDK.Common;
using System;
using System.Collections.Generic;
using System.Linq;


namespace RntCar.BusinessLibrary.Business
{
    public class EquipmentInventoryBL : BusinessHandler
    {
        public EquipmentInventoryBL(IOrganizationService orgService) : base(orgService)
        {
        }

        public EquipmentInventoryBL(IOrganizationService orgService, ITracingService tracingService) : base(orgService, tracingService)
        {
        }

        public EquipmentInventoryBL(IOrganizationService orgService, CrmServiceClient crmServiceClient) : base(orgService, crmServiceClient)
        {
        }

        public EquipmentInventoryBL(CrmServiceClient crmServiceClient) : base(crmServiceClient)
        {
        }

        public EquipmentInventoryBL(PluginInitializer initializer, IOrganizationService orgService, ITracingService tracingService) : base(initializer, orgService, tracingService)
        {
        }

        public List<EquipmentInventoryData> getLatestEquipmentInventoryInformation(Guid equipmentId, int langId)
        {
            EquipmentRepository equipmentRepository = new EquipmentRepository(this.OrgService, this.CrmServiceClient);
            var equipment = equipmentRepository.getEquipmentByIdByGivenColumns(equipmentId, new string[] { "rnt_equipmentinventoryid" });

            Entity equipmentInventory = null;
            if (equipment.Attributes.Contains("rnt_equipmentinventoryid"))
            {
                EquipmentInventoryRepository equipmentInventoryRepository = new EquipmentInventoryRepository(this.OrgService, this.CrmServiceClient);
                equipmentInventory = equipmentInventoryRepository.getEquipmentInventoryById(equipment.GetAttributeValue<EntityReference>("rnt_equipmentinventoryid").Id);
            }

            InventoryRepository inventoryRepository = new InventoryRepository(this.OrgService, this.CrmServiceClient);
            var inventories = inventoryRepository.getAllInventories();

            List<EquipmentInventoryData> equipmentInventoryDatas = new List<EquipmentInventoryData>();
            foreach (var item in inventories)
            {
                var logicalName = item.GetAttributeValue<string>("rnt_logicalname");
                EquipmentInventoryData equipmentInventoryData = new EquipmentInventoryData();
                if (equipmentInventory != null)
                {
                    equipmentInventoryData.equipmentInventoryId = item.Id;
                    equipmentInventoryData.isExist = equipmentInventory.GetAttributeValue<bool>(logicalName);
                }
                else
                {
                    equipmentInventoryData.isExist = false;
                }
                equipmentInventoryData.logicalName = logicalName;

                if (langId == (int)GlobalEnums.LangId.English)
                {
                    equipmentInventoryData.inventoryName = item?.GetAttributeValue<string>("rnt_englishname");
                }
                else if (langId == (int)GlobalEnums.LangId.Turkish)
                {
                    equipmentInventoryData.inventoryName = item?.GetAttributeValue<string>("rnt_name");
                }
                equipmentInventoryDatas.Add(equipmentInventoryData);
            }

            return equipmentInventoryDatas;
        }

        public List<EquipmentInventoryData> calculateMissingInventoryPriceByGroupCodeInformation(Guid groupCodeInformationId,
                                                                         List<EquipmentInventoryData> equipmentInventoryData,
                                                                         List<EquipmentInventoryData> currentEquipmentData)
        {
            List<EquipmentInventoryData> returnList = new List<EquipmentInventoryData>();

            EquipmentInventoryPriceRepository equipmentInventoryPriceRepository = new EquipmentInventoryPriceRepository(this.OrgService, this.CrmServiceClient);
            var prices = equipmentInventoryPriceRepository.getEquipmentInventoryPriceByGroupCodeByGivenColumns(groupCodeInformationId, new string[] { "rnt_inventoryid", "rnt_price" });

            var missingInventory = currentEquipmentData.Except(equipmentInventoryData, new EquipmentInventoryDataComparer());          
            //if system couldnt find any price return nothing
            if(prices == null)
            {
                return returnList;
            }
            missingInventory = missingInventory.Where(p => p.isExist == false).ToList();
            
            foreach (var item in missingInventory)
            {
                var _price = prices.Where(p => p.GetAttributeValue<EntityReference>("rnt_inventoryid").Id == item.equipmentInventoryId.Value).FirstOrDefault();
                //if there is no price retun nothing
                if (_price == null)
                {
                    continue;
                }
                item.price = _price.GetAttributeValue<Money>("rnt_price").Value;
                returnList.Add(item);
            }

            return returnList;
        }

        public Guid createEquipmentInventoryHistoryforDelivery(Guid equipmentId, Guid? contractId, Guid? transferId,
                                                               List<CreateEquipmentInventoryHistoryParameter> createEquipmentInventoryHistoryParameters)
        {
            Entity e = new Entity("rnt_equipmentinventoryhistory");
            foreach (var item in createEquipmentInventoryHistoryParameters)
            {
                e[item.logicalName] = item.isExists;
            }
            e["rnt_equipmentid"] = new EntityReference("rnt_equipment", equipmentId);
            if(contractId.HasValue)
                e["rnt_contractid"] = new EntityReference("rnt_contract", contractId.Value);
            if(transferId.HasValue)
                e["rnt_transferid"] = new EntityReference("rnt_transfer", transferId.Value);
            e["rnt_processtypecode"] = new OptionSetValue((int)EquipmentInventoryTransactionEnums.ProcessType.Delivery);
            return this.OrgService.Create(e);
        }
        public Guid createEquipmentInventoryHistoryforRental(Guid equipmentId, Guid? contractId, Guid? transferId,
                                                             List<CreateEquipmentInventoryHistoryParameter> createEquipmentInventoryHistoryParameters)
        {
            Entity e = new Entity("rnt_equipmentinventoryhistory");
            foreach (var item in createEquipmentInventoryHistoryParameters)
            {
                e[item.logicalName] = item.isExists;
            }
            e["rnt_equipmentid"] = new EntityReference("rnt_equipment", equipmentId);
            if (contractId.HasValue)
                e["rnt_contractid"] = new EntityReference("rnt_contract", contractId.Value);
            if (transferId.HasValue)
                e["rnt_transferid"] = new EntityReference("rnt_transfer", transferId.Value);
            e["rnt_processtypecode"] = new OptionSetValue((int)EquipmentInventoryTransactionEnums.ProcessType.Return);
            return this.OrgService.Create(e);
        }
        public void updateEquipmentInventoryHistories(List<EquipmentInventoryData> equipmentInventories)
        {
            foreach (var item in equipmentInventories)
            {
                Entity e = new Entity("rnt_equipmentinventoryhistory");
                e[item.logicalName] = item.isExist;
                e.Id = item.equipmentInventoryId.Value;
                this.OrgService.Update(e);
            }
        }


    }
    public class EquipmentInventoryDataComparer : IEqualityComparer<EquipmentInventoryData>
    {
        public bool Equals(EquipmentInventoryData x, EquipmentInventoryData y)
        {
            return (string.Equals(x.logicalName, y.logicalName) && string.Equals(x.isExist, y.isExist));
        }

        public int GetHashCode(EquipmentInventoryData obj)
        {
            return obj.isExist.GetHashCode();
        }
    }
}
