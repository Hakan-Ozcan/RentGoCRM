using Microsoft.Xrm.Sdk;
using RntCar.BusinessLibrary.Business;
using RntCar.SDK.Common;
using System;

namespace RntCar.CrmPlugins.BlackList
{
    // Tolga AYKURT - 04.03.2019
    public class ExecuteBlackListValidation : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            PluginInitializer initializer = new PluginInitializer(serviceProvider);
            String identityKeyInParam;
            initializer.PluginContext.GetContextParameter<string>("IdentityKey", out identityKeyInParam);

            int langId;
            initializer.PluginContext.GetContextParameter<int>("LangId", out langId);

            var blackListBL = new BlackListBL(initializer.Service, initializer.TracingService);

            initializer.PluginContext.OutputParameters["ValidationResponse"] = blackListBL.BlackListValidationReturnString(identityKeyInParam);
        }
    }
}
