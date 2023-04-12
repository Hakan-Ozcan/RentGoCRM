using Microsoft.Xrm.Sdk;
using RntCar.SDK.Common;
using System;

namespace RntCar.CrmPlugins.Invoice.Action
{
    public class ExecuteGetInvoiceDocument : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            PluginInitializer initializer = new PluginInitializer(serviceProvider);

            try
            {
                string logoInvoiceNumber;
                initializer.PluginContext.GetContextParameter<string>("logoInvoiceNumber", out logoInvoiceNumber);

                initializer.TraceMe("logoInvoiceNumber  : " + logoInvoiceNumber);
                LogoHelper logoHelper = new LogoHelper(initializer.Service, initializer.TracingService);
                var content  = logoHelper.getPDFContent(logoInvoiceNumber);
                initializer.TraceMe("content  : " + content);

                initializer.PluginContext.OutputParameters["logoInvoiceResponse"] = Convert.ToBase64String(content);
            }
            catch(Exception ex)
            {
                initializer.TraceMe("exception : " + ex.Message);
                initializer.PluginContext.ThrowException<InvalidPluginExecutionException>(ex.Message);
            }
        }
    }
}
