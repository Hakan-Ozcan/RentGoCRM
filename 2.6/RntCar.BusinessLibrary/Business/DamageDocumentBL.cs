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
    public class DamageDocumentBL : BusinessHandler
    {
        public DamageDocumentBL(IOrganizationService orgService) : base(orgService)
        {
        }

        public DamageDocumentBL(CrmServiceClient crmServiceClient) : base(crmServiceClient)
        {
        }

        public DamageDocumentBL(IOrganizationService orgService, CrmServiceClient crmServiceClient) : base(orgService, crmServiceClient)
        {
        }

        public DamageDocumentBL(IOrganizationService orgService, ITracingService tracingService) : base(orgService, tracingService)
        {
        }

        public List<DamageDocumentData> getDamageDocuments()
        {
            DamageDocumentRepository damageDocumentRepository = new DamageDocumentRepository(this.OrgService, this.CrmServiceClient);
            var damageDocumentData = damageDocumentRepository.getAllDamageDocuments();
            List<DamageDocumentData> damageDocuments = new List<DamageDocumentData>();
            foreach (var item in damageDocumentData)
            {
                damageDocuments.Add(new DamageDocumentData
                {
                    damageDocumentId = item.Id,
                    damageDocumentName = item.GetAttributeValue<string>("rnt_name"),
                    damageDocumentType = item.GetAttributeValue<OptionSetValue>("rnt_documenttype").Value
                });
            }
            return damageDocuments;
        }
    }
}
