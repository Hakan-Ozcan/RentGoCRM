using RntCar.ClassLibrary.odata;
using System.Collections.Generic;

namespace RntCar.ClassLibrary
{
    public class GetPriceCalculationSummariesResponse : ResponseBase
    {
        public List<DailyPrice> dailyPrices { get; set; }
    }

    public class GetPriceCalculationSummariesReelResponse : ResponseBase
    {
        public List<PriceCalculationSummaryDataReel> priceCalculationSummaries { get; set; }
    }
}
