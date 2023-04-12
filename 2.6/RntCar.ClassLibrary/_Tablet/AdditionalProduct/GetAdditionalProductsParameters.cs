using System;

namespace RntCar.ClassLibrary._Tablet
{
    public class GetAdditionalProductsParameters : RequestBase
    {
        public Guid groupCodeId { get; set; }
        public Guid customerId { get; set; }
        public Guid contractId { get; set; }
    }
}
