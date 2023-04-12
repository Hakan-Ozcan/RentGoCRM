﻿using System;

namespace RntCar.ClassLibrary._Mobile.Reservation
{
    public class ReservationData_Mobile
    {
        public Guid reservationId { get; set; }
        public string PnrNumber { get; set; }
        public DateTime PickupTime { get; set; }
        public DateTime DropoffTime { get; set; }
        public string PickupBranchId { get; set; }
        public string DropoffBranchId { get; set; }
        public int StatusCode { get; set; }
        public string StatusName { get; set; }
        public string DropoffBranchName { get; set; }
        public string PickupBranchName { get; set; }
        public string corporateCustomerName { get; set; }
        public Guid? corporateCustomerId { get; set; }
        public Guid? customerId { get; set; }
        public string customerName { get; set; }
        public int reservationType { get; set; }
        public string groupCodeName { get; set; }
        public Guid groupCodeId { get; set; }
    }
}
