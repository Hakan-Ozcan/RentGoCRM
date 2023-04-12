using RntCar.MongoDBHelper.Price.Interfaces;
using RntCar.SDK.Common;

namespace RntCar.MongoDBHelper.Price.Factors
{
    public class AvailabilityFactor : PriceCalculator
    {
        private IPrices _prices;

        public AvailabilityFactor(Prices prices)
        {
            _prices = prices;
        }
        public IPrices calculate(long priceDateTimeStamp, decimal basePrice, decimal availabilityRatio)
        {
            this._prices.basePrice = basePrice;
            this._prices.priceDateTimeStamp = priceDateTimeStamp;
            this._prices.priceDate = priceDateTimeStamp.converttoDateTime();

            var priceAfterAvailibillity = basePrice + (basePrice * availabilityRatio) / 100;
            this._prices.priceAfterAvailabilityFactor = priceAfterAvailibillity;
            this._prices.totalPrice = this._prices.priceAfterAvailabilityFactor;
            return _prices;
        }
    }
}
