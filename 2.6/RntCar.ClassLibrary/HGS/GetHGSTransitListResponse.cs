using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary
{
    public class GetHGSTransitListResponse : ClassLibrary._Tablet.ResponseBase
    {
        public List<HGSTransitData> transits { get; set; }
        public bool showErrorMessage { get; set; } = false;
    }
}
