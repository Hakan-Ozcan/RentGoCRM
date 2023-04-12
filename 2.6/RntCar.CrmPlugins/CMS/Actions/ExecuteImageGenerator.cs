using Microsoft.Xrm.Sdk;
using Newtonsoft.Json;
using RntCar.BusinessLibrary.Business;
using RntCar.ClassLibrary;
using RntCar.SDK.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.CrmPlugins.CMS.Actions
{
    public class ExecuteImageGenerator : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            PluginInitializer pluginInitializer = new PluginInitializer(serviceProvider);

            try
            {
                string imageGeneratorId;
                pluginInitializer.PluginContext.GetContextInputParameter<string>("ImageGeneratorId", out imageGeneratorId);

                string imageBytes;
                pluginInitializer.PluginContext.GetContextInputParameter<string>("Image", out imageBytes);

                string type;
                pluginInitializer.PluginContext.GetContextInputParameter<string>("Type", out type);

                AzureBlobStorageBL azureBlobStorageBL = new AzureBlobStorageBL(pluginInitializer.Service,pluginInitializer.TracingService);
                var response = azureBlobStorageBL.generateImage(new ImageGeneratorParameter
                {
                    Image = imageBytes,
                    Type = type
                });
                pluginInitializer.TraceMe("ImageGeneratorId" + imageGeneratorId);
                pluginInitializer.TraceMe("Type" + type);

                ConfigurationBL configurationBL = new ConfigurationBL(pluginInitializer.Service);
                var baseURL = configurationBL.GetConfigurationByName("blobstorage_baseurl");
                pluginInitializer.TraceMe("baseURL : " + baseURL);
                pluginInitializer.TraceMe("response : " + JsonConvert.SerializeObject(response));

                Entity e = new Entity("rnt_imagegenerator");
                e["rnt_url"] = baseURL + "cmsimages/" + response.Url.Replace("https://", string.Empty);
                e.Id = new Guid(imageGeneratorId);
                pluginInitializer.Service.Update(e);

                

                pluginInitializer.PluginContext.OutputParameters["Response"] = JsonConvert.SerializeObject(response);
            }
            catch (Exception ex)
            {
                pluginInitializer.PluginContext.ThrowException<InvalidPluginExecutionException>(ex.Message);
            }
        }
    }
}
