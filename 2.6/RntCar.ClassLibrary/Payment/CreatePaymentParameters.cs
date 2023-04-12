using RntCar.ClassLibrary._Enums_1033;
using System;
namespace RntCar.ClassLibrary
{
    public class CreatePaymentParameters
    {
        public Guid? reservationId { get; set; }
        public Guid? contractId { get; set; }
        public Guid individualCustomerId { get; set; }
        public Guid transactionCurrencyId { get; set; }
        public int installment { get; set; }
        public int langId { get; set; }
        public decimal installmentRatio { get; set; } = 0;
        public decimal paidAmount { get; set; }
        public decimal? installmentAmount { get; set; }
        public InvoiceAddressData invoiceAddressData { get; set; }
        public Guid? invoiceId { get; set; }
        public CreditCardData creditCardData { get; set; }
        public int? paymentTransactionType { get; set; }
        public string conversationId { get; set; }       
        public string userName { get; set; }
        public string password { get; set; }
        public string vendorCode { get; set; }
        public int vendorId { get; set; }
        public int? virtualPosId { get; set; }
        public string apikey { get; set; }
        public string secretKey { get; set; }
        public string baseurl { get; set; }
        public rnt_PaymentChannelCode paymentChannelCode { get; set; }
        public bool? rollbackOperation { get; set; }
        public bool use3DSecure { get; set; }
        public string callBackUrl { get; set; }
        public string pnrNumber { get; set; }
    }
}
