using Microsoft.Xrm.Sdk;
using Newtonsoft.Json;
using RntCar.BusinessLibrary;
using RntCar.BusinessLibrary.Business;
using RntCar.SDK.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.CrmPlugins
{
    public class ExecuteGetRevenueReport : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            PluginInitializer pluginInitializer = new PluginInitializer(serviceProvider);

            try
            {
                DateTime startDate;
                pluginInitializer.PluginContext.GetContextInputParameter<DateTime>("startDate", out startDate);
                pluginInitializer.TraceMe("Start date: " + startDate);

                DateTime endDate;
                pluginInitializer.PluginContext.GetContextInputParameter<DateTime>("endDate", out endDate);
                pluginInitializer.TraceMe("End date: " + endDate);

                ReportBL reportBL = new ReportBL(pluginInitializer.Service, pluginInitializer.TracingService);
                var response = reportBL.callGetRevenueReportService(startDate, endDate);
                pluginInitializer.PluginContext.OutputParameters["serviceResponse"] = JsonConvert.SerializeObject(response);
            }
            catch (Exception ex)
            {
                pluginInitializer.TraceMe("error : " + ex.Message);
                pluginInitializer.PluginContext.ThrowException<InvalidPluginExecutionException>(ex.Message);
            }
        }
    }
}
