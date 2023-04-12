using System;

namespace RntCar.ClassLibrary.Reservation
{
    public  class ReservationEquipmentParameters
    {
        public Guid groupCodeInformationId { get; set; }
        public string groupCodeInformationName { get; set; }
        public int segment { get; set; }
        public decimal depositAmount { get; set; }
        public string itemName { get; set; }
        public int? billingType { get; set; }

    }
}
