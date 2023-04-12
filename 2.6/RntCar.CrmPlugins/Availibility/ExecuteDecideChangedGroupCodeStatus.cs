using Microsoft.Xrm.Sdk;
using Newtonsoft.Json;
using RntCar.BusinessLibrary.Business;
using RntCar.ClassLibrary.MongoDB;
using RntCar.SDK.Common;
using System;
using System.Collections.Generic;

namespace RntCar.CrmPlugins.Availibility
{
    public class ExecuteDecideChangedGroupCodeStatus : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            PluginInitializer initializer = new PluginInitializer(serviceProvider);

            String CurrentGroupInformation;
            initializer.PluginContext.GetContextParameter<string>("CurrentGroupInformation", out CurrentGroupInformation);

            String ChangedGroupInformation;
            initializer.PluginContext.GetContextParameter<string>("ChangedGroupInformation", out ChangedGroupInformation);
            initializer.TraceMe("CurrentGroupInformation" + CurrentGroupInformation);
            initializer.TraceMe("ChangedGroupInformation" + ChangedGroupInformation);

            AvailibilityBL availibilityBL = new AvailibilityBL(initializer.Service, initializer.TracingService);
            var currentGroupCode = JsonConvert.DeserializeObject<AvailabilityData>(CurrentGroupInformation);
            var changedGroupCode = JsonConvert.DeserializeObject<List<AvailabilityData>>(ChangedGroupInformation);

            var response = availibilityBL.decideChangedGroupCodeStatus(currentGroupCode, changedGroupCode);
            //initializer.TraceMe("response" + JsonConvert.SerializeObject(response));
            initializer.PluginContext.OutputParameters["ResponseResult"] = JsonConvert.SerializeObject(response);
        }
    }
}
