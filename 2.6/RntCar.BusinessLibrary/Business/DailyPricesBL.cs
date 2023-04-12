using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Newtonsoft.Json;
using RestSharp;
using RntCar.BusinessLibrary.Handlers;
using RntCar.BusinessLibrary.Repository;
using RntCar.ClassLibrary._Enums_1033;
using RntCar.ClassLibrary.odata;
using RntCar.SDK.Common;
using System;
using System.Linq;

namespace RntCar.BusinessLibrary.Business
{
    public class DailyPricesBL : BusinessHandler
    {
        public DailyPricesBL(IOrganizationService orgService) : base(orgService)
        {
        }

        public DailyPricesBL(IOrganizationService orgService, ITracingService tracingService) : base(orgService, tracingService)
        {
        }

        public EntityCollection getDailyPrices(QueryExpression queryExpression)
        {
            this.Trace("lets start");
            var conditions = queryExpression.Criteria.Conditions;

            EntityCollection results = new EntityCollection();
            ConfigurationBL configurationBL = new ConfigurationBL(this.OrgService);
            //todo place in business classes
            var responseUrl = configurationBL.GetConfigurationByName("MongoDBodataURL");
            RestSharpHelper helper = new RestSharpHelper(responseUrl, "DailyPrices", Method.GET);

            if (conditions.Count > 0 &&
               conditions.FirstOrDefault()?.AttributeName == "rnt_reservationitemid" &&
               conditions.FirstOrDefault()?.Operator == ConditionOperator.Equal)
            {
                this.Trace("condition " + conditions.FirstOrDefault()?.Values.FirstOrDefault());
                helper = new RestSharpHelper(responseUrl, "DailyPrices", Method.GET);
                helper.AddQueryParameter("reservationItemId", Convert.ToString(conditions.FirstOrDefault()?.Values.FirstOrDefault()));
            }
            else if (conditions.Count > 0 &&
                conditions.FirstOrDefault()?.AttributeName == "rnt_contractitemid" &&
                conditions.FirstOrDefault()?.Operator == ConditionOperator.Equal)
            {
                this.Trace("condition " + conditions.FirstOrDefault()?.Values.FirstOrDefault());
                helper = new RestSharpHelper(responseUrl, "ContractDailyPrices", Method.GET);
                helper.AddQueryParameter("contractItemId", Convert.ToString(conditions.FirstOrDefault()?.Values.FirstOrDefault()));
            }
            var paymentChoice = 0;
            // execute the request   

            var responseReservation = helper.Execute();
            var v = JsonConvert.DeserializeObject<DailyPriceWrapper>(responseReservation.Content);
            results.EntityName = "rnt_dailyprice";


            if (conditions.Count > 0 &&
              conditions.FirstOrDefault()?.AttributeName == "rnt_reservationitemid" &&
              conditions.FirstOrDefault()?.Operator == ConditionOperator.Equal &&
              v.value.Count > 0)
            {
                ReservationItemRepository reservationItemRepository = new ReservationItemRepository(this.OrgService);
                var res = reservationItemRepository.getReservationItemById((Guid)conditions.FirstOrDefault()?.Values.FirstOrDefault(), new string[] { "rnt_reservationid" });

                this.Trace(res == null ? " res null" : "res not null");
                ReservationRepository reservationRepository = new ReservationRepository(this.OrgService);
                var r = reservationRepository.getReservationById(res.GetAttributeValue<EntityReference>("rnt_reservationid").Id, new string[] { "rnt_paymentchoicecode" });

                paymentChoice = r.GetAttributeValue<OptionSetValue>("rnt_paymentchoicecode").Value;

            }

            else if (conditions.Count > 0 &&
                    conditions.FirstOrDefault()?.AttributeName == "rnt_contractitemid" &&
                    conditions.FirstOrDefault()?.Operator == ConditionOperator.Equal &&
                    v.value.Count > 0)
            {
                ContractItemRepository contractItemRepository = new ContractItemRepository(this.OrgService);
                var cont = contractItemRepository.getContractItemIdByGivenColumns((Guid)conditions.FirstOrDefault()?.Values.FirstOrDefault(), new string[] { "rnt_contractid" });

                this.Trace(cont == null ? " cont null" : "cont not null");

                ContractRepository contractRepository = new ContractRepository(this.OrgService);
                var r = contractRepository.getContractById(cont.GetAttributeValue<EntityReference>("rnt_contractid").Id, new string[] { "rnt_paymentchoicecode" });

                paymentChoice = r.GetAttributeValue<OptionSetValue>("rnt_paymentchoicecode").Value;

            }

            this.Trace("paymentChoice" + paymentChoice);

            foreach (var item in v.value)
            {
                Entity e = new Entity("rnt_dailyprice");
                e["rnt_dailypriceid"] = item.dailyPricesId;
                e["rnt_name"] = item.trackingNumber;
                e["rnt_pricedate"] = item.priceDate;
                e["rnt_totalamount"] = item.totalAmount;
                e["rnt_baseprice"] = item.selectedGroupCodeAmount;
                e["rnt_availabilityrate"] = item.availabilityRate;
                e["rnt_priceafteravailability"] = item.priceAfterAvailabilityFactor;
                e["rnt_priceafterchannelfactor"] = item.priceAfterChannelFactor;
                e["rnt_priceafterweekdaysfactor"] = item.priceAfterWeekDaysFactor;
                e["rnt_priceafterspecialdaysfactor"] = item.priceAfterSpecialDaysFactor;
                e["rnt_priceaftercustomerfactor"] = item.priceAfterCustomerFactor;
                e["rnt_priceafterbranchfactor"] = item.priceAfterBranchFactor;
                e["rnt_priceafterupdatefactor"] = Convert.ToString(item.priceAfterBranch2Factor);
                e["rnt_priceafterequalityfactor"] = item.priceAfterEqualityFactor;

                if (paymentChoice == (int)rnt_reservation_rnt_paymentchoicecode.PayLater)
                {
                    item.priceAfterPayMethod = item.priceAfterPayMethodPayLater;
                    //e["rnt_price"] = item.priceAfterPayMethod;
                }
                else
                {
                    item.priceAfterPayMethod = item.priceAfterPayMethodPayNow;
                    //e["rnt_price"] = item.priceAfterPayMethod;
                }
                this.Trace("item.priceAfterPayMethod : " +  item.priceAfterPayMethod);
                e["rnt_paylaterprice"] = item.payLaterAmount;
                e["rnt_paynowprice"] = item.payNowAmount;
                e["rnt_priceafterpay"] = item.priceAfterPayMethod;
                if (item.reservationItemId != Guid.Empty)
                {
                    e["rnt_reservationitemid"] = new EntityReference("rnt_reservationitem", item.reservationItemId);
                }

                else if (item.contractItemId != Guid.Empty)
                {
                    e["rnt_contractitemid"] = new EntityReference("rnt_contractitem", item.contractItemId);
                }

                results.Entities.Add(e);
            }
            this.Trace(JsonConvert.SerializeObject(v.value));

            return results;
        }

    }
}
