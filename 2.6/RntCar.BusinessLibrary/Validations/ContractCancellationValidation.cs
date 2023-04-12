using Microsoft.Xrm.Sdk;
using RntCar.BusinessLibrary.Handlers;
using RntCar.ClassLibrary;
using RntCar.ClassLibrary._Enums_1033;
using RntCar.SDK.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.BusinessLibrary.Validations
{
    public class ContractCancellationValidation : ValidationHandler
    {
        public ContractCancellationValidation(IOrganizationService orgService) : base(orgService)
        {
        }

        public ContractCancellationValidation(IOrganizationService orgService, ITracingService tracingService) : base(orgService, tracingService)
        {
        }

        public bool checkPNRandContractNumber(Entity contractEntity)
        {

            if (string.IsNullOrEmpty(contractEntity.GetAttributeValue<string>("rnt_contractnumber")) ||
                string.IsNullOrEmpty(contractEntity.GetAttributeValue<string>("rnt_pnrnumber")))
            {
                return false;
            }
            return true;
        }
        public bool checkContractStatus(Entity contractEntity)
        {
            this.Trace("statuscode : " + contractEntity.GetAttributeValue<OptionSetValue>("statuscode").Value);
            this.Trace("(int)rnt_contract_StatusCode.WaitingForDelivery : " + (int)rnt_contract_StatusCode.WaitingForDelivery);
            // 1-->means new
            if (contractEntity.GetAttributeValue<OptionSetValue>("statuscode").Value != (int)rnt_contract_StatusCode.WaitingForDelivery)
            {
                return false;
            }
            return true;
        }
        public bool checkContractIsActive(Entity contractEntity)
        {
            // 0-->means active
            if (contractEntity.GetAttributeValue<OptionSetValue>("statecode").Value != 0)
            {
                return false;
            }
            return true;
        }
        public bool checkContractIsWaitingForDelivery(Entity contractEntity)
        {
            if (contractEntity.GetAttributeValue<OptionSetValue>("statuscode").Value != (int)ContractEnums.StatusCode.WaitingforDelivery)
            {
                return false;
            }
            return true;
        }
        public bool checkContractIsEnoughClosettoDueDate(Entity contractEntity, int contractCancellationDuration)
        {
            var pickupDateTime = contractEntity.GetAttributeValue<DateTime>("rnt_pickupdatetime");
            if (pickupDateTime.AddMinutes(-contractCancellationDuration) < DateTime.UtcNow.AddMinutes(StaticHelper.offset))
            {
                return false;
            }
            return true;
        }

        public bool checkContractWillChargeFromUser(int contractCancellationFineDuration, Entity contractEntity)
        {
            var pickupDateTime = contractEntity.GetAttributeValue<DateTime>("rnt_pickupdatetime");
            var totalMinutes = (pickupDateTime - DateTime.UtcNow.AddMinutes(StaticHelper.offset)).TotalMinutes;
            if (totalMinutes < contractCancellationFineDuration)
            {
                return true;
            }
            return false;
        }
        public bool checkContractHasCompletedEquipment(Entity contractEntity)
        {
            ContractItemRepository contractItemRepository = new ContractItemRepository(this.OrgService);
            var contractItems = contractItemRepository.getCompletedContractItemsByContractIdWithGivenColmuns(contractEntity.Id, new string[] { "rnt_itemtypecode" });
            if (contractItems.Count > 0)
            {
                var equipment = contractItems.Where(item => item.GetAttributeValue<OptionSetValue>("rnt_itemtypecode").Value == (int)ClassLibrary._Enums_1033.rnt_contractitem_rnt_itemtypecode.Equipment).FirstOrDefault();
                if (equipment != null)
                    return false;
            }
            return true;
        }
    }
}
