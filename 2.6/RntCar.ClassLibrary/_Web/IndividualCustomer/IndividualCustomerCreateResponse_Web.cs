using System;

namespace RntCar.ClassLibrary._Web
{
    public class IndividualCustomerCreateResponse_Web : ResponseBase
    {
        public Guid individualCustomerId { get; set; }
        public Guid individualAddressId { get; set; }
        public Guid invoiceAddressId { get; set; }
    }
}
