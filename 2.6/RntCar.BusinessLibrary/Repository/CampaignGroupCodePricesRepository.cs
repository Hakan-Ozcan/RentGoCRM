using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using RntCar.BusinessLibrary.Handlers;
using RntCar.ClassLibrary;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RntCar.BusinessLibrary.Repository
{
    public class CampaignGroupCodePricesRepository : RepositoryHandler
    {
        public CampaignGroupCodePricesRepository(IOrganizationService Service) : base(Service)
        {
        }

        public List<Entity> getCampaignGroupCodePricesByCampaignId(Guid campaignId)
        {
            QueryExpression queryExpression = new QueryExpression("rnt_campaigngroupcodeprice");
            queryExpression.ColumnSet = new ColumnSet(true);
            queryExpression.Criteria.AddCondition("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active);
            queryExpression.Criteria.AddCondition("rnt_campaignid", ConditionOperator.Equal, campaignId);
            return this.retrieveMultiple(queryExpression).Entities.ToList();

        }
    }
}
