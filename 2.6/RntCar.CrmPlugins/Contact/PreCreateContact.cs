using Microsoft.Xrm.Sdk;
using RntCar.BusinessLibrary.Business;
using RntCar.SDK.Common;
using System;

namespace RntCar.CrmPlugins.Contact
{
    public class PreCreateContact : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var initializer = new PluginInitializer(serviceProvider);
            Entity individualCustomerEntity;
            initializer.PluginContext.GetContextInputEntity<Entity>(initializer.TargetKey, out individualCustomerEntity);

            IndividualCustomerBL individualCustomerBL = new IndividualCustomerBL(initializer.Service, initializer.TracingService);
            individualCustomerBL.buildLogoAccountCode(individualCustomerEntity);
        }
    }
}
