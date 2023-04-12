using Microsoft.Xrm.Sdk;
using Newtonsoft.Json;
using RntCar.BusinessLibrary.Business;
using RntCar.SDK.Common;
using System;

namespace RntCar.CrmPlugins.Availibility
{
    public class ExecuteCheckAvailibility : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            PluginInitializer initializer = new PluginInitializer(serviceProvider);
            String availibilityParameters;
            initializer.PluginContext.GetContextInputParameter<string>("AvailibilityParameters", out availibilityParameters);

            int langId;
            initializer.PluginContext.GetContextParameter<int>("langId", out langId);

            int channel;
            initializer.PluginContext.GetContextParameter<int>("channel", out channel);

            AvailibilityBL availibilityBL = new AvailibilityBL(initializer, initializer.Service, initializer.TracingService);
            initializer.TraceMe("AvailibilityParameters" + availibilityParameters);
            initializer.TraceMe("channel" + channel);
            var result = availibilityBL.CheckAvailibility(availibilityParameters, langId, channel);

            initializer.PluginContext.OutputParameters["AvailibilityResponse"] = JsonConvert.SerializeObject(result);
        }
    }
}
