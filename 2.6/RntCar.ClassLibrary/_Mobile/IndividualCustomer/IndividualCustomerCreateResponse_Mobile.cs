using System;

namespace RntCar.ClassLibrary._Mobile
{
    public class IndividualCustomerCreateResponse_Mobile : ResponseBase
    {
        public Guid individualCustomerId { get; set; }
        public Guid individualAddressId { get; set; }
        public Guid invoiceAddressId { get; set; }
    }
}
