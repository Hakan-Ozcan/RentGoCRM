using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using RntCar.BusinessLibrary.Handlers;
using RntCar.ClassLibrary;
using RntCar.ClassLibrary.PriceFactor.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using static RntCar.ClassLibrary.PriceFactor.Validation.PriceFactorValidationInput;

namespace RntCar.BusinessLibrary.Repository
{
    // Tolhga AYKURT - 07.03.2019
    public class PriceFactorRepository : RepositoryHandler
    {
        #region CONSTRUCTORS
        // Tolhga AYKURT - 07.03.2019
        public PriceFactorRepository(IOrganizationService Service) : base(Service)
        {
        }

        // Tolhga AYKURT - 07.03.2019
        public PriceFactorRepository(IOrganizationService Service, Guid UserId) : base(Service, UserId)
        {
        }

        // Tolhga AYKURT - 07.03.2019
        public PriceFactorRepository(IOrganizationService Service, Guid UserId, string OrganizationName) : base(Service, UserId, OrganizationName)
        {
        }
        #endregion

        #region METHODS
        // Tolhga AYKURT - 14.03.2019
        public EntityCollection GetPriceFactors(PriceFactorValidationInput priceFactorValidationInput)
        {
            var query = new QueryExpression("rnt_pricefactor");
            query.ColumnSet = new ColumnSet(new string[] { "rnt_name" });

            var mainFilterAND = new FilterExpression(LogicalOperator.And);
            var beginEndDateMainFilterOR = new FilterExpression(LogicalOperator.Or);

            // Price Factor Type Code - Statecode
            mainFilterAND.AddCondition("rnt_type", ConditionOperator.ContainValues, priceFactorValidationInput.type.ToArray());
            mainFilterAND.AddCondition(new ConditionExpression("rnt_pricefactortypecode", ConditionOperator.Equal, priceFactorValidationInput.PriceFactorType));
            mainFilterAND.AddCondition(new ConditionExpression("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active));

            var beginEndDateFilterCaseOne = new FilterExpression(LogicalOperator.And);
            beginEndDateFilterCaseOne.AddCondition(new ConditionExpression("rnt_begindate", ConditionOperator.OnOrBefore, priceFactorValidationInput.BeginDate));
            beginEndDateFilterCaseOne.AddCondition(new ConditionExpression("rnt_enddate", ConditionOperator.OnOrAfter, priceFactorValidationInput.BeginDate));

            var beginEndDateFilterCaseTwo = new FilterExpression(LogicalOperator.And);
            beginEndDateFilterCaseTwo.AddCondition(new ConditionExpression("rnt_begindate", ConditionOperator.OnOrBefore, priceFactorValidationInput.EndDate));
            beginEndDateFilterCaseTwo.AddCondition(new ConditionExpression("rnt_enddate", ConditionOperator.OnOrAfter, priceFactorValidationInput.EndDate));

            var beginEndDateFilterCaseThree = new FilterExpression(LogicalOperator.And);
            beginEndDateFilterCaseThree.AddCondition(new ConditionExpression("rnt_begindate", ConditionOperator.OnOrAfter, priceFactorValidationInput.BeginDate));
            beginEndDateFilterCaseThree.AddCondition(new ConditionExpression("rnt_enddate", ConditionOperator.OnOrBefore, priceFactorValidationInput.EndDate));

            beginEndDateMainFilterOR.AddFilter(beginEndDateFilterCaseOne);
            beginEndDateMainFilterOR.AddFilter(beginEndDateFilterCaseTwo);
            beginEndDateMainFilterOR.AddFilter(beginEndDateFilterCaseThree);

            // Attribute Filter (For PriceFactorType = PayMethod || ReservationChannel || WeekDay)
            if (priceFactorValidationInput.AttributeToValidate != null)
            {
                mainFilterAND.AddFilter(GetCondition(priceFactorValidationInput.AttributeToValidate));
            }

            mainFilterAND.AddFilter(beginEndDateMainFilterOR);

            query.Criteria = mainFilterAND;

            return Service.RetrieveMultiple(query);
        }
        public EntityCollection GetPriceFactorsWihtOutDateConditions(PriceFactorValidationInput priceFactorValidationInput)
        {
            var query = new QueryExpression("rnt_pricefactor");
            query.ColumnSet = new ColumnSet(new string[] { "rnt_name" });

            var mainFilterAND = new FilterExpression(LogicalOperator.And);

            // Price Factor Type Code - Statecode
            mainFilterAND.AddCondition("rnt_type", ConditionOperator.ContainValues, priceFactorValidationInput.type.ToArray());
            mainFilterAND.AddCondition(new ConditionExpression("rnt_pricefactortypecode", ConditionOperator.Equal, priceFactorValidationInput.PriceFactorType));
            mainFilterAND.AddCondition(new ConditionExpression("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active));

            // Attribute Filter (For PriceFactorType = PayMethod || ReservationChannel || WeekDay)
            if (priceFactorValidationInput.AttributeToValidate != null)
            {
                mainFilterAND.AddFilter(GetCondition(priceFactorValidationInput.AttributeToValidate));
            }

            query.Criteria = mainFilterAND;

            return Service.RetrieveMultiple(query);
        }
        #endregion

        #region HELPER METHODS
        // Tolga AYKURT - 14.03.2019
        private FilterExpression GetCondition(List<ValidationAtrribute> validationAtrributes)
        {
            var filter = new FilterExpression();
            var optionSetValidation = validationAtrributes.Where(item => item.AttributeType == ValidationAtrribute.AttributeTypeEnum.OptionSet).FirstOrDefault();
            if (optionSetValidation != null)
            {
                validationAtrributes.Remove(optionSetValidation);
                filter.AddFilter(GetOptionSetCondition(optionSetValidation));
            }
            filter = GetMultiSelectOptionSetCondition(validationAtrributes);
            return filter;
        }

        // Tolga AYKURT - 14.03.2019
        private FilterExpression GetMultiSelectOptionSetCondition(List<ValidationAtrribute> validationAtrributes)
        {
            var parentFilter = new FilterExpression(LogicalOperator.And);

            foreach (var validationAtrribute in validationAtrributes)
            {
                var filter = new FilterExpression(LogicalOperator.Or);
                foreach (var value in validationAtrribute.AttributeValues)
                {
                    filter.AddCondition(new ConditionExpression(validationAtrribute.AttributeName, ConditionOperator.Like, "%" + value + "%"));
                }
                parentFilter.AddFilter(filter);
            }
            return parentFilter;
        }

        // Tolga AYKURT - 14.03.2019
        private FilterExpression GetOptionSetCondition(ValidationAtrribute validationAtrribute)
        {
            var filter = new FilterExpression(LogicalOperator.And);

            filter.AddCondition(new ConditionExpression(validationAtrribute.AttributeName, ConditionOperator.Equal, Convert.ToInt32(validationAtrribute.AttributeValues.FirstOrDefault().Replace("|", string.Empty))));

            return filter;
        }
        #endregion
    }
}
