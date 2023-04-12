using Microsoft.Xrm.Sdk;
using Newtonsoft.Json;
using RntCar.BusinessLibrary.Business;
using RntCar.ClassLibrary;
using RntCar.SDK.Common;
using System;
using System.Collections.Generic;

namespace RntCar.CrmPlugins.InvoiceAddress.actions
{
    public class ExecuteGetInvoiceAddresses
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            PluginInitializer initializer = new PluginInitializer(serviceProvider);

            string key;
            initializer.PluginContext.GetContextParameter<string>("govermentId", out key);
            initializer.TraceMe("key : " + key);

            var response = new List<InvoiceAddressData>();
            InvoiceAddressBL invoiceAddressBL = new InvoiceAddressBL(initializer.Service);
            response = invoiceAddressBL.getInvoiceAddressByGovermentIdOrByTaxNumber(key);

            initializer.TraceMe("invoice addresses : " + JsonConvert.SerializeObject(response));
            initializer.PluginContext.OutputParameters["InvoiceAddressesResponse"] = JsonConvert.SerializeObject(response);
        }
    }
}
