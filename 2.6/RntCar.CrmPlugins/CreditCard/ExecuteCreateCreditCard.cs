using Microsoft.Xrm.Sdk;
using Newtonsoft.Json;
using RntCar.BusinessLibrary.Business;
using RntCar.ClassLibrary;
using RntCar.SDK.Common;
using System;

namespace RntCar.CrmPlugins.CreditCard
{
    public class ExecuteCreateCreditCard : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            PluginInitializer initializer = new PluginInitializer(serviceProvider);
            initializer.TraceMe("ExecuteCreateCreditCard action started");

            string creditCardParameters;
            initializer.PluginContext.GetContextParameter<string>("creditCardParameters", out creditCardParameters);
            initializer.TraceMe("creditCardParameters" + creditCardParameters);

            try
            {
                var serializedCardParameter = JsonConvert.DeserializeObject<CreateCreditCardParameters>(creditCardParameters);
                CreditCardBL creditCardBL = new CreditCardBL(initializer.Service, initializer.TracingService);
                var response = creditCardBL.createCreditCard(serializedCardParameter);

                initializer.TraceMe("ExecuteCreateCreditCard Response:" + JsonConvert.SerializeObject(response));
                initializer.PluginContext.OutputParameters["ExecutionResult"] = JsonConvert.SerializeObject(response);

                initializer.TraceMe("ExecuteCreateCreditCard action end");

            }
            catch (Exception ex)
            {
                initializer.PluginContext.ThrowException<InvalidPluginExecutionException>(ex.Message);
            }
        }
    }
}
