using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary
{
    public class HGSTransitData
    {
        public long entryDateTime { get; set; }
        public DateTime _entryDateTime { get; set; }
        public long exitDateTime { get; set; }
        public DateTime _exitDateTime { get; set; }
        public string entryLocation { get; set; }
        public string exitLocation { get; set; }
        public string description { get; set; }
        public decimal amount { get; set; }
        public string plateNumber { get; set; }
        public string hgsNumber { get; set; }
    }
}
