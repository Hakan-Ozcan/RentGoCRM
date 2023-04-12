using Microsoft.Xrm.Sdk;
using RntCar.BusinessLibrary.Business;
using RntCar.ClassLibrary;
using RntCar.SDK.Common;
using System;

namespace RntCar.CrmPlugins.AvailabilityPriceLists.Create
{
    // Tolga AYKURT - 11.03.2019
    public class PreValidateCreateAvailabilityPriceList : IPlugin
    {
        // Tolga AYKURT - 11.03.2019
        public void Execute(IServiceProvider serviceProvider)
        {
            var initializer = new PluginInitializer(serviceProvider);
            var availabilityPriceListBL = new AvailabilityPriceListBL(initializer.Service);
            Entity targetEntity;
            initializer.PluginContext.GetContextInputEntity<Entity>(initializer.TargetKey, out targetEntity);

            if(targetEntity != null && 
                targetEntity.Attributes.Contains("rnt_maximumavailability") &&
                targetEntity.Attributes.Contains("rnt_minimumavailability") &&
                targetEntity.Attributes.Contains("rnt_pricelistid") &&
                targetEntity.Attributes.Contains("rnt_groupcodeid"))
            {
                string validationMessage = null;

                initializer.TraceMe("Group Code "  + targetEntity.GetAttributeValue<EntityReference>("rnt_groupcodeid").Id.ToString());
                initializer.TraceMe("Price List " + targetEntity.GetAttributeValue<EntityReference>("rnt_pricelistid").Id);
                initializer.TraceMe("MaximumAvailability " + targetEntity.GetAttributeValue<int>("rnt_maximumavailability"));
                initializer.TraceMe("MinimumAvailability " + targetEntity.GetAttributeValue<int>("rnt_minimumavailability"));

                var validationInput = new AvailabilityPriceListValidationInput
                {
                    groupCodeId = targetEntity.GetAttributeValue<EntityReference>("rnt_groupcodeid").Id,
                    InitiatingUserId = initializer.InitiatingUserId,
                    MaximumAvailability = targetEntity.GetAttributeValue<int>("rnt_maximumavailability"),
                    MinimumAvailability = targetEntity.GetAttributeValue<int>("rnt_minimumavailability"),
                    PriceListId = targetEntity.GetAttributeValue<EntityReference>("rnt_pricelistid").Id
                };

                if(availabilityPriceListBL.ValidateAvailabilityPriceListForCreate(validationInput, out validationMessage) == false)
                {
                    initializer.PluginContext.ThrowException<InvalidPluginExecutionException>(validationMessage);
                }
            }
        }
    }
}
