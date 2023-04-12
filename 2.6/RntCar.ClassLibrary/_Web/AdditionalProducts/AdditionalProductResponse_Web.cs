using System.Collections.Generic;

namespace RntCar.ClassLibrary._Web
{
    public class AdditionalProductResponse_Web : ResponseBase
    {
        public List<AdditionalProductData_Web> additionalProducts { get; set; }
        public List<AdditionalProductRule_Web> additionalProductRules { get; set; }

        public int duration { get; set; }
    }
}
