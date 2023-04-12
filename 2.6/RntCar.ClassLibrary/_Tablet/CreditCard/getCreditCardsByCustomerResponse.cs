using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary._Tablet
{
    public class getCreditCardsByCustomerResponse : ResponseBase
    {
        public List<CreditCardData> creditCardList { get; set; }
        public string selectedCreditCardId { get; set; }
    }
}
