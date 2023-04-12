using RntCar.MongoDBHelper.Price.Interfaces;
using RntCar.MongoDBHelper.Repository;
using RntCar.SDK.Common;

namespace RntCar.MongoDBHelper.Price.Factors
{
    public class WeekDaysFactor : PriceCalculator
    {
        private IPrices _prices;

        public WeekDaysFactor(Prices prices)
        {
            _prices = prices;
        }
        public IPrices calculate(long priceDateTimeStamp,
                                decimal basePrice, 
                                decimal totalPrice, 
                                string pickupBranchId,
                                string groupCodeInformation,
                                int customerType,
                                string accountGroup)
        {
            PriceFactorRepository priceFactorRepository = new PriceFactorRepository(this.mongoHostName, this.mongoDatabaseName);
            var res = priceFactorRepository.getPriceFactorByPriceDateandTypeandWeekdays(priceDateTimeStamp, 
                                                                                        ClassLibrary.Enums.PriceFactorEnums.PriceFactorType.Weekdays, 
                                                                                        pickupBranchId,
                                                                                        groupCodeInformation,
                                                                                        customerType,
                                                                                        accountGroup);

            this._prices.basePrice = basePrice;
            this._prices.priceDateTimeStamp = priceDateTimeStamp;
            this._prices.priceDate = priceDateTimeStamp.converttoDateTime();

            if (res != null)
            {
                this._prices.priceAfterWeekDaysFactor = totalPrice + (this._prices.basePrice * res.value) / 100;
            }
            else
            {
                this._prices.priceAfterWeekDaysFactor = totalPrice;
            }

            this._prices.totalPrice = this._prices.priceAfterWeekDaysFactor;
            return _prices;
        }
    }
}
