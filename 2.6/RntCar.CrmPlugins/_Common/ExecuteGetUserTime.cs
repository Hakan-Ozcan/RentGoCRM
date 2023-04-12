using Microsoft.Xrm.Sdk;
using Newtonsoft.Json;
using RntCar.SDK.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.CrmPlugins._Common
{
    public class ExecuteGetUserTime : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            PluginInitializer pluginInitializer = new PluginInitializer(serviceProvider);
            XrmHelper xrmHelper = new XrmHelper(pluginInitializer.Service);
            var result = xrmHelper.getCurrentUserTimeInfo();

            pluginInitializer.PluginContext.OutputParameters["UserTimeInfo"] = JsonConvert.SerializeObject(result);
        }
    }
}
