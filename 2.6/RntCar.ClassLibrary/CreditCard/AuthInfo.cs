using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary
{
    public class AuthInfo
    {
        //net tahsilat related
        public string userName { get; set; }
        public string password { get; set; }
        public string vendorCode { get; set; }
        public int vendorId { get; set; }
        //iyzico related
        public string apikey { get; set; }
        public string secretKey { get; set; }
        public string baseurl { get; set; }
    }
}
