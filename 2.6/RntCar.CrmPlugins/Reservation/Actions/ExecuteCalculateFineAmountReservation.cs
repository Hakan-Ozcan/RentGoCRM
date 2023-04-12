using Microsoft.Xrm.Sdk;
using Newtonsoft.Json;
using RntCar.BusinessLibrary.Business;
using RntCar.SDK.Common;
using RntCar.ClassLibrary;
using System;

namespace RntCar.CrmPlugins.Reservation.Actions
{
    public class ExecuteCalculateFineAmountReservation : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            PluginInitializer initializer = new PluginInitializer(serviceProvider);
            try
            {
                int langId;
                initializer.PluginContext.GetContextParameter<int>("langId", out langId);

                string reservationId;
                initializer.PluginContext.GetContextParameter<string>("reservationId", out reservationId);

                string pnrNumber;
                initializer.PluginContext.GetContextParameter<string>("pnrNumber", out pnrNumber);

                initializer.TraceMe("langId" + langId);
                initializer.TraceMe("reservationId" + reservationId);
                initializer.TraceMe("pnrNumber" + pnrNumber);

                ReservationBL reservationBL = new ReservationBL(initializer.Service, initializer.TracingService);
                var validationResponse = reservationBL.checkBeforeReservationCancellation(new ReservationCancellationParameters
                {
                    pnrNumber = pnrNumber,
                    reservationId = new Guid(reservationId)
                },
                langId);
                initializer.TraceMe("validationResponse : " + JsonConvert.SerializeObject(validationResponse));
                if (!validationResponse.ResponseResult.Result)
                {
                    initializer.PluginContext.OutputParameters["ReservationFineAmountResponse"] = JsonConvert.SerializeObject(validationResponse);
                    return;
                }
                validationResponse = reservationBL.calculateCancellationAmountForGivenReservation(validationResponse,
                                                                                                  new Guid(reservationId),
                                                                                                  validationResponse.willChargeFromUser,
                                                                                                  langId);

                initializer.TraceMe("validationResponse1 : " + JsonConvert.SerializeObject(validationResponse));

                initializer.PluginContext.OutputParameters["ReservationFineAmountResponse"] = JsonConvert.SerializeObject(validationResponse);


            }
            catch (Exception ex)
            {
                initializer.PluginContext.ThrowException<InvalidPluginExecutionException>(ex.Message);
            }
        }
    }
}
