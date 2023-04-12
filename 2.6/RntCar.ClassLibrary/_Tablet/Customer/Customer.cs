using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary._Tablet
{
    public class Customer
    {
        public Guid customerId { get; set; }
        public string fullName { get; set; }
        public string mobilePhone { get; set; }
        public string email { get; set; }
        public string drivingLicenseNumber { get; set; }
        public string drivingLicenseFrontImage { get; set; }
        public string drivingLicenseRearImage { get; set; }
        public int segment { get; set; }
        public Guid priceCodeId { get; set; }
        public int contractType { get; set; }
        public int paymentMethod { get; set; }
    }
}
