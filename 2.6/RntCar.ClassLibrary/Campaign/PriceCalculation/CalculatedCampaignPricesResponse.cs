using System.Collections.Generic;

namespace RntCar.ClassLibrary
{
    // Tolga AYKURT - 28.02.2019
    public class CalculatedCampaignPricesResponse : ResponseBase
    {
        public CalculatedCampaignPricesResponse()
        {
            this.ResponseResult = new ResponseResult();
        }


        public List<CalculatedCampaignPrice> CalculatedCampaignPrices { get; set; }
    }
}
