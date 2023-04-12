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
    public class PreUpdateStatusChange : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider) 
        {
            PluginInitializer initializer = new PluginInitializer(serviceProvider);
           
            Entity entity;
            initializer.PluginContext.GetContextInputEntity<Entity>(initializer.TargetKey, out entity);

            Entity preImg;
            initializer.PluginContext.GetContextPreImages(initializer.PreImgKey, out preImg);

            try
            {
                EquipmentBL equipmentBL = new EquipmentBL(initializer.Service, initializer.TracingService);
                var isUpdate = equipmentBL.updateEquipmentForVehicleInspection(entity, preImg);
                if (isUpdate)
                {
                    entity["statuscode"] = new OptionSetValue((int)ClassLibrary._Enums_1033.rnt_equipment_StatusCode.WaitingVehicleInspection);
                }
            }
            catch (Exception ex)
            {
                initializer.PluginContext.ThrowException<InvalidPluginExecutionException>(ex.Message);
            }

        }
    }
}
