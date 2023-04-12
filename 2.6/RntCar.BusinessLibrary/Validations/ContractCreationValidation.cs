using Microsoft.Xrm.Sdk;
using RntCar.BusinessLibrary.Business;
using RntCar.BusinessLibrary.Handlers;
using RntCar.BusinessLibrary.Repository;
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
    public class ContractCreationValidation : ValidationHandler
    {
        public ContractCreationValidation(IOrganizationService orgService) : base(orgService)
        {
        }

        public ContractCreationValidation(IOrganizationService orgService, ITracingService tracingService) : base(orgService, tracingService)
        {
        }

        public bool checkReservationPaymentTypeBeforeQuickContractCreation(Entity reservation)
        {
            //cari --> ödeme tiplerinde 
            if (reservation.GetAttributeValue<OptionSetValue>("rnt_paymentmethodcode").Value == (int)rnt_PaymentMethodCode.Current)
            {
                return true;
            }

            var paymentChoiceCode = reservation.Attributes.Contains("rnt_paymentchoicecode") ? reservation.GetAttributeValue<OptionSetValue>("rnt_paymentchoicecode").Value : 0;
            this.Trace("paymentChoiceCode : " + paymentChoiceCode);
            if (paymentChoiceCode != (int)ReservationEnums.ReservationPaymentChoise.PayLater)
                return true;
            return false;
        }

        public bool checkDateTimeBeforeQuickContractCreation(Entity reservation, int duration)
        {
            var pickupDate = reservation.GetAttributeValue<DateTime>("rnt_pickupdatetime");
            
            var now = this.DateNow.AddMinutes(duration).AddMinutes(StaticHelper.offset);
            this.Trace("nowTimeStamp : " + now);
            this.Trace("pickupTimeStamp : " + pickupDate);
            var nowTimeStamp = now.converttoTimeStamp();
            var pickupTimeStamp = pickupDate.converttoTimeStamp();
            if (nowTimeStamp > pickupTimeStamp)
            {
                this.Trace("return true from checkDateTimeBeforeQuickContractCreation");
                return true;
            }
            this.Trace("return false from checkDateTimeBeforeQuickContractCreation");
            return false;
        }

        public bool checkDateTimeBeforeContractCreation(Entity reservation, int duration)
        {
            var pickupDate = reservation.GetAttributeValue<DateTime>("rnt_pickupdatetime");
            this.Trace("pickupDateTime : " + pickupDate);
            var currentDateTime = this.DateNow.AddMinutes(StaticHelper.offset);
            this.Trace("currentDateTime : " + currentDateTime);
            var currentDateTimeWithDuration = currentDateTime.AddMinutes(duration);
            this.Trace("duration : " + duration);
            this.Trace("currentDateTimeWithDuration : " + currentDateTimeWithDuration);
            var currentTimeStamp = currentDateTimeWithDuration.converttoTimeStamp();
            this.Trace("currentTimeStamp : " + currentTimeStamp);
            this.Trace("pickupDateTimeStamp : " + pickupDate.converttoTimeStamp());
            var pickupTimeStamp = pickupDate.converttoTimeStamp();
            if (currentTimeStamp >= pickupTimeStamp)
                return true;
            return false;
        }
        public bool checkReservationBranchByUserId(Entity reservation, Guid userId)
        {
            SystemUserRepository systemUserRepository = new SystemUserRepository(this.OrgService);
            var userInformation = systemUserRepository.getSystemUserByIdWithGivenColumns(userId, new string[] { "businessunitid" });
            var userBusinessUnit = userInformation.GetAttributeValue<EntityReference>("businessunitid");
            var reservationPickupBranchId = reservation.GetAttributeValue<EntityReference>("rnt_pickupbranchid").Id;
            if (userBusinessUnit != null)
            {
                BranchRepository branchRepository = new BranchRepository(this.OrgService);
                var userBranchs = branchRepository.getBranchByBusinessUnitId(userBusinessUnit.Id);

                var userHasReservationBranch = userBranchs.Where(item => item.Id == reservationPickupBranchId).FirstOrDefault();
                if (userHasReservationBranch == null)
                    return false;
                //// no need to check if there is no branch 
                //// it means admin role
                return true;
            }
            // if there is no business unit, return  false
            return false;
        }
        public bool checkReservationStatusForCancel(Entity reservation)
        {
            var statusCode = reservation.GetAttributeValue<OptionSetValue>("statuscode").Value;
            if (statusCode == (int)ReservationEnums.CancellationReason.ByCustomer || statusCode == (int)ReservationEnums.CancellationReason.ByRentgo)
                return false;
            return true;
        }
        public bool checkReservationStatusFor3DWaiting(Entity reservation)
        {
            var statusCode = reservation.GetAttributeValue<OptionSetValue>("statuscode").Value;
            if (statusCode == (int)ReservationEnums.StatusCode.Waitingfor3D)
                return false;
            return true;
        }
        public bool checkReservationStatusForExisting(Entity reservation)
        {
            var statusCode = reservation.GetAttributeValue<OptionSetValue>("statuscode").Value;
            if (statusCode == (int)ReservationEnums.StatusCode.Completed || statusCode == (int)ReservationEnums.StatusCode.Rental)
                return false;
            return true;
        }

        public ValidationResponse checkExistingContractsDateTime(Guid customerId, DateTime pickupDate, DateTime dropoffDate, int langId)
        {
            ContractRepository contractRepository = new ContractRepository(this.OrgService);
            var contracts = contractRepository.getAllActiveContractsByCustomerIdByBetweenGivenDates(customerId,
                                                                                                    pickupDate,
                                                                                                    dropoffDate);
            if (contracts != null && contracts.Count > 0)
            {
                XrmHelper xrmHelper = new XrmHelper(this.OrgService);
                var message = xrmHelper.GetXmlTagContentByGivenLangId("ExistingActiveContract", langId, this.contractXmlPath);
                return new ValidationResponse
                {
                    ResponseResult = ResponseResult.ReturnError(message)
                };
            }
            return new ValidationResponse
            {
                ResponseResult = ResponseResult.ReturnSuccess()
            };
        }

        public bool checkReservationHasAdditionalDriver(Entity reservation)
        {
            ConfigurationRepository configurationRepository = new ConfigurationRepository(this.OrgService);
            var productCode = configurationRepository.GetConfigurationByKey("additionalProducts_additionalDriver");
            AdditionalProductRepository additionalProductRepository = new AdditionalProductRepository(this.OrgService);
            var additionalDriverProduct = additionalProductRepository.getAdditionalProductByProductCode(productCode, new string[] { "rnt_additionalproductid" });
            ReservationItemRepository reservationItemRepository = new ReservationItemRepository(this.OrgService);
            var reservationItems = reservationItemRepository.getReservationItemByAdditionalProductIdWithGivenColumns(reservation.Id, additionalDriverProduct.Id, new string[] { "rnt_additionalproductid" });
            if (reservationItems != null)
                return false;
            return true;
        }
    }
}
