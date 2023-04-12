using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Tooling.Connector;
using RntCar.BusinessLibrary.Handlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.BusinessLibrary.Repository
{
    public class CampaignRepository : RepositoryHandler
    {
        public CampaignRepository(IOrganizationService Service) : base(Service)
        {
        }

        public CampaignRepository(CrmServiceClient _crmServiceClient) : base(_crmServiceClient)
        {
        }

        public CampaignRepository(IOrganizationService Service, CrmServiceClient _crmServiceClient) : base(Service, _crmServiceClient)
        {
        }

        public Entity getCampaignById(Guid campaignId, string[] columns)
        {
            QueryExpression query = new QueryExpression("rnt_campaign");
            query.ColumnSet = new ColumnSet(columns);
            query.Criteria.AddCondition("rnt_campaignid", ConditionOperator.Equal, campaignId);
            return this.retrieveMultiple(query).Entities.FirstOrDefault();
        }

        public Entity getCampaignType()
        {
            QueryExpression queryExpression = new QueryExpression("rnt_campaign");
            queryExpression.ColumnSet = new ColumnSet("rnt_campaigntypecode");
            return this.retrieveMultiple(queryExpression).Entities.FirstOrDefault();
        }
    }
}
