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
    public class InventoryRepository : RepositoryHandler
    {
        public InventoryRepository(IOrganizationService Service) : base(Service)
        {
        }

        public InventoryRepository(CrmServiceClient _crmServiceClient) : base(_crmServiceClient)
        {
        }

        public InventoryRepository(IOrganizationService Service, Guid UserId) : base(Service, UserId)
        {
        }

        public InventoryRepository(IOrganizationService Service, CrmServiceClient _crmServiceClient) : base(Service, _crmServiceClient)
        {
        }

        public InventoryRepository(IOrganizationService Service, Guid UserId, string OrganizationName) : base(Service, UserId, OrganizationName)
        {
        }

        public List<Entity> getAllInventories()
        {
            QueryExpression queryExpression = new QueryExpression("rnt_inventory");
            queryExpression.ColumnSet = new ColumnSet(true);
            queryExpression.Criteria.AddCondition("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active);
            return this.retrieveMultiple(queryExpression).Entities.ToList();

        }
    }
}
