
using System;

namespace RntCar.ClassLibrary._Mobile
{
    public class AvailabilityData_Mobile
    {
        public string groupCodeName { get; set; }
        public Guid groupCodeId { get; set; }
        public decimal ratio { get; set; }    
        public decimal payNowTotalAmount { get; set; }
        public decimal payLaterTotalAmount { get; set; }
        public decimal campaign_payNowTotalAmount { get; set; }
        public decimal campaign_payLaterTotalAmount { get; set; }
    }
}
