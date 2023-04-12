using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary
{
    public class CreatePaymentResponse : ResponseBase
    {
        public string externalPaymentId { get; set; }
        public string externalPaymentTransactionId { get; set; }
        public string paymentId { get; set; }
        public string creditCardId { get; set; }
        public decimal paidAmount { get; set; }
        public bool use3DSecure { get; set; }
        public string htmlContent { get; set; }
    }
}
