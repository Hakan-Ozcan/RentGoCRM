using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary
{
    public class SelectedDateAndBranchInformation
    {
        public Guid pickupBranchId { get; set; }
        public Guid dropoffBranchId { get; set; }
        public DateTime pickupDate { get; set; }
        public DateTime dropoffDate { get; set; }

    }
}
