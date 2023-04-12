using RntCar.ClassLibrary.PaymentPlan;
using System;
using System.Collections.Generic;

namespace RntCar.ClassLibrary
{
    public class ContractPriceParameters
    {
        public decimal reservationPaidAmount { get; set; }
        public decimal initialPrice { get; set; }
        public decimal price { get; set; }
        public int paymentType { get; set; }
        public string pricingType { get; set; }
        public decimal depositAmount { get; set; }
        public int installment { get; set; }
        public int? virtualPosId { get; set; }
        public decimal installmentTotalAmount { get; set; }
        public decimal? amounttobePaid { get; set; }
        public Guid transactionCurrencyId { get; set; }
        public Guid? campaignId { get; set; }
        public List<CreditCardData> creditCardData { get; set; }
        public List<PaymentPlanData> paymentPlanData { get; set; }
        public bool use3DSecure { get; set; }
        public string callBackUrl { get; set; }

    }
}
