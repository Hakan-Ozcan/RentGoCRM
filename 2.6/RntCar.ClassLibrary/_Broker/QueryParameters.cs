using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary._Broker
{
    public class QueryParameters
    {
        public Guid pickupBranchId { get; set; }
        public Guid dropoffBranchId { get; set; }
        public DateTime pickupDateTime { get; set; }
        public DateTime dropoffDateTime { get; set; }
    }
}
