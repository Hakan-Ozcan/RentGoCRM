using Microsoft.Xrm.Sdk;
using RntCar.BusinessLibrary.Business;
using RntCar.SDK.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.CrmPlugins.AvailabilityPriceLists.Update
{
    //only triggers when rnt_mongodbintegrationtrigger,statecode,statuscode fields change
    public class PostUpdateAvailabilityPriceListMongoDB : IPlugin
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

            AvailabilityPriceListBL availabilityPriceListBL = new AvailabilityPriceListBL(initializer.Service, initializer.TracingService);

            initializer.TracingService.Trace("started");
            var actionResponse = availabilityPriceListBL.UpdateAvailabilityPriceListInMongoDB(postImage);
            initializer.TracingService.Trace("operations finished");
            if (!actionResponse.Result)
            {
                initializer.PluginContext.ThrowException<InvalidPluginExecutionException>(actionResponse.ExceptionDetail);
            }

            availabilityPriceListBL.UpdateMongoDBUpdateRelatedFields(postImage);
        }
    }
}
