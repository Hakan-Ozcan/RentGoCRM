using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary
{
    public class AdditionalProductResponse : ResponseBase
    {
        public List<AdditionalProductData> AdditionalProducts { get; set; }
        public decimal totaltobePaidAmount { get; set; }
    }
}
