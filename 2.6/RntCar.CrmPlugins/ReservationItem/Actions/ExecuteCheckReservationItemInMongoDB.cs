using Microsoft.Xrm.Sdk;
using Newtonsoft.Json;
using RntCar.BusinessLibrary.Business;
using RntCar.BusinessLibrary.Repository;
using RntCar.SDK.Common;
using System;

namespace RntCar.CrmPlugins.ReservationItem.Actions
{
    public class ExecuteCheckReservationItemInMongoDB : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            PluginInitializer initializer = new PluginInitializer(serviceProvider);
            string reservationId;
            initializer.PluginContext.GetContextParameter<string>("reservationId", out reservationId);
            reservationId = reservationId.Trim('"');
            initializer.TraceMe("reservationId: " + reservationId);

            try
            {
                ReservationItemBL reservationItemBL = new ReservationItemBL(initializer.Service);
                var equipment = reservationItemBL.getReservationItemEquipment(new Guid(reservationId));

                initializer.TraceMe("equipment: " + JsonConvert.SerializeObject(equipment));

                if (equipment != null)
                {
                    initializer.TraceMe(equipment.GetAttributeValue<string>("rnt_mongodbid"));

                    if (string.IsNullOrEmpty(equipment.GetAttributeValue<string>("rnt_mongodbid")))
                    {
                        initializer.TraceMe("rnt_mongodbid is null");
                        var resItem = reservationItemBL.CreateReservationItemInMongoDB(equipment);
                        if (!resItem.Result)
                        {
                            initializer.PluginContext.ThrowException<InvalidPluginExecutionException>(resItem.ExceptionDetail);
                        }
                        reservationItemBL.updateMongoDBCreateRelatedFields(equipment, resItem.Id);
                    }
                    else
                    {
                        initializer.TraceMe("rnt_mongodbid is not null");
                    }                   
                }
                initializer.PluginContext.OutputParameters["serviceResponse"] = JsonConvert.SerializeObject(ClassLibrary.ResponseResult.ReturnSuccess());
            }
            catch (Exception ex)
            {
                initializer.PluginContext.ThrowException<InvalidPluginExecutionException>(ex.Message);
            }
        }
    }
}
