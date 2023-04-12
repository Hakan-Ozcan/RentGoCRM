using RntCar.ClassLibrary;
using RntCar.ClassLibrary._Mobile;
using RntCar.ClassLibrary._Web;
using RntCar.SDK.Common;
using System.Collections.Generic;
using System.Linq;

namespace RntCar.SDK.Mappers
{
    public class BannerMapper
    {
        public List<BannerData_Web> buildWebBannerData(List<BannerData> bannerData)
        {
            return bannerData.ConvertAll(p => new BannerData_Web().Map(p)).ToList();
        }

        public List<BannerData_Mobile> buildMobileBannerData(List<BannerData> bannerData)
        {
            return bannerData.ConvertAll(p => new BannerData_Mobile().Map(p)).ToList();
        }
    }
}
