using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary
{
    public class GetCustomerCreditCardsResponse : ResponseBase
    {
        public List<CreditCardData> creditCards { get; set; }
        public string selectedCreditCardId { get; set; }
    }
}
