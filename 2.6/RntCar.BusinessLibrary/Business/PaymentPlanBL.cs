using Microsoft.Xrm.Sdk;
using RntCar.BusinessLibrary.Handlers;
using RntCar.BusinessLibrary.Repository;
using RntCar.ClassLibrary.PaymentPlan;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace RntCar.BusinessLibrary.Business
{
    public class PaymentPlanBL : BusinessHandler
    {
        public PaymentPlanBL(IOrganizationService orgService) : base(orgService)
        {
        }

        public PaymentPlanBL(IOrganizationService orgService, ITracingService tracingService) : base(orgService, tracingService)
        {
        }
        public void deletePaymentPlansByReservationId(Guid reservationId)
        {
            PaymentPlanRepository paymentPlanRepository = new PaymentPlanRepository(this.OrgService);
            var plans = paymentPlanRepository.getPaymentPlansByReservationId(reservationId);
            foreach (var item in plans)
            {
                this.OrgService.Delete("rnt_documentpaymentplan", item.Id);
            }

        }
        public void deletePaymentPlanById(Guid planId)
        {
            this.OrgService.Delete("rnt_documentpaymentplan", planId);
        }
        public void deletePaymentPlansByContractId(Guid contractId)
        {
            PaymentPlanRepository paymentPlanRepository = new PaymentPlanRepository(this.OrgService);
            var plans = paymentPlanRepository.getPaymentPlansByContractId(contractId);
            foreach (var item in plans)
            {
                this.OrgService.Delete("rnt_documentpaymentplan", item.Id);
            }

        }
        public void createPaymentPlan(PaymentPlanData paymentPlanData, Guid? reservationId, Guid? contractId)
        {

            Entity e = new Entity("rnt_documentpaymentplan");
            if (reservationId.HasValue)
            {
                e["rnt_reservation"] = new EntityReference("rnt_reservation", reservationId.Value);
            }
            else if (contractId.HasValue)
            {
                e["rnt_contractid"] = new EntityReference("rnt_contract", contractId.Value);
            }
            e["rnt_monthlypricelistid"] = new EntityReference("rnt_monthlypricelist", paymentPlanData.priceListId);
            e["rnt_amount"] = new Money(paymentPlanData.payLaterAmount);
            e["rnt_paynowamount"] = new Money(paymentPlanData.payNowAmount);
            e["rnt_month"] = new OptionSetValue(paymentPlanData.month);

            CultureInfo usEnglish = new CultureInfo("tr-TR");
            DateTimeFormatInfo englishInfo = usEnglish.DateTimeFormat;
            string monthName = englishInfo.MonthNames[paymentPlanData.month - 1];
            e["rnt_name"] = monthName;
            this.OrgService.Create(e);

        }
        public void createPaymentPlans(List<PaymentPlanData> paymentPlanData, Guid? reservationId, Guid? contractId)
        {
            foreach (var item in paymentPlanData)
            {
                Entity e = new Entity("rnt_documentpaymentplan");
                if (reservationId.HasValue)
                {
                    e["rnt_reservation"] = new EntityReference("rnt_reservation", reservationId.Value);
                }
                else if (contractId.HasValue)
                {
                    e["rnt_contractid"] = new EntityReference("rnt_contract", contractId.Value);
                }
                e["rnt_monthlypricelistid"] = new EntityReference("rnt_monthlypricelist", item.priceListId);
                e["rnt_amount"] = new Money(item.payLaterAmount);
                e["rnt_paynowamount"] = new Money(item.payNowAmount);
                e["rnt_month"] = new OptionSetValue(item.month);

                CultureInfo usEnglish = new CultureInfo("tr-TR");
                DateTimeFormatInfo englishInfo = usEnglish.DateTimeFormat;
                string monthName = englishInfo.MonthNames[item.month - 1];
                e["rnt_name"] = monthName;
                this.OrgService.Create(e);
            }

        }
    }
}
