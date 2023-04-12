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
    public class BranchTargetRevenueRepository : RepositoryHandler
    {
        public BranchTargetRevenueRepository(IOrganizationService Service) : base(Service)
        {
        }

        public BranchTargetRevenueRepository(CrmServiceClient _crmServiceClient) : base(_crmServiceClient)
        {
        }

        public BranchTargetRevenueRepository(IOrganizationService Service, CrmServiceClient _crmServiceClient) : base(Service, _crmServiceClient)
        {
        }

        public List<Entity> getBranchTargetRevenueByDate(DateTime startDate, DateTime endDate)
        {
            QueryExpression queryExpression = new QueryExpression("rnt_branchtargetrevenue");
            queryExpression.ColumnSet = new ColumnSet(true);
            queryExpression.Criteria.AddCondition("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active);
            queryExpression.Criteria.AddCondition("rnt_begindate", ConditionOperator.OnOrAfter, startDate);
            queryExpression.Criteria.AddCondition("rnt_enddate", ConditionOperator.OnOrAfter, endDate);

            return this.retrieveMultiple(queryExpression).Entities.ToList();
        }
    }
}
