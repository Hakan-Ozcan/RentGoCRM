using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary._Mobile
{
    public class CreateCreditCardParameters_Mobile:RequestBase
    {
        public string creditCardNumber { get; set; }
        public string cardHolderName { get; set; }
        public string cardAlias { get; set; }
        public int? expireMonth { get; set; }
        public int? expireYear { get; set; }
        public string cvc { get; set; }
        public string email { get; set; }
        public int isHidden { get; set; }
        public string individualCustomerId { get; set; }
    }
}
