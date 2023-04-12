using Microsoft.Xrm.Sdk;
using RntCar.BusinessLibrary.Business;
using RntCar.SDK.Common;
using System;

namespace RntCar.CrmPlugins.CrmConfiguration
{
    public class PostUpdateCrmConfigurationMongoDB : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            PluginInitializer initializer = new PluginInitializer(serviceProvider);
            Entity postImage;

            initializer.PluginContext.GetContextPostImages(initializer.PostImgKey, out postImage);

            ConfigurationBL configurationBL = new ConfigurationBL(initializer.Service);

            configurationBL.updateConfigurationtoMongoDB(postImage);
        }
    }
}
