using System.Collections.Generic;

namespace RntCar.ClassLibrary
{
    public class CreateCampaignPricesResponse : ResponseBase
    {
        public List<CalculatedCampaignPrice>  calculatedCampaignPrices { get; set; }
    }
}
