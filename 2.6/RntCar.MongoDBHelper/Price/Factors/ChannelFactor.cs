using RntCar.MongoDBHelper.Price.Interfaces;
using RntCar.MongoDBHelper.Repository;
using RntCar.SDK.Common;
using System;

namespace RntCar.MongoDBHelper.Price.Factors
{
    public class ChannelFactor : PriceCalculator
    {
        private IPrices _prices;

        public ChannelFactor(Prices prices)
        {
            _prices = prices;
        }
        public IPrices calculate(long priceDateTimeStamp, 
                                decimal basePrice, 
                                decimal totalPrice, 
                                int channelType , 
                                string pickupBranchId,
                                string groupCodeInformation,
                                int customerType,
                                string accountGroup)
        {
            PriceFactorRepository priceFactorRepository = new PriceFactorRepository(this.mongoHostName, this.mongoDatabaseName);
            var res = priceFactorRepository.getPriceFactorByPriceDateandTypeandChannelType(priceDateTimeStamp,
                                                                                           ClassLibrary.Enums.PriceFactorEnums.PriceFactorType.ReservationChannel,
                                                                                           Convert.ToString(channelType),
                                                                                           pickupBranchId,
                                                                                           groupCodeInformation,
                                                                                           customerType,
                                                                                           accountGroup);

            this._prices.basePrice = basePrice;
            this._prices.priceDateTimeStamp = priceDateTimeStamp;
            this._prices.priceDate = priceDateTimeStamp.converttoDateTime();

            
            if (res != null)
            {
                //always apply ratio value to base price
                this._prices.priceAfterChannelFactor = totalPrice + (this._prices.basePrice * res.value) / 100; 
            }
            else
            {
                this._prices.priceAfterChannelFactor = totalPrice;
            }
            this._prices.totalPrice = this._prices.priceAfterChannelFactor;
            return _prices;
        }
    }
}
