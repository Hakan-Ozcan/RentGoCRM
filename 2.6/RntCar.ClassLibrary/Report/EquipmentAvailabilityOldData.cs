using System;

namespace RntCar.ClassLibrary.Report
{
    public class EquipmentAvailabilityOldData
    {
        public Guid CurrentBranchId { get; set; }
        public string CurrentBranch { get; set; }
        public DateTime publishDate { get; set; }
        public long publishDateTimeStamp { get; set; }
        public decimal revenue { get; set; }
        public string externalId { get; set; }
        public int totalDays { get; set; }
    }
}
