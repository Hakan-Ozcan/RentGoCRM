using Microsoft.Xrm.Sdk;
using Newtonsoft.Json;
using RntCar.BusinessLibrary.Business;
using RntCar.SDK.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.CrmPlugins.Report.Actions
{
    public class ExecuteGetContractsWithFaultyInvoiceReport : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            PluginInitializer pluginInitializer = new PluginInitializer(serviceProvider);

            try
            {
                pluginInitializer.TraceMe("getContractsIsDebitReport");
                int dayFilter;
                pluginInitializer.PluginContext.GetContextInputParameter<int>("dayFilter", out dayFilter);

                string branchId;
                pluginInitializer.PluginContext.GetContextInputParameter<string>("branchId", out branchId);

                ReportBL reportBL = new ReportBL(pluginInitializer.Service, pluginInitializer.TracingService);

                pluginInitializer.TraceMe("dayFilter : " + dayFilter);
                pluginInitializer.TraceMe("branchId : " + branchId);

                var response = reportBL.getContractsIsDebitReport(branchId, dayFilter);
                pluginInitializer.PluginContext.OutputParameters["ServiceResponse"] = JsonConvert.SerializeObject(response);

            }
            catch (Exception ex)
            {
                pluginInitializer.PluginContext.ThrowException<InvalidPluginExecutionException>(ex.Message);
            }
        }
    }
}
