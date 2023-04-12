using System;

namespace RntCar.ClassLibrary.MongoDB.BusinessClosure
{
    public class BusinessClosureData
    {
        public string businessClosureId { get; set; }
        public string name { get; set; }
        public string branchValues { get; set; }
        public DateTime beginDate { get; set; }
        public long beginDateTimestamp { get; set; }
        public DateTime endDate { get; set; }
        public long endDateTimestamp { get; set; }
        public int statuscode { get; set; }
        public int statecode { get; set; }
    }
}
