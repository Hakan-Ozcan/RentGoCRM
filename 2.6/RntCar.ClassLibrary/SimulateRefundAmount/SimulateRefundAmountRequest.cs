using System;
using System.Collections.Generic;

namespace RntCar.ClassLibrary
{
    public class SimulateRefundAmountRequest : RequestBase
    {
        public Guid? reservationId { get; set; }
        public Guid? contractId { get; set; }
        public decimal refundAmount { get; set; }

        public List<AdditionalProductData> additionalProducts { get; set; }
    }

}
