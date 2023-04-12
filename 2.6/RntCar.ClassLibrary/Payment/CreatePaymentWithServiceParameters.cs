using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary
{
    public class CreatePaymentWithServiceParameters
    {
        public CreatePaymentParameters createPaymentParameters { get; set; }
        public PaymentResponse paymentResponse { get; set; }
        public List<RefundData> refundEntities { get; set; }
    }
}
