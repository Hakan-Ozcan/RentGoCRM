using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using RntCar.BusinessLibrary.Handlers;
using RntCar.ClassLibrary;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RntCar.BusinessLibrary.Repository
{
    public class FactorDatesRepository : RepositoryHandler
    {
        public FactorDatesRepository(IOrganizationService Service) : base(Service)
        {
        }

        public List<Entity> getFactorDatesByPriceFactorId(Guid priceFactorId)
        {
            QueryExpression factorDates = new QueryExpression("rnt_factordate");
            factorDates.ColumnSet = new ColumnSet(true);
            factorDates.Criteria.AddCondition("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active);
            factorDates.Criteria.AddCondition("rnt_pricefactorid", ConditionOperator.Equal, priceFactorId);
            return this.retrieveMultiple(factorDates).Entities.ToList();
        }
    }
}
