using System;

namespace RntCar.ClassLibrary._Mobile
{
    public class GetCustomerReservationsRequest_Mobile : RequestBase
    {
        public Guid individualCustomerId  { get; set; }
        public Guid corporateCustomerId  { get; set; }
    }
}
