using System;

namespace RntCar.ClassLibrary
{
    public class ReservationSearchParameters
    {
        public string reservationNumber { get; set; }
        public Guid? pickupBranchId { get; set; }
        public DateTime? pickupDate { get; set; }
        public Guid? dropoffBranchId { get; set; }
        public DateTime? dropoffDate { get; set; }
        public string customerId { get; set; }
        public string dummyInformation { get; set; }
    }
}
