using RntCar.ClassLibrary;
using RntCar.ClassLibrary._Broker;
using RntCar.ClassLibrary._Mobile;
using RntCar.ClassLibrary._Web;
using RntCar.SDK.Common;
using System.Collections.Generic;
using System.Linq;

namespace RntCar.SDK.Mappers
{
    public class ShowRoomProductMapper
    {
        public List<ShowRoomProduct_Web> createWebShowRoomProductList(List<ShowRoomProductData> showRoomProductDatas)
        {
            return showRoomProductDatas.ConvertAll(item => new ShowRoomProduct_Web().Map(item)).ToList();

        }

        public List<ShowRoomProduct_Mobile> createMobileShowRoomProductList(List<ShowRoomProductData> showRoomProductDatas)
        {
            return showRoomProductDatas.ConvertAll(item => new ShowRoomProduct_Mobile().Map(item)).ToList();
        }
    }
}
