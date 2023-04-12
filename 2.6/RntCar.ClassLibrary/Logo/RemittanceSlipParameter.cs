using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary
{
    public class RemittanceSlipParameter
    {
        public string description { get; set; }
        public string date { get; set; }
        public double total { get; set; }
        public int division { get; set; }
        public string iyzicoArpCode { get; set; }
        public string custemerArpCode { get; set; }
        public string projeCode { get; set; }
        public string specode { get; set; }
        public string lineDescription { get; set; }
        public bool canceledStatus { get; set; }

    }
}
