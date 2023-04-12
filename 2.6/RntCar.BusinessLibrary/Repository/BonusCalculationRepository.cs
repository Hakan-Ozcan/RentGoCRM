using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Tooling.Connector;
using RntCar.BusinessLibrary.Handlers;
using RntCar.ClassLibrary;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RntCar.BusinessLibrary
{
    public class BonusCalculationRepository : RepositoryHandler
    {
        public BonusCalculationRepository(IOrganizationService Service) : base(Service)
        {
        }

        public BonusCalculationRepository(CrmServiceClient _crmServiceClient) : base(_crmServiceClient)
        {
        }

        public BonusCalculationRepository(IOrganizationService Service, CrmServiceClient _crmServiceClient) : base(Service, _crmServiceClient)
        {
        }
        public Entity getBonusCalculationByDate(DateTime date)
        {
            QueryExpression queryExpression = new QueryExpression("rnt_bonuscalculation");
            queryExpression.ColumnSet = new ColumnSet(true);
            queryExpression.Criteria.AddCondition("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active);
            queryExpression.Criteria.AddCondition("rnt_begindate", ConditionOperator.OnOrBefore, date);
            queryExpression.Criteria.AddCondition("rnt_enddate", ConditionOperator.OnOrAfter, date);
            return this.retrieveMultiple(queryExpression).Entities.FirstOrDefault();
        }

        public List<Entity> getAdditionalProductBonusCalculations()
        {
            QueryExpression queryExpression = new QueryExpression("rnt_bonuscalculation");
            queryExpression.ColumnSet = new ColumnSet(true);
            queryExpression.Criteria.AddCondition("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active);
            queryExpression.Criteria.AddCondition("rnt_bonustypecode", ConditionOperator.Equal, (int)GlobalEnums.BonusType.AdditionalProduct);

            return this.retrieveMultiple(queryExpression).Entities.ToList();
        }

        public List<Entity> getSalesBonusCalculations()
        {
            QueryExpression queryExpression = new QueryExpression("rnt_bonuscalculation");
            queryExpression.ColumnSet = new ColumnSet(true);
            queryExpression.Criteria.AddCondition("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active);
            queryExpression.Criteria.AddCondition("rnt_bonustypecode", ConditionOperator.Equal, (int)GlobalEnums.BonusType.Sales);

            return this.retrieveMultiple(queryExpression).Entities.ToList();
        }
    }
}
