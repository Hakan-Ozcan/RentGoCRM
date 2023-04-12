

using RntCar.ClassLibrary.MongoDB;

namespace RntCar.ClassLibrary
{
    // Tolga AYKURT - 27.02.2019
    public class CalculatedCampaignPrice
    {
        public CampaignData CampaignInfo { get; set; }

        public decimal? payNowDailyPrice { get; set; }

        public decimal? payLaterDailyPrice { get; set; }
    }
}
