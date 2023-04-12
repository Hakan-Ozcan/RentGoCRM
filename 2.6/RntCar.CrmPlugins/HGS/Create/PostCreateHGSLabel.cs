using Microsoft.Xrm.Sdk;
using RntCar.BusinessLibrary.Business;
using RntCar.SDK.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.CrmPlugins.HGS.Create
{
    public class PostCreateHGSLabel : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            PluginInitializer initializer = new PluginInitializer(serviceProvider);
            Entity target;
            Entity postImage;
            initializer.PluginContext.GetContextInputEntity(initializer.TargetKey, out target);
            initializer.PluginContext.GetContextPostImages(initializer.PostImgKey, out postImage);
            if (postImage == null)
            {
                initializer.TracingService.Trace("image is null");
            }

            HGSBL HGSHelper = new HGSBL(initializer.Service, initializer.TracingService);
            try
            {
                string exceptionDetail = string.Empty;

                bool isExist = HGSHelper.IsExistHgsLabel(postImage);
                if (isExist)
                {
                    exceptionDetail = $"{postImage.GetAttributeValue<string>("rnt_label")} etkiketi Rentura sistemde tanımlıdır. Tekrar tanımlama yapılamaz.";
                    initializer.TracingService.Trace(exceptionDetail);
                    initializer.PluginContext.ThrowException<InvalidPluginExecutionException>(exceptionDetail);
                }

                initializer.TracingService.Trace("saleHGS");
                var actionResponseSale = HGSHelper.saleHGS(postImage);
                if (actionResponseSale.Result)
                {
                    //Kart Bilgisi Dolu ise Otomatik Ödeme Talimatı Verilir.
                    if (target.Contains("rnt_hgspaymentcardid") && target.GetAttributeValue<EntityReference>("rnt_hgspaymentcardid") != null)
                    {
                        var actionResponseUpdateLimit = HGSHelper.updateLimitHGS(postImage);
                        if (!actionResponseUpdateLimit.Result)
                        {
                            exceptionDetail = actionResponseUpdateLimit.ExceptionDetail;
                        }
                    }
                }
                else
                {
                    exceptionDetail = actionResponseSale.ExceptionDetail;
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
