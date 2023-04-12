using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary._Broker
{
    public class AdditionalProductResponse_Broker : ResponseBase
    {
        /// <summary>
        /// Rezervasyon ek ürün bilgisi
        /// </summary>
        public List<AdditionalProductData_Broker> additionalProducts { get; set; }
    }
}
