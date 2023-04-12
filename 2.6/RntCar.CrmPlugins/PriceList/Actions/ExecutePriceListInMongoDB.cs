using Microsoft.Xrm.Sdk;
using RntCar.BusinessLibrary.Business;
using RntCar.ClassLibrary;
using RntCar.SDK.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.CrmPlugins.PriceList.Actions
{
    public class ExecutePriceListInMongoDB : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            PluginInitializer initializer = new PluginInitializer(serviceProvider);
            Entity priceList;
            initializer.PluginContext.GetContextParameter<Entity>("PriceListEntity", out priceList);

            string messageName;
            initializer.PluginContext.GetContextParameter<string>("MessageName", out messageName);

            PriceFactorValidation priceListBL = new PriceFactorValidation(initializer.Service, initializer.TracingService);
            try
            {
                
                if (messageName.ToLower() == GlobalEnums.MessageNames.Create.ToString().ToLower())
                {
                    var res = priceListBL.CreatePriceListInMongoDB(priceList);
                    initializer.TracingService.Trace("mongodb response" + res.Id);
                    initializer.PluginContext.OutputParameters["ExecutionResult"] = res.ExceptionDetail;
                    if (!res.Result)
                    {
                        initializer.PluginContext.ThrowException<InvalidPluginExecutionException>(res.ExceptionDetail);
                    }
                    else
                    {
                        //update mongodb related fields in crm.
                        priceListBL.updateMongoDBCreateRelatedFields(priceList, res.Id);
                        initializer.PluginContext.OutputParameters["ID"] = res.Id;
                    }
                }

                else if (messageName.ToLower() == GlobalEnums.MessageNames.Update.ToString().ToLower())
                {
                    var res = priceListBL.UpdatePriceListInMongoDB(priceList);
                    initializer.TraceMe("mongodb response" + res.Id);
                    initializer.PluginContext.OutputParameters["ExecutionResult"] = res.ExceptionDetail;
                    if (!res.Result)
                    {
                        initializer.PluginContext.ThrowException<InvalidPluginExecutionException>(res.ExceptionDetail);
                    }
                    else
                    {
                        //update mongodb related fields in crm.
                        priceListBL.UpdateMongoDBUpdateRelatedFields(priceList);
                        initializer.PluginContext.OutputParameters["ID"] = res.Id;
                    }
                }
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
