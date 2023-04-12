using Microsoft.Xrm.Sdk;
using RntCar.ClassLibrary.PaymentPlan;
using System;
using System.Collections.Generic;

namespace RntCar.SDK.Mappers
{
    public class PaymentPlanMapper
    {
        public List<PaymentPlanData> buildPaymentPlans(List<Entity> plans, Guid groupCodeId,string groupCodeName)
        {
            List<PaymentPlanData> _plans = new List<PaymentPlanData>();
            foreach (var item in plans)
            {
                PaymentPlanData p = new PaymentPlanData
                {
                    paymentPlanId = item.Id.ToString(),
                    groupCodeId = groupCodeId,
                    groupCodeId_str = groupCodeName,
                    month = item.GetAttributeValue<OptionSetValue>("rnt_month").Value,
                    payLaterAmount = item.GetAttributeValue<Money>("rnt_amount").Value,
                    payNowAmount = item.GetAttributeValue<Money>("rnt_paynowamount").Value,
                    priceListId = item.GetAttributeValue<EntityReference>("rnt_monthlypricelistid").Id,
                    priceListName = item.GetAttributeValue<EntityReference>("rnt_monthlypricelistid").Name,
                };
                _plans.Add(p);
            }
            return _plans;
        }
    }
}
