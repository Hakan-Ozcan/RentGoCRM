using Microsoft.Xrm.Sdk;
using Newtonsoft.Json;
using RestSharp;
using RntCar.BusinessLibrary.Business;
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
    public class ExecuteHopiSales : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            PluginInitializer initializer = new PluginInitializer(serviceProvider);

            string notifyCheckoutParameters;
            initializer.PluginContext.GetContextParameter<string>("notifyCheckoutParameters", out notifyCheckoutParameters);

            HopiHelper hopiHelper = new HopiHelper(initializer.Service, initializer.TracingService);

            initializer.TraceMe("notifyCheckoutParameters : " + JsonConvert.SerializeObject(notifyCheckoutParameters));
            try
            {
                var notifyCheckoutParametersSerialized = JsonConvert.DeserializeObject<NotifyCheckoutRequest>(notifyCheckoutParameters);
                var response = hopiHelper.NotifyCheckout(notifyCheckoutParametersSerialized);

                initializer.TraceMe("response : " + JsonConvert.SerializeObject(response));
                initializer.PluginContext.OutputParameters["notifyCheckoutResponse"] = JsonConvert.SerializeObject(response);

            }
            catch (Exception ex)
            {
                initializer.TraceMe($"Exception Message: {ex.Message}");
                initializer.PluginContext.ThrowException<InvalidPluginExecutionException>(ex.Message);
            }
        }
    }
}
