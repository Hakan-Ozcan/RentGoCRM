using Microsoft.Xrm.Sdk;
using RntCar.BusinessLibrary.Business;
using RntCar.SDK.Common;
using System;

namespace RntCar.CrmPlugins.CreditCardSlip
{
    public class PreCreateCreditCardSlip_SetName : IPlugin
    {
        
        public void Execute(IServiceProvider serviceProvider)
        {
            PluginInitializer initializer = new PluginInitializer(serviceProvider);
            try
            {
                Entity creditCardSlipEntity;
                initializer.PluginContext.GetContextInputEntity<Entity>(initializer.TargetKey, out creditCardSlipEntity);

                CreditCardSlipBL creditCardSlipBL = new CreditCardSlipBL(initializer.Service, initializer.TracingService);
                creditCardSlipBL.setName(creditCardSlipEntity);
            }
            catch (Exception ex)
            {
                initializer.TraceMe("PreCreateCreditCardSlip_SetName  error : " + ex.Message);
            }
        }
    }
}
