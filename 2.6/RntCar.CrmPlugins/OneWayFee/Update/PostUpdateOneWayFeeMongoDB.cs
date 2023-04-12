using Microsoft.Xrm.Sdk;
using RntCar.BusinessLibrary.Business;
using RntCar.SDK.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.CrmPlugins.OneWayFee.Update
{
    //only triggers when rnt_mongodbintegrationtrigger,statecode,statuscode fields change
    public class PostUpdateOneWayFeeMongoDB : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            PluginInitializer initializer = new PluginInitializer(serviceProvider);
            Entity postImage;
            initializer.PluginContext.GetContextPostImages(initializer.PostImgKey, out postImage);
            if (postImage == null)
            {
                initializer.TracingService.Trace("image is null");
            }

            OneWayFeeBL oneWayFeeBL = new OneWayFeeBL(initializer.Service, initializer.TracingService);

            initializer.TracingService.Trace("started");
            var actionResponse = oneWayFeeBL.UpdateOneWayFeeInMongoDB(postImage);
            initializer.TracingService.Trace("operations finished");
            if (!actionResponse.Result)
            {
                initializer.PluginContext.ThrowException<InvalidPluginExecutionException>(actionResponse.ExceptionDetail);
            }

            oneWayFeeBL.UpdateMongoDBUpdateRelatedFields(postImage);
        }
    }
}
