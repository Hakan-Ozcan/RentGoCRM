using Microsoft.Xrm.Sdk;
using Newtonsoft.Json;
using RntCar.BusinessLibrary.Business;
using RntCar.BusinessLibrary.Repository;
using RntCar.ClassLibrary;
using RntCar.SDK.Common;
using System;

namespace RntCar.CrmPlugins.Reservation.Actions
{
    public class ExecuteCancelReservation : IPlugin
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

                int cancellationReason;
                initializer.PluginContext.GetContextParameter<int>("cancellationReason", out cancellationReason);

                int cancellationSubReason;
                initializer.PluginContext.GetContextParameter<int>("cancellationSubReason", out cancellationSubReason);

                string cancellationDescription;
                initializer.PluginContext.GetContextParameter<string>("cancellationDescription", out cancellationDescription);

                initializer.TraceMe("langId: " + langId);
                initializer.TraceMe("reservationId: " + reservationId);
                initializer.TraceMe("pnrNumber: " + pnrNumber);
                initializer.TraceMe("cancellationReason: " + cancellationReason);
                initializer.TraceMe("cancellationSubReason: " + cancellationSubReason);
                initializer.TraceMe("cancellationDescription: " + cancellationDescription);
                string cancellationBy = cancellationReason == (int)ReservationEnums.CancellationReason.ByCustomer ? "Customer" : "Rentgo";

                AnnotationBL annotationBL = new AnnotationBL(initializer.Service);
                var annotationId = annotationBL.createNewAnnotation(new ClassLibrary.AnnotationData
                {
                    Subject = "Cancelled By " + cancellationBy,
                    NoteText = cancellationDescription,
                    ObjectId = new Guid(reservationId),
                    ObjectName = "rnt_reservation"
                });

                //check validation again
                ReservationBL reservationBL = new ReservationBL(initializer.Service, initializer.TracingService);
                var validationResponse = reservationBL.checkBeforeReservationCancellation(new ClassLibrary.ReservationCancellationParameters
                {
                    pnrNumber = pnrNumber,
                    reservationId = new Guid(reservationId)
                }, langId);

                if (!validationResponse.ResponseResult.Result)
                {
                    initializer.PluginContext.OutputParameters["ReservationCancellationResponse"] = JsonConvert.SerializeObject(validationResponse);
                    return;
                }
                validationResponse = reservationBL.calculateCancellationAmountForGivenReservationByCancellationReason(validationResponse,
                                                                                                                      new Guid(reservationId),
                                                                                                                      validationResponse.willChargeFromUser,
                                                                                                                      langId,
                                                                                                                      cancellationReason);
                if (!validationResponse.ResponseResult.Result)
                {
                    initializer.PluginContext.OutputParameters["ReservationCancellationResponse"] = JsonConvert.SerializeObject(validationResponse);
                    return;
                }
                initializer.TraceMe("validations are done");
                initializer.TraceMe("fineAmount amount " + validationResponse.fineAmount);
                initializer.TraceMe("refund amount " + validationResponse.refundAmount);
                // cancel all items and header
                //todo if face error more user friendly message
                initializer.TraceMe("deactivation start");
                initializer.TraceMe("refund amount " + validationResponse.refundAmount);
                reservationBL.cancelReservation(reservationId, cancellationReason, validationResponse.fineAmount, validationResponse.isCorporateReservation, cancellationSubReason);
                initializer.TraceMe("deactivation end");

                PaymentBL paymentBL = new PaymentBL(initializer.Service, initializer.TracingService);
                if (validationResponse.refundAmount > StaticHelper._one_ ||
                    validationResponse.refundAmount < (-1 * StaticHelper._one_))
                {
                    initializer.TraceMe("payment provider refund start");
                    paymentBL.createRefund(new CreateRefundParameters
                    {
                        refundAmount = validationResponse.refundAmount,
                        reservationId = new Guid(reservationId),
                        langId = langId
                    });
                    initializer.TraceMe("payment provider refund end");
                }
                //error olarak paymentları güncelle 

                paymentBL.cancelPaymentsForWaiting3d(new Guid(reservationId));

                initializer.PluginContext.OutputParameters["ReservationCancellationResponse"] = JsonConvert.SerializeObject(validationResponse);
            }
            catch (Exception ex)
            {
                initializer.PluginContext.ThrowException<InvalidPluginExecutionException>(ex.Message);
            }
        }
    }
}
