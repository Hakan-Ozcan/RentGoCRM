using RntCar.ClassLibrary.PaymentPlan;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary.MongoDB
{
    public class AvailabilityData
    {
        public List<PaymentPlanData> paymentPlanData { get; set; }
        public Guid? documentItemId { get; set; }
        public Guid groupCodeInformationId { get; set; }
        public string groupCodeInformationName { get; set; }
        public decimal ratio { get; set; }
        public bool isUpgrade { get; set; }
        public bool isDowngrade { get; set; }
        public List<Guid> upgradeGroupCodes { get; set; }
        public bool isUpsell { get; set; }
        public bool isDownsell { get; set; }
        public decimal payNowTotalPrice { get; set; }
        public decimal payLaterTotalPrice { get; set; }
        public decimal totalPrice { get; set; }
        public decimal totalPaidPrice { get; set; }
        public decimal documentEquipmentPrice { get; set; }      
        public decimal paidPriceEquipment { get; set; } = decimal.Zero;
        public decimal amounttobePaid_Equipment { get; set; }
        public int equipmentCount { get; set; }
        public int totalDuration { get; set; }
        public string currencyCode { get; set; }
        public string currencyId { get; set; }
        public bool isPriceCalculatedSafely { get; set; } = true;
        public string priceErrorMessage { get; set; }
        public bool hasError { get; set; }
        public string errorMessage { get; set; }
        public bool canUserStillHasCampaignBenefit { get; set; }
        public bool documentHasCampaignBefore { get; set; }
        public CampaignData CampaignInfo { get; set; }
        public int operationType { get; set; }
    }
}
