using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Newtonsoft.Json;
using RntCar.BusinessLibrary.Business;
using RntCar.ClassLibrary;
using RntCar.ClassLibrary._Enums_1033;
using RntCar.Logger;
using RntCar.SDK.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static RntCar.ClassLibrary.ReservationEnums;

namespace RntCar.ThreedSecureCheck
{
    class Program
    {
        public static CrmServiceHelper crmServiceHelper;
        public static LoggerHelper loggerHelper;
        static void Main(string[] args)
        {
            crmServiceHelper = new CrmServiceHelper();
            loggerHelper = new LoggerHelper();
             
            var cancellationMinute = Convert.ToInt32(StaticHelper.GetConfiguration("cancellationMinute"));
            loggerHelper.traceInfo("getting reservation");
            QueryExpression getReservationItem = new QueryExpression("rnt_reservation");
            getReservationItem.ColumnSet = new ColumnSet("rnt_reservationid", "statuscode", "rnt_pnrnumber", "createdon");
            getReservationItem.Criteria.AddCondition("statuscode", ConditionOperator.Equal, (int)StatusCode.Waitingfor3D);
            getReservationItem.Criteria.AddCondition("createdon", ConditionOperator.OnOrBefore, DateTime.UtcNow);
            var reservationItems = crmServiceHelper.IOrganizationService.RetrieveMultiple(getReservationItem);
            loggerHelper.traceInfo($"total reservation : {reservationItems.Entities.Count.ToString()}");
            foreach (var reservationItem in reservationItems.Entities)
            {
                var reservationId = reservationItem.GetAttributeValue<Guid>("rnt_reservationid");
                DateTime createdOn = reservationItem.GetAttributeValue<DateTime>("createdon");
                if (createdOn<DateTime.UtcNow.AddMinutes(cancellationMinute * -1))
                {
                    try
                    {
                        OrganizationRequest organizationRequest = new OrganizationRequest("rnt_CancelReservation");
                        organizationRequest["langId"] = 1055;
                        organizationRequest["reservationId"] = reservationId.ToString();
                        organizationRequest["pnrNumber"] = reservationItem.GetAttributeValue<string>("rnt_pnrnumber");
                        organizationRequest["cancellationReason"] = (int)rnt_reservation_StatusCode.CancelledByRentgo;
                        organizationRequest["cancellationDescription"] = "Müşteri tarafından iptal - 3D Başarısız";
                        organizationRequest["cancellationSubReason"] = 100000009;
                        var response = crmServiceHelper.IOrganizationService.Execute(organizationRequest);
                    }
                    catch (Exception ex)
                    {
                        loggerHelper.traceError($"reservationId : {reservationId} error : {ex.Message}");
                    }
                }
            }
            loggerHelper.traceInfo("end reservations");

            loggerHelper.traceInfo("getting customer cards");
            QueryExpression getCustomerCreditCard = new QueryExpression("rnt_customercreditcard");
            getCustomerCreditCard.ColumnSet = new ColumnSet("rnt_customercreditcardid", "statuscode");
            getCustomerCreditCard.Criteria.AddCondition("statuscode", ConditionOperator.Equal, (int)rnt_customercreditcard_StatusCode.WaitingFor3D);
            getCustomerCreditCard.Criteria.AddCondition("createdon", ConditionOperator.LessEqual, DateTime.Now.AddMinutes(cancellationMinute * -1));
            var customerCreditCards = crmServiceHelper.IOrganizationService.RetrieveMultiple(getCustomerCreditCard);
            loggerHelper.traceInfo($"total card : {customerCreditCards.Entities.Count.ToString()}");
            foreach (var cardItem in customerCreditCards.Entities)
            {
                try
                {
                    crmServiceHelper.IOrganizationService.Delete(cardItem.LogicalName, cardItem.Id);
                }
                catch (Exception ex)
                {
                    loggerHelper.traceError($"cardId : {cardItem.GetAttributeValue<Guid>("rnt_customercreditcardid")} error : {ex.Message}");
                }
            }
            loggerHelper.traceInfo("end cards");




        }
    }
}
