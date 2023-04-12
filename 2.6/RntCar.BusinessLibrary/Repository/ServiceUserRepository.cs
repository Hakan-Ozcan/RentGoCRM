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
    public class ServiceUserRepository : RepositoryHandler
    {
        public ServiceUserRepository(IOrganizationService Service) : base(Service)
        {
        }

        public ServiceUserRepository(CrmServiceClient _crmServiceClient) : base(_crmServiceClient)
        {
        }

        public ServiceUserRepository(IOrganizationService Service, Guid UserId) : base(Service, UserId)
        {
        }

        public ServiceUserRepository(IOrganizationService Service, CrmServiceClient _crmServiceClient) : base(Service, _crmServiceClient)
        {
        }

        public ServiceUserRepository(IOrganizationService Service, Guid UserId, string OrganizationName) : base(Service, UserId, OrganizationName)
        {
        }

        public Entity getServiceUserByUserNameandPasswordByGivenColumns(string userName, string password,string[] columns)
        {
            QueryExpression queryExpression = new QueryExpression("rnt_serviceuser");
            queryExpression.ColumnSet = new ColumnSet(columns);
            queryExpression.Criteria.AddCondition("rnt_username",ConditionOperator.Equal, userName);
            queryExpression.Criteria.AddCondition("rnt_password", ConditionOperator.Equal, password);
            queryExpression.Criteria.AddCondition("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active);
            return this.retrieveMultiple(queryExpression).Entities.FirstOrDefault();

        }
        public Entity getServiceUserById(Guid userId)
        {
            return this.retrieveById("rnt_serviceuser", userId, true);
        }
    }
}
