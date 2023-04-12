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

namespace RntCar.CrmPlugins.AvailabilityFactors
{
    public class PostCreateandUpdateAvailabilityFactorsMongoDB : IPlugin
    {
        //only triggers when rnt_mongodbintegrationtrigger,statecode,statuscode fields change for update
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
            AvailabilityFactorsBL availabilityFactorsBL = new AvailabilityFactorsBL(initializer.Service, initializer.TracingService);

            if (initializer.PluginContext.MessageName.ToLower() == GlobalEnums.MessageNames.Create.ToString().ToLower())
            {
                initializer.TracingService.Trace("create");
                var createResponse = availabilityFactorsBL.createAvailabilityFactorInMongoDB(postImg);
                initializer.TracingService.Trace("create response : " + createResponse.Result);
                if (!createResponse.Result)
                {
                    initializer.PluginContext.ThrowException<InvalidPluginExecutionException>(createResponse.ExceptionDetail);
                }
                availabilityFactorsBL.updateMongoDBCreateRelatedFields(postImg, createResponse.Id);
            }
            else if (initializer.PluginContext.MessageName.ToLower() == "update")
            {
                var updateResponse = availabilityFactorsBL.updateAvailabilityFactorInMongoDB(postImg);
                if (!updateResponse.Result)
                {
                    initializer.PluginContext.ThrowException<InvalidPluginExecutionException>(updateResponse.ExceptionDetail);
                }
                availabilityFactorsBL.UpdateMongoDBUpdateRelatedFields(postImg);
            }

        }
    }
}
