namespace RntCar.MongoDBHelper.Price.Interfaces
{
    public interface IPriceBuilder
    {
        IPrices applyAvailabilityFactor(long priceDateTimeStamp, decimal basePrice, decimal availabilityRatio);
        IPrices applyPayMethod(decimal totalPrice, string pickupBranchId, string groupCodeInformation, int customerType, string accountGroup);
        IPrices applyPayMethodMonthly(decimal totalPrice, string pickupBranchId, string groupCodeInformation, int customerType, string accountGroup);
        IPrices applyChannel(long priceDateTimeStamp, decimal basePrice, decimal totalPrice, int channelType, string pickupBranchId, string groupCodeInformation, int customerType, string accountGroup);
        IPrices applyWeekdays(long priceDateTimeStamp, decimal basePrice, decimal totalPrice, string pickupBranchId, string groupCodeInformation, int customerType, string accountGroup);
        IPrices applySpecialDays(long priceDateTimeStamp, decimal basePrice, decimal totalPrice, string pickupBranchId, string groupCodeInformation, int customerType, string accountGroup);
        IPrices applyCustomer(long priceDateTimeStamp, decimal basePrice, decimal totalPrice, int? segment, string pickupBranchId, string groupCodeInformation, int customerType, string accountGroup);
        IPrices applyBranch(long priceDateTimeStamp, decimal basePrice, decimal totalPrice, string pickupBranchId, string groupCodeInformation, int customerType, string accountGroup);

    }
}
