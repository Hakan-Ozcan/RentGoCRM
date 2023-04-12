using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary
{
    public class RevenueReportRequest : RequestBase
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
