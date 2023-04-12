using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary._Mobile
{
    public class GetKilometerLimitsResponse_Mobile  : ResponseBase
    {
        public List<KmLimitData_Mobile> kmLimits { get; set; }
    }
}
