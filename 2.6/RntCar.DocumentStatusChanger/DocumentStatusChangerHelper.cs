using Microsoft.Xrm.Sdk;
using Newtonsoft.Json;
using RntCar.ClassLibrary;
using RntCar.ClassLibrary._Enums_1033;
using RntCar.MongoDBHelper.Helper;
using RntCar.MongoDBHelper.Repository;
using RntCar.SDK.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RntCar.DocumentStatusChanger
{
    public class DocumentStatusChangerHelper
    {
        //todo naming
        public static void make()
        {
            RntCar.Logger.LoggerHelper logger = new Logger.LoggerHelper();
            try
            {

                CrmServiceHelper crmServiceHelper = new CrmServiceHelper();
                XrmHelper xrmHelper = new XrmHelper(crmServiceHelper.IOrganizationService);

                var currentDate = DateTime.UtcNow.AddMinutes(StaticHelper.offset);
                logger.traceInfo("current date : " + currentDate);

                BusinessLibrary.Repository.ReservationRepository reservationRepository = new BusinessLibrary.Repository.ReservationRepository(crmServiceHelper.IOrganizationService);
                var noShowReservations = reservationRepository.getWillNoShowReservationsWithGivenColumns(currentDate, new string[] { "statuscode", "rnt_noshowtime", "rnt_pnrnumber" });
                logger.traceInfo("will no show reservations count : " + noShowReservations.Count);

                BusinessLibrary.Repository.ReservationItemRepository reservationItemRepository = new BusinessLibrary.Repository.ReservationItemRepository(crmServiceHelper.IOrganizationService);

                foreach (var reservation in noShowReservations)
                {
                    try
                    {
                        xrmHelper.setState("rnt_reservation", reservation.Id, (int)GlobalEnums.StateCode.Active, (int)ClassLibrary._Enums_1033.rnt_reservation_StatusCode.NoShow);
                        var reservationItems = reservationItemRepository.getActiveReservationItemsByReservationId(reservation.Id);
                        foreach (var reservationItem in reservationItems)
                        {
                            xrmHelper.setState("rnt_reservationitem", reservationItem.Id, (int)GlobalEnums.StateCode.Active, (int)ClassLibrary._Enums_1033.rnt_reservationitem_StatusCode.NoShow);
                        }
                        logger.traceInfo("updated reservation pnr : " + reservation.GetAttributeValue<string>("rnt_pnrnumber"));
                    }
                    catch (Exception ex)
                    {
                        logger.traceInfo(ex.Message);
                        continue;
                    }                   
                }

                var cancellationReservations = reservationRepository.getWillCancelReservationsWithGivenColumns(currentDate, new string[] { "statuscode", "rnt_cancellationtime", "rnt_pnrnumber" });
                logger.traceInfo("will cancel reservations count : " + cancellationReservations.Count);

                foreach (var item in cancellationReservations)
                {
                    try
                    {
                        var cancelReservationCustomAction = new OrganizationRequest("rnt_CancelReservation");

                        cancelReservationCustomAction["cancellationReason"] = (int)rnt_reservation_StatusCode.CancelledByRentgo;
                        cancelReservationCustomAction["pnrNumber"] = item.GetAttributeValue<string>("rnt_pnrnumber");
                        cancelReservationCustomAction["reservationId"] = Convert.ToString(item.Id);
                        cancelReservationCustomAction["cancellationDescription"] = "Sistem tarafından otomatik iptal edildi. Zamanı geçmiş rezervasyon";
                        cancelReservationCustomAction["langId"] = (int)1033;
                        logger.traceInfo($"reservationid : {Convert.ToString(item.Id)} - reservation pnr : {item.GetAttributeValue<string>("rnt_pnrnumber")}");

                        var response = crmServiceHelper.IOrganizationService.Execute(cancelReservationCustomAction);

                        var convertedResponse = JsonConvert.DeserializeObject<ReservationCancellationResponse>(Convert.ToString(response.Results["ReservationCancellationResponse"]));
                        logger.traceInputsInfo<ReservationCancellationResponse>(convertedResponse, "cancellation response : ");
                        Thread.Sleep(1000);
                    }
                    catch (Exception ex)
                    {
                        logger.traceInfo(ex.Message);
                        continue;
                    }
                  
                }

            }
            catch (Exception ex)
            {
                logger.traceError(ex.Message);
            }
            #region NoShowReservationsAndItemsUpdate Mongo
            //ReservationItemRepository reservationItemRepository = new ReservationItemRepository(StaticHelper.GetConfiguration("MongoDBHostName"),
            //                                                            StaticHelper.GetConfiguration("MongoDBDatabaseName"));
            //var equipmentReservationItems = reservationItemRepository.getWillNoShowReservationItems(currentDate);

            //foreach (var item in equipmentReservationItems)
            //{
            //    //updates equipment reservation items state.
            //    Entity equipmentReservationItemToUpdate = new Entity("rnt_reservationitem");
            //    equipmentReservationItemToUpdate.Id = new Guid(item.ReservationItemId);
            //    xrmHelper.setState(equipmentReservationItemToUpdate.LogicalName, equipmentReservationItemToUpdate.Id, (int)GlobalEnums.StateCode.Active, (int)rnt_reservationitem_StatusCode.NoShow);

            //    //updates additional products state.
            //    var additionalRelatedReservationItems = reservationItemRepository.getAdditionalReservationItemsByReservationId(item.ReservationId);
            //    foreach (var reservationItem in additionalRelatedReservationItems)
            //    {
            //        Entity reservationItemToUpdate = new Entity("rnt_reservationitem");
            //        reservationItemToUpdate.Id = new Guid(reservationItem.ReservationItemId);
            //        xrmHelper.setState(reservationItemToUpdate.LogicalName, reservationItemToUpdate.Id, (int)GlobalEnums.StateCode.Active, (int)rnt_reservationitem_StatusCode.NoShow);
            //    }

            //    //updates reservations state.
            //    Entity reservation = new Entity("rnt_reservation");
            //    reservation.Id = new Guid(item.ReservationId);
            //    xrmHelper.setState(reservation.LogicalName, reservation.Id, (int)GlobalEnums.StateCode.Active, (int)rnt_reservationitem_StatusCode.NoShow);
            //}
            #endregion

            #region CancelReservationsAndItemsUpdate Mongo
            //var equipmentNoShowReservationItems = reservationItemRepository.getWillCancelReservationItems(currentDate);

            //foreach (var item in equipmentNoShowReservationItems)
            //{
            //    var cancelReservationCustomAction = new OrganizationRequest()
            //    {
            //        RequestName = "rnt_CancelReservation"
            //    };

            //    cancelReservationCustomAction.Parameters.Add("cancellationReason", (int)rnt_reservation_StatusCode.CancelledByCustomer);
            //    //cancelReservationCustomAction.Parameters.Add("offset", (int)item.Offset);
            //    cancelReservationCustomAction.Parameters.Add("pnrNumber", item.PnrNumber);
            //    cancelReservationCustomAction.Parameters.Add("reservationId", item.ReservationId);
            //    cancelReservationCustomAction.Parameters.Add("langId", (int)1055);

            //    OrganizationResponse response = crmServiceHelper.IOrganizationService.Execute(cancelReservationCustomAction);
            //}
            #endregion

        }
    }
}
