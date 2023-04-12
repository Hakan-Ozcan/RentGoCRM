using Microsoft.Xrm.Sdk;
using RntCar.BusinessLibrary.Business;
using RntCar.SDK.Common;
using System;

namespace RntCar.CrmPlugins.Payment
{
    public class PreUpdatePayment_SetName : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            PluginInitializer initializer = new PluginInitializer(serviceProvider);
            if (initializer.PluginContext.Depth >= 3)
            {
                initializer.TraceMe("depth is : " + initializer.PluginContext.Depth);
                initializer.TraceMe("returning");
                return;
            }
            try
            {
                Entity paymentEntity;
                initializer.PluginContext.GetContextInputEntity<Entity>(initializer.TargetKey, out paymentEntity);

                Entity preImgPaymentEntity;
                initializer.PluginContext.GetContextPreImages<Entity>(initializer.PreImgKey, out preImgPaymentEntity);

                PaymentBL paymentBL = new PaymentBL(initializer.Service, initializer.TracingService);
                paymentBL.setName(paymentEntity, preImgPaymentEntity);
            }
            catch (Exception ex)
            {
                initializer.TraceMe("PreUpdatePayment_SetName  error : " + ex.Message);
            }
            
        }
    }
}
