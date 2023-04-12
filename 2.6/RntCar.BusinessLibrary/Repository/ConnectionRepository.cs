using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using RntCar.BusinessLibrary.Handlers;
using RntCar.ClassLibrary;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RntCar.BusinessLibrary.Repository
{
    public class ConnectionRepository : RepositoryHandler
    {
        public ConnectionRepository(IOrganizationService Service) : base(Service)
        {
        }

        public ConnectionRepository(IOrganizationService Service, Guid UserId) : base(Service, UserId)
        {
        }

        public ConnectionRepository(IOrganizationService Service, Guid UserId, string OrganizationName) : base(Service, UserId, OrganizationName)
        {
        }

        public List<Entity> getConnectionsByIndividualCustomerId(Guid individualCustomerId)
        {
            QueryExpression expression = new QueryExpression("rnt_connection");
            expression.ColumnSet = new ColumnSet(true);
            expression.Criteria.AddCondition("rnt_contactid", ConditionOperator.Equal, individualCustomerId);
            expression.Criteria.AddCondition("rnt_accountid", ConditionOperator.NotNull);
            expression.Criteria.AddCondition("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active);
            return this.Service.RetrieveMultiple(expression).Entities.ToList();
        }

        public List<Entity> getConnectionsByAccountId(Guid accountId)
        {
            QueryExpression expression = new QueryExpression("rnt_connection");
            expression.ColumnSet = new ColumnSet(true);
            expression.Criteria.AddCondition("rnt_accountid", ConditionOperator.Equal, accountId);
            expression.Criteria.AddCondition("rnt_contactid", ConditionOperator.NotNull);
            expression.Criteria.AddCondition("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active);

            return this.Service.RetrieveMultiple(expression).Entities.ToList();
        }

        public Entity getConnectionByGivenCriterias(Guid accountId, Guid contactId, int? relationCode)
        {
            QueryExpression expression = new QueryExpression("rnt_connection");
            expression.ColumnSet = new ColumnSet("rnt_connectionid");
            if (relationCode.HasValue)
            {
                expression.Criteria.AddCondition("rnt_relationcode", ConditionOperator.Equal, relationCode);
            }
            expression.Criteria.AddCondition("rnt_accountid", ConditionOperator.Equal, accountId);
            expression.Criteria.AddCondition("rnt_contactid", ConditionOperator.Equal, contactId);
            expression.Criteria.AddCondition("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active);

            return this.Service.RetrieveMultiple(expression).Entities.FirstOrDefault();
        }
    }
}
