using System.Collections.Generic;

namespace RntCar.ClassLibrary
{
    public class SimulateRefundAmountResponse : ResponseBase
    {
        public decimal totalAmount { get; set; }
        public decimal totaltobePaidAmount { get; set; }
        public List<AdditionalProductData> additionalProducts { get; set; }
    }
}
