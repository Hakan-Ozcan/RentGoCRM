using Microsoft.Xrm.Sdk;
using Newtonsoft.Json;
using RntCar.BusinessLibrary.Business;
using RntCar.ClassLibrary._Tablet;
using RntCar.SDK.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.CrmPlugins.Damage
{
    public class ExecuteCreateDamages : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            PluginInitializer initializer = new PluginInitializer(serviceProvider);
            try
            {
                string damageParameter;
                initializer.PluginContext.GetContextParameter<string>("DamageParameter", out damageParameter);
                var damageParameterObj = JsonConvert.DeserializeObject<CreateDamageParameter>(damageParameter);

                DamageBL damageBL = new DamageBL(initializer.Service, initializer.TracingService);
                var response = damageBL.createDamages(damageParameterObj);
                if (!response.responseResult.result)
                {
                    initializer.PluginContext.ThrowException<InvalidPluginExecutionException>(response.responseResult.exceptionDetail);
                }
                initializer.PluginContext.OutputParameters["CreateDamageResponse"] = JsonConvert.SerializeObject(response);
            }
            catch (Exception ex)
            {
                initializer.PluginContext.ThrowException<InvalidPluginExecutionException>(ex.Message);
            }
        }
    }
}
