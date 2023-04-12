using Microsoft.Xrm.Sdk;
using RntCar.BusinessLibrary.Business;
using RntCar.ClassLibrary._Enums_1033;
using RntCar.SDK.Common;
using System;

namespace RntCar.CrmPlugins.Transfer.MongoDB
{
    //fires only maintance km is field change
    public class PostUpdateTransferMaintenance : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            PluginInitializer initializer = new PluginInitializer(serviceProvider);
            try
            {
                Entity postImg;
                initializer.PluginContext.GetContextPostImages(initializer.PostImgKey, out postImg);

                var equipmentId = postImg.GetAttributeValue<EntityReference>("rnt_equipmentid").Id;
                var transferType = postImg.GetAttributeValue<OptionSetValue>("rnt_transfertype").Value;
                var statusCode = postImg.GetAttributeValue<OptionSetValue>("statuscode").Value;
                initializer.TraceMe("rnt_equipmentid : " + equipmentId);
                initializer.TraceMe("transferType : " + transferType);
                if (transferType == (int)rnt_TransferType.Bakim && statusCode == (int)rnt_transfer_StatusCode.Completed)
                {
                    EquipmentBL equipmentBL = new EquipmentBL(initializer.Service);
                    equipmentBL.updateEquipmentStatus(equipmentId, (int)rnt_equipment_StatusCode.Available);

                    Entity e = new Entity("rnt_equipment");
                    e["rnt_transfertype"] = null;
                    e.Id = equipmentId;
                    initializer.Service.Update(e);
                }


            }
            catch (Exception ex)
            {
                initializer.TraceMe("exception  :" + ex.Message);
            }
        }
    }
}
