using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary.Report
{
    public class FleetReportDataRequest : RequestBase
    {
        public long startDate { get; set; }
        public long endDate { get; set; }
        public int hour { get; set; }
    }
}
