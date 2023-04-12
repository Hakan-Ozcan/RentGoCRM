using System;

namespace RntCar.ClassLibrary._Web
{
    public class ReservationCustomerParameters_Web
    {
        public Guid? corporateCustomerId { get; set; }
        public Guid individualCustomerId { get; set; }
        public InvoiceAddressData_Web invoiceAddress { get; set; }
    }
}
