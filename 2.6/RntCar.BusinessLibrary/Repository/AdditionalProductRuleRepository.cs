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
    public class AdditionalProductRuleRepository : RepositoryHandler
    {
        public AdditionalProductRuleRepository(IOrganizationService Service) : base(Service)
        {
        }

        public AdditionalProductRuleRepository(CrmServiceClient _crmServiceClient) : base(_crmServiceClient)
        {
        }

        public AdditionalProductRuleRepository(IOrganizationService Service, CrmServiceClient _crmServiceClient) : base(Service, _crmServiceClient)
        {
        }

        public Entity getAdditonalProductRuleByAdditionalProductId(Guid productId)
        {
            QueryExpression query = new QueryExpression("rnt_additionalproductrules");
            query.ColumnSet = new ColumnSet(true);
            query.Criteria.AddCondition("rnt_product", ConditionOperator.Equal, productId);
            query.Criteria.AddCondition("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active);
            return this.retrieveMultiple(query).Entities.FirstOrDefault();
        }

        public EntityCollection getAdditonalProductRuleListByAdditionalProductId(Guid productId)
        {
            QueryExpression query = new QueryExpression("rnt_additionalproductrules");
            query.ColumnSet = new ColumnSet(true);
            query.Criteria.AddCondition("rnt_product", ConditionOperator.Equal, productId);
            query.Criteria.AddCondition("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active);
            return this.retrieveMultiple(query);
        }
        public Entity getAdditonalProductRuleByAdditionalProductandParentProductId(Guid productId, Guid parentProductId)
        {
            QueryExpression query = new QueryExpression("rnt_additionalproductrules");
            query.ColumnSet = new ColumnSet(true);
            query.Criteria.AddCondition("rnt_parentproduct", ConditionOperator.Equal, parentProductId);
            query.Criteria.AddCondition("rnt_product", ConditionOperator.Equal, productId);

            query.Criteria.AddCondition("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active);
            return this.retrieveMultiple(query).Entities.FirstOrDefault();
        }
        public List<Entity> getAdditonalProductRules()
        {
            QueryExpression query = new QueryExpression("rnt_additionalproductrules");
            query.ColumnSet = new ColumnSet(true);
            query.Criteria.AddCondition("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active);
            return this.retrieveMultiple(query).Entities.ToList();
        }
    }
}
