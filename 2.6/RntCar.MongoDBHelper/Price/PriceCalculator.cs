using RntCar.MongoDBHelper.Price.Abstract;
using RntCar.MongoDBHelper.Price.Factors;
using RntCar.MongoDBHelper.Price.Interfaces;
using System;

namespace RntCar.MongoDBHelper.Price
{
    public class PriceCalculator : PriceBuilderBase
    {
        public IPrices Prices;

        public PriceCalculator()
        {
            Prices = Prices ?? new Prices();
        }
        public PriceCalculator(Guid priceListId, string priceListName)
        {
            Prices = Prices ?? new Prices
            {
                priceListId = priceListId,
                priceListIdName = priceListName
            };
        }

        protected override IPrices applyAvailabilityFactorBase(long priceDateTimeStamp, decimal basePrice, decimal availabilityRatio)
        {
            return new AvailabilityFactor((Prices)this.Prices).calculate(priceDateTimeStamp, basePrice, availabilityRatio);
        }

        protected override IPrices applyBranchBase(long priceDateTimeStamp, decimal basePrice, decimal totalPrice, string pickupBranchId, string groupCodes, int customerType, string accountGroup)
        {
            return new BranchFactor((Prices)this.Prices).calculate(priceDateTimeStamp, basePrice, totalPrice, pickupBranchId, groupCodes, customerType, accountGroup);
        }
        protected override IPrices applyBranch2Base(long priceDateTimeStamp, decimal basePrice, decimal totalPrice, string pickupBranchId, string groupCodes, int customerType, string accountGroup)
        {
            return new BranchFactor2((Prices)this.Prices).calculate(priceDateTimeStamp, basePrice, totalPrice, pickupBranchId, groupCodes, customerType, accountGroup);
        }

        protected override IPrices applyChannelBase(long priceDateTimeStamp, decimal basePrice, decimal totalPrice, int channelType, string pickupBranchId, string groupCodeInformation, int customerType, string accountGroup)
        {
            return new ChannelFactor((Prices)this.Prices).calculate(priceDateTimeStamp, basePrice, totalPrice, channelType, pickupBranchId, groupCodeInformation, customerType, accountGroup);
        }

        protected override IPrices applyCustomerBase(long priceDateTimeStamp, decimal basePrice, decimal totalPrice, int? segment, string pickupBranchId, string groupCodeInformation, int customerType, string accountGroup)
        {
            return new CustomerFactor((Prices)this.Prices).calculate(priceDateTimeStamp, basePrice, totalPrice, segment, pickupBranchId, groupCodeInformation, customerType, accountGroup);
        }

        protected override IPrices applyPayMethodBase(decimal totalPrice, string pickupBranchId, string groupCodeInformation, int customerType, string accountGroup)
        {
            return new PayMethodFactor((Prices)this.Prices).calculate(totalPrice, pickupBranchId, groupCodeInformation, customerType, accountGroup);
        }
        protected override IPrices applyPayMethodMonthlyBase(decimal totalPrice, string pickupBranchId, string groupCodeInformation, int customerType, string accountGroup)
        {
            return new PayMethodMonthlyFactor((Prices)this.Prices).calculate(totalPrice, pickupBranchId, groupCodeInformation, customerType, accountGroup);
        }

        protected override IPrices applySpecialDaysBase(long priceDateTimeStamp, decimal basePrice, decimal totalPrice, string pickupBranchId, string groupCodeInformation, int customerType, string accountGroup)
        {
            return new SpecialDayFactor((Prices)this.Prices).calculate(priceDateTimeStamp, basePrice, totalPrice, pickupBranchId, groupCodeInformation, customerType, accountGroup);
        }

        protected override IPrices applyWeekdaysBase(long priceDateTimeStamp, decimal basePrice, decimal totalPrice, string pickupBranchId, string groupCodeInformation, int customerType, string accountGroup)
        {
            return new WeekDaysFactor((Prices)this.Prices).calculate(priceDateTimeStamp, basePrice, totalPrice, pickupBranchId, groupCodeInformation, customerType, accountGroup);
        }

        protected override IPrices applyEqualityBase(long priceDateTimeStamp, decimal basePrice, decimal totalPrice, string pickupBranchId, string groupCodes, int customerType, string accountGroup)
        {
            return new EqualityFactor((Prices)this.Prices).calculate(priceDateTimeStamp, basePrice, totalPrice, pickupBranchId, groupCodes, customerType, accountGroup);
        }

    }
}
