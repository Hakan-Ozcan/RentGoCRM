using System;
using Microsoft.Xrm.Sdk;
using RntCar.BusinessLibrary.Business;
using RntCar.SDK.Common;

namespace RntCar.CrmPlugins.ReservationItem.Create
{
    public class PreCreateReservationItem : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            PluginInitializer initializer = new PluginInitializer(serviceProvider);
            Entity entity;
            initializer.PluginContext.GetContextInputEntity(initializer.TargetKey, out entity);

            ReservationItemBL reservationItemBL = new ReservationItemBL(initializer.Service, initializer.TracingService);
            reservationItemBL.removeTaxAmount(entity);
        }
    }
}
