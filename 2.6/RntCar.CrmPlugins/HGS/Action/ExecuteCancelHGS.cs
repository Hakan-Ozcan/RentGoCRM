using Microsoft.Xrm.Sdk;
using RntCar.BusinessLibrary.Business;
using RntCar.SDK.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.CrmPlugins.HGS.Action
{
    class ExecuteCancelHGS : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            PluginInitializer initializer = new PluginInitializer(serviceProvider);
            Entity hgsLabel;
            initializer.PluginContext.GetContextParameter<Entity>("HGSEntity", out hgsLabel);
            
            HGSBL HGSBL = new HGSBL(initializer.Service, initializer.TracingService);
            initializer.TracingService.Trace("statuscode" + hgsLabel.GetAttributeValue<OptionSetValue>("statuscode").Value);
            try
            {
                var res = HGSBL.cancelHGS(hgsLabel);
                initializer.PluginContext.OutputParameters["ExecutionResult"] = res.ExceptionDetail;
                if (!res.Result)
                {
                    initializer.PluginContext.ThrowException<InvalidPluginExecutionException>(res.ExceptionDetail);
                }
            }
            catch (Exception e)
            {

                throw new Exception(e.Message);
            }
        }
    }
}
