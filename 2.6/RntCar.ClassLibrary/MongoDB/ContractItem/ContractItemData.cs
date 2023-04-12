using RntCar.ClassLibrary.PaymentPlan;
using System;
using System.Collections.Generic;

namespace RntCar.ClassLibrary.MongoDB
{
    public class ContractItemData
    {
        public string contractId { get; set; }
        public string contractItemId { get; set; }
        public string itemNo { get; set; }
        public int offset { get; set; }
        public int statuscode { get; set; }
        public int statecode { get; set; }
        public string additionalProductId { get; set; }
        public string additionalProductName { get; set; }
        public string equipmentName { get; set; }
        public string equipmentId { get; set; }
        public int changeReason { get; set; }
        public string groupCodeInformationsName { get; set; }
        public string groupCodeInformationsId { get; set; }
        public string pricingGroupCodeName { get; set; }
        public string pricingGroupCodeId { get; set; }
        public string transactioncurrencyid { get; set; }
        public decimal netAmount { get; set; }
        public decimal taxRatio { get; set; }
        public decimal taxAmount { get; set; }
        public decimal totalAmount { get; set; }
        public DateTime? pickupDateTime { get; set; }
        public DateTime? dropoffDateTime { get; set; }
        public string pickupBranchName { get; set; }
        public string pickupBranchId { get; set; }
        public string dropoffBranchName { get; set; }
        public string dropoffBranchId { get; set; }
        public string ownerId { get; set; }
        public string ownerName { get; set; }
        public string customerId { get; set; }
        public string customerName { get; set; }
        public int itemTypeCode { get; set; }
        public string contractNumber { get; set; }
        public string pnrNumber { get; set; }
        public int contractType { get; set; }
        public int paymentChoice { get; set; }
        public int contractChannel { get; set; }
        public decimal depositAmount { get; set; }
        public string trackingNumber { get; set; }
        public decimal overKilometerPrice { get; set; }
        public decimal kilometerLimit { get; set; }
        public string corporateCustomerId { get; set; }
        public Guid? campaignId { get; set; }
        public string campaignName { get; set; }
        public int paymentMethod { get; set; }
        public int billingType { get; set; }
        public bool processIndividualPrices { get; set; }
        public DateTime? pickupDateTime_Header { get; set; }
        public long? pickupDateTimeStamp_Header { get; set; }
        public bool changeCurrency { get; set; }
        public decimal exchangeRate { get; set; }
        public bool isMonthly { get; set; }
        public int monthValue { get; set; }
        public bool isClosedAmountZero { get; set; }
        public List<PaymentPlanData> paymentPlans { get; set; }
    }
}
