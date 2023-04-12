using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary.MongoDB
{
    public class AvailabilityFactorData
    {

        public string availabilityFactorId { get; set; }
        public string name { get; set; }
        public int availabilityFactorType { get; set; }
        public string channelValues { get; set; }
        public string branchValues { get; set; }
        public string groupCodeValues { get; set; }
        public string groupCodeList { get; set; }
        public DateTime createdon { get; set; }
        public DateTime modifiedon { get; set; }
        public string createdby { get; set; }
        public string modifiedby { get; set; }
        public int statecode { get; set; }
        public int statuscode { get; set; }
        public DateTime beginDate { get; set; }
        public DateTime endDate { get; set; }
        public int minimumReservationDuration { get; set; }
        public int capacityRatio { get; set; }
        public List<string> type { get; set; }
        public List<string> accountGroups { get; set; }
    }

    public class AvailabilityFactorGroupCodes
    {
        public string id { get; set; }
        public string name { get; set; }
    }
}
