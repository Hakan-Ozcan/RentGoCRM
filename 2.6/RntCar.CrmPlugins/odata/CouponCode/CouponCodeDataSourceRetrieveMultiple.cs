using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Newtonsoft.Json;
using RntCar.BusinessLibrary.Business;
using RntCar.SDK.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.CrmPlugins.odata.CouponCode
{
    public class CouponCodeDataSourceRetrieveMultiple : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            PluginInitializer pluginInitializer = new PluginInitializer(serviceProvider);
            try
            {
                var query = (QueryExpression)pluginInitializer.PluginContext.InputParameters["Query"];
                CouponCodeBL couponCodeBL = new CouponCodeBL(pluginInitializer.Service, pluginInitializer.TracingService);
                var results = couponCodeBL.getCouponCodes(query);
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
