using RntCar.ClassLibrary._Tablet;
using System.Collections.Generic;

namespace RntCar.ClassLibrary 
{
    public class GetHgsAdditionalProductsResponse : ClassLibrary._Tablet.ResponseBase
    {
        public List<AdditonalProductDataTablet> hgsAdditionalProductData { get; set; }
    }
}
