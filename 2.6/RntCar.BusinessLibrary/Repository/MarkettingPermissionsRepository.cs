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
    public class MarkettingPermissionsRepository : RepositoryHandler
    {
        public MarkettingPermissionsRepository(IOrganizationService Service) : base(Service)
        {
        }

        public MarkettingPermissionsRepository(CrmServiceClient _crmServiceClient) : base(_crmServiceClient)
        {
        }

        public MarkettingPermissionsRepository(IOrganizationService Service, CrmServiceClient _crmServiceClient) : base(Service, _crmServiceClient)
        {
        }

        public Entity getMarkettingPermissionByContactId(Guid contactId)
        {
            QueryExpression queryExpression = new QueryExpression("rnt_marketingpermissions");
            queryExpression.ColumnSet = new ColumnSet(true);
            queryExpression.Criteria.AddCondition("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active);
            queryExpression.Criteria.AddCondition("rnt_contactid", ConditionOperator.Equal, contactId);
            return this.retrieveMultiple(queryExpression).Entities.FirstOrDefault();
        }
    }
}
