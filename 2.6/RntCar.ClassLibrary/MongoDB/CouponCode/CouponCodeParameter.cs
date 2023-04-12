using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary.MongoDB
{
    public class CouponCodeParameter
    {
        public string couponCodeDefinitionId { get; set; }

        public List<string> couponCodes { get; set; }
    }
}
