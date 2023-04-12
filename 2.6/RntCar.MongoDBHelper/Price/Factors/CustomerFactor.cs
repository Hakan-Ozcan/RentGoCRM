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
    public class CustomerFactor : PriceCalculator
    {
        private IPrices _prices;

        public CustomerFactor(Prices prices)
        {
            _prices = prices;
        }
        public IPrices calculate(long priceDateTimeStamp,
                                decimal basePrice,
                                decimal totalPrice,
                                int? segment,
                                string pickupBranchId,
                                string groupCodeInformation,
                                int customerType,
                                string accountGroup)
        {
            this._prices.basePrice = basePrice;
            this._prices.priceDateTimeStamp = priceDateTimeStamp;
            this._prices.priceDate = priceDateTimeStamp.converttoDateTime();

            if (segment != null)
            {
                PriceFactorRepository priceFactorRepository = new PriceFactorRepository(this.mongoHostName, this.mongoDatabaseName);
                var res = priceFactorRepository.getPriceFactorByPriceDateandTypeandCustomer(priceDateTimeStamp,
                                                                                            ClassLibrary.Enums.PriceFactorEnums.PriceFactorType.Customer,
                                                                                            segment,
                                                                                            pickupBranchId,
                                                                                            groupCodeInformation,
                                                                                            customerType,
                                                                                            accountGroup);



                if (res != null)
                {
                    //always apply ratio value to base price
                    this._prices.priceAfterCustomerFactor = totalPrice + (this._prices.basePrice * res.value) / 100;
                }
                else
                {
                    this._prices.priceAfterCustomerFactor = totalPrice;
                }
            }
            else
            {
                this._prices.priceAfterCustomerFactor = totalPrice;
            }
            this._prices.totalPrice = this._prices.priceAfterCustomerFactor;
            return _prices;
        }
    }
}
