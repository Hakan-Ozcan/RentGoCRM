using RntCar.MongoDBHelper.Repository;
using RntCar.SDK.Common;
using System;

namespace RntCar.MongoDBHelper.Helper
{
    public class DurationHelper
    {
        public string mongoDBHostName { get; set; }
        public string mongoDbDatabaseName { get; set; }
        public DurationHelper(string _mongoDBHostName, string _mongoDbDatabaseName)
        {
            mongoDBHostName = _mongoDBHostName;
            mongoDbDatabaseName = _mongoDbDatabaseName;
        }
        public int calculateDocumentDurationByPriceHourEffect(DateTime pickupDateTime, DateTime dropoffDateTime)
        {
            var totalDuration = CommonHelper.calculateTotalDurationInMinutes(pickupDateTime, dropoffDateTime);
            if (totalDuration < 1440)
            {
                return CommonHelper.calculateTotalDurationInDaysCheckIfzero(pickupDateTime, dropoffDateTime);
            }

            var totalDays = CommonHelper.calculateTotalDurationInDays(pickupDateTime, dropoffDateTime);
            var precisionPart = totalDays - Math.Truncate(totalDays);

            PriceHourEffectRepository priceHourEffectRepository = new PriceHourEffectRepository(StaticHelper.GetConfiguration("MongoDBHostName"),
                                                                                                StaticHelper.GetConfiguration("MongoDBDatabaseName"));
            var priceHourEffect = priceHourEffectRepository.getZeroPriceEffect();
            var maximumRate = priceHourEffect.maximumMinute;
            //if precision part is bigger than zero rate , than we need to round up the day
            var maximumRateInDays = maximumRate / StaticHelper.dayDurationInMinutes;

            if (precisionPart > Convert.ToDouble(maximumRateInDays))
            {
                return (int)Math.Ceiling(totalDays);
            }

            return (int)Math.Floor(totalDays);
        }
        public decimal calculateDocumentDurationByPriceHourEffect_Decimal(DateTime pickupDateTime, DateTime dropoffDateTime)
        {
            var totalDuration = CommonHelper.calculateTotalDurationInMinutes(pickupDateTime, dropoffDateTime);
            if (totalDuration < 1440)
            {
                return CommonHelper.calculateTotalDurationInDaysCheckIfzero(pickupDateTime, dropoffDateTime);
            }

            var totalMinutes = CommonHelper.calculateTotalDurationInMinutes(pickupDateTime, dropoffDateTime);

            var quotient = Convert.ToInt32(Math.Floor(totalMinutes / 1440));
            var remainder = totalMinutes % 1440;

            PriceHourEffectRepository priceHourEffectRepository = new PriceHourEffectRepository(StaticHelper.GetConfiguration("MongoDBHostName"),
                                                                                                StaticHelper.GetConfiguration("MongoDBDatabaseName"));
            var priceHourEffect = priceHourEffectRepository.getPriceHourEffectByDuration(remainder);
            var decimalRemainder = decimal.Zero;
            if (priceHourEffect != null)
            {
                decimalRemainder = priceHourEffect.effectRate / 100M;
            }
            return quotient + decimalRemainder;
        }
        public bool checkPriceHourEffectIsBiggerEnoughHundredPercent(DateTime pickupDateTime, DateTime dropoffDateTime)
        {
            var totalDuration = CommonHelper.calculateTotalDurationInMinutes(pickupDateTime, dropoffDateTime);
           
            PriceHourEffectRepository priceHourEffectRepository = new PriceHourEffectRepository(StaticHelper.GetConfiguration("MongoDBHostName"),
                                                                                                StaticHelper.GetConfiguration("MongoDBDatabaseName"));
            var priceHourEffect = priceHourEffectRepository.getHundredPriceEffect();

            if(totalDuration >= priceHourEffect.minimumMinute)
            {
                return true;
            }
            return false;
        }
    }
}
