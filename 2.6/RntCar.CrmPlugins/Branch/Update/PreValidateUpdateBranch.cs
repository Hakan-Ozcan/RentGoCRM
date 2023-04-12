using Microsoft.Xrm.Sdk;
using RntCar.BusinessLibrary.Business;
using RntCar.BusinessLibrary.Validations;
using RntCar.ClassLibrary;
using RntCar.ClassLibrary._Enums_1033;
using RntCar.SDK.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.CrmPlugins.Branch.Update
{
    public class PreValidateUpdateBranch : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            PluginInitializer initializer = new PluginInitializer(serviceProvider);
            Entity entity;
            initializer.PluginContext.GetContextInputEntity<Entity>(initializer.TargetKey, out entity);
            try
            {
                //todo will implement in validation class
                if (entity.Attributes.Contains("statuscode") && entity.GetAttributeValue<OptionSetValue>("statuscode").Value  == (int)rnt_branch_StatusCode.Draft)
                {
                    return;
                }
                Guid userId = initializer.PluginContext.UserId;
                Guid initiatingUserId = initializer.PluginContext.InitiatingUserId;
                initializer.TraceMe($"{userId}, {initiatingUserId}");
                WorkingHourValidation workingHourValidation = new WorkingHourValidation(initializer.Service);
                ValidationResponse validationResponse = workingHourValidation.checkBranchWorkingHoursForWeekdays(entity.Id, userId);
                if (!validationResponse.ResponseResult.Result)
                {
                    initializer.PluginContext.ThrowException<InvalidPluginExecutionException>(validationResponse.ResponseResult.ExceptionDetail);
                }
            }
            catch (Exception ex)
            {
                initializer.PluginContext.ThrowException<InvalidPluginExecutionException>(ex.Message);
            }
        }
    }
}
