using Microsoft.Xrm.Sdk;
using RntCar.BusinessLibrary.Business;
using RntCar.SDK.Common;
using System;

namespace RntCar.CrmPlugins.ContractItem.Create
{
    public class PreCreateContractItem : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            PluginInitializer initializer = new PluginInitializer(serviceProvider);
            Entity entity;
            initializer.PluginContext.GetContextInputEntity(initializer.TargetKey, out entity);

            ContractItemBL contractItemBL = new ContractItemBL(initializer.Service, initializer.TracingService);
            contractItemBL.removeTaxAmount(entity);
        }
    }
}
