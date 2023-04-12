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
    /// Create mesajının PreValidate stage'inde çalışır. Price factor ile ilgili gerekli validasyonları yapar.
    /// </summary>
    public class PreValidationCreatePriceFactor : IPlugin
    {
        #region METHODS
        // Tolga AYKURT - 07.03.2019
        public void Execute(IServiceProvider serviceProvider)
        {
            var pluginInitializer = new PluginInitializer(serviceProvider);
            var priceFactorBusiness = new PriceFactorBL(pluginInitializer.Service, pluginInitializer.TracingService);
            Entity targetEntity;
            pluginInitializer.PluginContext.GetContextInputEntity<Entity>(pluginInitializer.TargetKey, out targetEntity);

            var priceFactorValidationInput = new PriceFactorValidationInput
            {
                BeginDate = targetEntity.GetAttributeValue<DateTime>("rnt_begindate"),
                EndDate = targetEntity.GetAttributeValue<DateTime>("rnt_enddate"),
                PriceFactorType = targetEntity.GetAttributeValue<OptionSetValue>("rnt_pricefactortypecode").Value,
                PluginInitializerUserId = pluginInitializer.InitiatingUserId
            };
            pluginInitializer.TraceMe("checking priceFactorValidationInput.PriceFactorType ");

            var validationAttributeCollection = new List<PriceFactorValidationInput.ValidationAtrribute>();
            var validationAttribute = new PriceFactorValidationInput.ValidationAtrribute();

            LoadOptionSetValueCollection("rnt_dummy_branchcode", targetEntity.GetAttributeValue<string>("rnt_dummy_branchcode"), out validationAttribute);
            validationAttributeCollection.Add(validationAttribute);

            LoadOptionSetValueCollection("rnt_dummy_groupcodeinformation", targetEntity.GetAttributeValue<string>("rnt_dummy_groupcodeinformation"), out validationAttribute);
            validationAttributeCollection.Add(validationAttribute);

            switch (priceFactorValidationInput.PriceFactorType)
            {
                case (int)ClassLibrary._Enums_1033.rnt_PriceFactorType.PayMethod: /* Pay Method */
                    LoadValuePayMethod(targetEntity.GetAttributeValue<OptionSetValue>("rnt_paymethodcode").Value.ToString(), out validationAttribute);
                    validationAttributeCollection.Add(validationAttribute);
                    break;
                case (int)ClassLibrary._Enums_1033.rnt_PriceFactorType.ReservationChannel: /* Reservation Channel */
                    LoadOptionSetValueCollection("rnt_dummy_reservationchannelcode", targetEntity.GetAttributeValue<string>("rnt_dummy_reservationchannelcode"), out validationAttribute);
                    validationAttributeCollection.Add(validationAttribute);
                    break;
                case (int)ClassLibrary._Enums_1033.rnt_PriceFactorType.Weekdays: /* Week Days */
                    LoadOptionSetValueCollection("rnt_dummy_weekdayscode", targetEntity.GetAttributeValue<string>("rnt_dummy_weekdayscode"), out validationAttribute);
                    validationAttributeCollection.Add(validationAttribute);
                    break;
                case (int)ClassLibrary._Enums_1033.rnt_PriceFactorType.Customer: /* Week Days */
                    LoadOptionSetValueCollection("rnt_dummy_segmentcode", targetEntity.GetAttributeValue<string>("rnt_dummy_segmentcode"), out validationAttribute);
                    validationAttributeCollection.Add(validationAttribute);
                    break;
            }

            priceFactorValidationInput.AttributeToValidate = validationAttributeCollection;
            priceFactorValidationInput.type = targetEntity.GetAttributeValue<OptionSetValueCollection>("rnt_type").Select(p => p.Value.ToString()).ToList();

            pluginInitializer.TraceMe("checking PriceFactorValidationForCreate ");
            var errorMessage = priceFactorBusiness.PriceFactorValidationForCreate(priceFactorValidationInput);

            if (string.IsNullOrWhiteSpace(errorMessage) == false)
                pluginInitializer.PluginContext.ThrowException<InvalidPluginExecutionException>(errorMessage);
        }
        #endregion

        #region HELPER METHODS
        // Tolga AYKURT - 14.03.2019
        private void LoadOptionSetValueCollection(string attributeName, string values, out PriceFactorValidationInput.ValidationAtrribute validationAtrribute)
        {
            validationAtrribute = new PriceFactorValidationInput.ValidationAtrribute();
            validationAtrribute.AttributeType = PriceFactorValidationInput.ValidationAtrribute.AttributeTypeEnum.MultiSelectOptionSet;
            validationAtrribute.AttributeValues = new List<string>();
            validationAtrribute.AttributeName = attributeName;

            foreach (var value in values.Split(','))
            {
                if (string.IsNullOrWhiteSpace(value) == false) validationAtrribute.AttributeValues.Add(value);
            }
        }

        // Tolga AYKURT - 14.03.2019
        private void LoadValuePayMethod(string value, out PriceFactorValidationInput.ValidationAtrribute validationAtrribute)
        {
            validationAtrribute = new PriceFactorValidationInput.ValidationAtrribute();
            validationAtrribute.AttributeType = PriceFactorValidationInput.ValidationAtrribute.AttributeTypeEnum.OptionSet;
            validationAtrribute.AttributeValues = new List<string>() { value };
            validationAtrribute.AttributeName = "rnt_paymethodcode";
        }
        #endregion
    }
}
