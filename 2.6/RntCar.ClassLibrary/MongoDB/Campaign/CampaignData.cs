using System;
using System.Collections.Generic;

namespace RntCar.ClassLibrary.MongoDB
{
    public class CampaignData 
    {
        public string campaignId { get; set; }
        public string name { get; set; }
        public int productType { get; set; }
        public int campaignType { get; set; }
        public string description { get; set; }
        public int priceEffect { get; set; }
        public DateTime? beginingDate { get; set; }
        public DateTime? endDate { get; set; }
        public int minReservationDay { get; set; }
        public int maxReservationDay { get; set; }
        public string reservationTypeCode { get; set; }
        public string reservatinChannelCode { get; set; }
        public string branchCode { get; set; }
        public string groupCode { get; set; }
        public string additionalProductCode { get; set; }
        public decimal payNowDailyPrice { get; set; }
        public decimal payLaterDailyPrice { get; set; }
        public decimal additionalProductDailyPrice { get; set; }
        public decimal payNowDiscountRatio { get; set; }
        public decimal payLaterDiscountRatio { get; set; }
        public decimal additionalProductDiscountRatio { get; set; }
        public DateTime createdon { get; set; }
        public DateTime modifiedon { get; set; }
        public string createdby { get; set; }
        public string modifiedby { get; set; }
        public int statecode { get; set; }
        public int statuscode { get; set; }
        public List<CampaignGroupCodePrices> groupCodePrices { get; set; }
    }

    public class CampaignGroupCodePrices
    {
        public string groupcodeId { get; set; }
        public string groupcodeName { get; set; }
        public decimal paynowPrice { get; set; }
        public decimal paylaterPrice { get; set; }
    }

}
