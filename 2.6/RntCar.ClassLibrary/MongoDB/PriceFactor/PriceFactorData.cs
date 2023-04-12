using System;
using System.Collections.Generic;

namespace RntCar.ClassLibrary.MongoDB
{
    public class PriceFactorData
    {
        public string priceFactorId { get; set; }
        public string name { get; set; }
        public int priceFactorType { get; set; }
        public int payMethod { get; set; }
        public string reservationChannel { get; set; }
        public string weekDays { get; set; }
        public string branchs { get; set; }
        public string groupCodes { get; set; }
        public string segments { get; set; }
        public int priceChangeType { get; set; }
        public decimal value { get; set; }
        public DateTime? beginDate { get; set; }
        public DateTime? endDate { get; set; }
        public DateTime createdon { get; set; }
        public DateTime modifiedon { get; set; }
        public string createdby { get; set; }
        public string modifiedby { get; set; }
        public int statuscode { get; set; }
        public int statecode { get; set; }
        public string type { get; set; }
        public string accountGroups { get; set; }
        public List<PriceFactorDatesData> dates { get; set; }
    }
    public class PriceFactorDatesData
    {
        public long beginDate { get; set; }
        public long endDate { get; set; }
    }
}
