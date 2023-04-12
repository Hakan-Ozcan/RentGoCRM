namespace RntCar.ClassLibrary.Report
{
    public class GetDailyReportRequest : RequestBase
    {
        public long StartDate { get; set; }
        public long EndDate { get; set; }
    }
}
