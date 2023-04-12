using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Tooling.Connector;
using RntCar.BusinessLibrary.Handlers;
using RntCar.BusinessLibrary.Repository;
using RntCar.ClassLibrary._Tablet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.BusinessLibrary.Business
{
    public class DamageTypesBL : BusinessHandler
    {
        public DamageTypesBL(IOrganizationService orgService) : base(orgService)
        {
        }

        public DamageTypesBL(CrmServiceClient crmServiceClient) : base(crmServiceClient)
        {
        }

        public DamageTypesBL(IOrganizationService orgService, ITracingService tracingService) : base(orgService, tracingService)
        {
        }

        public DamageTypesBL(IOrganizationService orgService, CrmServiceClient crmServiceClient) : base(orgService, crmServiceClient)
        {
        }

        public List<DamageTypeData> getDamageTypes()
        {
            DamageTypeRepository damageTypeRepository = new DamageTypeRepository(this.OrgService, this.CrmServiceClient);
            var damageTypeData = damageTypeRepository.getAllDamageTypes();
            List<DamageTypeData> damageTypes = new List<DamageTypeData>();
            foreach (var item in damageTypeData)
            {
                damageTypes.Add(new DamageTypeData
                {
                    damageTypeId = item.Id,
                    damageTypeName = item.GetAttributeValue<string>("rnt_name")
                });
            }
            return damageTypes;
        }
    }
}
