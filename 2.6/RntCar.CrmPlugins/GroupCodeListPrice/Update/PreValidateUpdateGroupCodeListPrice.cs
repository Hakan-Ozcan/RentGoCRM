using Microsoft.Xrm.Sdk;
using RntCar.BusinessLibrary.Business;
using RntCar.ClassLibrary.GroupCodeList.Validation;
using RntCar.SDK.Common;
using System;

namespace RntCar.CrmPlugins.GroupCodeListPrice.Update
{
    // Tolga AYKURT - 11.03.2019
    public class PreValidateUpdateGroupCodeListPrice : IPlugin
    {
        // Tolga AYKURT - 11.03.2019
        public void Execute(IServiceProvider serviceProvider)
        {
            var initializer = new PluginInitializer(serviceProvider);
            var groupCodeListBL = new GroupCodeListPriceBL(initializer.Service);

            // Target
            Entity groupCodeListEntity;
            initializer.PluginContext.GetContextInputEntity<Entity>(initializer.TargetKey, out groupCodeListEntity);

            // PreImage
            Entity preImgGroupCodeListEntity;
            initializer.PluginContext.GetContextPreImages<Entity>(initializer.PreImgKey, out preImgGroupCodeListEntity);

            if(groupCodeListEntity != null && preImgGroupCodeListEntity != null)
            {
                var groupCodeListValidationInput = new GroupCodeListValidationInput();

                if (groupCodeListEntity.Attributes.Contains("rnt_groupcodeid"))
                    groupCodeListValidationInput.GroupCodeId = groupCodeListEntity.GetAttributeValue<EntityReference>("rnt_groupcodeid").Id;
                else
                    groupCodeListValidationInput.GroupCodeId = preImgGroupCodeListEntity.GetAttributeValue<EntityReference>("rnt_groupcodeid").Id;

                if (groupCodeListEntity.Attributes.Contains("rnt_pricelistid"))
                    groupCodeListValidationInput.PriceListId = groupCodeListEntity.GetAttributeValue<EntityReference>("rnt_pricelistid").Id;
                else
                    groupCodeListValidationInput.PriceListId = preImgGroupCodeListEntity.GetAttributeValue<EntityReference>("rnt_pricelistid").Id;

                if (groupCodeListEntity.Attributes.Contains("rnt_minimumday"))
                    groupCodeListValidationInput.MinDay = groupCodeListEntity.GetAttributeValue<int>("rnt_minimumday");
                else
                    groupCodeListValidationInput.MinDay = preImgGroupCodeListEntity.GetAttributeValue<int>("rnt_minimumday");

                if (groupCodeListEntity.Attributes.Contains("rnt_maximumday"))
                    groupCodeListValidationInput.MaxDay = groupCodeListEntity.GetAttributeValue<int>("rnt_maximumday");
                else
                    groupCodeListValidationInput.MaxDay = preImgGroupCodeListEntity.GetAttributeValue<int>("rnt_maximumday");

                string validationMessage = null;
                var isOK = groupCodeListBL.ValidateGroupCodeListForUpdate(groupCodeListValidationInput, initializer.PluginContext.PrimaryEntityId, out validationMessage);
                
                if (isOK == false)
                    initializer.PluginContext.ThrowException<InvalidPluginExecutionException>(validationMessage);
            }
        }
    }
}
