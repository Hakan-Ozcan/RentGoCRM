using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary._Web
{
    public class AvailabilityResponse_Web : ResponseBase
    {
        public List<AvailabilityData_Web> availabilityData { get; set; }
        public string trackingNumber { get; set; }
        public decimal duration { get; set; }
    }
}
