using RntCar.ClassLibrary.MongoDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary
{
    public class CouponCodeDefinitionResponse : ResponseBase
    {
        public CouponCodeDefinitionData CouponCodeDefinition { get; set; }
    }
}
