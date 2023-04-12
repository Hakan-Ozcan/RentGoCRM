using RntCar.MongoDBHelper.Price.Interfaces;
using System;

namespace RntCar.MongoDBHelper.Price
{
    public class Prices : IPrices
    {
        public Prices()
        {
        }
        public decimal basePrice { get; set; }
        public decimal priceAfterAvailabilityFactor { get; set; }
        public decimal priceAfterChannelFactor { get; set; }
        public decimal priceAfterWeekDaysFactor { get; set; }
        public decimal priceAfterSpecialDaysFactor { get; set; }
        public decimal priceAfterCustomerFactor { get; set; }
        public decimal priceAfterBranchFactor { get; set; }
        public decimal priceAfterBranch2Factor { get; set; }
        public decimal payNowPrice { get; set; }
        public decimal payLaterPrice { get; set; }
        public Guid priceListId { get; set; }
        public string priceListIdName { get; set; }
        public DateTime priceDate { get; set; }
        public long priceDateTimeStamp { get; set; }
        public decimal totalPrice { get; set; } = decimal.Zero;
        public decimal priceAfterEqualityFactor { get; set; }
    }
}
