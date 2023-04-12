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
    public class BonusAdditionalProductRateRepository : RepositoryHandler
    {
        public BonusAdditionalProductRateRepository(IOrganizationService Service) : base(Service)
        {
        }

        public BonusAdditionalProductRateRepository(CrmServiceClient _crmServiceClient) : base(_crmServiceClient)
        {
        }

        public BonusAdditionalProductRateRepository(IOrganizationService Service, CrmServiceClient _crmServiceClient) : base(Service, _crmServiceClient)
        {
        }
        public Entity getBonusAdditionalProductRateByBusinessRoleandAdditionalProduct(int businessRoleCode, Guid additionalProductId)
        {
            QueryExpression queryExpression = new QueryExpression("rnt_bonusadditionalproductrate");
            queryExpression.ColumnSet = new ColumnSet(true);
            queryExpression.Criteria.AddCondition("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active);
            queryExpression.Criteria.AddCondition("rnt_additionalproductid", ConditionOperator.Equal, additionalProductId);
            queryExpression.Criteria.AddCondition("rnt_businessrolecode", ConditionOperator.Equal, businessRoleCode);
            return this.retrieveMultiple(queryExpression).Entities.FirstOrDefault();
        }
        public List<Entity> getBonusAdditionalProductRateByBonusCalcuationandAdditionalProduct(Guid bonusCalculationId, Guid additionalProductId)
        {
            QueryExpression queryExpression = new QueryExpression("rnt_bonusadditionalproductrate");
            queryExpression.ColumnSet = new ColumnSet(true);
            queryExpression.Criteria.AddCondition("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active);
            queryExpression.Criteria.AddCondition("rnt_additionalproductid", ConditionOperator.Equal, additionalProductId);
            queryExpression.Criteria.AddCondition("rnt_bonuscalculationid", ConditionOperator.Equal, bonusCalculationId);
            return this.retrieveMultiple(queryExpression).Entities.ToList();
        }
    }
}
