using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary.t
{
    public class GetEIhlalPenalParameters
    {
        public string plateNumber { get; set; }
        public long pickupDateTimeStamp { get; set; }
        public long dropoffDatetimeStamp { get; set; }
        public bool isManuelProcess { get; set; }
    }
}
