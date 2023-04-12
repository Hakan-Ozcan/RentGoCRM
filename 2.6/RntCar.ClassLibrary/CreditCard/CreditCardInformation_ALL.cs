using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary
{
    public class CreditCardInformation_ALL
    {
        public string creditCardNumber { get; set; }
        public string cardHolderName { get; set; }
        public int expireMonth { get; set; }
        public int expireYear { get; set; }
        public string cvc { get; set; }
        public string conversationId { get; set; }
        public string binNumber { get; set; }
        public string bankName { get; set; }
        public string cardType { get; set; }
        public string cardFamily { get; set; }
        public string cardAssociation { get; set; }
        public long? bankCode { get; set; }
        public string errorCode { get; set; }
        public string errorGroup { get; set; }
        public string errorMessage { get; set; }
        public string status { get; set; }
        public string cardUserKey { get; set; }
        public string cardToken { get; set; }
        public string individualCustomerId { get; set; }

    }
}
