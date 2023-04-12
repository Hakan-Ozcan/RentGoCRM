using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Tooling.Connector;
using RntCar.BusinessLibrary.Handlers;
using RntCar.ClassLibrary;
using RntCar.ClassLibrary._Enums_1033;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.BusinessLibrary.Repository
{
    public class InvoiceItemRepository : RepositoryHandler
    {
        public InvoiceItemRepository(IOrganizationService Service) : base(Service)
        {
        }

        public InvoiceItemRepository(CrmServiceClient _crmServiceClient) : base(_crmServiceClient)
        {
        }

        public InvoiceItemRepository(IOrganizationService Service, CrmServiceClient _crmServiceClient) : base(Service, _crmServiceClient)
        {
        }

        public Entity getInvoiceItemById(Guid invoiceItemId)
        {
            return this.retrieveById("rnt_invoiceitem", invoiceItemId);
        }
        public List<Entity> getInvoiceItemsByInvoiceId(Guid invoiceId)
        {
            QueryExpression query = new QueryExpression("rnt_invoiceitem");
            query.ColumnSet = new ColumnSet(true);
            FilterExpression filterExpression = new FilterExpression();
            filterExpression.FilterOperator = LogicalOperator.Or;
            filterExpression.AddCondition("statuscode", ConditionOperator.Equal, (int)RntCar.ClassLibrary._Enums_1033.rnt_invoiceitem_StatusCode.Draft);
            filterExpression.AddCondition("statuscode", ConditionOperator.Equal, (int)RntCar.ClassLibrary._Enums_1033.rnt_invoiceitem_StatusCode.IntegrationError);
            filterExpression.AddCondition("statuscode", ConditionOperator.Equal, (int)RntCar.ClassLibrary._Enums_1033.rnt_invoiceitem_StatusCode.InternalError);
            query.Criteria.AddFilter(filterExpression);
            query.Criteria.AddCondition("rnt_invoiceid", ConditionOperator.Equal, invoiceId);
            return this.retrieveMultiple(query).Entities.ToList();
        }
        public List<Entity> getInvoiceItemsByInvoiceIdAllStatus(Guid invoiceId)
        {
            QueryExpression query = new QueryExpression("rnt_invoiceitem");
            query.ColumnSet = new ColumnSet(true);
            query.Criteria.AddCondition("rnt_invoiceid", ConditionOperator.Equal, invoiceId);
            return this.retrieveMultiple(query).Entities.ToList();
        }
        public List<Entity> getIntegratedInvoiceItemsByContractItemId(Guid contractItemId)
        {
            QueryExpression query = new QueryExpression("rnt_invoiceitem");
            query.ColumnSet = new ColumnSet(true);
            query.Criteria.AddCondition("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active);
            FilterExpression filterExpression = new FilterExpression();
            filterExpression.FilterOperator = LogicalOperator.Or;
            filterExpression.AddCondition("statuscode", ConditionOperator.Equal, (int)rnt_invoiceitem_StatusCode.DealerInvoicing);
            filterExpression.AddCondition("statuscode", ConditionOperator.Equal, (int)rnt_invoiceitem_StatusCode.IntegratedWithLogo);
            query.Criteria.AddFilter(filterExpression);
            query.Criteria.AddCondition("rnt_contractitemid", ConditionOperator.Equal, contractItemId);
            return this.retrieveMultiple(query).Entities.ToList();
        }
        public Entity getDraftInvoiceItemsByContractItemId(Guid contractItemId)
        {
            QueryExpression query = new QueryExpression("rnt_invoiceitem");
            query.ColumnSet = new ColumnSet(true);
            query.Criteria.AddCondition("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active);
            query.Criteria.AddCondition("statuscode", ConditionOperator.Equal, (int)rnt_invoiceitem_StatusCode.Draft);
            query.Criteria.AddCondition("rnt_contractitemid", ConditionOperator.Equal, contractItemId);

            return this.retrieveMultiple(query).Entities.FirstOrDefault();
        }
        public Entity getDraftInvoiceItemsByContractItemIdandInvoiceId(Guid contractItemId, Guid invoiceId)
        {
            QueryExpression query = new QueryExpression("rnt_invoiceitem");
            query.ColumnSet = new ColumnSet(true);
            query.Criteria.AddCondition("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active);
            query.Criteria.AddCondition("statuscode", ConditionOperator.Equal, (int)rnt_invoiceitem_StatusCode.Draft);
            query.Criteria.AddCondition("rnt_contractitemid", ConditionOperator.Equal, contractItemId);
            query.Criteria.AddCondition("rnt_invoiceid", ConditionOperator.Equal, invoiceId);
            return this.retrieveMultiple(query).Entities.FirstOrDefault();
        }
        public Entity getInvoiceItemsByContractItemId(Guid contractItemId)
        {
            QueryExpression query = new QueryExpression("rnt_invoiceitem");
            query.ColumnSet = new ColumnSet(true);
            query.Criteria.AddCondition("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active);
            query.Criteria.AddCondition("rnt_contractitemid", ConditionOperator.Equal, contractItemId);
            return this.retrieveMultiple(query).Entities.FirstOrDefault();
        }
        public Entity getInvoiceItemsByReservationItemId(Guid reservationItemId)
        {
            QueryExpression query = new QueryExpression("rnt_invoiceitem");
            query.ColumnSet = new ColumnSet(true);
            query.Criteria.AddCondition("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active);
            query.Criteria.AddCondition("rnt_reservationitemid", ConditionOperator.Equal, reservationItemId);
            return this.retrieveMultiple(query).Entities.FirstOrDefault();
        }
        public Entity getInvoiceItemsByContractItemIdByGivenColumns(Guid contractItemId, string[] columns)
        {
            QueryExpression query = new QueryExpression("rnt_invoiceitem");
            query.ColumnSet = new ColumnSet(columns);
            query.Criteria.AddCondition("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active);
            query.Criteria.AddCondition("rnt_contractitemid", ConditionOperator.Equal, contractItemId);
            return this.retrieveMultiple(query).Entities.FirstOrDefault();
        }
        public List<Entity> getInvoiceItemsByDateByGivenColumns(DateTime startDate, DateTime endDate, string[] columns)
        {
            List<Entity> entities = new List<Entity>();
            int queryCount = 5000;
            // Initialize the page number.
            int pageNumber = 1;
            string pagingCookie = null;
            while (true)
            {
                QueryExpression query = new QueryExpression("rnt_invoiceitem");
                query.PageInfo = new PagingInfo
                {
                    PageNumber = pageNumber,
                    Count = queryCount,
                    PagingCookie = pagingCookie
                };
                query.ColumnSet = new ColumnSet(columns);
                query.LinkEntities.Add(new LinkEntity("rnt_invoiceitem", "rnt_reservation", "rnt_reservationid", "rnt_reservationid", JoinOperator.LeftOuter));
                query.LinkEntities[0].Columns.AddColumns("rnt_reservationid",
                                                         "rnt_pickupbranchid");

                query.LinkEntities.Add(new LinkEntity("rnt_invoiceitem", "rnt_contract", "rnt_contractid", "rnt_contractid", JoinOperator.LeftOuter));
                query.LinkEntities[1].Columns.AddColumns("rnt_contractid",
                                                        "rnt_pickupbranchid");

                query.LinkEntities[0].EntityAlias = "reservation";
                query.LinkEntities[1].EntityAlias = "contract";

                query.LinkEntities.Add(new LinkEntity("rnt_invoiceitem", "rnt_contractitem", "rnt_contractitemid", "rnt_contractitemid", JoinOperator.LeftOuter));
                query.LinkEntities[2].Columns.AddColumns("rnt_additionalproductid");
                query.LinkEntities[2].EntityAlias = "contractitem";

                query.Criteria = new FilterExpression(LogicalOperator.And);
                FilterExpression pickupDateFilter = query.Criteria.AddFilter(LogicalOperator.And);
                pickupDateFilter.AddCondition("rnt_invoicedate", ConditionOperator.OnOrBefore, endDate.Date);
                pickupDateFilter.AddCondition("rnt_invoicedate", ConditionOperator.OnOrAfter, startDate.Date);

                FilterExpression filterExpression = new FilterExpression();
                filterExpression.FilterOperator = LogicalOperator.Or;
                filterExpression.AddCondition("statuscode", ConditionOperator.Equal, (int)RntCar.ClassLibrary._Enums_1033.rnt_invoiceitem_StatusCode.DealerInvoicing);
                filterExpression.AddCondition("statuscode", ConditionOperator.Equal, (int)RntCar.ClassLibrary._Enums_1033.rnt_invoiceitem_StatusCode.IntegratedWithLogo);
                query.Criteria.AddFilter(filterExpression);

                //query.Criteria.AddCondition("rnt_invoicedate", ConditionOperator.Between, new object[] { startDate, endDate });
                query.Criteria.AddCondition("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active);
                var results = this.retrieveMultiple(query);

                if (results.MoreRecords)
                {
                    entities.AddRange(results.Entities.ToList());
                    pageNumber++;
                    pagingCookie = results.PagingCookie;
                }
                else
                {
                    if (results.Entities.Count > 0)
                    {
                        entities.AddRange(results.Entities.ToList());
                    }
                    break;
                }
            }
            return entities;
        }
        public List<Entity> getInvoiceItemsByDateByGivenColumns(DateTime startDate, DateTime endDate)
        {
            List<Entity> entities = new List<Entity>();
            int queryCount = 5000;
            // Initialize the page number.
            int pageNumber = 1;
            string pagingCookie = null;
            while (true)
            {
                QueryExpression query = new QueryExpression("rnt_invoiceitem");
                query.PageInfo = new PagingInfo
                {
                    PageNumber = pageNumber,
                    Count = queryCount,
                    PagingCookie = pagingCookie
                };
                query.ColumnSet = new ColumnSet(true);
                query.LinkEntities.Add(new LinkEntity("rnt_invoiceitem", "rnt_reservation", "rnt_reservationid", "rnt_reservationid", JoinOperator.LeftOuter));
                query.LinkEntities[0].Columns.AddColumns("rnt_reservationid",
                                                         "rnt_pickupbranchid");

                query.LinkEntities.Add(new LinkEntity("rnt_invoiceitem", "rnt_contract", "rnt_contractid", "rnt_contractid", JoinOperator.LeftOuter));
                query.LinkEntities[1].Columns.AddColumns("rnt_contractid",
                                                        "rnt_pickupbranchid");

                query.LinkEntities[0].EntityAlias = "reservation";
                query.LinkEntities[1].EntityAlias = "contract";

                query.LinkEntities.Add(new LinkEntity("rnt_invoiceitem", "rnt_contractitem", "rnt_contractitemid", "rnt_contractitemid", JoinOperator.LeftOuter));
                query.LinkEntities[2].Columns.AddColumns("rnt_additionalproductid");
                query.LinkEntities[2].EntityAlias = "contractitem";

                query.Criteria = new FilterExpression(LogicalOperator.And);
                FilterExpression pickupDateFilter = query.Criteria.AddFilter(LogicalOperator.And);
                pickupDateFilter.AddCondition("rnt_invoicedate", ConditionOperator.OnOrBefore, endDate.Date);
                pickupDateFilter.AddCondition("rnt_invoicedate", ConditionOperator.OnOrAfter, startDate.Date);

                FilterExpression filterExpression = new FilterExpression();
                filterExpression.FilterOperator = LogicalOperator.Or;
                filterExpression.AddCondition("statuscode", ConditionOperator.Equal, (int)RntCar.ClassLibrary._Enums_1033.rnt_invoiceitem_StatusCode.DealerInvoicing);
                filterExpression.AddCondition("statuscode", ConditionOperator.Equal, (int)RntCar.ClassLibrary._Enums_1033.rnt_invoiceitem_StatusCode.IntegratedWithLogo);
                query.Criteria.AddFilter(filterExpression);

                //query.Criteria.AddCondition("rnt_invoicedate", ConditionOperator.Between, new object[] { startDate, endDate });
                query.Criteria.AddCondition("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active);
                var results = this.retrieveMultiple(query);

                if (results.MoreRecords)
                {
                    entities.AddRange(results.Entities.ToList());
                    pageNumber++;
                    pagingCookie = results.PagingCookie;
                }
                else
                {
                    if (results.Entities.Count > 0)
                    {
                        entities.AddRange(results.Entities.ToList());
                    }
                    break;
                }
            }
            return entities;
        }

        public List<Entity> getInvoiceItemsByGivenDatesByPickupBranch(DateTime startDate, DateTime endDate, List<object> pickupBranchId)
        {
            List<Entity> entities = new List<Entity>();
            int queryCount = 5000;
            // Initialize the page number.
            int pageNumber = 1;
            string pagingCookie = null;
            while (true)
            {
                QueryExpression query = new QueryExpression("rnt_invoiceitem");
                query.PageInfo = new PagingInfo
                {
                    PageNumber = pageNumber,
                    Count = queryCount,
                    PagingCookie = pagingCookie
                };
                query.ColumnSet = new ColumnSet(true);


                query.LinkEntities.Add(new LinkEntity("rnt_invoiceitem", "rnt_contract", "rnt_contractid", "rnt_contractid", JoinOperator.Inner));
                query.LinkEntities[0].Columns.AddColumns("rnt_contractid",
                                                         "rnt_contractnumber",
                                                         "rnt_pickupbranchid");

                query.LinkEntities[0].EntityAlias = "contract";

                query.LinkEntities.Add(new LinkEntity("rnt_invoiceitem", "rnt_contractitem", "rnt_contractitemid", "rnt_contractitemid", JoinOperator.Inner));
                query.LinkEntities[1].Columns.AddColumns("rnt_additionalproductid", "createdby","rnt_itemtypecode", "rnt_reservationitemid", "rnt_externalusercreatedbyid");
                query.LinkEntities[1].EntityAlias = "contractitem";
                query.LinkEntities[1].LinkCriteria.Conditions.Add(new ConditionExpression("rnt_pickupbranchid", ConditionOperator.In, pickupBranchId));
                query.LinkEntities[1].LinkCriteria.Conditions.Add(new ConditionExpression("rnt_itemtypecode", ConditionOperator.NotIn, new object[] { 1, 5 }));
                query.Criteria = new FilterExpression(LogicalOperator.And);
                FilterExpression pickupDateFilter = query.Criteria.AddFilter(LogicalOperator.And);
                pickupDateFilter.AddCondition("rnt_invoicedate", ConditionOperator.OnOrBefore, endDate.Date);
                pickupDateFilter.AddCondition("rnt_invoicedate", ConditionOperator.OnOrAfter, startDate.Date);
                query.Criteria.AddFilter(pickupDateFilter);

                FilterExpression filterExpression = new FilterExpression();
                filterExpression.FilterOperator = LogicalOperator.Or;
                filterExpression.AddCondition("statuscode", ConditionOperator.Equal, (int)RntCar.ClassLibrary._Enums_1033.rnt_invoiceitem_StatusCode.DealerInvoicing);
                filterExpression.AddCondition("statuscode", ConditionOperator.Equal, (int)RntCar.ClassLibrary._Enums_1033.rnt_invoiceitem_StatusCode.IntegratedWithLogo);
                query.Criteria.AddFilter(filterExpression);

                //query.Criteria.AddCondition("rnt_invoicedate", ConditionOperator.Between, new object[] { startDate, endDate });
                query.Criteria.AddCondition("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active);
                var results = this.retrieveMultiple(query);

                if (results.MoreRecords)
                {
                    entities.AddRange(results.Entities.ToList());
                    pageNumber++;
                    pagingCookie = results.PagingCookie;
                }
                else
                {
                    if (results.Entities.Count > 0)
                    {
                        entities.AddRange(results.Entities.ToList());
                    }
                    break;
                }
            }
            return entities;
        }
    }
}
