using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary
{
    public class CreditCardSlipResponse : ResponseBase
    {
        public int plugReference { get; set; }
        public string plugNumber { get; set; }
    }
}
