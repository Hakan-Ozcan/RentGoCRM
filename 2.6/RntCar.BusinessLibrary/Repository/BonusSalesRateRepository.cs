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

namespace RntCar.BusinessLibrary
{
    public class BonusSalesRateRepository : RepositoryHandler
    {
        public BonusSalesRateRepository(IOrganizationService Service) : base(Service)
        {
        }

        public BonusSalesRateRepository(CrmServiceClient _crmServiceClient) : base(_crmServiceClient)
        {
        }

        public BonusSalesRateRepository(IOrganizationService Service, CrmServiceClient _crmServiceClient) : base(Service, _crmServiceClient)
        {
        }

        public List<Entity> getBonusSalesRates()
        {
            QueryExpression queryExpression = new QueryExpression("rnt_bonussalesrate");
            queryExpression.ColumnSet = new ColumnSet(true);
            queryExpression.Criteria.AddCondition("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active);

            return this.retrieveMultiple(queryExpression).Entities.ToList();
        }
    }
}
