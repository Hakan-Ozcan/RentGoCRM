using Microsoft.Xrm.Sdk;
using RntCar.BusinessLibrary.Business;
using RntCar.SDK.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.CrmPlugins.Equipment.Update
{
    public class PostUpdateCurrentKmChange : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            PluginInitializer initializer = new PluginInitializer(serviceProvider);
            if(initializer.PluginContext.Depth > 3)
            {
                return;
            }
            Entity postImage;
            initializer.PluginContext.GetContextPostImages(initializer.PostImgKey, out postImage);

            if (postImage == null)
            {
                initializer.TraceMe("image is null");
            }

            try
            {
                EquipmentBL equipmentBL = new EquipmentBL(initializer.Service, initializer.TracingService);
                equipmentBL.updateEquipmentforMaintenance(postImage);
            }
            catch (Exception ex)
            {
                initializer.PluginContext.ThrowException<InvalidPluginExecutionException>(ex.Message);
            }
        }
    }
}
