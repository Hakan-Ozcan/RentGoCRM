using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary._Broker
{
    public class Branch_Broker
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
    }
}
