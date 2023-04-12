using System;

namespace RntCar.ClassLibrary._Web
{
    public class GetCustomerReservationsRequest_Web : RequestBase
    {
        public Guid individualCustomerId  { get; set; }
        public Guid corporateCustomerId  { get; set; }
    }
}
