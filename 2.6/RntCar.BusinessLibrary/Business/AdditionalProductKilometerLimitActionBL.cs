using Microsoft.Xrm.Sdk;
using RntCar.BusinessLibrary.Handlers;
using RntCar.BusinessLibrary.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.BusinessLibrary.Business
{
    public class AdditionalProductKilometerLimitActionBL : BusinessHandler
    {
        public AdditionalProductKilometerLimitActionBL(IOrganizationService orgService) : base(orgService)
        {
        }

        public AdditionalProductKilometerLimitActionBL(IOrganizationService orgService, ITracingService tracingService) : base(orgService, tracingService)
        {

        }

        public int getAdditionalProductKilometerLimitActionByAdditionalProductId(Guid additionalProductId)//Bu kod, verilen bir ek ürün ID'sine göre, ilgili ek ürüne bağlı olan kilometre limit etkisini getiren bir işlevi içerir. Kodun satır satır açıklaması aşağıdaki gibidir:
        {
            var kilometer = 0;//Bir "kilometer" değişkeni oluşturulur ve başlangıçta 0 değeri atanır.

            AdditionalProductKilometerLimitActionRepository additionalProductKilometerLimitActionRepository = new AdditionalProductKilometerLimitActionRepository(this.OrgService);//"AdditionalProductKilometerLimitActionRepository" sınıfından bir nesne oluşturulur ve "this.OrgService" ile başlatılır.
            var kilometerEffect = additionalProductKilometerLimitActionRepository.getAdditionalProductKilometerLimitActionByAdditionalProductId(additionalProductId);//Verilen ek ürün ID'sine göre, ilgili ek ürüne bağlı olan kilometre limit etkisini "getAdditionalProductKilometerLimitActionByAdditionalProductId" işlevi kullanılarak alırız.
            if (kilometerEffect != null)
            {
                ////Eğer bir kilometre etkisi varsa, "kilometer" değişkeni, "rnt_kilometerlimiteffect" özniteliğini içeriyorsa, ilgili değeri alır; Aksi takdirde 0 olarak kalır.
                kilometer = kilometerEffect.Attributes.Contains("rnt_kilometerlimiteffect") ? kilometerEffect.GetAttributeValue<int>("rnt_kilometerlimiteffect") : 0;
            }
            return kilometer;
        }
    }
}
