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
    public class BonusCalculationLogRepository : RepositoryHandler
    {
        public BonusCalculationLogRepository(IOrganizationService Service) : base(Service)
        {
        }

        public BonusCalculationLogRepository(CrmServiceClient _crmServiceClient) : base(_crmServiceClient)
        {
        }

        public BonusCalculationLogRepository(IOrganizationService Service, CrmServiceClient _crmServiceClient) : base(Service, _crmServiceClient)
        {
        }
        public List<Entity> getBonusCalculationLogsByContractItem(Guid contractItemId)
        {
            QueryExpression queryExpression = new QueryExpression("rnt_bonuscalculationlog");
            queryExpression.Criteria.AddCondition("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active);
            queryExpression.Criteria.AddCondition("rnt_contractitemid", ConditionOperator.Equal, contractItemId);
            return this.retrieveMultiple(queryExpression).Entities.ToList();
        }
    }
}
