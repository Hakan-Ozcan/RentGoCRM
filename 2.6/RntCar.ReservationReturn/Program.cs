using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using RntCar.BusinessLibrary.Business;
using RntCar.BusinessLibrary.Repository;
using RntCar.ClassLibrary;
using RntCar.ClassLibrary._Enums_1033;
using RntCar.Logger;
using RntCar.SDK.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ReservationRefund
{
    class Program
    {
        static void Main(string[] args)
        {
            CrmServiceHelper crmServiceHelper = new CrmServiceHelper();
            LoggerHelper loggerHelper = new LoggerHelper();

            PaymentBL paymentBL = new PaymentBL(crmServiceHelper.IOrganizationService);

            int queryCount = 5000;
            int pageNumber = 1;
            string pagingCookie = null;

            EntityCollection reservations = new EntityCollection();

            while (true)
            {

                QueryExpression query = new QueryExpression("rnt_reservation");
                query.ColumnSet = new ColumnSet(true);

                var mainFilter = new FilterExpression(LogicalOperator.And);
                var statusFilter = new FilterExpression(LogicalOperator.Or);
                var counterFilter = new FilterExpression(LogicalOperator.Or);

                statusFilter.AddCondition("statuscode", ConditionOperator.Equal, (int)rnt_reservation_StatusCode.CancelledByCustomer);
                statusFilter.AddCondition("statuscode", ConditionOperator.Equal, (int)rnt_reservation_StatusCode.CancelledByRentgo);

                counterFilter.AddCondition("rnt_refundtrialcount", ConditionOperator.GreaterThan, 0);
                counterFilter.AddCondition("rnt_refundtrialcount", ConditionOperator.Null);
                mainFilter.AddCondition("rnt_netpayment", ConditionOperator.GreaterThan, 0);
                mainFilter.AddCondition("rnt_pickupdatetime", ConditionOperator.OnOrAfter, new DateTime(2022, 01, 01));

                mainFilter.AddFilter(statusFilter);
                mainFilter.AddFilter(counterFilter);
                query.Criteria.AddFilter(mainFilter);
                query.AddOrder("rnt_pickupdatetime", OrderType.Ascending);
                query.PageInfo = new PagingInfo
                {
                    PageNumber = pageNumber,
                    Count = queryCount,
                    PagingCookie = pagingCookie
                };

                var l = crmServiceHelper.IOrganizationService.RetrieveMultiple(query);
                if (l.MoreRecords)
                {
                    reservations.Entities.AddRange(l.Entities);
                    pageNumber++;
                    pagingCookie = l.PagingCookie;
                }
                else
                {
                    reservations.Entities.AddRange(l.Entities);
                    break;
                }


            }

            List<Entity> clearEntityList = reservations.Entities.Where(x => x.GetAttributeValue<Money>("rnt_netpayment").Value - x.GetAttributeValue<Money>("rnt_totalamount").Value < 1).ToList();

            foreach (var item in clearEntityList)
            {
                Entity tempEntity = reservations.Entities.Where(x => x.Id == item.Id).FirstOrDefault();
                reservations.Entities.Remove(tempEntity);

                Entity resUpdate = new Entity("rnt_reservation");

                resUpdate.Id = item.Id;
                resUpdate["rnt_refundtrialcount"] = 0;

                crmServiceHelper.IOrganizationService.Update(resUpdate);
            }


            List<Entity> firstCheckList = reservations.Entities.Where(x => !x.Contains("rnt_refundtrialcount")).ToList();
            List<Entity> remainCheckList = reservations.Entities.Where(x => x.Contains("rnt_refundtrialcount") && x.GetAttributeValue<int>("rnt_refundtrialcount") < 7).ToList();

            List<Entity> finalList = new List<Entity>();
            finalList.AddRange(firstCheckList);
            finalList.AddRange(remainCheckList);
            finalList = finalList.OrderBy(x => x.GetAttributeValue<DateTime>("rnt_pickupdatetime")).ToList();

            foreach (var finalItem in finalList)
            {
                try
                {
                    decimal refundAmount = finalItem.GetAttributeValue<Money>("rnt_netpayment").Value - finalItem.GetAttributeValue<Money>("rnt_totalamount").Value;

                    var refundList = paymentBL.createRefund(new CreateRefundParameters
                    {
                        isDepositRefund = false,
                        refundAmount = refundAmount,
                        reservationId = finalItem.Id
                    });


                    foreach (var refundEntity in refundList)
                    {
                        var trialCount = finalItem.GetAttributeValue<int>("rnt_refundtrialcount");
                        if (refundEntity.GetAttributeValue<OptionSetValue>("rnt_transactionresult").Value == (int)PaymentEnums.PaymentTransactionResult.Error)
                        {
                            trialCount++;
                        }

                        Entity resUpdate = new Entity("rnt_reservation");

                        resUpdate.Id = finalItem.Id;
                        resUpdate["rnt_refundtrialcount"] = trialCount;
                        resUpdate["rnt_lastrefundtrial"] = DateTime.Today;

                        crmServiceHelper.IOrganizationService.Update(resUpdate);
                    }
                }
                catch (Exception)
                {
                }
            }
        }
    }
}
