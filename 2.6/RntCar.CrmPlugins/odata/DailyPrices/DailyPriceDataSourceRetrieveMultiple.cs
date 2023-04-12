using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Newtonsoft.Json;
using RntCar.BusinessLibrary.Business;
using RntCar.SDK.Common;
using System;

namespace RntCar.CrmPlugins.odata.DailyPrices
{
    public class DailyPriceDataSourceRetrieveMultiple : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            PluginInitializer pluginInitializer = new PluginInitializer(serviceProvider);
            try
            {
                //todo will implement filter machanism
                var query = (QueryExpression)pluginInitializer.PluginContext.InputParameters["Query"];
      
                DailyPricesBL dailyPricesBL = new DailyPricesBL(pluginInitializer.Service, pluginInitializer.TracingService);
                var results = dailyPricesBL.getDailyPrices(query);
                pluginInitializer.PluginContext.OutputParameters["BusinessEntityCollection"] = results;
                pluginInitializer.TraceMe(JsonConvert.SerializeObject(results));
            }
            catch (Exception ex)
            {
                pluginInitializer.PluginContext.ThrowException<InvalidPluginExecutionException>(ex.Message);
            }
        }

    }
}
