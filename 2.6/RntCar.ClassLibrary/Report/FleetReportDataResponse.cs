using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary.Report
{
    public class FleetReportDataResponse : ResponseBase
    {
        public List<FleetReportData> fleetReportDatas { get; set; }
    }
}
