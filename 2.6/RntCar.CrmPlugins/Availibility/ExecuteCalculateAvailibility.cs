using Microsoft.Xrm.Sdk;
using Newtonsoft.Json;
using RntCar.BusinessLibrary.Business;
using RntCar.ClassLibrary.MongoDB;
using RntCar.SDK.Common;
using System;

namespace RntCar.CrmPlugins.Availibility
{
    public class ExecuteCalculateAvailibility : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            PluginInitializer initializer = new PluginInitializer(serviceProvider);
            string availibilityParameters;
            initializer.PluginContext.GetContextParameter<string>("AvailibilityParameters", out availibilityParameters);

            int langId;
            initializer.PluginContext.GetContextParameter<int>("LangId", out langId);

            AvailibilityBL availibilityBL = new AvailibilityBL(initializer.Service, initializer.TracingService);
            //initializer.TraceMe("availability start");
           
            var param = JsonConvert.DeserializeObject<AvailabilityParameters>(availibilityParameters);
            initializer.PluginContext.OutputParameters["AvailibilityResponse"] =  availibilityBL.calculateAvailibility(param, langId);
            initializer.TraceMe("availibilityParameters" + JsonConvert.SerializeObject(param));
        }
    }
}
