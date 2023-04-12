using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary
{
    public class DeviceTokenData
    {
        public string token { get; set; }
        public string uId { get; set; }
        public long lastLgn { get; set; } = DateTime.Now.Ticks;
    }
}
