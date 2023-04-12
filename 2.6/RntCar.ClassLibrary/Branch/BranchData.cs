using System;

namespace RntCar.ClassLibrary
{
    public class BranchData
    {
        public Guid? regionManagerId { get; set; }
        public string regionManagerName { get; set; }
        public string BranchId { get; set; }
        public string BranchName { get; set; }
        public string CityId { get; set; }
        public string CityName { get; set; }
        public string addressDetail { get; set; }
        public double latitude { get; set; }
        public double longitude { get; set; }
        public string telephone { get; set; }
        public string emailaddress { get; set; }
        public string seoKeyword { get; set; }
        public string seoTitle { get; set; }
        public string seoDescription { get; set; }
        public int? earlistPickupTime { get; set; }
        public int? branchZone { get; set; }
        public int? branchType { get; set; }
        public int? webRank { get; set; }
        public string postalCode { get; set; }
    }
}
