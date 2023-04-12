using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Tooling.Connector;
using RntCar.BusinessLibrary.Handlers;
using RntCar.BusinessLibrary.Helpers;
using RntCar.ClassLibrary;
using RntCar.ClassLibrary._Enums_1033;
using RntCar.SDK.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.BusinessLibrary.Repository
{
    public class ContractRepository : RepositoryHandler
    {
        public ContractRepository(IOrganizationService Service) : base(Service)
        {
        }

        public ContractRepository(CrmServiceClient _crmServiceClient) : base(_crmServiceClient)
        {
        }

        public ContractRepository(IOrganizationService Service, Guid UserId) : base(Service, UserId)
        {
        }

        public ContractRepository(IOrganizationService Service, CrmServiceClient _crmServiceClient) : base(Service, _crmServiceClient)
        {
        }

        public ContractRepository(IOrganizationService Service, Guid UserId, string OrganizationName) : base(Service, UserId, OrganizationName)
        {
        }

        public Entity getContractById(Guid Id)
        {
            try
            {
                return this.Service.Retrieve("rnt_contract", Id, new ColumnSet(true));
            }
            catch
            {
                return null;
            }
        }
        public Entity getContractById(Guid contractId, string[] columns)
        {
            return this.retrieveById("rnt_contract", contractId, columns);
        }
        public Entity getRentalContractByEquipment(Guid EquipmentId, string[] columns)
        {
            QueryExpression queryExpression = new QueryExpression("rnt_contract");
            queryExpression.ColumnSet = new ColumnSet(columns);
            queryExpression.Criteria.AddCondition(new ConditionExpression("statuscode", ConditionOperator.Equal, (int)ContractEnums.StatusCode.Rental));
            queryExpression.Criteria.AddCondition("rnt_equipmentid", ConditionOperator.Equal, EquipmentId);

            return this.retrieveMultiple(queryExpression).Entities.FirstOrDefault();
        }
        public List<Entity> getAllActiveContracts(string[] columns)
        {
            QueryExpression queryExpression = new QueryExpression("rnt_contract");
            queryExpression.ColumnSet = new ColumnSet(columns);
            queryExpression.Criteria.AddCondition("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active);
            queryExpression.Criteria.AddCondition("rnt_pickupbranchid", ConditionOperator.NotNull);

            return this.retrieveMultiple(queryExpression).Entities.ToList();
        }
        public Entity getContractByReservationId(string reservationId)
        {
            QueryByAttribute querybyattribute = new QueryByAttribute("rnt_contract");
            querybyattribute.ColumnSet = new ColumnSet(true);
            querybyattribute.Attributes.AddRange("rnt_reservationid");
            querybyattribute.Values.AddRange(reservationId);
            return this.retrieveMultiple(querybyattribute).Entities.FirstOrDefault();

        }
        public Entity getContractByPnrNumber(string pnrNumber, string[] columns)
        {
            QueryByAttribute querybyattribute = new QueryByAttribute("rnt_contract");
            querybyattribute.ColumnSet = new ColumnSet(columns);
            querybyattribute.Attributes.AddRange("rnt_pnrnumber");
            querybyattribute.Values.AddRange(pnrNumber);
            return this.retrieveMultiple(querybyattribute).Entities.FirstOrDefault();

        }
        public Entity getContractByPnrNumber(string pnrNumber)
        {
            QueryByAttribute querybyattribute = new QueryByAttribute("rnt_contract");
            querybyattribute.ColumnSet = new ColumnSet(true);
            querybyattribute.Attributes.AddRange("rnt_pnrnumber");
            querybyattribute.Values.AddRange(pnrNumber);
            return this.retrieveMultiple(querybyattribute).Entities.FirstOrDefault();

        }
        public Entity getContractByContractNumber(string contractNumber, string[] columns)
        {
            QueryByAttribute querybyattribute = new QueryByAttribute("rnt_contract");
            querybyattribute.ColumnSet = new ColumnSet(columns);
            querybyattribute.Attributes.AddRange("rnt_contractnumber");
            querybyattribute.Values.AddRange(contractNumber);
            return this.retrieveMultiple(querybyattribute).Entities.FirstOrDefault();

        }
        public Entity getContractByContractNumber(string contractNumber)
        {
            QueryByAttribute querybyattribute = new QueryByAttribute("rnt_contract");
            querybyattribute.ColumnSet = new ColumnSet(true);
            querybyattribute.Attributes.AddRange("rnt_contractnumber");
            querybyattribute.Values.AddRange(contractNumber);
            return this.retrieveMultiple(querybyattribute).Entities.FirstOrDefault();

        }
        public List<Entity> getYesterdayCompletedContracts()
        {
            QueryExpression queryExpression = new QueryExpression("rnt_contract");
            queryExpression.ColumnSet = new ColumnSet(true);
            queryExpression.Criteria.AddCondition("rnt_dropoffdatetime", ConditionOperator.Yesterday);
            queryExpression.Criteria.AddCondition("statuscode", ConditionOperator.Equal, (int)rnt_contract_StatusCode.Completed);
            //queryExpression.Criteria.AddCondition("rnt_dropoffdatetime", ConditionOperator.On, "2020-06-16");             
            return this.retrieveMultiple(queryExpression).Entities.ToList();
        }
        public List<Entity> getCreatedContractsByXlastDays(int days)
        {
            QueryExpression queryExpression = new QueryExpression("rnt_contract");
            queryExpression.ColumnSet = new ColumnSet(true);
            queryExpression.Criteria.AddCondition("createdon", ConditionOperator.LastXDays, days);
            return this.retrieveMultiple(queryExpression).Entities.ToList();
        }
        public List<Entity> getMonthlyCompletedContractsByXlastDays(int days)
        {
            QueryExpression queryExpression = new QueryExpression("rnt_contract");
            queryExpression.ColumnSet = new ColumnSet(true);
            queryExpression.Criteria.AddCondition("rnt_dropoffdatetime", ConditionOperator.LastXDays, days);
            queryExpression.Criteria.AddCondition("rnt_ismonthly", ConditionOperator.Equal, true);
            queryExpression.Criteria.AddCondition("statuscode", ConditionOperator.Equal, (int)rnt_contract_StatusCode.Completed);
            return this.retrieveMultiple(queryExpression).Entities.ToList();
        }
        public List<Entity> getCompletedPegasusContracts(string campaignId)
        {
            QueryExpression queryExpression = new QueryExpression("rnt_contract");
            queryExpression.ColumnSet = new ColumnSet(true);
            var benefitFilter = new FilterExpression(LogicalOperator.Or);
            benefitFilter.AddCondition(new ConditionExpression("rnt_benefitfrompegasus", ConditionOperator.Equal, false));
            benefitFilter.AddCondition(new ConditionExpression("rnt_benefitfrompegasus", ConditionOperator.Null));

            queryExpression.Criteria.AddFilter(benefitFilter);
            queryExpression.Criteria.AddCondition("rnt_campaignid", ConditionOperator.Equal, campaignId);
            queryExpression.Criteria.AddCondition("statuscode", ConditionOperator.Equal, (int)rnt_contract_StatusCode.Completed);
            return this.retrieveMultiple(queryExpression).Entities.ToList();
        }
        public List<Entity> getCompletedContractsByXlastDays(int days)
        {
            QueryExpression queryExpression = new QueryExpression("rnt_contract");
            queryExpression.ColumnSet = new ColumnSet(true);
            queryExpression.Criteria.AddCondition("rnt_dropoffdatetime", ConditionOperator.LastXDays, days);
            queryExpression.Criteria.AddCondition("statuscode", ConditionOperator.Equal, (int)rnt_contract_StatusCode.Completed);
            return this.retrieveMultiple(queryExpression).Entities.ToList();
        }
        public List<Entity> getCompletedContractsByXlastDays(int days, string[] columns)
        {
            QueryExpression queryExpression = new QueryExpression("rnt_contract");
            queryExpression.ColumnSet = new ColumnSet(columns);
            queryExpression.Criteria.AddCondition("rnt_dropoffdatetime", ConditionOperator.LastXDays, days);
            queryExpression.Criteria.AddCondition("statuscode", ConditionOperator.Equal, (int)rnt_contract_StatusCode.Completed);
            return this.retrieveMultiple(queryExpression).Entities.ToList();
        }
        public List<Entity> getCompletedDepthContractsByXlastDays(int days, string[] columns)
        {
            List<Entity> contracts = new List<Entity>();
            // Set the number of records per page to retrieve.
            int queryCount = 5000;
            // Initialize the page number.
            int pageNumber = 1;
            string pagingCookie = null;
            while (true)
            {
                QueryExpression queryExpression = new QueryExpression("rnt_contract");
                queryExpression.ColumnSet = new ColumnSet(columns);
                queryExpression.Criteria.AddCondition("rnt_dropoffdatetime", ConditionOperator.LastXDays, days);
                queryExpression.Criteria.AddCondition("statuscode", ConditionOperator.Equal, (int)rnt_contract_StatusCode.Completed);
                //queryExpression.Criteria.AddCondition("rnt_contractid", ConditionOperator.Equal, "32300ee0-8159-ea11-a811-000d3a2c0643");                  
                queryExpression.PageInfo = new PagingInfo
                {
                    PageNumber = pageNumber,
                    Count = queryCount,
                    PagingCookie = pagingCookie
                };
                var l = this.retrieveMultiple(queryExpression); ;
                if (l.MoreRecords)
                {
                    contracts.AddRange(l.Entities.ToList());
                    pageNumber++;
                    pagingCookie = l.PagingCookie;
                }
                else
                {
                    contracts.AddRange(l.Entities.ToList());
                    break;
                }

            }
            return contracts;
        }
        public List<Entity> getCompletedDepthContractsByXlastDays(int days)
        {
            List<Entity> contracts = new List<Entity>();
            // Set the number of records per page to retrieve.
            int queryCount = 5000;
            // Initialize the page number.
            int pageNumber = 1;
            string pagingCookie = null;
            while (true)
            {
                QueryExpression queryExpression = new QueryExpression("rnt_contract");
                queryExpression.ColumnSet = new ColumnSet(true);
                queryExpression.Criteria.AddCondition("rnt_dropoffdatetime", ConditionOperator.LastXDays, days);
                queryExpression.Criteria.AddCondition("statuscode", ConditionOperator.Equal, (int)rnt_contract_StatusCode.Completed);
                //queryExpression.Criteria.AddCondition("rnt_contractid", ConditionOperator.Equal, "32300ee0-8159-ea11-a811-000d3a2c0643");                  
                queryExpression.PageInfo = new PagingInfo
                {
                    PageNumber = pageNumber,
                    Count = queryCount,
                    PagingCookie = pagingCookie
                };
                var l = this.retrieveMultiple(queryExpression); ;
                if (l.MoreRecords)
                {
                    contracts.AddRange(l.Entities.ToList());
                    pageNumber++;
                    pagingCookie = l.PagingCookie;
                }
                else
                {
                    contracts.AddRange(l.Entities.ToList());
                    break;
                }

            }
            return contracts;
        }
        public List<Entity> getContractsBySearchParameters(ContractSearchParameters searchParameters)
        {
            QueryExpression query = new QueryExpression("rnt_contract");
            query.ColumnSet = new ColumnSet("rnt_contractnumber",
                                            "rnt_contractid",
                                            "rnt_corporatetotalamount",
                                            "rnt_pnrnumber",
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
                                            "rnt_contracttypecode",
                                            "rnt_corporateid",
                                            "rnt_pricingtype",
                                            "transactioncurrencyid",
                                            "rnt_paymentmethodcode",
                                            "rnt_totalfineamount");

            query.Criteria = new FilterExpression(LogicalOperator.And);

            if (searchParameters.pickupBranchId.HasValue)
                query.Criteria.AddCondition("rnt_pickupbranchid", ConditionOperator.Equal, searchParameters.pickupBranchId);
            if (searchParameters.dropoffBranchId.HasValue)
                query.Criteria.AddCondition("rnt_dropoffbranchid", ConditionOperator.Equal, searchParameters.dropoffBranchId);

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

            if (!string.IsNullOrEmpty(searchParameters.contractNumber))
            {
                FilterExpression numberFilter = query.Criteria.AddFilter(LogicalOperator.Or);
                numberFilter.AddCondition("rnt_contractnumber", ConditionOperator.Equal, searchParameters.contractNumber);
                numberFilter.AddCondition("rnt_pnrnumber", ConditionOperator.Equal, searchParameters.contractNumber);
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
            if (!string.IsNullOrEmpty(searchParameters.plateNumber))
            {
                var index = query.LinkEntities.Count;
                query.LinkEntities[index] = query.AddLink("rnt_contractitem", "rnt_contractid", "rnt_contractid", JoinOperator.Inner);
                query.LinkEntities[index].LinkCriteria.AddCondition(new ConditionExpression("rnt_itemtypecode", ConditionOperator.Equal, (int)GlobalEnums.ItemTypeCode.Equipment));
                query.LinkEntities[index].LinkEntities[0] = query.LinkEntities[index].AddLink("rnt_equipment", "rnt_equipment", "rnt_equipmentid", JoinOperator.Inner);
                query.LinkEntities[index].LinkEntities[0].LinkCriteria.AddCondition(new ConditionExpression("rnt_platenumber", ConditionOperator.Equal, searchParameters.plateNumber));

            }
            return this.Service.RetrieveMultiple(query).Entities.ToList();
        }
        public List<Entity> getAllActiveContractsByCustomerIdByBetweenGivenDates(Guid customerId, DateTime pickupDate, DateTime dropoffDate)
        {
            QueryExpression query = new QueryExpression("rnt_contract");
            var mainFilter = new FilterExpression(LogicalOperator.And);
            mainFilter.AddCondition(new ConditionExpression("rnt_customerid", ConditionOperator.Equal, customerId));

            var statusFilter = new FilterExpression(LogicalOperator.Or);
            statusFilter.AddCondition(new ConditionExpression("statuscode", ConditionOperator.Equal, (int)ContractEnums.StatusCode.Rental));
            statusFilter.AddCondition(new ConditionExpression("statuscode", ConditionOperator.Equal, (int)ContractEnums.StatusCode.WaitingforDelivery));

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
        public List<Entity> getCompletedContractsWithFaultyInvoice(string branchId, int? dayFilter)
        {
            QueryExpression queryExpression = new QueryExpression("rnt_contract");
            queryExpression.ColumnSet = new ColumnSet("rnt_name", "rnt_dropoffdatetime", "rnt_pnrnumber", "rnt_customerid", "rnt_totalamount", "rnt_billedamount");
            queryExpression.Criteria.AddCondition("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active);
            var days = 0;
            if (dayFilter.HasValue && dayFilter.Value > 0)
            {
                days += dayFilter.Value;
            }
            queryExpression.Criteria.AddCondition("rnt_dropoffdatetime", ConditionOperator.OnOrBefore, DateTime.Now.AddHours(StaticHelper.offset).AddDays(-3));
            queryExpression.Criteria.AddCondition("rnt_dropoffdatetime", ConditionOperator.LastXDays, days);
            queryExpression.Criteria.AddCondition("rnt_pickupbranchid", ConditionOperator.Equal, branchId);
            queryExpression.Criteria.AddCondition("statuscode", ConditionOperator.Equal, (int)rnt_contract_StatusCode.Completed);

            return this.retrieveMultiple(queryExpression).Entities.ToList();
        }
        public List<Entity> getContractsByDateFilterWithGivenStatusAndColumns(string branchId, int? dayFilter, string[] columns, int? contractStatus = null)
        {
            List<Entity> contracts = new List<Entity>();
            QueryExpression queryExpression = new QueryExpression("rnt_contract");
            queryExpression.ColumnSet = new ColumnSet(columns);
            queryExpression.Criteria.AddCondition("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active);

            var days = 0;
            if (dayFilter.HasValue && dayFilter.Value > 0)
            {
                days += dayFilter.Value;
                queryExpression.Criteria.AddCondition("rnt_dropoffdatetime", ConditionOperator.LastXDays, days);
            }
            queryExpression.Criteria.AddCondition("rnt_dropoffdatetime", ConditionOperator.OnOrBefore, DateTime.Now.AddHours(StaticHelper.offset).AddDays(-3));
            queryExpression.Criteria.AddCondition("rnt_pickupbranchid", ConditionOperator.Equal, branchId);
            if (contractStatus != null)
                queryExpression.Criteria.AddCondition("statuscode", ConditionOperator.Equal, (int)rnt_contract_StatusCode.Completed);
            int queryCount = 5000;
            // Initialize the page number.
            int pageNumber = 1;
            string pagingCookie = null;
            while (true)
            {
                queryExpression.PageInfo = new PagingInfo
                {
                    PageNumber = pageNumber,
                    Count = queryCount,
                    PagingCookie = pagingCookie
                };
                var l = this.retrieveMultiple(queryExpression); ;
                if (l.MoreRecords)
                {
                    contracts.AddRange(l.Entities.ToList());
                    pageNumber++;
                    pagingCookie = l.PagingCookie;
                }
                else
                {
                    contracts.AddRange(l.Entities.ToList());
                    break;
                }

            }
            return contracts;
        }
        public List<Entity> getRentalContractsByDate(DateTime date, string[] columns)
        {
            QueryExpression queryExpression = new QueryExpression("rnt_contract");
            queryExpression.ColumnSet = new ColumnSet(columns);
            queryExpression.Criteria.AddCondition("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active);
            queryExpression.Criteria.AddCondition("statuscode", ConditionOperator.Equal, (int)ContractEnums.StatusCode.Rental);
            queryExpression.Criteria.AddCondition("rnt_dropoffdatetime", ConditionOperator.On, date);

            return this.retrieveMultiple(queryExpression).Entities.ToList();
        }
        public List<Entity> getAllRentalContracts(string[] columns)
        {
            QueryExpression queryExpression = new QueryExpression("rnt_contract");
            queryExpression.ColumnSet = new ColumnSet(columns);
            queryExpression.Criteria.AddCondition("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active);
            queryExpression.Criteria.AddCondition("statuscode", ConditionOperator.Equal, (int)ContractEnums.StatusCode.Rental);

            return this.retrieveMultiple(queryExpression).Entities.ToList();
        }
        public List<Entity> getOutgoingContractsByDate(DateTime date, string[] columns)
        {
            QueryExpression queryExpression = new QueryExpression("rnt_contract");
            queryExpression.ColumnSet = new ColumnSet(columns);

            var mainFilter = new FilterExpression(LogicalOperator.And);
            mainFilter.AddCondition("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active);
            mainFilter.AddCondition("rnt_pickupdatetime", ConditionOperator.On, date);

            var statusFilter = new FilterExpression(LogicalOperator.Or);
            statusFilter.AddCondition("statuscode", ConditionOperator.Equal, (int)ContractEnums.StatusCode.Completed);
            statusFilter.AddCondition("statuscode", ConditionOperator.Equal, (int)ContractEnums.StatusCode.WaitingforDelivery);
            statusFilter.AddCondition("statuscode", ConditionOperator.Equal, (int)ContractEnums.StatusCode.Rental);

            mainFilter.AddFilter(statusFilter);
            queryExpression.Criteria = mainFilter;

            return this.retrieveMultiple(queryExpression).Entities.ToList();
        }
        public List<Entity> getAllActiveContractsByBetweenGivenDates(DateTime pickupDate, DateTime dropoffDate, string[] columns)
        {
            var first = new DateTime(dropoffDate.Year, dropoffDate.Month, 1);
            List<Entity> contracts = new List<Entity>();
            // Set the number of records per page to retrieve.
            int queryCount = 5000;
            // Initialize the page number.
            int pageNumber = 1;
            string pagingCookie = null;
            while (true)
            {
                QueryExpression queryExpression = new QueryExpression("rnt_contract");
                queryExpression.ColumnSet = new ColumnSet(columns);
                queryExpression.Criteria.AddCondition("rnt_dropoffdatetime", ConditionOperator.OnOrBefore, dropoffDate.Date.ToString("yyyy-MM-dd"));
                queryExpression.Criteria.AddCondition("rnt_dropoffdatetime", ConditionOperator.OnOrAfter, first.Date.ToString("yyyy-MM-dd"));
                queryExpression.Criteria.AddCondition("statuscode", ConditionOperator.Equal, (int)ContractEnums.StatusCode.Completed);
                queryExpression.Criteria.AddCondition("rnt_ismonthly", ConditionOperator.Equal, false);

                queryExpression.PageInfo = new PagingInfo
                {
                    PageNumber = pageNumber,
                    Count = queryCount,
                    PagingCookie = pagingCookie
                };
                var l = this.retrieveMultiple(queryExpression); ;
                if (l.MoreRecords)
                {
                    contracts.AddRange(l.Entities.ToList());
                    pageNumber++;
                    pagingCookie = l.PagingCookie;
                }
                else
                {
                    contracts.AddRange(l.Entities.ToList());
                    break;
                }

            }
            return contracts;

            //var str = string.Format(@"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
            //          <entity name='rnt_contract'>
            //            <attribute name='rnt_contractid' />
            //            <attribute name='rnt_pickupbranchid' />
            //            <attribute name='rnt_totalduration' />                        
            //            <filter type='and'>
            //              <condition attribute='rnt_dropoffdatetime' operator='on-or-before' value='{1}' />
            //              <condition attribute='statuscode' operator='eq' value='{2}' />                    
            //              <condition attribute='rnt_ismonthly' operator='eq' value='0' />
            //              <condition attribute='rnt_dropoffdatetime' operator='on-or-after' value='{0}' />
            //            </filter>
            //          </entity>
            //        </fetch>", first.Date.ToString("yyyy-MM-dd"), dropoffDate.Date.ToString("yyyy-MM-dd"), (int)ContractEnums.StatusCode.Completed);
            //return this.retrieveMultiple(new FetchExpression(str)).Entities.ToList();
        }

        public List<Entity> getMonthlyContractsDuration(DateTime pickupDate, DateTime dropoffDate, string[] columns)
        {
            //var first = new DateTime(dropoffDate.Year, dropoffDate.Month, 1);

            List<Entity> contracts = new List<Entity>();
            // Set the number of records per page to retrieve.
            int queryCount = 5000;
            // Initialize the page number.
            int pageNumber = 1;
            string pagingCookie = null;
            while (true)
            {
                QueryExpression queryExpression = new QueryExpression("rnt_contractinvoicedate");
                queryExpression.ColumnSet = new ColumnSet(columns);
                queryExpression.Criteria.AddCondition("rnt_dropoffdatetime", ConditionOperator.OnOrBefore, dropoffDate);
                queryExpression.Criteria.AddCondition("rnt_dropoffdatetime", ConditionOperator.OnOrAfter, pickupDate);
                queryExpression.Criteria.AddCondition("statecode", ConditionOperator.Equal, 0);
                queryExpression.LinkEntities.Add(new LinkEntity("rnt_contractinvoicedate", "rnt_contract", "rnt_contractid", "rnt_contractid", JoinOperator.LeftOuter));
                queryExpression.LinkEntities[0].Columns.AddColumns("rnt_pickupbranchid");
                queryExpression.LinkEntities[0].EntityAlias = "contract";

                queryExpression.PageInfo = new PagingInfo
                {
                    PageNumber = pageNumber,
                    Count = queryCount,
                    PagingCookie = pagingCookie
                };
                var l = this.retrieveMultiple(queryExpression); ;
                if (l.MoreRecords)
                {
                    contracts.AddRange(l.Entities.ToList());
                    pageNumber++;
                    pagingCookie = l.PagingCookie;
                }
                else
                {
                    contracts.AddRange(l.Entities.ToList());
                    break;
                }

            }
            return contracts;
        }
    }
}
