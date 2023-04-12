﻿using RntCar.ClassLibrary._Enums_1033;
using RntCar.MongoDBHelper.Price.Interfaces;
using RntCar.MongoDBHelper.Repository;
using RntCar.SDK.Common;
using System.Linq;

namespace RntCar.MongoDBHelper.Price.Factors
{
    public class BranchFactor : PriceCalculator
    {
        private IPrices _prices;

        public BranchFactor(Prices prices)
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
            var res = priceFactorRepository.getPriceFactorByDate(priceDateTimeStamp,
                                                                 (int)rnt_PriceFactorType.Branch,
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
                this._prices.priceAfterBranchFactor = totalPrice + (this._prices.basePrice * res.value) / 100;
            }
            else
            {
                this._prices.priceAfterBranchFactor = totalPrice;
            }

            this._prices.totalPrice = this._prices.priceAfterBranchFactor;
            return _prices;
        }
    }
}
