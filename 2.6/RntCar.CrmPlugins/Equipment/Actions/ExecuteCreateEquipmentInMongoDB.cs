using Microsoft.Xrm.Sdk;
using RntCar.BusinessLibrary.Business;
using RntCar.ClassLibrary;
using RntCar.SDK.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.CrmPlugins.Equipment.Actions
{
    public class ExecuteCreateEquipmentInMongoDB : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            PluginInitializer initializer = new PluginInitializer(serviceProvider);
            Entity equipment;
            initializer.PluginContext.GetContextParameter<Entity>("EquipmentEntity", out equipment);

            string messageName; 
            initializer.PluginContext.GetContextParameter<string>("MessageName", out messageName);

            EquipmentBL equipmentBL = new EquipmentBL(initializer.Service, initializer.TracingService);
            initializer.TracingService.Trace("updatestarted" + messageName);
            initializer.TracingService.Trace("statuscode" + equipment.GetAttributeValue<OptionSetValue>("statuscode").Value);
            try
            {
                if (messageName.ToLower() == GlobalEnums.MessageNames.Create.ToString().ToLower())
                {
                    var res = equipmentBL.CreateEquipmentInMongoDB(equipment);
                    initializer.PluginContext.OutputParameters["ExecutionResult"] = res.ExceptionDetail;
                    if (!res.Result)
                    {
                        initializer.PluginContext.ThrowException<InvalidPluginExecutionException>(res.ExceptionDetail);
                    }
                    else
                    {
                        //update mongodb related fields in crm.
                        equipmentBL.updateMongoDBCreateRelatedFields(equipment, res.Id);
                        initializer.PluginContext.OutputParameters["ID"] = res.Id;
                    }
                }
                else if (messageName.ToLower() == GlobalEnums.MessageNames.Update.ToString().ToLower())
                {
                    
                    var res = equipmentBL.UpdateEquipmentInMongoDB(equipment);
                    initializer.PluginContext.OutputParameters["ExecutionResult"] = res.ExceptionDetail;
                    if (!res.Result)
                    {
                        initializer.PluginContext.ThrowException<InvalidPluginExecutionException>(res.ExceptionDetail);
                    }
                    else
                    {
                        //update mongodb related fields in crm.
                        equipmentBL.UpdateMongoDBUpdateRelatedFields(equipment);
                    }
                }              
            }
            catch (Exception e)
            {
            
                throw new Exception(e.Message);
            }
        }
    }
}
