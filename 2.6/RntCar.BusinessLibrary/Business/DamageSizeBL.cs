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
    public class DamageSizeBL : BusinessHandler
    {
        public DamageSizeBL(IOrganizationService orgService) : base(orgService)
        {
        }

        public DamageSizeBL(CrmServiceClient crmServiceClient) : base(crmServiceClient)
        {
        }

        public DamageSizeBL(IOrganizationService orgService, CrmServiceClient crmServiceClient) : base(orgService, crmServiceClient)
        {
        }

        public List<DamageSizeData> getDamageSizes()
        {
            DamageSizeRepository damageSizeRepository = new DamageSizeRepository(this.OrgService, this.CrmServiceClient);
            var damageSizeData = damageSizeRepository.getAllDamageSizes();
            List<DamageSizeData> damageSizes = new List<DamageSizeData>();
            foreach (var item in damageSizeData)
            {
                damageSizes.Add(new DamageSizeData
                {
                    damageSizeId = item.Id,
                    damageSizeName = item.GetAttributeValue<string>("rnt_name")
                });
            }
            return damageSizes;
        }
    }
}
