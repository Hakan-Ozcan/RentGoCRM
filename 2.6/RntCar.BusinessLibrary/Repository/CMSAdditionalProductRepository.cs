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
    public class CMSAdditionalProductRepository : RepositoryHandler
    {
        public CMSAdditionalProductRepository(IOrganizationService Service) : base(Service)
        {
        }

        public CMSAdditionalProductRepository(CrmServiceClient _crmServiceClient) : base(_crmServiceClient)
        {
        }

        public CMSAdditionalProductRepository(IOrganizationService Service, CrmServiceClient _crmServiceClient) : base(Service, _crmServiceClient)
        {
        }

        public CMSAdditionalProductRepository(IOrganizationService Service, Guid UserId) : base(Service, UserId)
        {
        }

        public CMSAdditionalProductRepository(IOrganizationService Service, Guid UserId, string OrganizationName) : base(Service, UserId, OrganizationName)
        {
        }
        public List<Entity> GetCMSAdditionalProducts()
        {
            QueryExpression queryExpression = new QueryExpression("rnt_cmsadditionalproduct");
            queryExpression.ColumnSet = new ColumnSet(true);
            queryExpression.Criteria.AddCondition("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active);
            return this.retrieveMultiple(queryExpression).Entities.ToList();
        }
    }
}
