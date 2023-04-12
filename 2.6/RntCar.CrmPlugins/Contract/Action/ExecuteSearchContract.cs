using Microsoft.Xrm.Sdk;
using Newtonsoft.Json;
using RntCar.BusinessLibrary.Business;
using RntCar.ClassLibrary;
using RntCar.SDK.Common;
using System;

namespace RntCar.CrmPlugins.Contract.Action
{
    public class ExecuteSearchContract : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            PluginInitializer initializer = new PluginInitializer(serviceProvider);
            try
            {
                string contractSearchParameters;
                initializer.PluginContext.GetContextParameter<string>("ContractSearchParameters", out contractSearchParameters);

                int langId;
                initializer.PluginContext.GetContextParameter<int>("langId", out langId);

                var param = JsonConvert.DeserializeObject<ContractSearchParameters>(contractSearchParameters);
                ContractBL contractBL = new ContractBL(initializer.Service, initializer.TracingService);
                var response = contractBL.searchContractByParameters(param, langId);

                initializer.PluginContext.OutputParameters["ContractSearchResponse"] = JsonConvert.SerializeObject(response);
            }
            catch (Exception ex)
            {
                initializer.PluginContext.ThrowException<InvalidPluginExecutionException>(ex.Message);
            }
        }
    }
}
