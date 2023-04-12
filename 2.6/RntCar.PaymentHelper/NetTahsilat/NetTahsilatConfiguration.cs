using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.PaymentHelper.NetTahsilat
{
    public class NetTahsilatConfiguration
    {
        public string nettahsilat_username{ get; set; }
        public string nettahsilat_password { get; set; }
        public string nettahsilat_url { get; set; }
        public string nettahsilat_vendorcode { get; set; }
        public int nettahsilat_vendorid { get; set; }

    }
}
