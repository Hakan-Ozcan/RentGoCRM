using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Tooling.Connector;
using RntCar.BusinessLibrary.Handlers;
using RntCar.ClassLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.BusinessLibrary.Repository
{
    public class DamageDocumentRepository : RepositoryHandler
    {
        public DamageDocumentRepository(IOrganizationService Service) : base(Service)
        {
        }

        public DamageDocumentRepository(CrmServiceClient _crmServiceClient) : base(_crmServiceClient)
        {
        }

        public DamageDocumentRepository(IOrganizationService Service, CrmServiceClient _crmServiceClient) : base(Service, _crmServiceClient)
        {
        }

        public List<Entity> getAllDamageDocuments()
        {
            QueryExpression query = new QueryExpression("rnt_damagedocument");
            query.ColumnSet = new ColumnSet(true);
            query.Criteria.AddCondition("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active);
            return this.retrieveMultiple(query).Entities.ToList();
        }
    }
}
