using Microsoft.Xrm.Sdk;
using RntCar.BusinessLibrary.Business;
using RntCar.ClassLibrary.PriceFactor.Validation;
using RntCar.SDK.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RntCar.CrmPlugins.PriceFactors.Validation
{
    // Tolga AYKURT - 07.03.2019
    /// <summary>
    /// Update mesajının PreValidate stage'inde çalışır. Price factor ile ilgili gerekli validasyonları yapar.
    /// </summary>
    public class PreValidationUpdatePriceFactor : IPlugin
    {
        #region METHODS
        // Tolga AYKURT - 07.03.2019
        public void Execute(IServiceProvider serviceProvider)
        {
            var pluginInitializer = new PluginInitializer(serviceProvider);
            var priceFactorBusiness = new PriceFactorBL(pluginInitializer.Service, pluginInitializer.TracingService);

            // Target
            Entity targetEntity;
            pluginInitializer.PluginContext.GetContextInputEntity<Entity>(pluginInitializer.TargetKey, out targetEntity);

            // PreImage
            Entity preImageEntity;
            pluginInitializer.PluginContext.GetContextPreImages<Entity>(pluginInitializer.PreImgKey, out preImageEntity);

            if (targetEntity != null && preImageEntity != null)
            {
                var priceFactorValidationInput = new PriceFactorValidationInput();
                priceFactorValidationInput.BeginDate = targetEntity.Attributes.Contains("rnt_begindate") == true ? targetEntity.GetAttributeValue<DateTime>("rnt_begindate") : preImageEntity.GetAttributeValue<DateTime>("rnt_begindate");
                priceFactorValidationInput.EndDate = targetEntity.Attributes.Contains("rnt_enddate") == true ? targetEntity.GetAttributeValue<DateTime>("rnt_enddate") : preImageEntity.GetAttributeValue<DateTime>("rnt_enddate");
                priceFactorValidationInput.PriceFactorType = targetEntity.Attributes.Contains("rnt_pricefactortypecode") == true ? targetEntity.GetAttributeValue<OptionSetValue>("rnt_pricefactortypecode").Value : preImageEntity.GetAttributeValue<OptionSetValue>("rnt_pricefactortypecode").Value;

                var validationAttributeCollection = new List<PriceFactorValidationInput.ValidationAtrribute>();
                var validationAttribute = new PriceFactorValidationInput.ValidationAtrribute();

                var dummyBranchCodes = targetEntity.Attributes.Contains("rnt_dummy_branchcode") ? targetEntity.GetAttributeValue<string>("rnt_dummy_branchcode") :
                                                    preImageEntity.GetAttributeValue<string>("rnt_dummy_branchcode");
                pluginInitializer.TraceMe("dummyBranchCodes " + dummyBranchCodes);
                LoadOptionSetValueCollection("rnt_dummy_branchcode", dummyBranchCodes, out validationAttribute);
                validationAttributeCollection.Add(validationAttribute);

                var dummyGroupCodes = targetEntity.Attributes.Contains("rnt_dummy_groupcodeinformation") ? targetEntity.GetAttributeValue<string>("rnt_dummy_groupcodeinformation") :
                                    preImageEntity.GetAttributeValue<string>("rnt_dummy_groupcodeinformation");
                pluginInitializer.TraceMe("dummyGroupCodes " + dummyGroupCodes);
                LoadOptionSetValueCollection("rnt_dummy_groupcodeinformation", dummyGroupCodes, out validationAttribute);
                validationAttributeCollection.Add(validationAttribute);
              

                switch (priceFactorValidationInput.PriceFactorType)
                {
                    case (int)ClassLibrary._Enums_1033.rnt_PriceFactorType.PayMethod: /* Pay Method */
                        var paymentMethodCode = targetEntity.Attributes.Contains("rnt_paymethodcode") == true ? targetEntity.GetAttributeValue<OptionSetValue>("rnt_paymethodcode").Value : preImageEntity.GetAttributeValue<OptionSetValue>("rnt_paymethodcode").Value;
                        LoadValuePayMethod(paymentMethodCode.ToString(), out validationAttribute);
                        validationAttributeCollection.Add(validationAttribute);
                        break;
                    case (int)ClassLibrary._Enums_1033.rnt_PriceFactorType.ReservationChannel: /* Reservation Channel */
                        var reservationChannelCodes = targetEntity.Attributes.Contains("rnt_dummy_reservationchannelcode") == true ? targetEntity.GetAttributeValue<string>("rnt_dummy_reservationchannelcode") : preImageEntity.GetAttributeValue<string>("rnt_dummy_reservationchannelcode");
                        LoadOptionSetValueCollection("rnt_dummy_reservationchannelcode", reservationChannelCodes, out validationAttribute);
                        validationAttributeCollection.Add(validationAttribute);
                        break;
                    case (int)ClassLibrary._Enums_1033.rnt_PriceFactorType.Weekdays: /* Week Days */
                        var weekDaysCode = targetEntity.Attributes.Contains("rnt_dummy_weekdayscode") == true ? targetEntity.GetAttributeValue<string>("rnt_dummy_weekdayscode") : preImageEntity.GetAttributeValue<string>("rnt_dummy_weekdayscode");
                        LoadOptionSetValueCollection("rnt_dummy_weekdayscode", weekDaysCode, out validationAttribute);
                        validationAttributeCollection.Add(validationAttribute);
                        break;
                    case (int)ClassLibrary._Enums_1033.rnt_PriceFactorType.Customer: /* Week Days */
                        var segment = targetEntity.Attributes.Contains("rnt_dummy_segmentcode") == true ? targetEntity.GetAttributeValue<string>("rnt_dummy_segmentcode") : preImageEntity.GetAttributeValue<string>("rnt_dummy_segmentcode");
                        LoadOptionSetValueCollection("rnt_dummy_segmentcode", segment, out validationAttribute);
                        validationAttributeCollection.Add(validationAttribute);
                        break;
                }

                priceFactorValidationInput.AttributeToValidate = validationAttributeCollection;
                pluginInitializer.TraceMe("target entity contains " + targetEntity.Attributes.Contains("rnt_type"));
                pluginInitializer.TraceMe("preImageEntity entity contains " + preImageEntity.Attributes.Contains("rnt_type"));
                priceFactorValidationInput.type = targetEntity.Attributes.Contains("rnt_type") == true ?
                                                  targetEntity.GetAttributeValue<OptionSetValueCollection>("rnt_type").Select(p => p.Value.ToString()).ToList() : 
                                                  preImageEntity.GetAttributeValue<OptionSetValueCollection>("rnt_type").Select(p => p.Value.ToString()).ToList();
                pluginInitializer.TraceMe("before check");
                var errorMessage = priceFactorBusiness.PriceFactorValidationForUpdate(priceFactorValidationInput, pluginInitializer.PluginContext.PrimaryEntityId);

                if (string.IsNullOrWhiteSpace(errorMessage) == false)
                    pluginInitializer.PluginContext.ThrowException<InvalidPluginExecutionException>(errorMessage);
            }
        }
        #endregion

        #region HELPER METHODS
        // Tolga AYKURT - 14.03.2019
        private void LoadOptionSetValueCollection(string attributeName, string values, out PriceFactorValidationInput.ValidationAtrribute validationAtrribute)
        {
            validationAtrribute = new PriceFactorValidationInput.ValidationAtrribute();
            validationAtrribute.AttributeValues = new List<string>();
            validationAtrribute.AttributeName = attributeName;
            validationAtrribute.AttributeType = PriceFactorValidationInput.ValidationAtrribute.AttributeTypeEnum.MultiSelectOptionSet;

            foreach (var value in values.Split(','))
            {
                if (string.IsNullOrWhiteSpace(value) == false) validationAtrribute.AttributeValues.Add(value);
            }
        }

        // Tolga AYKURT - 14.03.2019
        private void LoadValuePayMethod(string value, out PriceFactorValidationInput.ValidationAtrribute validationAtrribute)
        {
            validationAtrribute = new PriceFactorValidationInput.ValidationAtrribute();
            validationAtrribute.AttributeValues = new List<string>() { value };
            validationAtrribute.AttributeName = "rnt_paymethodcode";
            validationAtrribute.AttributeType = PriceFactorValidationInput.ValidationAtrribute.AttributeTypeEnum.OptionSet;
        }
        #endregion
    }
}
