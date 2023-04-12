using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary._Tablet
{
    public class CalculateAvailabilityResponse : ResponseBase
    {
        public List<TabletAvailabilityData> availabilityData { get; set; }
        public string trackingNumber { get; set; }
    }
}
