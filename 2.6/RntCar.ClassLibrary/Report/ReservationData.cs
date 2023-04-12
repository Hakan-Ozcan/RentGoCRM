using System;

namespace RntCar.ClassLibrary
{
    public class ReservationData
    {
        public DateTime PickupDatetime { get; set; }
        public Guid PickupBranchId { get; set; }
        public int Status { get; set; }
    }
}
