using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary._Tablet
{
    public class GetAdditionalProductsResponse : ResponseBase
    {
        public List<AdditionalProductRuleDataTablet> additionalProductRuleData { get; set; }
        public List<AdditonalProductDataTablet>  additionalProductData { get; set; }
    }
}
