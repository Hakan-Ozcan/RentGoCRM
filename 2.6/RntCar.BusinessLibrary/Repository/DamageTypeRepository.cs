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
    public class DamageTypeRepository : RepositoryHandler
    {
        public DamageTypeRepository(IOrganizationService Service) : base(Service)
        {
        }

        public DamageTypeRepository(CrmServiceClient _crmServiceClient) : base(_crmServiceClient)
        {
        }

        public DamageTypeRepository(IOrganizationService Service, CrmServiceClient _crmServiceClient) : base(Service, _crmServiceClient)
        {
        }

        public List<Entity> getAllDamageTypes()
        {
            QueryExpression query = new QueryExpression("rnt_damagetype");
            query.ColumnSet = new ColumnSet(true);
            query.Criteria.AddCondition("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active);
            return this.retrieveMultiple(query).Entities.ToList();
        }
    }
}
