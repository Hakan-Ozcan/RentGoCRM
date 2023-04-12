using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary
{
    public class WorkingHourData
    {
        public Guid WorkingHourId { get; set; }
        public Guid BranchId { get; set; }
        public string BranchName { get; set; }
        public int DayCode { get; set; }
        public int BeginingTime { get; set; }
        public string BeginingTimeStr { get; set; }
        public int EndTime { get; set; }
        public string EndTimeStr { get; set; }
    }
}
