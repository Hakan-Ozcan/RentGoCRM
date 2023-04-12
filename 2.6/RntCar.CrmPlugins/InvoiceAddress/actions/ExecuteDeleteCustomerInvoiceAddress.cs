using Microsoft.Xrm.Sdk;
using Newtonsoft.Json;
using RntCar.BusinessLibrary.Business;
using RntCar.SDK.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.CrmPlugins.InvoiceAddress.actions
{
    public class ExecuteDeleteCustomerInvoiceAddress : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            PluginInitializer initializer = new PluginInitializer(serviceProvider);

            ConfigurationBL configurationBL = new ConfigurationBL(initializer.Service);
            var id  =configurationBL.GetConfigurationByName("CrmAdminGuid");

            PluginInitializer initializerAdmin = new PluginInitializer(serviceProvider, id);

            string invoiceAddressId;
            initializerAdmin.PluginContext.GetContextParameter<string>("invoiceAddressId", out invoiceAddressId);
        
            try
            {
                InvoiceAddressBL invoiceAddressBL = new InvoiceAddressBL(initializerAdmin.Service, initializerAdmin.TracingService);
                var response = invoiceAddressBL.deleteInvoiceAddress(new Guid(invoiceAddressId));
                initializerAdmin.PluginContext.OutputParameters["deleteInvoiceAddressResponse"] = JsonConvert.SerializeObject(response);
            }
            catch (Exception ex)
            {
                initializerAdmin.PluginContext.ThrowException<InvalidPluginExecutionException>(ex.Message);
            }
        }
    }
}
