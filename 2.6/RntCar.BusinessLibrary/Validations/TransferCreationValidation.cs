using Microsoft.Xrm.Sdk;
using Newtonsoft.Json;
using RntCar.BusinessLibrary.Handlers;
using RntCar.BusinessLibrary.Repository;
using RntCar.ClassLibrary._Enums_1033;
using RntCar.ClassLibrary._Tablet;
using RntCar.SDK.Common;
using System;
using System.Linq;

namespace RntCar.BusinessLibrary.Validations
{
    public class TransferCreationValidation : ValidationHandler
    {
        public TransferCreationValidation(IOrganizationService orgService) : base(orgService)
        {
        }

        public TransferCreationValidation(IOrganizationService orgService, ITracingService tracingService) : base(orgService, tracingService)
        {
        }

        public ValidateTransferResponse validateTransfer(Entity transferEntity)
        {
            XrmHelper xrmHelper = new XrmHelper(this.OrgService);
            this.Trace("transferEntity: " + JsonConvert.SerializeObject(transferEntity));

            #region Period validation
            ProductRepository productRepository = new ProductRepository(this.OrgService);
            var product = productRepository.getProductByEquipmentId(transferEntity.GetAttributeValue<EntityReference>("rnt_equipmentid").Id, new string[] { "rnt_maintenanceperiod", "rnt_name" });

            if (transferEntity.Attributes.Contains("rnt_maintenancekm"))
            {
                if (transferEntity.GetAttributeValue<int>("rnt_maintenancekm") % product.GetAttributeValue<OptionSetValue>("rnt_maintenanceperiod").Value != 0)
                {
                    var message = string.Format(xrmHelper.GetXmlTagContentByGivenLangId("TansferPeriod", 1055), product.GetAttributeValue<string>("rnt_name"), product.GetAttributeValue<OptionSetValue>("rnt_maintenanceperiod").Value);
                    return new ValidateTransferResponse { Result = false, Message = message };
                }
            }
            #endregion

            TransferRepository transferRepository = new TransferRepository(this.OrgService);
            var transfers = transferRepository.getTransfersByEquipmentId(transferEntity.GetAttributeValue<EntityReference>("rnt_equipmentid").Id, new string[] { "rnt_estimateddropoffdate", "rnt_estimatedpickupdate", "rnt_actualdropoffdate", "rnt_actualpickupdate", "rnt_equipmentid", "rnt_transfernumber", "rnt_transfertype", "rnt_maintenancekm" });

            // If there is no transfer record for corresponding equipment, no need to validate duplication and date
            if (transfers == null)
            {
                return new ValidateTransferResponse { Result = true };
            }

            var itemToRemove = transfers.SingleOrDefault(t => t.Id == transferEntity.Id);
            if (itemToRemove != null)
            {
                transfers.Remove(itemToRemove);
            }
            string transferNumber = string.Empty;

            #region Duplicate transfer km validation
            var maintenanceTransfers = transfers.Where(t => t.Attributes.Contains("rnt_transfertype") && t.GetAttributeValue<OptionSetValue>("rnt_transfertype").Value == (int)ClassLibrary._Enums_1033.rnt_TransferType.Bakim).ToList();
            bool isDuplicateKm = false;
            maintenanceTransfers.ForEach(transfer =>
            {
                if (transfer.GetAttributeValue<int>("rnt_maintenancekm") == transferEntity.GetAttributeValue<int>("rnt_maintenancekm"))
                {
                    isDuplicateKm = true;
                    transferNumber = transfer.GetAttributeValue<string>("rnt_transfernumber");
                    return;
                }
            });

            if (isDuplicateKm && transferEntity.GetAttributeValue<int>("rnt_maintenancekm") != 0)
            {
                var message = string.Format(xrmHelper.GetXmlTagContentByGivenLangId("DuplicateTansferKm", 1055), transferNumber);
                return new ValidateTransferResponse { Result = false, Message = message };
            }
            #endregion

            int status = transferEntity.GetAttributeValue<OptionSetValue>("statuscode").Value;

            if (status == (int)rnt_transfer_StatusCode.Completed ||
                status == (int)rnt_transfer_StatusCode.Cancelled ||
                status == (int)rnt_transfer_StatusCode.Inactive ||
                status == (int)rnt_transfer_StatusCode.Transferred)
            {
                return new ValidateTransferResponse
                {
                    Result = true,
                };
            }

            #region Date validation
            var pickupDateTime = (transferEntity.GetAttributeValue<DateTime>("rnt_estimatedpickupdate").AddMinutes(-StaticHelper.offset)).converttoTimeStamp();
            var dropoffDateTime = (transferEntity.GetAttributeValue<DateTime>("rnt_estimateddropoffdate").AddMinutes(-StaticHelper.offset)).converttoTimeStamp();
            bool isDatesOverlap = false;

            transfers.ForEach(transfer =>
            {
                var estimatedDropoffDateTimestamp = (transfer.GetAttributeValue<DateTime>("rnt_estimateddropoffdate").AddMinutes(-StaticHelper.offset)).converttoTimeStamp();
                var estimatedPickupDateTimestamp = (transfer.GetAttributeValue<DateTime>("rnt_estimatedpickupdate").AddMinutes(-StaticHelper.offset)).converttoTimeStamp();
                var actualPickupDateTimestamp = transfer.Attributes.Contains("rnt_actualpickupdate") ? (int?)(transfer.GetAttributeValue<DateTime>("rnt_actualpickupdate").AddMinutes(-StaticHelper.offset)).converttoTimeStamp() : null;
                var actualDropoffDateTimestamp = transfer.Attributes.Contains("rnt_actualdropoffdate") ? (int?)(transfer.GetAttributeValue<DateTime>("rnt_actualdropoffdate").AddMinutes(-StaticHelper.offset)).converttoTimeStamp() : null;

                if(actualPickupDateTimestamp !=null && actualDropoffDateTimestamp != null)
                {
                    if (pickupDateTime <= actualDropoffDateTimestamp && actualPickupDateTimestamp <= dropoffDateTime)
                    {
                        isDatesOverlap = true;
                        transferNumber = transfer.GetAttributeValue<string>("rnt_transfernumber");
                        return;
                    }
                }
                else if (pickupDateTime <= estimatedDropoffDateTimestamp && estimatedPickupDateTimestamp <= dropoffDateTime)
                {
                    isDatesOverlap = true;
                    transferNumber = transfer.GetAttributeValue<string>("rnt_transfernumber");
                    return;
                }
            });

            if (isDatesOverlap)
            {
                var message = string.Format(xrmHelper.GetXmlTagContentByGivenLangId("OverlappedTransferDate", 1055), transferNumber);

                return new ValidateTransferResponse { Result = false, Message = message };
            }
            #endregion

            return new ValidateTransferResponse { Result = true };
        }
    }
}
