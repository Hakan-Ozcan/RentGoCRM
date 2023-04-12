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
    public class OneWayFeeRepository : RepositoryHandler
    {
        public OneWayFeeRepository(IOrganizationService Service) : base(Service)
        {
        }

        public OneWayFeeRepository(CrmServiceClient _crmServiceClient) : base(_crmServiceClient)
        {
        }

        public OneWayFeeRepository(IOrganizationService Service, Guid UserId) : base(Service, UserId)
        {
        }

        public OneWayFeeRepository(IOrganizationService Service, CrmServiceClient _crmServiceClient) : base(Service, _crmServiceClient)
        {
        }

        public OneWayFeeRepository(IOrganizationService Service, Guid UserId, string OrganizationName) : base(Service, UserId, OrganizationName)
        {
        }
        public Entity getOneWayFeeByPickupandDropoffBranch(Guid pickupBranchId , Guid dropoffBranchId)
        {
            QueryExpression expression = new QueryExpression("rnt_onewayfee");
            expression.ColumnSet = new ColumnSet("rnt_price");
            expression.Criteria.AddCondition(new ConditionExpression("rnt_dropoffbranchid", ConditionOperator.Equal, dropoffBranchId));
            expression.Criteria.AddCondition(new ConditionExpression("rnt_pickupbranchid", ConditionOperator.Equal, pickupBranchId));
            expression.Criteria.AddCondition(new ConditionExpression("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active));
            return this.retrieveMultiple(expression).Entities.FirstOrDefault();
        }
        public List<Entity> getOneWayFees()
        {
            QueryExpression expression = new QueryExpression("rnt_onewayfee");
            expression.ColumnSet = new ColumnSet(true);            
            return this.retrieveMultiple(expression).Entities.ToList();
        }
    }
}
