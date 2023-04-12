using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary._Mobile.DeviceToken
{
    public class DeviceTokenResponse_Mobile:ResponseBase
    {
        public string id { get; set; }
        public string deviceToken { get; set; }
    }
}
