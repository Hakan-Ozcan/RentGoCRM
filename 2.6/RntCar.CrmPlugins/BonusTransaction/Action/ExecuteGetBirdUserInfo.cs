using Microsoft.Xrm.Sdk;
using Newtonsoft.Json;
using RntCar.BusinessLibrary.Repository;
using RntCar.ClassLibrary;
using RntCar.IntegrationHelper;
using RntCar.SDK.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.CrmPlugins.BonusTransaction.Action
{
    public class ExecuteGetBirdUserInfo : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            PluginInitializer initializer = new PluginInitializer(serviceProvider);
            string getBirdUserInfoParameters;
            initializer.PluginContext.GetContextParameter<string>("getBirdUserInfoParameters", out getBirdUserInfoParameters);

            HopiHelper hopiHelper = new HopiHelper(initializer.Service, initializer.TracingService);

            initializer.TraceMe("getBirdUserInfoParameters : " + JsonConvert.SerializeObject(getBirdUserInfoParameters));
            try
            {
                ConfigurationRepository configurationRepository = new ConfigurationRepository(initializer.Service);
                string[] parameters = configurationRepository.GetConfigurationByKey("hopiService.Parameters").Split(';');

                var getBirdUserInfoParametersSerialized = JsonConvert.DeserializeObject<GetBirdUserInfoRequest>(getBirdUserInfoParameters);
                getBirdUserInfoParametersSerialized.merchantCode = parameters[0];
                getBirdUserInfoParametersSerialized.storeCode = parameters[1];
                var response = hopiHelper.getBirdUserInfo(getBirdUserInfoParametersSerialized);

                initializer.TraceMe("response : " + JsonConvert.SerializeObject(response));
                initializer.PluginContext.OutputParameters["Response"] = JsonConvert.SerializeObject(response);

            }
            catch (Exception ex)
            {
                initializer.TraceMe($"Exception Message: {ex.Message}");
                initializer.PluginContext.ThrowException<InvalidPluginExecutionException>(ex.Message);

            }
        }
    }
}
