using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Tooling.Connector;
using RntCar.BusinessLibrary.Handlers;
using RntCar.ClassLibrary;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RntCar.BusinessLibrary.Repository
{
    public class ConfigurationRepository : RepositoryHandler
    {
        public ConfigurationRepository(IOrganizationService Service) : base(Service)
        {
        }

        public ConfigurationRepository(CrmServiceClient _crmServiceClient) : base(_crmServiceClient)
        {
        }

        public ConfigurationRepository(IOrganizationService Service, Guid UserId) : base(Service, UserId)
        {
        }

        public ConfigurationRepository(IOrganizationService Service, CrmServiceClient _crmServiceClient) : base(Service, _crmServiceClient)
        {
        }

        public ConfigurationRepository(IOrganizationService Service, Guid UserId, string OrganizationName) : base(Service, UserId, OrganizationName)
        {
        }
        public String GetConfigurationByKey(string key)
        {
            QueryExpression expression = new QueryExpression("rnt_configuration");
            expression.ColumnSet = new ColumnSet(new string[] { "rnt_value" });
            expression.Criteria.AddCondition(new ConditionExpression("rnt_name", ConditionOperator.Equal, key));
            expression.Criteria.AddCondition(new ConditionExpression("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active));
            var result = this.retrieveMultiple(expression).Entities.FirstOrDefault();
            return result != null ? result.GetAttributeValue<string>("rnt_value") : string.Empty;
        }
        public List<string> GetConfigurationByKeys(string[] keys)
        {
            QueryExpression expression = new QueryExpression("rnt_configuration");
            expression.ColumnSet = new ColumnSet(new string[] { "rnt_value", "rnt_name" });
            expression.Criteria.AddCondition(new ConditionExpression("rnt_name", ConditionOperator.In, keys));
            expression.Criteria.AddCondition(new ConditionExpression("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active));
            return this.retrieveMultiple(expression).Entities.Select(p => p.GetAttributeValue<string>("rnt_value")).ToList();
        }
        // Tolga AYKURT - 12.03.2019
        public Entity GetConfigurationEntity(string key)
        {
            QueryExpression expression = new QueryExpression("rnt_configuration");
            expression.ColumnSet = new ColumnSet(new string[] { "rnt_value" });
            expression.Criteria.AddCondition(new ConditionExpression("rnt_name", ConditionOperator.Equal, key));
            expression.Criteria.AddCondition(new ConditionExpression("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active));
            return this.retrieveMultiple(expression).Entities.FirstOrDefault();
        }
    }
}
