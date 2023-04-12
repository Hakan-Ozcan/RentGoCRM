using Microsoft.Xrm.Sdk;
using RntCar.BusinessLibrary.Business;
using RntCar.ClassLibrary;
using RntCar.SDK.Common;
using System;


namespace RntCar.CrmPlugins.ReservationItem.Actions
{
    public class ExecuteCreateReservationItemInMongoDB : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {

            PluginInitializer initializer = new PluginInitializer(serviceProvider);
            Entity reservationItem;
            initializer.PluginContext.GetContextParameter<Entity>("ReservationItemEntity", out reservationItem);

            string messageName;
            initializer.PluginContext.GetContextParameter<string>("MessageName", out messageName);

            ReservationItemBL reservationItemBL = new ReservationItemBL(initializer.Service,initializer.TracingService);
            try
            {
                //create
                if (messageName.ToLower() == GlobalEnums.MessageNames.Create.ToString().ToLower())
                {
                    var res = reservationItemBL.CreateReservationItemInMongoDB(reservationItem);
                    initializer.PluginContext.OutputParameters["ExecutionResult"] = res.ExceptionDetail;
                    if (!res.Result)
                    {
                        initializer.PluginContext.ThrowException<InvalidPluginExecutionException>(res.ExceptionDetail);
                    }
                    else
                    {
                        //update mongodb related fields in crm.
                        reservationItemBL.updateMongoDBCreateRelatedFields(reservationItem, res.Id);
                        initializer.PluginContext.OutputParameters["ID"] = res.Id;
                    }

                }
                else if (messageName.ToLower() == GlobalEnums.MessageNames.Update.ToString().ToLower())
                {
                    var res = reservationItemBL.UpdateReservationItemInMongoDB(reservationItem);
                    initializer.PluginContext.OutputParameters["ExecutionResult"] = res.ExceptionDetail;
                    if (!res.Result)
                    {
                        initializer.PluginContext.ThrowException<InvalidPluginExecutionException>(res.ExceptionDetail);
                    }
                    else
                    {
                        //update mongodb related fields in crm.
                        reservationItemBL.UpdateMongoDBUpdateRelatedFields(reservationItem);
                    }
                }

            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
            finally
            {
                //initializer.TraceFlush();

            }
        }
    }
}
