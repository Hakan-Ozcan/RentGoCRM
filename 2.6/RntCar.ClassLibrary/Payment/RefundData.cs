using RntCar.ClassLibrary._Enums_1033;
using System;

namespace RntCar.ClassLibrary
{
    public class RefundData
    {
        public Guid? reservationId { get; set; }
        public Guid? contractId { get; set; }
        public decimal paidAmount { get; set; }
        public decimal refundAmount { get; set; }
        public rnt_PaymentChannelCode paymentChannelCode { get; set; }
        public int transactionResult { get; set; }
        public Guid? parentPaymentId { get; set; }
        public string paymentTransactionId { get; set; }
        public string paymentResultId { get; set; }
        public Guid? customerCreditCard { get; set; }
        public Guid transactionCurrencyId { get; set; }
        public int refundStatus { get; set; }
        public Guid? contactId { get; set; }
    }
}
