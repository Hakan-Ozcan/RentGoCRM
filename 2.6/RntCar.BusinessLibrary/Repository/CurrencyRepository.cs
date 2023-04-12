using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Tooling.Connector;
using RntCar.BusinessLibrary.Handlers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RntCar.BusinessLibrary.Repository
{
    public class CurrencyRepository : RepositoryHandler
    {
        public CurrencyRepository(IOrganizationService Service) : base(Service)
        {
        }

        public CurrencyRepository(IOrganizationService Service, Guid UserId) : base(Service, UserId)
        {
        }

        public CurrencyRepository(IOrganizationService Service, Guid UserId, string OrganizationName) : base(Service, UserId, OrganizationName)
        {
        }

        public CurrencyRepository(CrmServiceClient _crmServiceClient) : base(_crmServiceClient)
        {
        }
        public Entity getCurrencyById(Guid currencyId, string[] columns)
        {
            QueryExpression query = new QueryExpression("transactioncurrency");
            query.ColumnSet = new ColumnSet(columns);
            query.Criteria.AddCondition("transactioncurrencyid", ConditionOperator.Equal, currencyId);
            return this.retrieveMultiple(query).Entities.FirstOrDefault();
        }
        public List<Entity> getAllCurrencies()
        {
            QueryExpression query = new QueryExpression("transactioncurrency");
            query.ColumnSet = new ColumnSet(true);
            return this.retrieveMultiple(query).Entities.ToList();
        }
        public string getCurrencyCode(Guid currencyId)
        {
            QueryExpression query = new QueryExpression("transactioncurrency");
            query.ColumnSet = new ColumnSet(true);
            query.Criteria.AddCondition("transactioncurrencyid", ConditionOperator.Equal, currencyId);
            return this.retrieveMultiple(query).Entities.FirstOrDefault()?.GetAttributeValue<string>("currencysymbol");
        }
    }
}
