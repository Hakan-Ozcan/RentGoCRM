using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using RntCar.BusinessLibrary.Handlers;
using System;
using System.Linq;

namespace RntCar.BusinessLibrary.Repository
{
    public class FineRateRepository : RepositoryHandler
    {
        public FineRateRepository(IOrganizationService Service) : base(Service)
        {
        }
        public decimal getFineAmount(int type, decimal amount)
        {
            var returnAmount = decimal.Zero;
            QueryExpression queryExpression = new QueryExpression("rnt_hgsrate");
            queryExpression.Criteria.AddCondition("statuscode", ConditionOperator.Equal, 1);
            queryExpression.Criteria.AddCondition("rnt_typecode", ConditionOperator.Equal, type);
            queryExpression.Criteria.AddCondition("rnt_minimumamount", ConditionOperator.LessEqual, amount);
            queryExpression.Criteria.AddCondition("rnt_maximumamount", ConditionOperator.GreaterEqual, amount);
            queryExpression.ColumnSet = new ColumnSet(true);
            var res = this.retrieveMultiple(queryExpression).Entities.FirstOrDefault();
            if (res != null)
            {
                if(res.GetAttributeValue<OptionSetValue>("rnt_chargedtype").Value == 1)
                {
                    returnAmount = (amount * res.GetAttributeValue<decimal>("rnt_chargedvalue")) / 100;
                }
                else
                {
                    returnAmount = res.GetAttributeValue<decimal>("rnt_chargedvalue");
                }
            }
            return returnAmount;
        }
        public decimal getFineAmount(Guid additionalProductId, decimal amount)
        {

            var returnAmount = decimal.Zero;

            FilterExpression maxValueSubFilter = new FilterExpression(LogicalOperator.And);
            maxValueSubFilter.AddCondition("rnt_maximumamount", ConditionOperator.GreaterEqual, amount);
            maxValueSubFilter.AddCondition("rnt_maximumamount", ConditionOperator.NotNull);

            FilterExpression maxValueFilter = new FilterExpression(LogicalOperator.Or);
            maxValueFilter.AddCondition("rnt_maximumamount", ConditionOperator.Null);
            maxValueFilter.AddFilter(maxValueSubFilter);

            QueryExpression getFineRateQuery = new QueryExpression("rnt_finerate");
            getFineRateQuery.Criteria.AddCondition("statuscode", ConditionOperator.Equal, 1);
            getFineRateQuery.Criteria.AddCondition("rnt_additionalproductid", ConditionOperator.Equal, additionalProductId);
            getFineRateQuery.Criteria.AddCondition("rnt_minimumamount", ConditionOperator.LessEqual, amount);
            getFineRateQuery.Criteria.AddFilter(maxValueFilter);
            getFineRateQuery.ColumnSet = new ColumnSet(true);
            var res = this.retrieveMultiple(getFineRateQuery).Entities.FirstOrDefault();
            if (res != null)
            {
                if (res.GetAttributeValue<OptionSetValue>("rnt_chargedtype").Value == 1)
                {
                    returnAmount = (amount * res.GetAttributeValue<decimal>("rnt_chargedvalue")) / 100;
                }
                else
                {
                    returnAmount = res.GetAttributeValue<decimal>("rnt_chargedvalue");
                }
            }
            return returnAmount;
        }
    }
}
