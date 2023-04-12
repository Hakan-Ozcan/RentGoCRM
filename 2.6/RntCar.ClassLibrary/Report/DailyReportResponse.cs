using System.Collections.Generic;

namespace RntCar.ClassLibrary.Report
{
    public class DailyReportResponse : ResponseBase
    {
        public List<DailyReportData> dailyReportDatas { get; set; }
    }
}
