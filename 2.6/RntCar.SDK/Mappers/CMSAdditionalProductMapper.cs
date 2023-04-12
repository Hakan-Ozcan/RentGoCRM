using RntCar.ClassLibrary;
using RntCar.ClassLibrary._Broker;
using RntCar.ClassLibrary._Mobile;
using RntCar.ClassLibrary._Web;
using RntCar.SDK.Common;
using System.Collections.Generic;
using System.Linq;

namespace RntCar.SDK.Mappers
{
    public class CMSAdditionalProductMapper
    {
        public List<CMSAdditionalProductData_Web> buildCMSAdditionalProducts(List<CMSAdditionalProductData> cmsAdditionalProductDatas)
        {
            return cmsAdditionalProductDatas.ConvertAll(p => new CMSAdditionalProductData_Web().Map(p)).ToList();
        }

        public List<CMSAdditionalProductData_Mobile> buildCMSAdditionalProducts_Mobile(List<CMSAdditionalProductData> cmsAdditionalProductDatas)
        {
            return cmsAdditionalProductDatas.ConvertAll(p => new CMSAdditionalProductData_Mobile().Map(p)).ToList();
        }
        public List<CMSAdditionalProductData_Broker> buildCMSAdditionalProducts_Broker(List<CMSAdditionalProductData> cmsAdditionalProductDatas)
        {
            return cmsAdditionalProductDatas.ConvertAll(p => new CMSAdditionalProductData_Broker().Map(p)).ToList();
        }
    }
}
