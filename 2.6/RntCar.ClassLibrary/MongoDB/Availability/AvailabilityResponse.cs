
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary.MongoDB
{
    public class AvailabilityResponse : ResponseBase
    {
        public List<AvailabilityData> availabilityData { get; set; }
        public string trackingNumber { get; set; }
        public decimal documentTotalAmount { get; set; }

    }
}
