using Microsoft.Xrm.Sdk;
using Newtonsoft.Json;
using RntCar.BusinessLibrary.Business;
using RntCar.SDK.Common;
using System;

namespace RntCar.CrmPlugins.Report.Actions
{
    public class ExecuteGetDailyReport : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            PluginInitializer pluginInitializer = new PluginInitializer(serviceProvider);

            try
            {
                string GetDailyReportParameter;
                pluginInitializer.PluginContext.GetContextInputParameter<string>("GetDailyReportParameter", out GetDailyReportParameter);

                pluginInitializer.TraceMe("GetDailyReportParameter " + GetDailyReportParameter);
                var param = JsonConvert.DeserializeObject<DateParams>(GetDailyReportParameter);
                pluginInitializer.TraceMe("GetDailyReportParameter " + JsonConvert.SerializeObject(param));

                ReportBL reportBL = new ReportBL(pluginInitializer.Service, pluginInitializer.TracingService);
                var response = reportBL.callDailyReport(param.startDate, param.endDate);
                pluginInitializer.PluginContext.OutputParameters["GetDailyReportResponse"] = JsonConvert.SerializeObject(response);

            }
            catch (Exception ex)
            {
                pluginInitializer.PluginContext.ThrowException<InvalidPluginExecutionException>(ex.Message);
            }
        }
    }
    class DateParams
    {
        public DateTime startDate { get; set; }
        public DateTime endDate { get; set; }
    }
}
