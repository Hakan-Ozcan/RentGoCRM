using Microsoft.Xrm.Sdk;
using Newtonsoft.Json;
using RntCar.BusinessLibrary.Business;
using RntCar.ClassLibrary;
using RntCar.SDK.Common;
using System;

namespace RntCar.CrmPlugins.Reservation.Actions
{
    public class ExecuteSearchReservation : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            PluginInitializer initializer = new PluginInitializer(serviceProvider);
            
            string ReservationSearchParameters;
            initializer.PluginContext.GetContextParameter<string>("ReservationSearchParameters", out ReservationSearchParameters);
            initializer.TracingService.Trace("Reservation Parameters: " + ReservationSearchParameters);

            int LangId;
            initializer.PluginContext.GetContextParameter<int>("langId", out LangId);

            ReservationBL reservationBL = new ReservationBL(initializer.Service, initializer.TracingService);

            var reservationSearchParameters = JsonConvert.DeserializeObject<ReservationSearchParameters>(ReservationSearchParameters);
            var response = reservationBL.searchReservationByParameters(reservationSearchParameters, LangId);

            initializer.PluginContext.OutputParameters["ReservationSearchResponse"] = JsonConvert.SerializeObject(response);
            initializer.TraceMe("response lenght" + response.ReservationData.Count);
            initializer.TraceMe("reservation search finished");
        }
    }
}
