using System;

namespace RntCar.ClassLibrary._Web
{
    public class Branch_Web
    {
        public Guid branchId { get; set; }
        public string branchName { get; set; }
        public Guid? cityId { get; set; }
        public string cityName { get; set; }
        public string addressDetail { get; set; }
        public double latitude { get; set; }
        public double longitude { get; set; }
        public string telephone { get; set; }
        public string emailaddress { get; set; }
        public string seoKeyword { get; set; }
        public string seoTitle { get; set; }
        public string seoDescription { get; set; }
        public int? earlistPickupTime { get; set; }
        public int? webRank { get; set; }
        public string postalCode { get; set; }
    }
}
