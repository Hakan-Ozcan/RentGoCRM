using Microsoft.Xrm.Sdk;
using RntCar.BusinessLibrary.Business;
using RntCar.SDK.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.CrmPlugins.OneWayFee.Create
{
    public class PostCreateOneWayFeeMongoDB : IPlugin
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
            if (postImage.Attributes.Contains("rnt_mongodbintegrationtrigger"))
            {
                OneWayFeeBL oneWayFeeBL = new OneWayFeeBL(initializer.Service, initializer.TracingService);

                initializer.TracingService.Trace("started");
                var actionResponse = oneWayFeeBL.CreateOneWayFeeInMongoDB(postImage);
                initializer.TracingService.Trace("operations finished");
                if (!actionResponse.Result)
                {
                    initializer.PluginContext.ThrowException<InvalidPluginExecutionException>(actionResponse.ExceptionDetail);
                }

                oneWayFeeBL.updateMongoDBCreateRelatedFields(postImage, actionResponse.Id);
            }
        }
    }
}
