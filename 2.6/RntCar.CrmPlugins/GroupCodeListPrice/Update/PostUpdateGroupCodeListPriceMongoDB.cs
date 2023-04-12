using Microsoft.Xrm.Sdk;
using RntCar.BusinessLibrary.Business;
using RntCar.SDK.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.CrmPlugins.GroupCodeListPrice.Update
{
    public class PostUpdateGroupCodeListPriceMongoDB : IPlugin
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

            GroupCodeListPriceBL groupCodeListPriceBL = new GroupCodeListPriceBL(initializer.Service, initializer.TracingService);

            initializer.TracingService.Trace("started");
            var actionResponse = groupCodeListPriceBL.UpdateGroupCodeListPriceInMongoDB(postImage);
            initializer.TracingService.Trace("operations finished");
            if (!actionResponse.Result)
            {
                initializer.PluginContext.ThrowException<InvalidPluginExecutionException>(actionResponse.ExceptionDetail);
            }

            groupCodeListPriceBL.UpdateMongoDBUpdateRelatedFields(postImage);

        }
    }
}
