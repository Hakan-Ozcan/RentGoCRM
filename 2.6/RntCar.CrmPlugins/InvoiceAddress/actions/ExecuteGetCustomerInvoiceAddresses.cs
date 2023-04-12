using Microsoft.Xrm.Sdk;
using Newtonsoft.Json;
using RntCar.BusinessLibrary.Business;
using RntCar.BusinessLibrary.Repository;
using RntCar.ClassLibrary;
using RntCar.SDK.Common;
using System;
using System.Collections.Generic;

namespace RntCar.CrmPlugins.InvoiceAddress.actions
{
    public class ExecuteGetCustomerInvoiceAddresses : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            PluginInitializer initializer = new PluginInitializer(serviceProvider);

            string individualCustomerId;
            initializer.PluginContext.GetContextParameter<string>("individualCustomerId", out individualCustomerId);

            string corporateCustomerId;
            initializer.PluginContext.GetContextParameter<string>("corporateCustomerId", out corporateCustomerId);

            string reservationId;
            initializer.PluginContext.GetContextParameter<string>("reservationId", out reservationId);

            initializer.TraceMe("individualCustomerId : " + individualCustomerId);
            initializer.TraceMe("corporateCustomerId : " + corporateCustomerId);
            initializer.TraceMe("reservationId : " + reservationId);

            Guid _corpId = Guid.Empty;

            var response = new List<InvoiceAddressData>();
            if (!string.IsNullOrEmpty(corporateCustomerId) &&
               Guid.TryParse(corporateCustomerId, out _corpId) &&
               _corpId != Guid.Empty)
            {
                initializer.TraceMe("getting corp address : " + _corpId);
                CorporateCustomerBL corporateCustomerBL = new CorporateCustomerBL(initializer.Service, initializer.TracingService);
                var r = corporateCustomerBL.getCorporateCustomerAddress(_corpId);
                response.Add(r);
            }
            else
            {
                initializer.TraceMe("individual address");
                InvoiceAddressBL invoiceAddressBL = new InvoiceAddressBL(initializer.Service);
                response = invoiceAddressBL.getCustomerInvoiceAddresses(individualCustomerId);
            }

            initializer.TraceMe("invoice adress : " + JsonConvert.SerializeObject(response));
            initializer.PluginContext.OutputParameters["InvoiceAddressResponse"] = JsonConvert.SerializeObject(response);
        }
    }
}
