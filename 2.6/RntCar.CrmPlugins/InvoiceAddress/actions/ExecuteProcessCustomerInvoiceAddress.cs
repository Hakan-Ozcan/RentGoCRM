using Microsoft.Xrm.Sdk;
using Newtonsoft.Json;
using RntCar.BusinessLibrary.Business;
using RntCar.ClassLibrary;
using RntCar.SDK.Common;
using System;

namespace RntCar.CrmPlugins.InvoiceAddress.actions
{
    public class ExecuteProcessCustomerInvoiceAddress : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            PluginInitializer initializer = new PluginInitializer(serviceProvider);

            string invoiceAddressParameters;
            initializer.PluginContext.GetContextParameter<string>("invoiceAddressParameters",out  invoiceAddressParameters);
            initializer.TraceMe("invoiceAddressParameters" + invoiceAddressParameters);
            try
            {
                var param = JsonConvert.DeserializeObject<InvoiceAddressCreateParameters>(invoiceAddressParameters);
                InvoiceAddressBL invoiceAddressBL = new InvoiceAddressBL(initializer.Service, initializer.TracingService);
                Guid invoiceAddressId = Guid.Empty;
                //need to update
                if (param.invoiceAddressId.HasValue)
                {
                    invoiceAddressBL.updateInvoiceAddress(param);
                    invoiceAddressId = param.invoiceAddressId.Value;
                }
                else
                {
                    invoiceAddressId = invoiceAddressBL.createInvoiceAddress(param);
                }
                InvoiceAddressProcessResponse invoiceAddressProcessResponse = new InvoiceAddressProcessResponse
                {
                    invoiceAddressId = invoiceAddressId,
                    ResponseResult = ResponseResult.ReturnSuccess()
                };
                initializer.PluginContext.OutputParameters["invoiceAddressResponse"] = JsonConvert.SerializeObject(invoiceAddressProcessResponse);

            }
            catch (Exception ex)
            {
                initializer.PluginContext.ThrowException<InvalidPluginExecutionException>(ex.Message);
            }
        }
    }
}
