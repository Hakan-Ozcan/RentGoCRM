using RntCar.ClassLibrary._Mobile.AdditionalProductRules;
using System.Collections.Generic;

namespace RntCar.ClassLibrary._Mobile
{
    public class AdditionalProductResponse_Mobile : ResponseBase
    {
        public List<AdditionalProductData_Mobile> additionalProducts { get; set; }
        public List<AdditionalProductRule_Mobile> additionalProductRules { get; set; }
    }
}
