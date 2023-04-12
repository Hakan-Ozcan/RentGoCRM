using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary.MongoDB
{
    public class ExcludeAvailability
    {
        public Dictionary<string, decimal> availabilityResults { get; set; }

        public Dictionary<string, Dictionary<DateTime, decimal>> availabilityDayResults { get; set; }

    }
}
