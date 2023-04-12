using System;

namespace RntCar.ClassLibrary
{
    public class PaymentObject
    {
        public int refundStatus { get; set; }
        public Guid paymentId { get; set; }
        public decimal refundAmount { get; set; }
    }
}
