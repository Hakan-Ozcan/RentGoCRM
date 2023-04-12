using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary._Broker
{
    public class WorkingHour_Broker
    {
        public Guid workingHourId { get; set; }
        public Guid branchId { get; set; }
        public string branchName { get; set; }
        public int dayCode { get; set; }
        public int beginingTime { get; set; }
        public int endTime { get; set; }
    }
}
