using Microsoft.Xrm.Sdk;
using Newtonsoft.Json;
using RntCar.BusinessLibrary.Business;
using RntCar.SDK.Common;
using System;

namespace RntCar.CrmPlugins.Report.Actions
{
    public class ExecuteGetEquipmentAvailabilityReport : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            PluginInitializer pluginInitializer = new PluginInitializer(serviceProvider);

            try
            {
                string publishDate;
                pluginInitializer.PluginContext.GetContextInputParameter<string>("publishDate", out publishDate);

                DateTime startDatetime;
                pluginInitializer.PluginContext.GetContextInputParameter<DateTime>("startDate", out startDatetime);
                startDatetime =  startDatetime.AddMinutes(StaticHelper.offset);

                DateTime endDatetime;
                pluginInitializer.PluginContext.GetContextInputParameter<DateTime>("endDate", out endDatetime);
                endDatetime = endDatetime.AddMinutes(StaticHelper.offset);
                pluginInitializer.TraceMe("publishDate" + publishDate);
                pluginInitializer.TraceMe("startDate" + startDatetime);
                pluginInitializer.TraceMe("endDatetime" + endDatetime);
                ReportBL reportBL = new ReportBL(pluginInitializer.Service, pluginInitializer.TracingService);
                var response = reportBL.callGetEquipmentAvailabilityService(startDatetime, endDatetime);

                //pluginInitializer.TraceMe("service response " + JsonConvert.SerializeObject(response));
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
