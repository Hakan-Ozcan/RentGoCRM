using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary.MongoDB
{
    public class CouponCodeResponse : ResponseBase
    {
        public List<CouponCodeData> couponCodeList { get; set; }
    }
}
