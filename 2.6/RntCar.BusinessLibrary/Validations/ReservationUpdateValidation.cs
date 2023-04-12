using Microsoft.Xrm.Sdk;
using RntCar.BusinessLibrary.Handlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.BusinessLibrary.Validations
{
    public class ReservationUpdateValidation : ValidationHandler
    {
        public ReservationUpdateValidation(IOrganizationService orgService) : base(orgService)
        {
        }

        public ReservationUpdateValidation(IOrganizationService orgService, ITracingService tracingService) : base(orgService, tracingService)
        {
        }

        public bool checkReservationStatus(Entity reservationEntity)
        {
            // 1-->means new
            if (reservationEntity.GetAttributeValue<OptionSetValue>("statuscode").Value != 1)
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
    }
}
