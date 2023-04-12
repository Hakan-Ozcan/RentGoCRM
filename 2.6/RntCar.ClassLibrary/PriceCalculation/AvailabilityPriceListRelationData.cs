using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary
{
    public class AvailabilityPriceListRelationData
    {
        public string rnt_name { get; set; }
        public int rnt_maximumavailability { get; set; }
        public int rnt_minimumavailability { get; set; }
        public decimal rnt_ratio { get; set; }
    }
}
