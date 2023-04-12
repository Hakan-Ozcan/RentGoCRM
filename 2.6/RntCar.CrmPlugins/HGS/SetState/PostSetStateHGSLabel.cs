using Microsoft.Xrm.Sdk;
using RntCar.BusinessLibrary.Business;
using RntCar.SDK.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.CrmPlugins.HGS.SetState
{
    public class PostSetStateHGSLabel : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            PluginInitializer initializer = new PluginInitializer(serviceProvider);

            EntityReference entityMoniker = (EntityReference)initializer.PluginContext.InputParameters[initializer.EntityMoniker];
            OptionSetValue statecode = (OptionSetValue)initializer.PluginContext.InputParameters[initializer.State];
            OptionSetValue statuscode = (OptionSetValue)initializer.PluginContext.InputParameters[initializer.Status];


            if (entityMoniker == null)
            {
                initializer.TracingService.Trace("entityMoniker is null");
            }

            HGSBL HGSHelper = new HGSBL(initializer.Service, initializer.TracingService);
            try
            {
                if (statecode.Value != 0)
                {
                    initializer.TracingService.Trace($"entityMoniker LogicalName:{entityMoniker.LogicalName} - Id:{entityMoniker.Id}");
                    Entity hgsLabel = initializer.Service.Retrieve(entityMoniker.LogicalName, entityMoniker.Id, new Microsoft.Xrm.Sdk.Query.ColumnSet(true));
                    hgsLabel.Attributes["statuscode"] = new OptionSetValue(statuscode.Value);

                    initializer.TracingService.Trace($"LabelName :{hgsLabel.GetAttributeValue<string>("rnt_label")}");

                    initializer.TracingService.Trace("cancelHGS");
                    var actionResponse = HGSHelper.cancelHGS(hgsLabel);
                    if (!actionResponse.Result)
                    {
                        initializer.TracingService.Trace($"service exception Detail{ actionResponse.ExceptionDetail}");
                        initializer.PluginContext.ThrowException<InvalidPluginExecutionException>(actionResponse.ExceptionDetail);
                    }
                }
                else
                {
                    initializer.TracingService.Trace($"Süreç olarak HGS Etiketleri tekrar aktifleştirilemez");
                    initializer.PluginContext.ThrowException<InvalidPluginExecutionException>("Süreç olarak HGS Etiketleri tekrar aktifleştirilemez");
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
