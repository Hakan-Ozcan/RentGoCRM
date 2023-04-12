using Microsoft.Xrm.Sdk;
using RntCar.BusinessLibrary.Business;
using RntCar.SDK.Common;
using System;

namespace RntCar.CrmPlugins.GroupCodeListPrice.Create
{
    public class PostCreateGroupCodeListPriceMongoDB : IPlugin
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
            var actionResponse = groupCodeListPriceBL.CreateGroupCodeListPriceInMongoDB(postImage);
            initializer.TracingService.Trace("operations finished");
            if (!actionResponse.Result)
            {
                initializer.PluginContext.ThrowException<InvalidPluginExecutionException>(actionResponse.ExceptionDetail);
            }

            groupCodeListPriceBL.updateMongoDBCreateRelatedFields(postImage, actionResponse.Id);

        }
    }
}
