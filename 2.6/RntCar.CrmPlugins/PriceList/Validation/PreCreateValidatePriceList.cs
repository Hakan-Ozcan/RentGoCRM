using Microsoft.Xrm.Sdk;
using RntCar.BusinessLibrary.Business;
using RntCar.ClassLibrary.PriceList.Validation;
using RntCar.SDK.Common;
using System;

namespace RntCar.CrmPlugins.PriceList.Validation
{
    // Tolga AYKURT - 07.03.2019
    /// <summary>
    /// Create ve Update mesajlarının PreValidate stage'inde çalışır. PriceList ile ilgili gerekli validasyonları yapar.
    /// </summary>
    public class PreCreateValidatePriceList : IPlugin
    {
        // Tolga AYKURT - 07.03.2019
        public void Execute(IServiceProvider serviceProvider)
        {
            var pluginInitializer = new PluginInitializer(serviceProvider);
            var priceListBusiness = new PriceFactorValidation(pluginInitializer.Service, pluginInitializer.TracingService);
            Entity pluginInputEntity;
            pluginInitializer.PluginContext.GetContextInputEntity<Entity>(pluginInitializer.TargetKey, out pluginInputEntity);
            
            var errorMessage = priceListBusiness.PriceValidationForCreate(new PriceListValidationInput
            {
                BeginDate = pluginInputEntity.GetAttributeValue<DateTime>("rnt_begindate"),
                EndDate = pluginInputEntity.GetAttributeValue<DateTime>("rnt_enddate"),
                PriceListTypeCode = pluginInputEntity.GetAttributeValue<OptionSetValue>("rnt_pricetypecode").Value,
                PriceCodeId = pluginInputEntity.GetAttributeValue<EntityReference>("rnt_pricecodeid").Id,
                PluginInitializerUserId = pluginInitializer.InitiatingUserId
            });

            if (string.IsNullOrWhiteSpace(errorMessage) == false)
                pluginInitializer.PluginContext.ThrowException<InvalidPluginExecutionException>(errorMessage);
        }
    }
}
