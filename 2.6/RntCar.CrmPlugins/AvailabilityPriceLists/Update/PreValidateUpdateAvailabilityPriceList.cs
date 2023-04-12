using Microsoft.Xrm.Sdk;
using RntCar.BusinessLibrary.Business;
using RntCar.ClassLibrary;
using RntCar.SDK.Common;
using System;

namespace RntCar.CrmPlugins.AvailabilityPriceLists.Update
{
    // Tolga AYKURT - 11.03.2019
    public class PreValidateUpdateAvailabilityPriceList : IPlugin
    {
        // Tolga AYKURT - 11.03.2019
        public void Execute(IServiceProvider serviceProvider)
        {
            var initializer = new PluginInitializer(serviceProvider);
            var availabilityPriceListBL = new AvailabilityPriceListBL(initializer.Service);

            // Target
            Entity targetEntity;
            initializer.PluginContext.GetContextInputEntity<Entity>(initializer.TargetKey, out targetEntity);

            // PreImage
            Entity preImgEntity;
            initializer.PluginContext.GetContextPreImages<Entity>(initializer.PreImgKey, out preImgEntity);

            if(targetEntity != null && preImgEntity != null)
            {
                var validationInput = new AvailabilityPriceListValidationInput();
                string validationMessage = null;

                if (targetEntity.Attributes.Contains("rnt_maximumavailability"))
                    validationInput.MaximumAvailability = targetEntity.GetAttributeValue<int>("rnt_maximumavailability");
                else
                    validationInput.MaximumAvailability = preImgEntity.GetAttributeValue<int>("rnt_maximumavailability");

                if (targetEntity.Attributes.Contains("rnt_minimumavailability"))
                    validationInput.MinimumAvailability = targetEntity.GetAttributeValue<int>("rnt_minimumavailability");
                else
                    validationInput.MinimumAvailability = preImgEntity.GetAttributeValue<int>("rnt_minimumavailability");

                if (targetEntity.Attributes.Contains("rnt_pricelistid"))
                    validationInput.PriceListId = targetEntity.GetAttributeValue<EntityReference>("rnt_pricelistid").Id;
                else
                    validationInput.PriceListId = preImgEntity.GetAttributeValue<EntityReference>("rnt_pricelistid").Id;

                if (targetEntity.Attributes.Contains("rnt_groupcodeid"))
                    validationInput.groupCodeId = targetEntity.GetAttributeValue<EntityReference>("rnt_groupcodeid").Id;
                else
                    validationInput.groupCodeId = preImgEntity.GetAttributeValue<EntityReference>("rnt_groupcodeid").Id;

                validationInput.InitiatingUserId = initializer.InitiatingUserId;

                if(availabilityPriceListBL.ValidateAvailabilityPriceListForUpdate(validationInput, initializer.PluginContext.PrimaryEntityId, out validationMessage) == false)
                {
                    initializer.PluginContext.ThrowException<InvalidPluginExecutionException>(validationMessage);
                }
            }
        }
    }
}
