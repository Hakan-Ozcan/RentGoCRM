using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using RntCar.BusinessLibrary.Handlers;
using RntCar.ClassLibrary;
using System;

namespace RntCar.BusinessLibrary.Repository
{
    public class AvailabilityPriceListRepository : RepositoryHandler
    {
        #region CONSTRUCTORS
        public AvailabilityPriceListRepository(IOrganizationService Service) : base(Service)
        {
        }

        public AvailabilityPriceListRepository(IOrganizationService Service, Guid UserId) : base(Service, UserId)
        {

        }
        #endregion

        #region  METHODS
        // Tolga AYKURT - 11.03.2019
        public EntityCollection GetAvailabilityPriceList(AvailabilityPriceListValidationInput availabilityPriceListValidationInput)
        {
            var query = new QueryExpression("rnt_availabilitypricelist");
            query.ColumnSet = new ColumnSet(new string[] { "rnt_name" });

            var mainFilter = new FilterExpression(LogicalOperator.Or);
            var embracingFilter = new FilterExpression(LogicalOperator.And);
            embracingFilter.AddCondition(new ConditionExpression("rnt_pricelistid", ConditionOperator.Equal, availabilityPriceListValidationInput.PriceListId));
            embracingFilter.AddCondition(new ConditionExpression("rnt_groupcodeid", ConditionOperator.Equal, availabilityPriceListValidationInput.groupCodeId));
            embracingFilter.AddCondition(new ConditionExpression("rnt_minimumavailability", ConditionOperator.LessEqual, availabilityPriceListValidationInput.MinimumAvailability));
            embracingFilter.AddCondition(new ConditionExpression("rnt_maximumavailability", ConditionOperator.GreaterEqual, availabilityPriceListValidationInput.MaximumAvailability));
            embracingFilter.AddCondition(new ConditionExpression("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active));

            var containedFilter = new FilterExpression(LogicalOperator.And);
            
            containedFilter.AddCondition(new ConditionExpression("rnt_pricelistid", ConditionOperator.Equal, availabilityPriceListValidationInput.PriceListId));
            containedFilter.AddCondition(new ConditionExpression("rnt_groupcodeid", ConditionOperator.Equal, availabilityPriceListValidationInput.groupCodeId));
            containedFilter.AddCondition(new ConditionExpression("rnt_minimumavailability", ConditionOperator.GreaterEqual, availabilityPriceListValidationInput.MinimumAvailability));
            containedFilter.AddCondition(new ConditionExpression("rnt_maximumavailability", ConditionOperator.LessEqual, availabilityPriceListValidationInput.MaximumAvailability));
            containedFilter.AddCondition(new ConditionExpression("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active));

            var minAvailabilityFilter = new FilterExpression(LogicalOperator.And);
            minAvailabilityFilter.AddCondition(new ConditionExpression("rnt_pricelistid", ConditionOperator.Equal, availabilityPriceListValidationInput.PriceListId));
            minAvailabilityFilter.AddCondition(new ConditionExpression("rnt_groupcodeid", ConditionOperator.Equal, availabilityPriceListValidationInput.groupCodeId));
            minAvailabilityFilter.AddCondition(new ConditionExpression("rnt_minimumavailability", ConditionOperator.LessEqual, availabilityPriceListValidationInput.MinimumAvailability));
            minAvailabilityFilter.AddCondition(new ConditionExpression("rnt_maximumavailability", ConditionOperator.GreaterEqual, availabilityPriceListValidationInput.MinimumAvailability));
            minAvailabilityFilter.AddCondition(new ConditionExpression("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active));

            var maxAvailabilityFilter = new FilterExpression(LogicalOperator.And);
            maxAvailabilityFilter.AddCondition(new ConditionExpression("rnt_pricelistid", ConditionOperator.Equal, availabilityPriceListValidationInput.PriceListId));
            maxAvailabilityFilter.AddCondition(new ConditionExpression("rnt_groupcodeid", ConditionOperator.Equal, availabilityPriceListValidationInput.groupCodeId));
            maxAvailabilityFilter.AddCondition(new ConditionExpression("rnt_minimumavailability", ConditionOperator.LessEqual, availabilityPriceListValidationInput.MaximumAvailability));
            maxAvailabilityFilter.AddCondition(new ConditionExpression("rnt_maximumavailability", ConditionOperator.GreaterEqual, availabilityPriceListValidationInput.MaximumAvailability));
            maxAvailabilityFilter.AddCondition(new ConditionExpression("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active));

            mainFilter.AddFilter(embracingFilter);
            mainFilter.AddFilter(containedFilter);
            mainFilter.AddFilter(minAvailabilityFilter);
            mainFilter.AddFilter(maxAvailabilityFilter);

            query.Criteria = mainFilter;

            return Service.RetrieveMultiple(query);
        }
        #endregion
    }
}
