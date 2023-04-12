using RntCar.MongoDBHelper.Price.Interfaces;
using RntCar.MongoDBHelper.Repository;
using RntCar.SDK.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.MongoDBHelper.Price.Factors
{
    public class PayMethodFactor : PriceCalculator
    {
        private IPrices _prices;
        public PayMethodFactor(Prices prices)
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
            var res = priceFactorRepository.getPriceFactorByFactor(ClassLibrary.Enums.PriceFactorEnums.PriceFactorType.PayMethod,
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
