using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Workflow;
using RntCar.BusinessLibrary.Business;
using RntCar.ClassLibrary.MongoDB;
using RntCar.SDK.Common;
using System;
using System.Activities;

namespace RntCar.CrmWorkflows
{
    public class UpdateDepositAmountForMongoDB : CodeActivity
    {
        [Input("Reservation Reference")]
        [ReferenceTarget("rnt_reservation")]
        public InArgument<EntityReference> _reservation { get; set; }
        protected override void Execute(CodeActivityContext context)
        {
            PluginInitializer initializer = new PluginInitializer(context);
            var reservationRef = _reservation.Get<EntityReference>(context);
            initializer.TraceMe("process start!");

            try
            {
                ReservationItemBL reservationItemBL = new ReservationItemBL(initializer.Service, initializer.TracingService);
                var reservationItemEquipment = reservationItemBL.getReservationItemEquipment(reservationRef.Id);
                initializer.TraceMe("res id: " + reservationRef.Id);
                initializer.TraceMe("itemEquipment id: " + reservationItemEquipment.Id);
                var res = reservationItemBL.UpdateReservationDeposit(reservationItemEquipment);
                
                reservationItemBL.UpdateMongoDBUpdateRelatedFields(reservationItemEquipment, res);
            }
            catch (Exception ex)
            {
                initializer.PluginContext.ThrowException<InvalidPluginExecutionException>(ex.Message);
            }
        }
    }
}
