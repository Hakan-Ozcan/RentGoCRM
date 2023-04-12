using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using RntCar.BusinessLibrary.Handlers;
using RntCar.ClassLibrary;
using System.Collections.Generic;
using System.Linq;

namespace RntCar.BusinessLibrary.Repository
{
    public class TransactionCurrencyRepository : RepositoryHandler
    {
        public TransactionCurrencyRepository(IOrganizationService Service) : base(Service)
        {
        }

        public List<Entity> GetAllCurrencies()
        {
            QueryExpression expression = new QueryExpression("transactioncurrency");
            expression.ColumnSet = new ColumnSet(true);
            expression.Criteria.AddCondition(new ConditionExpression("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active));

            return this.retrieveMultiple(expression).Entities.ToList();
        }

        public Entity getCurrencyByCode(string code)
        {
            QueryExpression expression = new QueryExpression("transactioncurrency");
            expression.ColumnSet = new ColumnSet(true);
            expression.Criteria.AddCondition(new ConditionExpression("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active));
            expression.Criteria.AddCondition(new ConditionExpression("isocurrencycode", ConditionOperator.Equal, code));

            return this.retrieveMultiple(expression).Entities.FirstOrDefault();
        }
    }
}
