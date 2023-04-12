using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary
{
    public interface IDateAndBranch
    {
        Guid pickupBranchId { get; set; }
        Guid dropoffBranchId { get; set; }
        DateTime pickupDate { get; set; }
        DateTime dropoffDate { get; set; }
    }
}
