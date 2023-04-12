using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary
{
    public class PaymentResponse 
    {
        public string clientReferenceCode { get; set; }
        public string paymentId { get; set; }
        public string paymentTransactionId { get; set; }
        public bool status { get; set; }
        public decimal providerComission { get; set; } = 0;
        public string errorCode { get; set; }
        public string errorMessage { get; set; }
        public string errorGroup { get; set; }
        public string cardOrganizationType { get; set; }
        public string cardProgram { get; set; }
        public string cardType { get; set; }
        public string cardGroup { get; set; }
        public string cardBin { get; set; }
        public bool? creditCardSaveSafely { get; set; }
        public string merchantSafeKey { get; set; }
        public bool use3DSecure { get; set; }
        public string htmlContent { get; set; }
    }
}
