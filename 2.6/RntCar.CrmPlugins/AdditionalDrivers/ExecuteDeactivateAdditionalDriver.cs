using Microsoft.Xrm.Sdk;
using Newtonsoft.Json;
using RntCar.BusinessLibrary.Business;
using RntCar.SDK.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.CrmPlugins.AdditionalDrivers
{
    public class ExecuteDeactivateAdditionalDriver : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            PluginInitializer initializer = new PluginInitializer(serviceProvider);
            try
            {
                string contactId;
                initializer.PluginContext.GetContextParameter<string>("ContactId", out contactId);

                string contractId;
                initializer.PluginContext.GetContextParameter<string>("ContractId", out contractId);

                int langId;
                initializer.PluginContext.GetContextParameter<int>("LangId", out langId);

                initializer.TraceMe("contactId : " + contactId);
                initializer.TraceMe("contractId : " + contractId);
                initializer.TraceMe("langId : " + langId);

                initializer.TraceMe("deactivation start");
                AdditionalDriversBL additionalDriversBL = new AdditionalDriversBL(initializer.Service, initializer.TracingService);
                var response = additionalDriversBL.deactivateAdditionalDriverByContractandContactId(contactId, contractId);
                initializer.PluginContext.OutputParameters["AdditionalDriverDeactivateResponse"] = JsonConvert.SerializeObject(response);
                initializer.TraceMe("deactivation end");
            }
            catch (Exception ex)
            {
                initializer.PluginContext.ThrowException<InvalidPluginExecutionException>(ex.Message);
            }
        }
    }
}
