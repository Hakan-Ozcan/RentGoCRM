using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary._Tablet
{
    public class ContractDateandBranchData
    {
        public Guid dropoffBranchId { get; set; }
        public Guid pickupBranchId { get; set; }
        public DateTime dropoffDateTime { get; set; }
        public DateTime pickupDateTime { get; set; }
        public string contractId { get; set; }
    }
}
