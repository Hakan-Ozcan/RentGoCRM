using Microsoft.Xrm.Sdk;
using Newtonsoft.Json;
using RntCar.ClassLibrary;
using RntCar.IntegrationHelper;
using RntCar.SDK.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.CrmPlugins.BonusTransaction.Action
{
    public class ExecuteHopiReturn : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {

            PluginInitializer initializer = new PluginInitializer(serviceProvider);
            string startReturnTransactionParameters;
            initializer.PluginContext.GetContextParameter<string>("startReturnTransactionParameters", out startReturnTransactionParameters);

            HopiHelper hopiHelper = new HopiHelper(initializer.Service, initializer.TracingService);
            initializer.TraceMe("startReturnTransactionParameters : " + JsonConvert.SerializeObject(startReturnTransactionParameters));
            try
            {
                var startReturnTransactionParametersSerialized = JsonConvert.DeserializeObject<StartReturnTransactionRequest>(startReturnTransactionParameters);
                var response = hopiHelper.StartReturnTransaction(startReturnTransactionParametersSerialized);

                initializer.TraceMe("response : " + JsonConvert.SerializeObject(response));
                initializer.PluginContext.OutputParameters["startReturnTransactionResponse"] = JsonConvert.SerializeObject(response);

            }
            catch (Exception ex)
            {
                initializer.TraceMe($"Exception Message: {ex.Message}");
                initializer.PluginContext.ThrowException<InvalidPluginExecutionException>(ex.Message);
            }
        }
    }
}
