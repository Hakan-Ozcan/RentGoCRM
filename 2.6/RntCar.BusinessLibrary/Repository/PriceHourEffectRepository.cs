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
    public class PriceHourEffectRepository : RepositoryHandler
    {
        public PriceHourEffectRepository(IOrganizationService Service) : base(Service)
        {
        }

        public PriceHourEffectRepository(CrmServiceClient _crmServiceClient) : base(_crmServiceClient)
        {
        }

        public PriceHourEffectRepository(IOrganizationService Service, CrmServiceClient _crmServiceClient) : base(Service, _crmServiceClient)
        {
        }

        public PriceHourEffectRepository(IOrganizationService Service, Guid UserId) : base(Service, UserId)
        {
        }

        public PriceHourEffectRepository(IOrganizationService Service, Guid UserId, string OrganizationName) : base(Service, UserId, OrganizationName)
        {
        }
        public Entity getPriceHourEffectByDuration(int duration)
        {
            QueryExpression queryExpression = new QueryExpression("rnt_pricehoureffects");
            queryExpression.ColumnSet = new ColumnSet(true);
            queryExpression.Criteria.AddCondition("rnt_minimumminute", ConditionOperator.LessEqual, duration);
            queryExpression.Criteria.AddCondition("rnt_maximumminute", ConditionOperator.GreaterEqual, duration);
            queryExpression.Criteria.AddCondition("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active);
            return this.retrieveMultiple(queryExpression).Entities.FirstOrDefault();
        }

        public Entity getZeroPriceEffect()
        {
            QueryExpression queryExpression = new QueryExpression("rnt_pricehoureffects");
            queryExpression.ColumnSet = new ColumnSet(true);
            queryExpression.Criteria.AddCondition("rnt_effectrate", ConditionOperator.Equal, 0);
            queryExpression.Criteria.AddCondition("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active);
            return this.retrieveMultiple(queryExpression).Entities.FirstOrDefault();
        }
    }
}
