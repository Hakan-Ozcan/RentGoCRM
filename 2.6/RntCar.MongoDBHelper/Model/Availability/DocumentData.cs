using MongoDB.Bson;
using RntCar.ClassLibrary.PaymentPlan;
using System;
using System.Collections.Generic;
using static RntCar.ClassLibrary.GlobalEnums;

namespace RntCar.MongoDBHelper.Model
{
    public class DocumentData
    {
        public ObjectId _id { get; set; }
        public BsonTimestamp pickupTimeStamp { get; set; }
        public BsonTimestamp dropoffTimeStamp { get; set; }
        public DocumentType type { get; set; }
        public string documentId { get; set; }
        public string documentItemId { get; set; }
        public string itemNo { get; set; }
        public int statuscode { get; set; }
        public int statecode { get; set; }
        public string equipmentName { get; set; }
        public string equipmentId { get; set; }
        public int? changeReason { get; set; }
        public string groupCodeInformationName { get; set; }
        public string groupCodeInformationId { get; set; }
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
        public int offset { get; set; }
        public string documentNumber { get; set; }
        public string pnrNumber { get; set; }
        public int documentType { get; set; }
        public int? paymentChoice { get; set; }
        public int? paymentMethod { get; set; }
        public int documentChannel { get; set; }
        public decimal depositAmount { get; set; }
        public string trackingNumber { get; set; }
        public string plateNumber { get; set; }
        public Guid? campaignId { get; set; }
        public bool processIndividualPrices { get; set; }
        public string currencyCode { get; set; }
        public bool isMonthly { get; set; }
        public int? monthValue { get; set; }
        public DateTime? pickupDateTime_Header { get; set; }
        public List<PaymentPlanData> paymentPlans { get; set; }
    }
}
