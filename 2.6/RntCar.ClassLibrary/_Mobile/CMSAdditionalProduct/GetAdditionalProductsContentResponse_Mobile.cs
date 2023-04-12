using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary._Mobile
{
    public class GetAdditionalProductsContentResponse_Mobile : ResponseBase
    {
        public List<CMSAdditionalProductData_Mobile> additionalProducts { get; set; }
    }
}
