using Microsoft.Xrm.Sdk;
using RntCar.BusinessLibrary.Business;
using RntCar.ClassLibrary.GroupCodeList.Validation;
using RntCar.SDK.Common;
using System;

namespace RntCar.CrmPlugins.GroupCodeListPrice.Create
{
    // Tolga AYKURT - 11.03.2019
    public class PreValidateCreateGroupCodeListPrice : IPlugin
    {
        // Tolga AYKURT - 11.03.2019
        public void Execute(IServiceProvider serviceProvider)
        {
            var initializer = new PluginInitializer(serviceProvider);
            var groupCodeListBL = new GroupCodeListPriceBL(initializer.Service);
            string validationMessage = null;
            Entity groupCodeListEntity;
            initializer.PluginContext.GetContextInputEntity<Entity>(initializer.TargetKey, out groupCodeListEntity);

            if(groupCodeListEntity != null &&
                groupCodeListEntity.Attributes.Contains("rnt_groupcodeid") &&
                groupCodeListEntity.Attributes.Contains("rnt_pricelistid") &&
                groupCodeListEntity.Attributes.Contains("rnt_minimumday") &&
                groupCodeListEntity.Attributes.Contains("rnt_maximumday"))
            {
                var isOK = groupCodeListBL.ValidateGroupCodeListForCreate(new GroupCodeListValidationInput
                {
                    GroupCodeId = groupCodeListEntity.GetAttributeValue<EntityReference>("rnt_groupcodeid").Id,
                    InitiatingUserId = initializer.InitiatingUserId,
                    PriceListId = groupCodeListEntity.GetAttributeValue<EntityReference>("rnt_pricelistid").Id,
                    MinDay = groupCodeListEntity.GetAttributeValue<int>("rnt_minimumday"),
                    MaxDay = groupCodeListEntity.GetAttributeValue<int>("rnt_maximumday")
                }, out validationMessage);

                if(isOK == false)
                    initializer.PluginContext.ThrowException<InvalidPluginExecutionException>(validationMessage);
            }
        }
    }
}
