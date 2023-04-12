using System;

namespace RntCar.ClassLibrary._Mobile
{
    public class ProcessCustomerInvoiceAddressRequest_Mobile
    {
        public string addressDetail { get; set; }
        public Guid individualCustomerId { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string governmentId { get; set; }
        public string companyName { get; set; }
        public string taxNumber { get; set; }
        public Guid? taxOfficeId { get; set; }
        public int invoiceType { get; set; }
        public Guid addressCountryId { get; set; }
        public Guid? addressCityId { get; set; }
        public Guid? addressDistrictId { get; set; }
        public Guid? invoiceAddressId { get; set; }
        public string invoiceName { get; set; }
    }
}
