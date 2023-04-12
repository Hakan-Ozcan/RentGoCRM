using Microsoft.Xrm.Sdk;
using RntCar.BusinessLibrary.Business;
using RntCar.SDK.Common;
using System;

namespace RntCar.CrmPlugins.Equipment.Update
{
    //only triggers when rnt_mongodbintegrationtrigger,statecode,statuscode fields change
    public class PostUpdateEquipmentMongoDB : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            PluginInitializer initializer = new PluginInitializer(serviceProvider);
            Entity postImage;
            initializer.PluginContext.GetContextPostImages(initializer.PostImgKey, out postImage);

            EquipmentBL equipmentBL = new EquipmentBL(initializer.Service,initializer.TracingService);

            var response = equipmentBL.sendEquipmenttoMongoDB(initializer.PluginContext.MessageName, postImage);

            if (!response.Result)
            {
                initializer.PluginContext.ThrowException<InvalidPluginExecutionException>(response.ExceptionDetail);
            }
        }
    }
}
