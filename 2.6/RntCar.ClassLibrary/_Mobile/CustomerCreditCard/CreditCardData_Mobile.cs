using System;

namespace RntCar.ClassLibrary._Mobile
{
    public class CreditCardData_Mobile
    {
        public Guid creditCardId { get; set; }
        public string bankName { get; set; }
        public string cardType { get; set; }
        public string cardUserKey { get; set; }
        public string cardToken { get; set; }
        public string creditCardNumber { get; set; }
        public string cardHolderName { get; set; }
        public int? expireYear { get; set; }
        public int? expireMonth { get; set; }
        public string cvc { get; set; }
    }
}
