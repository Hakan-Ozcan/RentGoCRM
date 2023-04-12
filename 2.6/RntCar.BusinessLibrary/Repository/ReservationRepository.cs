using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Tooling.Connector;
using RntCar.BusinessLibrary.Business;
using RntCar.BusinessLibrary.Handlers;
using RntCar.ClassLibrary;
using RntCar.ClassLibrary._Enums_1033;
using RntCar.SDK.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RntCar.BusinessLibrary.Repository
{
    public class ReservationRepository : RepositoryHandler
    {
        public ReservationRepository(IOrganizationService Service) : base(Service)
        {
        }

        public ReservationRepository(IOrganizationService Service, Guid UserId) : base(Service, UserId)
        {
        }

        public ReservationRepository(IOrganizationService Service, Guid UserId, string OrganizationName) : base(Service, UserId, OrganizationName)
        {
        }

        public ReservationRepository(IOrganizationService Service, CrmServiceClient _crmServiceClient) : base(Service, _crmServiceClient)
        {
        }

        public Entity getReservationById(Guid Id)
        {
            try
            {
                return this.Service.Retrieve("rnt_reservation", Id, new ColumnSet(true));
            }
            catch
            {
                return null;
            }
        }
        public Entity getReservationById(Guid Id, string[] columns)
        {
            try
            {
                return this.Service.Retrieve("rnt_reservation", Id, new ColumnSet(columns));
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public Entity getReservationByPnrNumber(string pnrNumber)
        {
            QueryExpression query = new QueryExpression("rnt_reservation");
            query.ColumnSet = new ColumnSet(true);
            query.Criteria.AddCondition("rnt_pnrnumber", ConditionOperator.Equal, pnrNumber);
            query.Criteria.AddCondition("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active);

            return this.Service.RetrieveMultiple(query).Entities.FirstOrDefault();
        }
        public Entity getReservationByPnrNumberByGivenColumns(string pnrNumber, string[] columns)
        {
            QueryByAttribute querybyattribute = new QueryByAttribute("rnt_reservation");
            querybyattribute.ColumnSet = new ColumnSet(columns);
            querybyattribute.Attributes.AddRange("rnt_pnrnumber");
            querybyattribute.Values.AddRange(pnrNumber);
            return this.Service.RetrieveMultiple(querybyattribute).Entities.FirstOrDefault();
        }
        public List<Entity> getAllActiveReservationByCustomerIdByBetweenGivenDates(Guid customerId, DateTime pickupDate, DateTime dropoffDate)
        {
            QueryExpression query = new QueryExpression("rnt_reservation");
            var mainFilter = new FilterExpression(LogicalOperator.And);
            mainFilter.AddCondition(new ConditionExpression("rnt_customerid", ConditionOperator.Equal, customerId));

            var statusFilter = new FilterExpression(LogicalOperator.Or);
            statusFilter.AddCondition(new ConditionExpression("statuscode", ConditionOperator.Equal, (int)ReservationEnums.StatusCode.New));
            statusFilter.AddCondition(new ConditionExpression("statuscode", ConditionOperator.Equal, (int)ReservationEnums.StatusCode.WaitingforDelivery));

            #region datetime filters
            var mainDateTimeFilter = new FilterExpression(LogicalOperator.Or);
            var dateTimeFilterForPickup = new FilterExpression(LogicalOperator.And);
            dateTimeFilterForPickup.AddCondition(new ConditionExpression("rnt_pickupdatetime", ConditionOperator.LessEqual, pickupDate));
            dateTimeFilterForPickup.AddCondition(new ConditionExpression("rnt_dropoffdatetime", ConditionOperator.GreaterEqual, pickupDate));

            var dateTimeFilterForDropoff = new FilterExpression(LogicalOperator.And);
            dateTimeFilterForDropoff.AddCondition(new ConditionExpression("rnt_pickupdatetime", ConditionOperator.LessEqual, dropoffDate));
            dateTimeFilterForDropoff.AddCondition(new ConditionExpression("rnt_dropoffdatetime", ConditionOperator.GreaterEqual, dropoffDate));

            //var dateTimeFilterForPickupandDropoff = new FilterExpression(LogicalOperator.And);
            //dateTimeFilterForPickupandDropoff.AddCondition(new ConditionExpression("rnt_pickupdatetime", ConditionOperator.OnOrAfter, pickupDate));
            //dateTimeFilterForPickupandDropoff.AddCondition(new ConditionExpression("rnt_dropoffdatetime", ConditionOperator.OnOrBefore, dropoffDate));

            mainDateTimeFilter.AddFilter(dateTimeFilterForPickup);
            mainDateTimeFilter.AddFilter(dateTimeFilterForDropoff);
            //mainDateTimeFilter.AddFilter(dateTimeFilterForPickupandDropoff);
            #endregion

            mainFilter.AddFilter(statusFilter);
            mainFilter.AddFilter(mainDateTimeFilter);
            query.Criteria.AddFilter(mainFilter);
            return this.Service.RetrieveMultiple(query).Entities.ToList();
        }
        public List<ReservationSearchData> getReservationsBySearchParameters(ReservationSearchParameters searchParameters, int langId)
        {
            List<ReservationSearchData> reservations = new List<ReservationSearchData>();

            QueryExpression query = new QueryExpression("rnt_reservation");
            query.ColumnSet = new ColumnSet("rnt_reservationnumber",
                                            "rnt_pnrnumber",
                                            "rnt_ismonthly",
                                            "rnt_howmanymonths",
                                            "rnt_corporatetotalamount",
                                            "rnt_customerid",
                                            "rnt_pickupbranchid",
                                            "rnt_pickupdatetime",
                                            "rnt_dropoffbranchid",
                                            "rnt_dropoffdatetime",
                                            "rnt_groupcodeid",
                                            "rnt_paymentchoicecode",
                                            "statuscode",
                                            "statecode",
                                            "rnt_totalamount",
                                            "rnt_paidamount",
                                            "rnt_refundamount",
                                            "rnt_reservationtypecode",
                                            "rnt_corporateid",
                                            "rnt_pricingtype",
                                            "rnt_paymentmethodcode",
                                            "rnt_doublecreditcard",
                                            "rnt_depositamount",
                                            "rnt_findeks",
                                            "rnt_kilometerlimit",
                                            "rnt_minimumage",
                                            "rnt_youngdriverage",
                                            "rnt_minimumdriverlicience",
                                            "rnt_youngdriverlicence",
                                            "rnt_overkilometerprice",
                                            "transactioncurrencyid",
                                            "rnt_dummycontactinformation",
                                            "rnt_segmentcode",
                                            "rnt_couponcode");

            query.Criteria = new FilterExpression(LogicalOperator.And);

            if (searchParameters.pickupBranchId.HasValue)
                query.Criteria.AddCondition("rnt_pickupbranchid", ConditionOperator.Equal, searchParameters.pickupBranchId);
            if (searchParameters.dropoffBranchId.HasValue)
                query.Criteria.AddCondition("rnt_dropoffbranchid", ConditionOperator.Equal, searchParameters.dropoffBranchId);
            //if (searchParameters.customerId.HasValue)
            //    query.Criteria.AddCondition("rnt_customerid", ConditionOperator.Equal, searchParameters.customerId);

            if (searchParameters.pickupDate.HasValue)
            {
                var pickup = searchParameters.pickupDate.Value;
                var endTime = pickup.Date.AddHours(23).AddMinutes(59);
                var beginTime = pickup.Date;
                FilterExpression pickupDateFilter = query.Criteria.AddFilter(LogicalOperator.And);
                //todo will may check later for performance issue
                pickupDateFilter.AddCondition("rnt_pickupdatetime", ConditionOperator.LessEqual, endTime);
                pickupDateFilter.AddCondition("rnt_pickupdatetime", ConditionOperator.GreaterEqual, beginTime);
            }

            if (searchParameters.dropoffDate.HasValue)
            {
                var dropOff = searchParameters.dropoffDate.Value;
                var endTime = dropOff.Date.AddHours(23).AddMinutes(59);
                var beginTime = dropOff.Date;
                FilterExpression dropoffDateFilter = query.Criteria.AddFilter(LogicalOperator.And);
                //todo will may check later for performance issue
                dropoffDateFilter.AddCondition("rnt_dropoffdatetime", ConditionOperator.LessEqual, endTime);
                dropoffDateFilter.AddCondition("rnt_dropoffdatetime", ConditionOperator.GreaterEqual, beginTime);
            }
            if (!string.IsNullOrEmpty(searchParameters.dummyInformation))
            {
                query.Criteria.AddCondition("rnt_dummycontactinformation", ConditionOperator.Like, "%" + searchParameters.dummyInformation + "%");
            }

            if (!string.IsNullOrEmpty(searchParameters.reservationNumber))
            {
                FilterExpression numberFilter = query.Criteria.AddFilter(LogicalOperator.Or);
                numberFilter.AddCondition("rnt_reservationnumber", ConditionOperator.Equal, searchParameters.reservationNumber);
                numberFilter.AddCondition("rnt_pnrnumber", ConditionOperator.Equal, searchParameters.reservationNumber);
                numberFilter.AddCondition("rnt_referencenumber", ConditionOperator.Equal, searchParameters.reservationNumber);
            }
            if (!string.IsNullOrEmpty(searchParameters.customerId))
            {
                query.LinkEntities[0] = query.AddLink("contact", "rnt_customerid", "contactid", JoinOperator.Inner);
                query.LinkEntities[0].LinkCriteria.Filters.Add(
                   new FilterExpression()
                   {
                       FilterOperator = LogicalOperator.Or,
                       Conditions =
                       {
                           new ConditionExpression("fullname", ConditionOperator.Like, "%" + searchParameters.customerId + "%"),
                           new ConditionExpression("governmentid", ConditionOperator.Like, "%" + searchParameters.customerId + "%"),
                           new ConditionExpression("rnt_passportnumber", ConditionOperator.Like, "%" + searchParameters.customerId + "%"),
                       }
                   }
               );
            }
            var result = this.Service.RetrieveMultiple(query);
            ConfigurationBL configurationBL = new ConfigurationBL(this.Service);
            var currencyTL = configurationBL.GetConfigurationByName("currency_TRY");
            var currencyCode = StaticHelper.tlSymbol;
            var exchangeRate = decimal.Zero;
            foreach (var item in result.Entities)
            {
                if (new Guid(currencyTL) != item.GetAttributeValue<EntityReference>("transactioncurrencyid").Id)
                {
                    CurrencyRepository currencyRepository = new CurrencyRepository(this.Service);
                    var c = currencyRepository.getCurrencyById(item.GetAttributeValue<EntityReference>("transactioncurrencyid").Id, new string[] { "currencysymbol", "exchangerate" });
                    currencyCode = c.GetAttributeValue<string>("currencysymbol");
                    exchangeRate = c.GetAttributeValue<decimal>("exchangerate");
                }

                var paidAmount = item.Attributes.Contains("rnt_paidamount") ? item.GetAttributeValue<Money>("rnt_paidamount").Value : 0;
                var refundAmount = item.Attributes.Contains("rnt_refundamount") ? item.GetAttributeValue<Money>("rnt_refundamount").Value : 0;
                reservations.Add(new ReservationSearchData
                {
                    isMonthly = item.GetAttributeValue<bool>("rnt_ismonthly"),
                    howManyMonths = item.Attributes.Contains("rnt_howmanymonths") ? item.GetAttributeValue<OptionSetValue>("rnt_howmanymonths").Value : 0,
                    exchangeRate = exchangeRate,
                    currencySymbol = currencyCode,
                    corporateTotalAmount = item.GetAttributeValue<Money>("rnt_corporatetotalamount")?.Value < 0 ? 0 : item.GetAttributeValue<Money>("rnt_corporatetotalamount")?.Value,
                    reservationNumber = item.GetAttributeValue<string>("rnt_reservationnumber"),
                    pnrNumber = item.GetAttributeValue<string>("rnt_pnrnumber"),
                    contactName = item.Attributes.Contains("rnt_customerid") ? item.GetAttributeValue<EntityReference>("rnt_customerid").Name : null,
                    contactId = item.Attributes.Contains("rnt_customerid") ? item.GetAttributeValue<EntityReference>("rnt_customerid").Id : Guid.Empty,
                    pickupBranchId = item.Attributes.Contains("rnt_pickupbranchid") ? item.GetAttributeValue<EntityReference>("rnt_pickupbranchid").Id : Guid.Empty,
                    pickupBranchName = item.Attributes.Contains("rnt_pickupbranchid") ? item.GetAttributeValue<EntityReference>("rnt_pickupbranchid").Name : null,
                    pickupDateTime = item.GetAttributeValue<DateTime>("rnt_pickupdatetime"),
                    dropoffBranchId = item.Attributes.Contains("rnt_dropoffbranchid") ? item.GetAttributeValue<EntityReference>("rnt_dropoffbranchid").Id : Guid.Empty,
                    dropoffBranchName = item.Attributes.Contains("rnt_dropoffbranchid") ? item.GetAttributeValue<EntityReference>("rnt_dropoffbranchid").Name : null,
                    dropoffDateTime = item.GetAttributeValue<DateTime>("rnt_dropoffdatetime"),
                    groupCodeId = item.Attributes.Contains("rnt_groupcodeid") ? item.GetAttributeValue<EntityReference>("rnt_groupcodeid").Id : Guid.Empty,
                    groupCodeName = item.Attributes.Contains("rnt_groupcodeid") ? item.GetAttributeValue<EntityReference>("rnt_groupcodeid").Name : string.Empty,
                    paymentType = item.Attributes.Contains("rnt_paymentchoicecode") ? item.GetAttributeValue<OptionSetValue>("rnt_paymentchoicecode").Value : 0,
                    status = item.GetAttributeValue<OptionSetValue>("statuscode").Value,
                    statusName = Convert.ToString(item.FormattedValues["statuscode"]),
                    state = item.GetAttributeValue<OptionSetValue>("statecode").Value,
                    totalAmount = item.Attributes.Contains("rnt_totalamount") ? item.GetAttributeValue<Money>("rnt_totalamount").Value : 0,
                    reservationPaidAmount = paidAmount - refundAmount,
                    reservationId = item.Id,
                    reservationType = item.Attributes.Contains("rnt_reservationtypecode") ? item.GetAttributeValue<OptionSetValue>("rnt_reservationtypecode").Value : 0,
                    corporateName = item.Attributes.Contains("rnt_corporateid") ? item.GetAttributeValue<EntityReference>("rnt_corporateid").Name : string.Empty,
                    corporateId = item.Attributes.Contains("rnt_corporateid") ? item.GetAttributeValue<EntityReference>("rnt_corporateid").Id : Guid.Empty,
                    pricingType = item.Attributes.Contains("rnt_pricingtype") ? item.GetAttributeValue<string>("rnt_pricingtype") : string.Empty,
                    paymentMethod = item.Attributes.Contains("rnt_paymentmethodcode") ? item.GetAttributeValue<OptionSetValue>("rnt_paymentmethodcode").Value : 0,
                    doubleCreditCard = item.Attributes.Contains("rnt_doublecreditcard") ? item.GetAttributeValue<bool>("rnt_doublecreditcard") : false,
                    depositAmount = item.Attributes.Contains("rnt_depositamount") ? item.GetAttributeValue<Money>("rnt_depositamount").Value : decimal.Zero,
                    findeksPoint = item.Attributes.Contains("rnt_findeks") ? item.GetAttributeValue<int>("rnt_findeks") : 0,
                    kilometerLimit = item.GetAttributeValue<int>("rnt_kilometerlimit"),
                    minimumAge = item.GetAttributeValue<int>("rnt_minimumage"),
                    youngMinimumAge = item.GetAttributeValue<int>("rnt_youngdriverage"),
                    minimumDrivingLicense = item.GetAttributeValue<int>("rnt_minimumdriverlicience"),
                    minimumYoungDriverLicense = item.GetAttributeValue<int>("rnt_youngdriverlicence"),
                    overKilometerPrice = item.Attributes.Contains("rnt_overkilometerprice") ? item.GetAttributeValue<Money>("rnt_overkilometerprice").Value : decimal.Zero,
                    segment = item.Attributes.Contains("rnt_segmentcode") ? item.GetAttributeValue<OptionSetValue>("rnt_segmentcode").Value : 0,
                    couponCode = item.GetAttributeValue<string>("rnt_couponcode")
                });

            }

            return reservations;
        }
        public List<Entity> getNoShowReservations(DateTime currentDate)
        {
            List<Entity> reservations = new List<Entity>();

            QueryExpression query = new QueryExpression("rnt_reservation");
            query.ColumnSet = new ColumnSet("rnt_reservationnumber",
                                            "rnt_pnrnumber",
                                            "rnt_pickupdatetime",
                                            "rnt_dropoffdatetime",
                                            "statuscode",
                                            "statecode"
                                           );

            FilterExpression pickupDateFilter = query.Criteria.AddFilter(LogicalOperator.And);
            FilterExpression statusFilter = query.Criteria.AddFilter(LogicalOperator.And);

            pickupDateFilter.AddCondition("rnt_pickupdatetime", ConditionOperator.LessEqual, currentDate);
            statusFilter.AddCondition("statuscode", ConditionOperator.Equal, (Int32)rnt_reservation_StatusCode.New);
            var a = this.Service.RetrieveMultiple(query);
            return reservations = this.Service.RetrieveMultiple(query).Entities.ToList();
        }
        public List<Entity> getWillNoShowReservationsWithGivenColumns(DateTime currentDate, string[] columns)
        {
            QueryExpression query = new QueryExpression("rnt_reservation");
            query.ColumnSet = new ColumnSet(columns);
            query.Criteria.AddCondition("rnt_noshowtime", ConditionOperator.LessEqual, currentDate);
            query.Criteria.AddCondition("statuscode", ConditionOperator.Equal, (int)rnt_reservation_StatusCode.New);
            return this.Service.RetrieveMultiple(query).Entities.ToList();
        }
        public List<Entity> getWillCancelReservationsWithGivenColumns(DateTime currentDate, string[] columns)
        {
            QueryExpression query = new QueryExpression("rnt_reservation");
            query.ColumnSet = new ColumnSet(columns);
            query.Criteria.AddCondition("rnt_cancellationtime", ConditionOperator.LessEqual, currentDate);
            query.Criteria.AddCondition("statuscode", ConditionOperator.Equal, (int)rnt_reservation_StatusCode.NoShow);
            return this.Service.RetrieveMultiple(query).Entities.ToList();
        }
        public List<Entity> getFurtherReservations(string[] columns, DateTime date)
        {
            QueryExpression query = new QueryExpression("rnt_reservation");
            query.ColumnSet = new ColumnSet(columns);
            query.Criteria.AddCondition("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active);
            query.Criteria.AddCondition("rnt_pickupdatetime", ConditionOperator.OnOrAfter, date);

            return this.Service.RetrieveMultiple(query).Entities.ToList();
        }

        public List<Entity> getActiveReservationsByCorporateId(string[] columns, Guid corporateId)
        {
            QueryExpression query = new QueryExpression("rnt_reservation");
            query.ColumnSet = new ColumnSet(columns);
            query.Criteria.AddCondition("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active);
            query.Criteria.AddCondition("rnt_corporateid", ConditionOperator.Equal, corporateId);

            return this.Service.RetrieveMultiple(query).Entities.ToList();
        }
        public List<Entity> getNewReservationsByCorporateId(string[] columns, Guid corporateId)
        {
            QueryExpression query = new QueryExpression("rnt_reservation");
            query.ColumnSet = new ColumnSet(columns);
            query.Criteria.AddCondition("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active);
            query.Criteria.AddCondition("statuscode", ConditionOperator.Equal, (int)rnt_reservation_StatusCode.New);
            query.Criteria.AddCondition("rnt_corporateid", ConditionOperator.Equal, corporateId);

            return this.Service.RetrieveMultiple(query).Entities.ToList();
        }

    }
}
