using Microsoft.Xrm.Sdk;
using RntCar.BusinessLibrary.Business;
using RntCar.SDK.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.CrmPlugins.HGS.Update
{
    public class PostUpdateHGSLabel : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            PluginInitializer initializer = new PluginInitializer(serviceProvider);
            Entity target;
            Entity postImage;
            Entity preImage;
            initializer.PluginContext.GetContextInputEntity(initializer.TargetKey, out target);
            initializer.PluginContext.GetContextPostImages(initializer.PostImgKey, out postImage);
            initializer.PluginContext.GetContextPreImages(initializer.PreImgKey, out preImage);
            if (postImage == null)
            {
                initializer.TracingService.Trace("image is null");
            }

            HGSBL HGSHelper = new HGSBL(initializer.Service, initializer.TracingService);

            try
            {
                string exceptionDetail = string.Empty;

                //Araç Bilgisi Dolu ise Etiket bağlanır.
                if (target.Contains("rnt_equipmentid"))
                {
                    EntityReference preEquipment = preImage.GetAttributeValue<EntityReference>("rnt_equipmentid");
                    EntityReference postEquipment = target.GetAttributeValue<EntityReference>("rnt_equipmentid");
                    if (postEquipment == null)
                    {
                        Entity updateEquipment = new Entity(preEquipment.LogicalName, preEquipment.Id);
                        updateEquipment.Attributes["rnt_hgsnumber"] = null;
                        updateEquipment.Attributes["rnt_hgslabelid"] = null;
                        initializer.Service.Update(updateEquipment);
                    }
                    //else if ((preEquipment == null && postEquipment != null) ||
                    //    (preEquipment != null && postEquipment != null && preEquipment.Id != postEquipment.Id))
                    //{
                    //    initializer.TracingService.Trace("updateVehicle");
                    //    var actionResponseUpdateLimit = HGSHelper.updateVehicle(postImage);
                    //    if (!actionResponseUpdateLimit.Result)
                    //    {
                    //        exceptionDetail = actionResponseUpdateLimit.ExceptionDetail;
                    //    }
                    //} 
                }
                //Kart Bilgisi Dolu ise Otomatik Ödeme Talimatı Verilir.
                if (target.Contains("rnt_hgspaymentcardid") && target.GetAttributeValue<EntityReference>("rnt_hgspaymentcardid") != null)
                {
                    initializer.TracingService.Trace("updateLimitHGS");
                    var actionResponseUpdateLimit = HGSHelper.updateLimitHGS(postImage);
                    if (!actionResponseUpdateLimit.Result)
                    {
                        exceptionDetail = actionResponseUpdateLimit.ExceptionDetail;
                    }
                }

                if (!string.IsNullOrWhiteSpace(exceptionDetail))
                {
                    initializer.TracingService.Trace($"service exception Detail{ exceptionDetail}");
                    initializer.PluginContext.ThrowException<InvalidPluginExecutionException>(exceptionDetail);
                }
            }
            catch (InvalidPluginExecutionException ex)
            {
                throw ex;
            }
            catch (Exception e)
            {
                initializer.TracingService.Trace($"exception Detail{ e.Message}");
                throw new Exception(e.Message);
            }

        }
    }
}
