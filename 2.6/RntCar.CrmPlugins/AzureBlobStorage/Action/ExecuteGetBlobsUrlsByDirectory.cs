using Microsoft.Xrm.Sdk;
using Newtonsoft.Json;
using RntCar.BusinessLibrary.Business;
using RntCar.SDK.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.CrmPlugins.AzureBlobStorage.Action
{
    public class ExecuteGetBlobsUrlsByDirectory : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            PluginInitializer initializer = new PluginInitializer(serviceProvider);
            try
            {
                string blobParameters;
                initializer.PluginContext.GetContextParameter<string>("BlobParameters", out blobParameters);

                AzureBlobStorageBL azureBlobStorageBL = new AzureBlobStorageBL(initializer.Service);
                var response = azureBlobStorageBL.getBlobUrlsByDirectory(blobParameters);
                initializer.PluginContext.OutputParameters["BlobResponse"] = JsonConvert.SerializeObject(response);
            }
            catch (Exception ex)
            {
                initializer.PluginContext.ThrowException<InvalidPluginExecutionException>(ex.Message);
            }
        }
    }
}
