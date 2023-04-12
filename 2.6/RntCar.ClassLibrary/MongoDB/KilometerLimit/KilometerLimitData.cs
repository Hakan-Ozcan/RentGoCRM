using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary.MongoDB
{
    public class KilometerLimitData
    {
        public string kilometerLimitId { get; set; }
        public int durationCode { get; set; }
        public Guid groupCodeInformationId { get; set; }
        public int kilometerLimit { get; set; }
        public int maximumDay { get; set; }
        public string name { get; set; }
        public DateTime createdon { get; set; }
        public DateTime modifiedon { get; set; }
        public string createdby { get; set; }
        public string modifiedby { get; set; }
        public int statuscode { get; set; }
        public int statecode { get; set; }
    }
}
