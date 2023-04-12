using RntCar.ClassLibrary.MongoDB;
using RntCar.ClassLibrary.PaymentPlan;
using System.Collections.Generic;

namespace RntCar.ClassLibrary
{
    public class CalculatePricesForUpdateContractResponse : ResponseBase
    {
        public decimal calculatedEquipmentPrice { get; set; }
        public decimal documentEquipmentPrice { get; set; }
        public decimal amountobePaid { get; set; }
        public string trackingNumber { get; set; }
        public decimal ratio { get; set; }
        public bool canUserStillHasCampaignBenefit { get; set; }
        public bool documentHasCampaignBefore { get; set; }
        public CampaignData campaignInfo { get; set; }
        public int? operationType { get; set; }

        public List<PaymentPlanData> paymentPlanData { get; set; }
    }
}
