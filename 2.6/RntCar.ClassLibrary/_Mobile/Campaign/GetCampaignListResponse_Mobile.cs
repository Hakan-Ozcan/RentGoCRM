using System.Collections.Generic;

namespace RntCar.ClassLibrary._Mobile
{
    public class GetCampaignListResponse_Mobile : ResponseBase
    {
        public List<CampaignData_Mobile> campaigns { get; set; }
    }
}
