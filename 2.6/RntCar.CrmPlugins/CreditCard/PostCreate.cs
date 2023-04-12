using Microsoft.Xrm.Sdk;
using RntCar.BusinessLibrary.Business;
using RntCar.SDK.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.CrmPlugins.CreditCard
{
    public class PostCreate : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            PluginInitializer initializer = new PluginInitializer(serviceProvider);
            try
            {
                Entity creditCard;
                initializer.PluginContext.GetContextInputEntity<Entity>(initializer.TargetKey, out creditCard);
                EntityReference customerRef = creditCard.GetAttributeValue<EntityReference>("rnt_contactid");
                OptionSetValue provider = creditCard.GetAttributeValue<OptionSetValue>("rnt_provider");

                initializer.TraceMe("customerRef : " + customerRef.Id);
                initializer.TraceMe("provider : " + provider.Value);
                CreditCardBL CreditCardBL = new CreditCardBL(initializer.Service, initializer.TracingService);
                CreditCardBL.checkFraudCustomerControl(customerRef.Id, provider.Value);
            }
            catch (Exception ex)
            {
                initializer.TraceMe("PostCreate  error : " + ex.Message);
            }
        }
    }
}
