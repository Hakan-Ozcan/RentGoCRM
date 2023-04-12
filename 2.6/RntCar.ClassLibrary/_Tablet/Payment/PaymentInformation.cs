using System.Collections.Generic;

namespace RntCar.ClassLibrary._Tablet
{
    public class PaymentInformation
    {
        public List<CreditCardData> creditCardData { get; set; }
        public bool use3DSecure { get; set; }
        public string callBackUrl { get; set; }
    }
}
