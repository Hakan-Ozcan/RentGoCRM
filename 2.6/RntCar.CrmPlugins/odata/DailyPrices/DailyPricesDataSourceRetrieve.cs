using Microsoft.Xrm.Sdk;
using RntCar.SDK.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.CrmPlugins.odata.DailyPrices
{
    public class DailyPricesDataSourceRetrieve : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            //EntityReference target = (EntityReference)context.InputParameters["Target"];
            //PluginInitializer pluginInitializer = new PluginInitializer(serviceProvider);
            //Entity e = new Entity("rnt_dailyprice")
            //pluginInitializer.PluginContext.OutputParameters["BusinessEntity"] = joke;
        }
    }
}
