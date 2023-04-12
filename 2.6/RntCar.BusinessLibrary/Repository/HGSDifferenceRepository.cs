using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using RntCar.BusinessLibrary.Handlers;
using System.Collections.Generic;
using System.Linq;

namespace RntCar.BusinessLibrary.Repository
{
    public class HGSDifferenceRepository : RepositoryHandler
    {
        public HGSDifferenceRepository(IOrganizationService Service) : base(Service)
        {
        }

        public List<Entity> getDraftHGSDifference()
        {
            QueryExpression queryExpression = new QueryExpression("rnt_hgsdifference");
            queryExpression.Criteria.AddCondition("statuscode", ConditionOperator.Equal, 1);
            queryExpression.ColumnSet = new ColumnSet(true);
            return this.retrieveMultiple(queryExpression).Entities.ToList();
        }
    }
}
