using Microsoft.Xrm.Sdk.Workflow;
using RntCar.BusinessLibrary.Business;
using RntCar.SDK.Common;
using System;
using System.Activities;

namespace RntCar.CrmWorkflows
{
    public class UpdateCrmConfigurationCacheValue : CodeActivity
    {
        [Input("Cache Key")]
        public InArgument<string> CacheKey { get; set; }

        protected override void Execute(CodeActivityContext context)
        {
            var initializer = new PluginInitializer(context);

            if (initializer.WorkflowContext.Depth > 1) return;

            var configurationBL = new ConfigurationBL(initializer.Service);
            configurationBL.UpdateConfiguration(CacheKey.Get(context), Guid.NewGuid().ToString());
        }
    }
}
