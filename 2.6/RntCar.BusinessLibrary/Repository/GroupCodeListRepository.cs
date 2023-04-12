using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using RntCar.BusinessLibrary.Handlers;
using RntCar.ClassLibrary;
using RntCar.ClassLibrary.GroupCodeList.Validation;
using System;

namespace RntCar.BusinessLibrary.Repository
{
    // Tolga AYKURT - 11.03.2019
    public class GroupCodeListRepository : RepositoryHandler
    {
        #region CONSTRUCTORS
        public GroupCodeListRepository(IOrganizationService Service) : base(Service)
        {
        }

        public GroupCodeListRepository(IOrganizationService Service, Guid UserId) : base(Service, UserId)
        {
        }

        public GroupCodeListRepository(IOrganizationService Service, Guid UserId, string OrganizationName) : base(Service, UserId, OrganizationName)
        {
        }
        #endregion

        #region METHODS
        // Tolga AYKURT - 11.03.2019
        public EntityCollection GetGroupCodeLists(GroupCodeListValidationInput groupCodeListValidationInput)
        {
            var query = new QueryExpression("rnt_listprice");
            query.ColumnSet = new ColumnSet(new string[] { "rnt_name" });

            var mainFilter = new FilterExpression(LogicalOperator.Or);
            var embracingFilter = new FilterExpression(LogicalOperator.And);
            embracingFilter.AddCondition(new ConditionExpression("rnt_pricelistid", ConditionOperator.Equal, groupCodeListValidationInput.PriceListId));
            embracingFilter.AddCondition(new ConditionExpression("rnt_groupcodeid", ConditionOperator.Equal, groupCodeListValidationInput.GroupCodeId));
            embracingFilter.AddCondition(new ConditionExpression("rnt_minimumday", ConditionOperator.LessEqual, groupCodeListValidationInput.MinDay));
            embracingFilter.AddCondition(new ConditionExpression("rnt_maximumday", ConditionOperator.GreaterEqual, groupCodeListValidationInput.MaxDay));
            embracingFilter.AddCondition(new ConditionExpression("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active));

            var containedFilter = new FilterExpression(LogicalOperator.And);
            containedFilter.AddCondition(new ConditionExpression("rnt_pricelistid", ConditionOperator.Equal, groupCodeListValidationInput.PriceListId));
            containedFilter.AddCondition(new ConditionExpression("rnt_groupcodeid", ConditionOperator.Equal, groupCodeListValidationInput.GroupCodeId));
            containedFilter.AddCondition(new ConditionExpression("rnt_minimumday", ConditionOperator.GreaterEqual, groupCodeListValidationInput.MinDay));
            containedFilter.AddCondition(new ConditionExpression("rnt_maximumday", ConditionOperator.LessEqual, groupCodeListValidationInput.MaxDay));
            containedFilter.AddCondition(new ConditionExpression("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active /* Active */));

            var minDayFilter = new FilterExpression(LogicalOperator.And);
            minDayFilter.AddCondition(new ConditionExpression("rnt_pricelistid", ConditionOperator.Equal, groupCodeListValidationInput.PriceListId));
            minDayFilter.AddCondition(new ConditionExpression("rnt_groupcodeid", ConditionOperator.Equal, groupCodeListValidationInput.GroupCodeId));
            minDayFilter.AddCondition(new ConditionExpression("rnt_minimumday", ConditionOperator.LessEqual, groupCodeListValidationInput.MinDay));
            minDayFilter.AddCondition(new ConditionExpression("rnt_maximumday", ConditionOperator.GreaterEqual, groupCodeListValidationInput.MinDay));
            minDayFilter.AddCondition(new ConditionExpression("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active /* Active */));

            var maxDayFilter = new FilterExpression(LogicalOperator.And);
            maxDayFilter.AddCondition(new ConditionExpression("rnt_pricelistid", ConditionOperator.Equal, groupCodeListValidationInput.PriceListId));
            maxDayFilter.AddCondition(new ConditionExpression("rnt_groupcodeid", ConditionOperator.Equal, groupCodeListValidationInput.GroupCodeId));
            maxDayFilter.AddCondition(new ConditionExpression("rnt_minimumday", ConditionOperator.LessEqual, groupCodeListValidationInput.MaxDay));
            maxDayFilter.AddCondition(new ConditionExpression("rnt_maximumday", ConditionOperator.GreaterEqual, groupCodeListValidationInput.MaxDay));
            maxDayFilter.AddCondition(new ConditionExpression("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active /* Active */));

            mainFilter.AddFilter(embracingFilter);
            mainFilter.AddFilter(containedFilter);
            mainFilter.AddFilter(minDayFilter);
            mainFilter.AddFilter(maxDayFilter);

            query.Criteria = mainFilter;

            return Service.RetrieveMultiple(query);
        }
        #endregion
    }
}
