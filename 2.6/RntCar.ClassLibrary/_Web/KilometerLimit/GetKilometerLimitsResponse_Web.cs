using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary._Web
{
    public class GetKilometerLimitsResponse_Web  : ResponseBase
    {
        public List<KmLimitData_Web> kmLimits { get; set; }
    }
}
