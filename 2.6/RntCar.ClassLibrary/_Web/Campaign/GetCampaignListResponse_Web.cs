using System.Collections.Generic;

namespace RntCar.ClassLibrary._Web
{
    public class GetCampaignListResponse_Web : ResponseBase
    {
        public List<CampaignData_Web> campaigns { get; set; }
    }
}
