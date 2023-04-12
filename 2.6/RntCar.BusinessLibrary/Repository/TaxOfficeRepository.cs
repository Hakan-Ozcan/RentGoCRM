using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using RntCar.BusinessLibrary.Handlers;
using RntCar.ClassLibrary;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RntCar.BusinessLibrary.Repository
{
    public class TaxOfficeRepository : RepositoryHandler
    {
        public TaxOfficeRepository(IOrganizationService Service) : base(Service)
        {
        }

        public TaxOfficeRepository(IOrganizationService Service, Guid UserId) : base(Service, UserId)
        {
        }

        public TaxOfficeRepository(IOrganizationService Service, Guid UserId, string OrganizationName) : base(Service, UserId, OrganizationName)
        {
        }
        public List<Entity> getActiveTaxOffices()
        {
            QueryExpression expression = new QueryExpression("rnt_taxoffice");
            expression.ColumnSet = new ColumnSet(true);
            expression.Criteria.AddCondition("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active);
            return this.Service.RetrieveMultiple(expression).Entities.ToList();
        }
    }
}
