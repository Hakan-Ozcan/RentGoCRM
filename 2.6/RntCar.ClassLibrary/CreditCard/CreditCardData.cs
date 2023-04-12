using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary
{
    public class CreditCardData
    {
        public Guid? creditCardId { get; set; }
        public string binNumber { get; set; }
        public string conversationId { get; set; }
        public string externalId { get; set; }
        public string cardUserKey { get; set; }
        public string cardToken { get; set; }       
        public string creditCardNumber { get; set; }
        public int? expireYear { get; set; }
        public int? expireMonth { get; set; }
        public string cardHolderName { get; set; }
        public string cvc { get; set; }
        public int? cardType { get; set; }
    }
}
