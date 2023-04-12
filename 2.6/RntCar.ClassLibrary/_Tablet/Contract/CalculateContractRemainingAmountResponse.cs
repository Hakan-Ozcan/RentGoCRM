using System.Collections.Generic;

namespace RntCar.ClassLibrary._Tablet
{
    public class CalculateContractRemainingAmountResponse : ResponseBase
    {
        public List<AdditonalProductDataTablet> otherAdditionalProductData { get; set; }
        public CalculateAdditionalProductResponse additionalProductResponse { get; set; }
        public AdditonalProductDataTablet otherCostAdditionalProductData { get; set; }
        public CalculatePricesForUpdateContractResponse calculatePricesForUpdateContractResponse { get; set; }

        public long dateNow;
    }
}
