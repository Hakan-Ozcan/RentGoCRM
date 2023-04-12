using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using RntCar.BusinessLibrary.Handlers;
using RntCar.ClassLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.BusinessLibrary.Repository
{
    public class PriceListRepository : RepositoryHandler
    {
        #region CONSTRUCTORS
        public PriceListRepository(IOrganizationService Service) : base(Service)
        {
        }
        public PriceListRepository(IOrganizationService Service, Guid UserId) : base(Service, UserId)
        {
        }
        public PriceListRepository(IOrganizationService Service, Guid UserId, string OrganizationName) : base(Service, UserId, OrganizationName)
        {
        }
        #endregion

        #region METHODS
        public EntityCollection getPriceListDataByDate(DateTime beginDate, DateTime endDate)
        {
            QueryExpression query = new QueryExpression("rnt_pricelist");
            query.ColumnSet = new ColumnSet("rnt_name", "statecode");
            FilterExpression mainFilter = new FilterExpression(LogicalOperator.Or);
            FilterExpression beginDateFilter = new FilterExpression(LogicalOperator.And);
            beginDateFilter.AddCondition("rnt_begindate", ConditionOperator.Between, new object[] { beginDate, endDate });
            beginDateFilter.AddCondition("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active);

            FilterExpression endDateFilter = new FilterExpression(LogicalOperator.And);
            endDateFilter.AddCondition("rnt_enddate", ConditionOperator.Between, new object[] { beginDate, endDate });
            endDateFilter.AddCondition("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active);

            mainFilter.AddFilter(beginDateFilter);
            mainFilter.AddFilter(endDateFilter);

            query.Criteria = mainFilter;

            var collection = this.Service.RetrieveMultiple(query);

            return collection;
        }

        // Tolga AYKURT - 07.03.2019
        public EntityCollection GetPriceLists(DateTime beginDate, DateTime endDate, int priceTypeCode, Guid priceCodeId)
        {
            var query = new QueryExpression("rnt_pricelist");
            query.ColumnSet = new ColumnSet(new string[] { "rnt_name" });

            var mainFilterAND = new FilterExpression(LogicalOperator.And);

            mainFilterAND.AddCondition(new ConditionExpression("rnt_pricetypecode", ConditionOperator.Equal, priceTypeCode));
            mainFilterAND.AddCondition(new ConditionExpression("rnt_pricecodeid", ConditionOperator.Equal, priceCodeId));
            mainFilterAND.AddCondition(new ConditionExpression("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active /* Active */));

            var beginEndDateMainFilterOR = new FilterExpression(LogicalOperator.Or);

            var beginEndDateFilterCaseOne = new FilterExpression(LogicalOperator.And);
            beginEndDateFilterCaseOne.AddCondition(new ConditionExpression("rnt_begindate", ConditionOperator.OnOrBefore, beginDate));
            beginEndDateFilterCaseOne.AddCondition(new ConditionExpression("rnt_enddate", ConditionOperator.OnOrAfter, beginDate));

            var beginEndDateFilterCaseTwo = new FilterExpression(LogicalOperator.And);
            beginEndDateFilterCaseTwo.AddCondition(new ConditionExpression("rnt_begindate", ConditionOperator.OnOrBefore, endDate));
            beginEndDateFilterCaseTwo.AddCondition(new ConditionExpression("rnt_enddate", ConditionOperator.OnOrAfter, endDate));

            var beginEndDateFilterCaseThree = new FilterExpression(LogicalOperator.And);
            beginEndDateFilterCaseThree.AddCondition(new ConditionExpression("rnt_begindate", ConditionOperator.OnOrAfter, beginDate));
            beginEndDateFilterCaseThree.AddCondition(new ConditionExpression("rnt_enddate", ConditionOperator.OnOrBefore, endDate));

            beginEndDateMainFilterOR.AddFilter(beginEndDateFilterCaseOne);
            beginEndDateMainFilterOR.AddFilter(beginEndDateFilterCaseTwo);
            beginEndDateMainFilterOR.AddFilter(beginEndDateFilterCaseThree);

            mainFilterAND.AddFilter(beginEndDateMainFilterOR);

            query.Criteria = mainFilterAND;

            return Service.RetrieveMultiple(query);
        }

        public Entity getPriceListByPriceCodeId(Guid priceCodeId,DateTime pickupDateTime, string[] colums)
        {
            QueryExpression queryExpression = new QueryExpression("rnt_pricelist");
            queryExpression.ColumnSet = new ColumnSet(colums);
            queryExpression.Criteria.AddCondition("rnt_pricecodeid", ConditionOperator.Equal, priceCodeId);
            queryExpression.Criteria.AddCondition(new ConditionExpression("rnt_begindate", ConditionOperator.LessEqual, pickupDateTime));
            queryExpression.Criteria.AddCondition(new ConditionExpression("rnt_enddate", ConditionOperator.GreaterEqual, pickupDateTime));
            return this.retrieveMultiple(queryExpression).Entities.FirstOrDefault();
        }
        #endregion
    }
}
