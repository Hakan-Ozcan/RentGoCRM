using RntCar.MongoDBHelper.Price.Interfaces;
using RntCar.MongoDBHelper.Repository;

namespace RntCar.MongoDBHelper.Price.Factors
{
    public class PayMethodMonthlyFactor : PriceCalculator
    {
        private IPrices _prices;
        public PayMethodMonthlyFactor(Prices prices)
        {
            _prices = prices;
        }
        public IPrices calculate(decimal totalPrice,
                                string pickupBranchId,
                                string groupCodeInformation,
                                int customerType,
                                string accountGroup)
        {

            PriceFactorRepository priceFactorRepository = new PriceFactorRepository(this.mongoHostName, this.mongoDatabaseName);
            var res = priceFactorRepository.getPriceFactorByFactor(ClassLibrary.Enums.PriceFactorEnums.PriceFactorType.PayMethodMonthly,
                                                                   pickupBranchId,
                                                                   groupCodeInformation,
                                                                   customerType,
                                                                   accountGroup);

            if (res != null)
            {
                //always apply ratio value to base price
                this._prices.payNowPrice = totalPrice - (res.value * totalPrice / 100);
            }
            else
            {
                this._prices.payNowPrice = totalPrice;
            }

            return _prices;
        }
    }
}
