using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary._Mobile.DeviceToken
{
    public class DeviceTokenRequest_Mobile:RequestBase
    {
        public string deviceToken { get; set; }
        public string userId { get; set; }
    }
}
