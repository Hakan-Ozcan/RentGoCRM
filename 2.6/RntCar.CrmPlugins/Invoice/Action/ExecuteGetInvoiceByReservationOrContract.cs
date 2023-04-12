using Microsoft.Xrm.Sdk;
using Newtonsoft.Json;
using RntCar.BusinessLibrary.Business;
using RntCar.ClassLibrary;
using RntCar.SDK.Common;
using System;

namespace RntCar.CrmPlugins.Invoice.Action
{
    public class ExecuteGetInvoiceByReservationOrContract : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            PluginInitializer initializer = new PluginInitializer(serviceProvider);
            try
            {
                string invoiceParameters;
                initializer.PluginContext.GetContextParameter<string>("InvoiceParameters", out invoiceParameters);
                initializer.TraceMe("InvoiceParameters" + invoiceParameters);

                var parameters = JsonConvert.DeserializeObject<GetInvoiceByReservationOrContractParameter>(invoiceParameters);

                InvoiceBL invoiceBL = new InvoiceBL(initializer.Service);
                var response = invoiceBL.getInvoiceByReservationOrContractAsInvoiceAddress(parameters);

                if (!response.ResponseResult.Result)
                    initializer.PluginContext.ThrowException<InvalidPluginExecutionException>(response.ResponseResult.ExceptionDetail);

                initializer.TraceMe("response " + JsonConvert.SerializeObject(response));

                initializer.PluginContext.OutputParameters["InvoiceResponse"] = JsonConvert.SerializeObject(response);
            }
            catch (Exception ex)
            {
                initializer.PluginContext.ThrowException<InvalidPluginExecutionException>(ex.Message);
            }
        }
    }
}
