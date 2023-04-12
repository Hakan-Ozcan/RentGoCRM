using RntCar.MongoDBHelper.Price.Interfaces;
using RntCar.SDK.Common;
using System;

namespace RntCar.MongoDBHelper.Price.Abstract
{
    public abstract class PriceBuilderBase : IPriceBuilder
    {
        public string mongoHostName { get; set; } = StaticHelper.GetConfiguration("MongoDBHostName");
        public string mongoDatabaseName { get; set; } = StaticHelper.GetConfiguration("MongoDBDatabaseName");

        protected abstract IPrices applyChannelBase(long priceDateTimeStamp, decimal basePrice, decimal totalPrice, int channelType, string pickupBranchId, string groupCodeInformation, int customerType, string accountGroup);
        protected abstract IPrices applyAvailabilityFactorBase(long priceDateTimeStamp, decimal basePrice, decimal availabilityRatio);
        protected abstract IPrices applyCustomerBase(long priceDateTimeStamp, decimal basePrice, decimal totalPrice, int? segment, string pickupBranchId, string groupCodeInformation, int customerType, string accountGroup);
        protected abstract IPrices applySpecialDaysBase(long priceDateTimeStamp, decimal basePrice, decimal totalPrice, string pickupBranchId, string groupCodeInformation, int customerType, string accountGroup);
        protected abstract IPrices applyWeekdaysBase(long priceDateTimeStamp, decimal basePrice, decimal totalPrice, string pickupBranchId, string groupCodeInformation, int customerType, string accountGroup);
        protected abstract IPrices applyPayMethodBase(decimal totalPrice, string pickupBranchId, string groupCodeInformation, int customerType, string accountGroup);
        protected abstract IPrices applyPayMethodMonthlyBase(decimal totalPrice, string pickupBranchId, string groupCodeInformation, int customerType, string accountGroup);
        protected abstract IPrices applyBranchBase(long priceDateTimeStamp, decimal basePrice, decimal totalPrice, string pickupBranchId, string groupCodeInformation, int customerType, string accountGroup);
        protected abstract IPrices applyBranch2Base(long priceDateTimeStamp, decimal basePrice, decimal totalPrice, string pickupBranchId, string groupCodeInformation, int customerType, string accountGroup);
        protected abstract IPrices applyEqualityBase(long priceDateTimeStamp, decimal basePrice, decimal totalPrice, string pickupBranchId, string groupCodeInformation, int customerType, string accountGroup);



        public IPrices applyChannel(long priceDateTimeStamp, decimal basePrice, decimal totalPrice, int channelType, string pickupBranchId, string groupCodeInformation, int customerType, string accountGroup)
        {
            return this.applyChannelBase(priceDateTimeStamp, basePrice, totalPrice, channelType, pickupBranchId, groupCodeInformation, customerType, accountGroup);
        }

        public IPrices applyCustomer(long priceDateTimeStamp, decimal basePrice, decimal totalPrice, int? segment, string pickupBranchId, string groupCodeInformation, int customerType, string accountGroup)
        {
            return this.applyCustomerBase(priceDateTimeStamp, basePrice, totalPrice, segment, pickupBranchId, groupCodeInformation, customerType, accountGroup);
        }

        public IPrices applySpecialDays(long priceDateTimeStamp, decimal basePrice, decimal totalPrice, string pickupBranchId, string groupCodeInformation, int customerType, string accountGroup)
        {
            return this.applySpecialDaysBase(priceDateTimeStamp, basePrice, totalPrice, pickupBranchId, groupCodeInformation, customerType, accountGroup);
        }

        public IPrices applyWeekdays(long priceDateTimeStamp, decimal basePrice, decimal totalPrice, string pickupBranchId, string groupCodeInformation, int customerType, string accountGroup)
        {
            return this.applyWeekdaysBase(priceDateTimeStamp, basePrice, totalPrice, pickupBranchId, groupCodeInformation, customerType, accountGroup);
        }

        public IPrices applyPayMethod(decimal totalPrice, string pickupBranchId, string groupCodeInformation, int customerType, string accountGroup)
        {
            return this.applyPayMethodBase(totalPrice, pickupBranchId, groupCodeInformation, customerType, accountGroup);
        }
        public IPrices applyPayMethodMonthly(decimal totalPrice, string pickupBranchId, string groupCodeInformation, int customerType, string accountGroup)
        {
            return this.applyPayMethodMonthlyBase(totalPrice, pickupBranchId, groupCodeInformation, customerType, accountGroup);
        }
        public IPrices applyAvailabilityFactor(long priceDateTimeStamp, decimal basePrice, decimal availabilityRatio)
        {
            return this.applyAvailabilityFactorBase(priceDateTimeStamp, basePrice, availabilityRatio);
        }

        public IPrices applyBranch(long priceDateTimeStamp, decimal basePrice, decimal totalPrice, string pickupBranchId, string groupCodeInformation, int customerType, string accountGroup)
        {
            return this.applyBranchBase(priceDateTimeStamp, basePrice, totalPrice, pickupBranchId, groupCodeInformation, customerType, accountGroup);
        }
        public IPrices applyBranch2(long priceDateTimeStamp, decimal basePrice, decimal totalPrice, string pickupBranchId, string groupCodeInformation, int customerType, string accountGroup)
        {
            return this.applyBranch2Base(priceDateTimeStamp, basePrice, totalPrice, pickupBranchId, groupCodeInformation, customerType, accountGroup);
        }
        public IPrices applyEquality(long priceDateTimeStamp, decimal basePrice, decimal totalPrice, string pickupBranchId, string groupCodeInformation, int customerType, string accountGroup)
        {
            return this.applyEqualityBase(priceDateTimeStamp, basePrice, totalPrice, pickupBranchId, groupCodeInformation, customerType, accountGroup);
        }

        
    }
}
