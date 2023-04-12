using Microsoft.Xrm.Sdk;
using RntCar.BusinessLibrary.Business;
using RntCar.SDK.Common;
using System;

namespace RntCar.CrmPlugins.Reservation
{
    public class PostReservationTotalDurationCalculation : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            PluginInitializer initializer = new PluginInitializer(serviceProvider);

            try
            {
                Entity reservation;
                initializer.PluginContext.GetContextPostImages<Entity>(initializer.PostImgKey, out reservation);
                var pickupDateTime = reservation.GetAttributeValue<DateTime>("rnt_pickupdatetime");
                var dropoffDateTime = reservation.GetAttributeValue<DateTime>("rnt_dropoffdatetime");
                var reservationId = reservation.GetAttributeValue<Guid>("rnt_reservationid");

                ReservationBL reservationBL = new ReservationBL(initializer.Service, initializer.TracingService);
                reservationBL.updateReservationTotalDuration(reservationId, pickupDateTime, dropoffDateTime);
            }
            catch (Exception ex)
            {
                initializer.TraceMe("PostReservationTotalDurationCalculation exception : " + ex.Message);
            }
        }
    }
}
