using System;

namespace RntCar.ClassLibrary.PaymentPlan
{
    public class PaymentPlanData
    {
        public Guid groupCodeId { get; set; }
        public string groupCodeId_str { get; set; }
        public string paymentPlanId { get; set; }
        public decimal payLaterAmount { get; set; }
        public decimal payNowAmount { get; set; }
        public Guid priceListId { get; set; }
        public string priceListName { get; set; }
        public int month { get; set; }

    }   
}
