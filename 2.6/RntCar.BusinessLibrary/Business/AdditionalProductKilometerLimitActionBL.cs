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

        public int getAdditionalProductKilometerLimitActionByAdditionalProductId(Guid additionalProductId)
        {
            var kilometer = 0;

            AdditionalProductKilometerLimitActionRepository additionalProductKilometerLimitActionRepository = new AdditionalProductKilometerLimitActionRepository(this.OrgService);
            var kilometerEffect = additionalProductKilometerLimitActionRepository.getAdditionalProductKilometerLimitActionByAdditionalProductId(additionalProductId);
            if(kilometerEffect != null)
            {
                kilometer = kilometerEffect.Attributes.Contains("rnt_kilometerlimiteffect") ? kilometerEffect.GetAttributeValue<int>("rnt_kilometerlimiteffect") : 0;
            }
            return kilometer;
        }
    }
}
