using System;

namespace RntCar.MongoDBHelper.Model
{
    public class PriceResponse
    {
        public decimal totalAmount { get; set; }
        public decimal baseAmount { get; set; }
        public decimal payLaterAmount { get; set; }
        public decimal payNowAmount { get; set; }
        public decimal availibilityRatio { get; set; }
        public decimal priceAfterAvailabilityFactor { get; set; }
        public decimal priceAfterCustomerFactor { get; set; }
        public decimal priceAfterChannelFactor { get; set; }
        public decimal priceAfterSpecialDaysFactor { get; set; }
        public decimal priceAfterWeekDaysFactor { get; set; }
        public decimal priceAfterBranchFactor { get; set; }
        public decimal priceAfterBranch2Factor { get; set; }
        public decimal priceAfterEqualityFactor { get; set; }        
        public decimal priceAfterPayMethodPayLater { get; set; }        
        public decimal priceAfterPayMethodPayNow { get; set; }        
        public Guid? campaignId { get; set; }
        public DateTime relatedDay { get; set; }
        public AvailabilityPriceListDataMongoDB selectedAvailabilityPriceList { get; set; }
        public GroupCodeListPriceDataMongoDB selectedGroupCodeListPrice { get; set; }
        public PriceListDataMongoDB selectedPriceList { get; set; }
        public bool isTickDay { get; set; }
        public string currencyId { get; set; }
        public string currencyCode { get; set; }
        //bu alanlar tick'in tick'ini hesaplarken kullanılır
        public decimal payNowWithoutTickDayAmount { get; set; }
        public decimal payLaterWithoutTickDayAmount { get; set; }
        public static PriceResponse notCalculated()
        {
            return new PriceResponse
            {
                baseAmount = decimal.MinValue,
                totalAmount = decimal.MinValue,
                payLaterAmount = decimal.MinValue,
                payNowAmount = decimal.MinValue
            };
        }
    }
}
