using Microsoft.Xrm.Sdk;
using RntCar.BusinessLibrary.Business;
using RntCar.ClassLibrary.PriceList.Validation;
using RntCar.SDK.Common;
using System;

namespace RntCar.CrmPlugins.PriceList.Validation
{
    // Tolga AYKURT - 10.03.2019
    public class PreUpdateValidatePriceList : IPlugin
    {
        // Tolga AYKURT - 10.03.2019
        public void Execute(IServiceProvider serviceProvider)
        {
            var pluginInitializer = new PluginInitializer(serviceProvider);
            var priceListValidationInput = new PriceListValidationInput();
            var priceListBusiness = new PriceFactorValidation(pluginInitializer.Service, pluginInitializer.TracingService);

            // PreImage
            Entity preImgPriceListEntity;
            pluginInitializer.PluginContext.GetContextPreImages<Entity>(pluginInitializer.PreImgKey, out preImgPriceListEntity);

            // Target
            Entity priceListEntity;
            pluginInitializer.PluginContext.GetContextInputEntity<Entity>(pluginInitializer.TargetKey, out priceListEntity);

            if(priceListEntity != null && preImgPriceListEntity != null)
            {
                if (priceListEntity.Attributes.Contains("rnt_begindate"))
                    priceListValidationInput.BeginDate = priceListEntity.GetAttributeValue<DateTime>("rnt_begindate");
                else
                    priceListValidationInput.BeginDate = preImgPriceListEntity.GetAttributeValue<DateTime>("rnt_begindate");

                if (priceListEntity.Attributes.Contains("rnt_enddate"))
                    priceListValidationInput.EndDate = priceListEntity.GetAttributeValue<DateTime>("rnt_enddate");
                else
                    priceListValidationInput.EndDate = preImgPriceListEntity.GetAttributeValue<DateTime>("rnt_enddate");

                if (priceListEntity.Attributes.Contains("rnt_pricetypecode"))
                    priceListValidationInput.PriceListTypeCode = priceListEntity.GetAttributeValue<OptionSetValue>("rnt_pricetypecode").Value;
                else
                    priceListValidationInput.PriceListTypeCode = preImgPriceListEntity.GetAttributeValue<OptionSetValue>("rnt_pricetypecode").Value;

                if (priceListEntity.Attributes.Contains("rnt_pricecodeid"))
                    priceListValidationInput.PriceCodeId = priceListEntity.GetAttributeValue<EntityReference>("rnt_pricecodeid").Id;
                else
                    priceListValidationInput.PriceCodeId = preImgPriceListEntity.GetAttributeValue<EntityReference>("rnt_pricecodeid").Id;

                priceListValidationInput.PluginInitializerUserId = pluginInitializer.InitiatingUserId;

                var errorMessage = priceListBusiness.PriceValidationForUpdate(priceListValidationInput, pluginInitializer.PluginContext.PrimaryEntityId);

                if (string.IsNullOrWhiteSpace(errorMessage) == false)
                    pluginInitializer.PluginContext.ThrowException<InvalidPluginExecutionException>(errorMessage);
            }
        }
    }
}
