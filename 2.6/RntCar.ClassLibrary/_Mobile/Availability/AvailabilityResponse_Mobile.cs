using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary._Mobile
{
    public class AvailabilityResponse_Mobile : ResponseBase
    {
        public List<AvailabilityData_Mobile> availabilityData { get; set; }
        public string trackingNumber { get; set; }
        public decimal duration { get; set; }
    }
}
