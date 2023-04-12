using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary._Web
{
    public class GetAdditionalProductsContentResponse_Web : ResponseBase
    {
        public List<CMSAdditionalProductData_Web> additionalProducts { get; set; }
    }
}
