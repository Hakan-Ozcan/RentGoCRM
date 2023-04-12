using Microsoft.Xrm.Sdk;
using RntCar.BusinessLibrary.Business;
using RntCar.ClassLibrary;
using RntCar.SDK.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.CrmPlugins.BusinessClosure
{
    public class PostCreateandUpdateBusinessClosureMongoDB : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            PluginInitializer initializer = new PluginInitializer(serviceProvider);

            Entity postImg;
            initializer.PluginContext.GetContextPostImages(initializer.PostImgKey, out postImg);
            if (postImg == null)
            {
                initializer.TracingService.Trace("image is null");
            }

            initializer.TracingService.Trace("trigger ok");
            BusinessClosureBL businessClosureBL = new BusinessClosureBL(initializer.Service, initializer.TracingService);

            if (initializer.PluginContext.MessageName.ToLower() == GlobalEnums.MessageNames.Create.ToString().ToLower())
            {
                initializer.TracingService.Trace("create");
                var createResponse = businessClosureBL.createBusinessClosureInMongoDB(postImg);
                initializer.TracingService.Trace("create response : " + createResponse.Result);
                if (!createResponse.Result)
                {
                    initializer.PluginContext.ThrowException<InvalidPluginExecutionException>(createResponse.ExceptionDetail);
                }
                
            }
            else if (initializer.PluginContext.MessageName.ToLower() == "update")
            {
                var updateResponse = businessClosureBL.updateBusinessClosureInMongoDB(postImg);
                if (!updateResponse.Result)
                {
                    initializer.PluginContext.ThrowException<InvalidPluginExecutionException>(updateResponse.ExceptionDetail);
                }
               
            }
        }
    }
}
