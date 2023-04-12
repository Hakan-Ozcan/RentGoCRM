using System;

namespace RntCar.ClassLibrary._Mobile
{
    public class ReservationCustomerParameters_Mobile
    {
        public Guid? corporateCustomerId { get; set; }
        public Guid individualCustomerId { get; set; }
        public InvoiceAddressData_Mobile invoiceAddress { get; set; }
    }
}
