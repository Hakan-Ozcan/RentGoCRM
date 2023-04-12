using Microsoft.Xrm.Sdk;
using Newtonsoft.Json;
using RntCar.BusinessLibrary.Business;
using RntCar.ClassLibrary;
using RntCar.SDK.Common;
using System;

namespace RntCar.CrmPlugins.CreditCard
{
    public class ExecuteRetrieveInstallment : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            PluginInitializer initializer = new PluginInitializer(serviceProvider);
            CreditCardBL creditCardBL = new CreditCardBL(initializer.Service, initializer.TracingService);

            string retrieveInstallmentParameters;
            initializer.PluginContext.GetContextParameter<string>("retrieveInstallmentParameters", out retrieveInstallmentParameters);
            initializer.TraceMe("retrieveInstallmentParameters" + retrieveInstallmentParameters);
            try
            {
                var param = JsonConvert.DeserializeObject<RetrieveInstallmentParameters>(retrieveInstallmentParameters);
                var response = creditCardBL.retrieveInstallmentforGivenCard(param);
                initializer.TraceMe("retrieveInstallmentResponse" + JsonConvert.SerializeObject(response));
                initializer.PluginContext.OutputParameters["retrieveInstallmentResponse"] = JsonConvert.SerializeObject(response);

            }
            catch (Exception ex)
            {
                initializer.PluginContext.ThrowException<InvalidPluginExecutionException>(ex.Message);
            }

        }
    }
}
