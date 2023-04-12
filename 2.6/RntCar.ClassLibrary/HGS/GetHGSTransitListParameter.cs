using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary
{
    public class GetHGSTransitListParameter
    {
        public string productId { get; set; }
        public long startDateTimeStamp { get; set; }
        public long finishDateTimeStamp { get; set; }
        public bool isManuelProcess { get; set; }
    }
}
