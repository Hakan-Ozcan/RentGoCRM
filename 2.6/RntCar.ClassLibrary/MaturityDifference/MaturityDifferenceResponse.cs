using System.Collections.Generic;

namespace RntCar.ClassLibrary
{
    public class MaturityDifferenceResponse : ResponseBase
    {
        public List<AdditionalProductData> additionalProducts { get; set; }
        public decimal totalAmount { get; set; }
        public decimal totaltobePaidAmount { get; set; }
    }
}
