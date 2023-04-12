using System;

namespace RntCar.ClassLibrary._Web
{
    public class WorkingHour_Web
    {
        public Guid workingHourId { get; set; }
        public Guid branchId { get; set; }
        public string branchName { get; set; }
        public int dayCode { get; set; }
        public int beginingTime { get; set; }
        public int endTime { get; set; }
    }
}
