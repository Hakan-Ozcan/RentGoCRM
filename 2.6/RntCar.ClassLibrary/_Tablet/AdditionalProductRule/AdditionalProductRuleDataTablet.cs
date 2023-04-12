using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary._Tablet
{
    public class AdditionalProductRuleDataTablet
    {
        public Guid parentProductId { get; set; }
        public string parentProductName { get; set; }
        public Guid productId { get; set; }
        public string productName { get; set; }
    }
}
