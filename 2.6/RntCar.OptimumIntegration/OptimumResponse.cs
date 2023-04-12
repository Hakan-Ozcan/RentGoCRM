using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.OptimumIntegration
{
    public class OptimumResponse
    {
        public string messages { get; set; }
        public string input { get; set; }
        public List<Output> output { get; set; }
        public string status { get; set; }
        public string extras { get; set; }
    }

    public class Output
    {
        public string registrationNumber { get; set; }
        public string currentKm { get; set; }
        public string maintenanceKm { get; set; }
        public string claimType { get; set; }
        public bool isMaintained { get; set; }
        public int nextMaintenanceKm { get; set; }
    }
}
