using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary
{
    public class BranchStatusReport
    {
        public int dailyBranchReservationCount { get; set; }
        public int dailyGlobalReservationCount { get; set; }
        public double branchStatusRatio { get; set; }
    }
}
