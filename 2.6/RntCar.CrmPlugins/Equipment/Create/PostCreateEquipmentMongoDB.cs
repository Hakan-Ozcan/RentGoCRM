using Microsoft.Xrm.Sdk;
using RntCar.BusinessLibrary.Business;
using RntCar.SDK.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.CrmPlugins.Equipment.Create
{
    public class PostCreateEquipmentMongoDB : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            PluginInitializer initializer = new PluginInitializer(serviceProvider);
            Entity postImage;
            initializer.PluginContext.GetContextPostImages(initializer.PostImgKey, out postImage);
            initializer.TracingService.Trace("lets start");
            if (postImage == null)
            {
                initializer.TracingService.Trace("image is null");
            }

            EquipmentBL equipmentBL = new EquipmentBL(initializer.Service, initializer.TracingService);
            var actionResponse = equipmentBL.sendEquipmenttoMongoDB(initializer.PluginContext.MessageName, postImage);
            initializer.TracingService.Trace("operations finished");
            if (!actionResponse.Result)
            {
                initializer.PluginContext.ThrowException<InvalidPluginExecutionException>(actionResponse.ExceptionDetail);
            }
        }
    }
}
