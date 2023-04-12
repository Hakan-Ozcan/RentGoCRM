using RntCar.ClassLibrary.PaymentPlan;
using System;
using System.Collections.Generic;

namespace RntCar.ClassLibrary.Reservation
{
    public class ReservationPriceParameters
    {
        public decimal price { get; set; }
        public decimal? discountAmount { get; set; }
        public int paymentType { get; set; }
        public string pricingType { get; set; }
        public int installment { get; set; }
        public int? virtualPosId { get; set; }
        public decimal? installmentTotalAmount { get; set; }
        public Guid? campaignId { get; set; }
        public List<CreditCardData> creditCardData { get; set; }
        public List<PaymentPlanData> paymentPlans { get; set; }
        public bool isMonthly { get; set; } = false;
        public int monthValue { get; set; } = 0;
        public bool use3DSecure { get; set; }
        public string callBackUrl { get; set; }
    }
}
