using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using RntCar.BusinessLibrary.Handlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.BusinessLibrary.Repository
{
    public class AutoNumberRepository : RepositoryHandler
    {
        public AutoNumberRepository(IOrganizationService Service) : base(Service)
        {
        }

        public AutoNumberRepository(IOrganizationService Service, Guid UserId) : base(Service, UserId)
        {
        }

        public AutoNumberRepository(IOrganizationService Service, Guid UserId, string OrganizationName) : base(Service, UserId, OrganizationName)
        {
        }
        public EntityCollection GetAutoNumberDefinitions4GivenEntityAsIfPublished(Entity masterEntity)
        {
            //just publishedones
            QueryExpression expression = new QueryExpression("rnt_autonumber");
            expression.ColumnSet = new ColumnSet(true);
            expression.Criteria.AddCondition(new ConditionExpression("rnt_entity", ConditionOperator.Equal, masterEntity.LogicalName));
            expression.Criteria.AddCondition(new ConditionExpression("statuscode", ConditionOperator.Equal, 100000000));
            return this.Service.RetrieveMultiple(expression);
        }
    }
}
