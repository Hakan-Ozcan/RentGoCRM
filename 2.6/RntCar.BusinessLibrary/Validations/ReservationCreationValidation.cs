using Microsoft.Xrm.Sdk;
using RntCar.BusinessLibrary.Business;
using RntCar.BusinessLibrary.Handlers;
using RntCar.BusinessLibrary.Repository;
using RntCar.ClassLibrary;
using RntCar.ClassLibrary.Reservation;
using RntCar.SDK.Common;
using System;

namespace RntCar.BusinessLibrary.Validations
{
    public class ReservationCreationValidation : ValidationHandler
    {
        public ReservationCreationValidation(IOrganizationService orgService) : base(orgService)
        {
        }

        public ReservationCreationValidation(IOrganizationService orgService, ITracingService tracingService) : base(orgService, tracingService)
        {
        }

        public ReservationCreationValidation(IOrganizationService orgService, Guid userId) : base(orgService, userId)
        {
        }

        public void checkBeforeReservationCreation(ReservationCustomerParameters reservationCustomerParameters)
        {
            if (reservationCustomerParameters.customerType != (int)GlobalEnums.CustomerType.Individual &&
               reservationCustomerParameters.customerType != (int)GlobalEnums.CustomerType.Corporate &&
               reservationCustomerParameters.customerType != (int)GlobalEnums.CustomerType.Broker &&
               reservationCustomerParameters.customerType != (int)GlobalEnums.CustomerType.Agency)
            {
                XrmHelper xrmHelper = new XrmHelper(this.OrgService);

            }
        }
        public bool checkReservationInvoiceAddressNullCheck(int invoiceType)
        {
            // if invoicetype is zero, it is null
            if (invoiceType == 0)
                return false;
            return true;
        }
        public bool checkReservationDateandBranch(DateTime pickupDateTime, DateTime dropoffDateTime, Guid pickupBranchId, Guid dropoffBranchId)
        {
            if (pickupBranchId == null || dropoffBranchId == null || pickupDateTime == null || dropoffDateTime == null)
                return false;
            return true;
        }
        public ValidationResponse checkExistingReservationsDateTime(Guid customerId, Guid? reservationId, DateTime pickupDate, DateTime dropoffDate, int langId)
        {
            ConfigurationBL configurationBL = new ConfigurationBL(this.OrgService);
            var dummyContactId = configurationBL.GetConfigurationByName("DUMMYCONTACTID_BROKER");

            if (new Guid(dummyContactId) != customerId)
            {
                ReservationRepository reservationRepository = new ReservationRepository(this.OrgService);
                var reservations = reservationRepository.getAllActiveReservationByCustomerIdByBetweenGivenDates(customerId,
                                                                                                                pickupDate,
                                                                                                                dropoffDate);
                //remove reservation from list for reservation update
                if (reservationId.HasValue)
                    reservations.RemoveAll(p => p.Id == reservationId.Value);

                if (reservations != null && reservations.Count > 0)
                {
                    XrmHelper xrmHelper = new XrmHelper(this.OrgService);
                    var message = xrmHelper.GetXmlTagContentByGivenLangId("ExistingActiveReservation", langId, this.reservationXmlPath);
                    return new ValidationResponse
                    {
                        ResponseResult = ResponseResult.ReturnError(message)
                    };
                }
            }
            return new ValidationResponse
            {
                ResponseResult = ResponseResult.ReturnSuccess()
            };
        }
    }
}
