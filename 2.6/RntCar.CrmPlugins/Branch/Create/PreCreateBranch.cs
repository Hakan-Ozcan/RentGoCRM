using Microsoft.Xrm.Sdk;
using RntCar.BusinessLibrary.Business;
using RntCar.ClassLibrary;
using RntCar.ClassLibrary._Enums_1033;
using RntCar.SDK.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.CrmPlugins.Branch.Create
{
    public class PreCreateBranch : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            PluginInitializer initializer = new PluginInitializer(serviceProvider);
            try
            {
                Entity branch;
                initializer.PluginContext.GetContextInputEntity<Entity>(initializer.TargetKey, out branch);

                branch["statecode"] = new OptionSetValue((int)GlobalEnums.StateCode.Active);
                branch["statuscode"] = new OptionSetValue((int)rnt_branch_StatusCode.Draft);

            }
            catch (Exception ex)
            {
                initializer.PluginContext.ThrowException<InvalidPluginExecutionException>(ex.Message);
            }
            

        }
    }
}
