using System.Collections.Generic;

namespace RntCar.ClassLibrary._Web
{
    public class GetBannersResponse_Web : ResponseBase
    {
        public List<BannerData_Web> bannerDatas { get; set; }
    }
}
