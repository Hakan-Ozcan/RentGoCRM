using Microsoft.Xrm.Sdk;
using RntCar.BusinessLibrary.Handlers;
using RntCar.BusinessLibrary.Repository;
using RntCar.ClassLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.BusinessLibrary.Business
{
    public class EquipmentTransactionBL : BusinessHandler
    {
        public EquipmentTransactionBL(IOrganizationService orgService) : base(orgService)
        {
        }

        public EquipmentTransactionBL(IOrganizationService orgService, ITracingService tracingService) : base(orgService, tracingService)
        {
        }

        public Guid createEquipmentTransactionHistoryforDelivery(CreateEquipmentTransactionHistoryDeliveryParameters createEquipmentTransactionHistoryDeliveryParameters)
        {
            Entity e = new Entity("rnt_equipmenttransactionhistory");

            if (createEquipmentTransactionHistoryDeliveryParameters.contractId.HasValue)
            {
                e["rnt_contractid"] = new EntityReference("rnt_contract", createEquipmentTransactionHistoryDeliveryParameters.contractId.Value);
            }

            if(createEquipmentTransactionHistoryDeliveryParameters.transferId.HasValue)
                e["rnt_transferid"] = new EntityReference("rnt_transfer", createEquipmentTransactionHistoryDeliveryParameters.transferId.Value);

            e["rnt_deliverykilometer"] = createEquipmentTransactionHistoryDeliveryParameters.deliveryKmValue;
            e["rnt_deliverykilometerdifference"] = createEquipmentTransactionHistoryDeliveryParameters.deliveryKmValue - createEquipmentTransactionHistoryDeliveryParameters.kmValue;
            e["rnt_deliveryfuel"] = new OptionSetValue(createEquipmentTransactionHistoryDeliveryParameters.deliveryFuelValue);

            var diff = createEquipmentTransactionHistoryDeliveryParameters.deliveryFuelValue - createEquipmentTransactionHistoryDeliveryParameters.fuelValue;

            this.Trace("fuel value difference " + diff);
            if (diff < 0)
            {
                diff = diff * 10;
                diff = diff * -1;
            }
            else
            {
                diff = diff * 100;
            }
            e["rnt_deliveryfueldifference"] = new OptionSetValue(diff);
            e["rnt_equipmentid"] = new EntityReference("rnt_equipment", createEquipmentTransactionHistoryDeliveryParameters.equipmentId.Value);
            return this.OrgService.Create(e);
        }

        public void updateEquipmentTransactionHistoryforRental(CreateEquipmentTransactionHistoryRentalParameters createEquipmentTransactionHistoryRentalParameters)
        {
            EquipmentTransactionHistoryRepository equipmentTransactionHistoryRepository = new EquipmentTransactionHistoryRepository(this.OrgService, this.CrmServiceClient);
            var currentEquipmentTransaction = equipmentTransactionHistoryRepository.
                                              getEquipmentTransactionHistoryByIdByGivenColumns(createEquipmentTransactionHistoryRentalParameters.equipmentTransactionId.Value,
                                                                                               new string[] { "rnt_deliveryfuel",
                                                                                                              "rnt_deliverykilometer"});
            Entity e = new Entity("rnt_equipmenttransactionhistory");

            if (createEquipmentTransactionHistoryRentalParameters.contractId.HasValue)
            {
                e["rnt_contractid"] = new EntityReference("rnt_contract", createEquipmentTransactionHistoryRentalParameters.contractId.Value);
            }

            if (createEquipmentTransactionHistoryRentalParameters.transferId.HasValue)
                e["rnt_transferid"] = new EntityReference("rnt_transfer", createEquipmentTransactionHistoryRentalParameters.transferId.Value);

            e["rnt_returnfuel"] = new OptionSetValue(createEquipmentTransactionHistoryRentalParameters.rentalFuelValue);
            e["rnt_returnkilometer"] = createEquipmentTransactionHistoryRentalParameters.rentalKmValue;

            var diff = createEquipmentTransactionHistoryRentalParameters.rentalFuelValue - createEquipmentTransactionHistoryRentalParameters.firstFuelValue;

            if (diff < 0)
            {
                diff = diff * 10;
                diff = diff * -1;
            }
            else
            {
                diff = diff * 100;
            }
            e["rnt_returnfueldifference"] = new OptionSetValue(diff);
            e["rnt_equipmentid"] = new EntityReference("rnt_equipment", createEquipmentTransactionHistoryRentalParameters.equipmentId.Value);
            e.Id = currentEquipmentTransaction.Id;
            this.OrgService.Update(e);

        }

        public int getCompletedEquipmentTransactionHistoryByContractId(Guid contractId)
        {
            EquipmentTransactionHistoryRepository equipmentTransactionHistoryRepository = new EquipmentTransactionHistoryRepository(this.OrgService, this.CrmServiceClient);
            var result = equipmentTransactionHistoryRepository.getEquipmentTransactionHistoryByContractIdByGivenColumns(contractId,
                                                                                                                        new string[] { "rnt_returnkilometerdifference" });
            var sum = result.Sum(p => p.GetAttributeValue<int>("rnt_returnkilometerdifference"));

            return sum;
        }
    }
}
