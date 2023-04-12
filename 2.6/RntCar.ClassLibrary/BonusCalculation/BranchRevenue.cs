using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary
{
    public class BranchRevenue
    {
        public Guid branchId { get; set; }
        public decimal totalAmount { get; set; }
        public decimal revenueAmount { get; set; }
    }
}
