using System;

namespace RntCar.ClassLibrary.MongoDB
{
    public class VirtualBranchData
    {
        public string virtualBranchId { get; set; }
        public string name { get; set; }
        public Guid branch { get; set; }
        public string brokerCode { get; set; }
        public string accountBranch { get; set; }
        public string channelCode { get; set; }
        public int statuscode { get; set; }
        public int statecode { get; set; }
        public bool useBranchInformation { get; set; }
        public Guid? cityId { get; set; }
        public string cityName { get; set; }
        public string email { get; set; }
        public string telephone { get; set; }
        public string addressDetail { get; set; }
        public double latitude { get; set; }
        public double longitude { get; set; }
        public int? webRank { get; set; }
    }
}
