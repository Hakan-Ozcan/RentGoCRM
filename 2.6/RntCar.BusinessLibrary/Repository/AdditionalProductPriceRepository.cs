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
    public class AdditionalProductPriceRepository : RepositoryHandler
    {
        public AdditionalProductPriceRepository(IOrganizationService Service) : base(Service)
        {
        }

        public AdditionalProductPriceRepository(CrmServiceClient _crmServiceClient) : base(_crmServiceClient)
        {
        }

        public AdditionalProductPriceRepository(IOrganizationService Service, CrmServiceClient _crmServiceClient) : base(Service, _crmServiceClient)
        {
        }

        public AdditionalProductPriceRepository(IOrganizationService Service, Guid UserId) : base(Service, UserId)
        {
        }

        public AdditionalProductPriceRepository(IOrganizationService Service, Guid UserId, string OrganizationName) : base(Service, UserId, OrganizationName)
        {
        }
        public List<Entity> getAdditionalProductPrices(Guid currencyId)
        {
            QueryExpression expression = new QueryExpression("rnt_additionalproductprice");
            expression.ColumnSet = new ColumnSet(true);
            expression.Criteria.AddCondition("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active);
            expression.Criteria.AddCondition("transactioncurrencyid", ConditionOperator.Equal, currencyId);
            return this.retrieveMultiple(expression).Entities.ToList();
        }

        public List<Entity> getActiveAdditionalProductPrices()
        {
            QueryExpression expression = new QueryExpression("rnt_additionalproductprice");
            expression.ColumnSet = new ColumnSet(true);
            expression.Criteria.AddCondition("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active);
            return this.retrieveMultiple(expression).Entities.ToList();
        }
    }
}
