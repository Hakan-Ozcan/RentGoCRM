using RntCar.ClassLibrary.PaymentPlan;
using System;
using System.Collections.Generic;

namespace RntCar.ClassLibrary.MongoDB
{
    public class ReservationItemData
    {
        public string ReservationItemId { get; set; }
        public int Offset { get; set; }
        public int StatusCode { get; set; }
        public int StateCode { get; set; }
        public string EquipmentId { get; set; }
        public string EquipmentName { get; set; }
        public int? ChangeReason { get; set; }
        public string GroupCodeInformationId { get; set; }
        public string GroupCodeInformationName { get; set; }
        public string pricingGroupCodeName { get; set; }
        public string pricingGroupCodeId { get; set; }
        public string CurrencyId { get; set; }
        public DateTime PickupTime { get; set; }
        public DateTime DropoffTime { get; set; }
        public DateTime? CancellationTime { get; set; }
        public DateTime? NoShowTime { get; set; }
        public string PickupBranchId { get; set; }
        public string PickupBranchName { get; set; }
        public string DropoffBranchId { get; set; }
        public string DropoffBranchName { get; set; }
        public decimal NetAmount { get; set; }
        public decimal TaxRatio { get; set; }
        public decimal TotalAmount { get; set; }
        public string OwnernId { get; set; }
        public string OwnerName { get; set; }
        public string CreatedBy { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime ModifiedOn { get; set; }
        public string CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string ReservationId { get; set; }
        public string ReservationNumber { get; set; }
        public string PnrNumber { get; set; }
        public int ReservationType { get; set; }
        public int ReservationChannel { get; set; }
        public int? PaymentChoice { get; set; }
        public int? PaymentMethod { get; set; }
        public decimal DepositAmount { get; set; }
        public decimal ReservationTotalAmount { get; set; }
        public int ItemTypeCode { get; set; }
        public string trackingNumber { get; set; }
        public decimal overKilometerPrice { get; set; }
        public Guid? campaignId { get; set; }
        public string campaignName { get; set; }
        public int? billingType { get; set; }
        public bool processIndividualPrices { get; set; }
        public Guid? corporateCustomerId { get; set; }
        public string corporateCustomerName { get; set; }
        public Guid? transactionCurrencyId { get; set; }
        public string transactionCurrencyName { get; set; }
        public string transactionCurrencyCode { get; set; }
        public string dummyContactInformation { get; set; }
        public bool isMonthly { get; set; }
        public int monthValue { get; set; }
        public List<PaymentPlanData> paymentPlans { get; set; }
        
    }
}
