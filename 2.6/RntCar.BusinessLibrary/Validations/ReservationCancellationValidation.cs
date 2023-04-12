using Microsoft.Xrm.Sdk;
using Newtonsoft.Json;
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
    public class ReservationCancellationValidation : ValidationHandler
    {
        public ReservationCancellationValidation(IOrganizationService orgService) : base(orgService)
        {
        }

        public ReservationCancellationValidation(IOrganizationService orgService, ITracingService tracingService) : base(orgService, tracingService)
        {
        }

        public ReservationCancellationValidation(IOrganizationService orgService, Guid userId) : base(orgService, userId)
        {
        }
        public bool checkPNRandReservationNumber(Entity reservationEntity)
        {

            if (string.IsNullOrEmpty(reservationEntity.GetAttributeValue<string>("rnt_reservationnumber")) ||
                string.IsNullOrEmpty(reservationEntity.GetAttributeValue<string>("rnt_pnrnumber")))
            {
                return false;
            }
            return true;
        }
        public bool checkReservationStatus(Entity reservationEntity)
        {
            // 1-->means new
            if (reservationEntity.GetAttributeValue<OptionSetValue>("statuscode").Value != (int)rnt_reservation_StatusCode.New && 
                reservationEntity.GetAttributeValue<OptionSetValue>("statuscode").Value != (int)rnt_reservation_StatusCode.NoShow &&
                reservationEntity.GetAttributeValue<OptionSetValue>("statuscode").Value != (int)rnt_reservation_StatusCode.Waitingfor3D)
            {
                return false;
            }
            return true;
        }
        public bool checkReservationIsActive(Entity reservationEntity)
        {
            // 0-->means active
            if (reservationEntity.GetAttributeValue<OptionSetValue>("statecode").Value != 0)
            {
                return false;
            }
            return true;
        }
        public bool checkReservationIsEnoughClosettoDueDate(Entity reservationEntity,int reservationCancellationDuration)
        {
            var pickupDateTime = reservationEntity.GetAttributeValue<DateTime>("rnt_pickupdatetime");
            if (pickupDateTime.AddMinutes(-reservationCancellationDuration) < DateTime.UtcNow.AddMinutes(StaticHelper.offset))
            {
                return false;
            }
            return true;
        }

        public bool checkReservationWillChargeFromUser(int reservationCancellationFineDuration, Entity reservationEntity)
        {
            var pickupDateTime = reservationEntity.GetAttributeValue<DateTime>("rnt_pickupdatetime");
            var totalMinutes = (pickupDateTime - DateTime.UtcNow.AddMinutes(StaticHelper.offset)).TotalMinutes;
            if(totalMinutes < reservationCancellationFineDuration)
            {
                return true;
            }
            return false;
        }
    }
}
