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
    public class BannerRepository : RepositoryHandler
    {
        public BannerRepository(IOrganizationService Service) : base(Service)
        {
        }

        public BannerRepository(CrmServiceClient _crmServiceClient) : base(_crmServiceClient)
        {
        }

        public BannerRepository(IOrganizationService Service, Guid UserId) : base(Service, UserId)
        {

        }

        public BannerRepository(IOrganizationService Service, CrmServiceClient _crmServiceClient) : base(Service, _crmServiceClient)
        {
        }

        public BannerRepository(IOrganizationService Service, Guid UserId, string OrganizationName) : base(Service, UserId, OrganizationName)
        {
        }

        public List<Entity> getCampaignBanners(Guid campaignId)
        {
            QueryExpression queryExpression = new QueryExpression("rnt_banner");
            queryExpression.ColumnSet = new ColumnSet(true);
            queryExpression.Criteria.AddCondition("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active);
            queryExpression.Criteria.AddCondition("rnt_campaignid", ConditionOperator.Equal, campaignId);
            return this.retrieveMultiple(queryExpression).Entities.ToList();

        }
        public List<Entity> getBanners()
        {
            QueryExpression queryExpression = new QueryExpression("rnt_banner");
            queryExpression.ColumnSet = new ColumnSet(true);
            queryExpression.Criteria.AddCondition("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active);
            return this.retrieveMultiple(queryExpression).Entities.ToList();

        }
    }
}
