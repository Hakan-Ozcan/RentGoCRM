using Microsoft.Xrm.Sdk;
using RntCar.BusinessLibrary.Business;
using RntCar.SDK.Common;
using System;

namespace RntCar.CrmPlugins.CreditCardSlip
{
    public class PostCreateCreditCardSlip_UpdatePayment : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            PluginInitializer initializer = new PluginInitializer(serviceProvider);
            try
            {
                Entity creditCardSlipEntity;
                initializer.PluginContext.GetContextInputEntity<Entity>(initializer.TargetKey, out creditCardSlipEntity);

                CreditCardSlipBL creditCardSlipBL = new CreditCardSlipBL(initializer.Service, initializer.TracingService);
                creditCardSlipBL.updatePaymentWithCreditCardSlip(creditCardSlipEntity.Id,
                                                                 creditCardSlipEntity.GetAttributeValue<EntityReference>("rnt_paymentid").Id);

                //update credit cardslip contract lookup
                creditCardSlipBL.updateCreditCardSlipContract(creditCardSlipEntity);

            }
            catch (Exception ex)
            {
                initializer.TraceMe("PreCreateCreditCardSlip_UpdatePayment  error : " + ex.Message);
            }
        }
    }
}
