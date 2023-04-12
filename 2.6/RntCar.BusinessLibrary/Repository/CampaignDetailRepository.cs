using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Tooling.Connector;
using RntCar.BusinessLibrary.Handlers;
using RntCar.ClassLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.BusinessLibrary.Repository
{
    public class CampaignDetailRepository : RepositoryHandler
    {
        public CampaignDetailRepository(IOrganizationService Service) : base(Service)
        {
        }

        public CampaignDetailRepository(CrmServiceClient _crmServiceClient) : base(_crmServiceClient)
        {
        }

        public CampaignDetailRepository(IOrganizationService Service, CrmServiceClient _crmServiceClient) : base(Service, _crmServiceClient)
        {
        }

        public CampaignDetailRepository(IOrganizationService Service, Guid UserId) : base(Service, UserId)
        {
        }

        public CampaignDetailRepository(IOrganizationService Service, Guid UserId, string OrganizationName) : base(Service, UserId, OrganizationName)
        {
        }

        public Entity getCampaignDetail(Guid cmsCampaignId)
        {
            return this.retrieveById("rnt_cmscampaign", cmsCampaignId);
        }
        public Entity getCampaignDetailByCampaignId(Guid campaignId)
        {
            QueryExpression queryExpression = new QueryExpression("rnt_cmscampaign");
            queryExpression.ColumnSet = new ColumnSet(true);
            queryExpression.Criteria.AddCondition("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active);
            queryExpression.Criteria.AddCondition("rnt_campaignid", ConditionOperator.Equal, campaignId);
            return this.retrieveMultiple(queryExpression).Entities.FirstOrDefault();
        }
        public List<Entity> getCMSCampaigns(string[] columns)
        {
            QueryExpression queryExpression = new QueryExpression("rnt_cmscampaign");
            queryExpression.ColumnSet = new ColumnSet(columns);
            queryExpression.Criteria.AddCondition("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active);            
            return this.retrieveMultiple(queryExpression).Entities.ToList();
        }
    }
}
