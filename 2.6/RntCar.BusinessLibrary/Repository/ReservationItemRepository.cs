using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using RntCar.BusinessLibrary.Business;
using RntCar.BusinessLibrary.Handlers;
using RntCar.ClassLibrary;
using RntCar.ClassLibrary._Enums_1033;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RntCar.BusinessLibrary.Repository
{
    public class ReservationItemRepository : RepositoryHandler
    {
        public ReservationItemRepository(IOrganizationService Service) : base(Service)
        {
        }

        public ReservationItemRepository(IOrganizationService Service, Guid UserId) : base(Service, UserId)
        {
        }

        public ReservationItemRepository(IOrganizationService Service, Guid UserId, string OrganizationName) : base(Service, UserId, OrganizationName)
        {
        }
        public Entity getReservationItemById(Guid Id)
        {
            try
            {
                return this.Service.Retrieve("rnt_reservationitem", Id, new ColumnSet(true));
            }
            catch
            {
                return null;
            }
        }

        public Entity getReservationItemById(Guid Id, string[] columns)
        {
            try
            {
                return this.Service.Retrieve("rnt_reservationitem", Id, new ColumnSet(columns));
            }
            catch
            {
                return null;
            }
        }
        public Entity getReservationItemByIdByGivenColumns(Guid Id, string[] columns)
        {
            try
            {
                return this.Service.Retrieve("rnt_reservationitem", Id, new ColumnSet(columns));
            }
            catch
            {
                return null;
            }
        }
        public Entity getReservationEquipmentItemByTrackingNumber(string trackingNumber)
        {

            QueryExpression queryExpression = new QueryExpression("rnt_reservationitem");
            queryExpression.ColumnSet = new ColumnSet(true);
            queryExpression.Criteria.AddCondition("rnt_mongodbtrackingnumber", ConditionOperator.Equal, trackingNumber);

            return this.retrieveMultiple(queryExpression).Entities.FirstOrDefault();
        }
        public Entity getDiscountReservationItem(Guid reservationId, string[] columns)
        {
            ConfigurationBL configurationBL = new ConfigurationBL(this.Service);
            var manualDiscountProduct = configurationBL.GetConfigurationByName("additionalProducts_manuelDiscount");

            AdditionalProductRepository additionalProductRepository = new AdditionalProductRepository(this.Service);
            var additionalProduct = additionalProductRepository.getAdditionalProductByProductCode(manualDiscountProduct);

            QueryExpression queryExpression = new QueryExpression("rnt_reservationitem");
            queryExpression.ColumnSet = new ColumnSet(columns);
            queryExpression.Criteria.AddCondition("rnt_additionalproductid", ConditionOperator.Equal, additionalProduct.Id);
            queryExpression.Criteria.AddCondition("rnt_reservationid", ConditionOperator.Equal, reservationId);

            return this.retrieveMultiple(queryExpression).Entities.FirstOrDefault();
        }
        public Entity getReservationEquipmentCorporate(Guid reservationId, string[] columns)
        {
            QueryExpression queryExpression = new QueryExpression("rnt_reservationitem");
            queryExpression.ColumnSet = new ColumnSet(columns);
            queryExpression.Criteria.AddCondition("rnt_reservationid", ConditionOperator.Equal, reservationId);
            queryExpression.Criteria.AddCondition("rnt_billingtypecode", ConditionOperator.Equal, (int)rnt_BillingTypeCode.Corporate);
            queryExpression.Criteria.AddCondition("rnt_itemtypecode", ConditionOperator.Equal, (int)rnt_reservationitem_rnt_itemtypecode.Equipment);

            return this.retrieveMultiple(queryExpression).Entities.FirstOrDefault();
        }
        public List<Entity> getNewNoShowReservationItemsByReservationId(Guid reservationId)
        {
            QueryExpression expression = new QueryExpression("rnt_reservationitem");
            expression.ColumnSet = new ColumnSet(true);
            //always new reservation item --> statuscode == 1 is new 
            FilterExpression filterStatusCode = new FilterExpression(LogicalOperator.Or);
            filterStatusCode.AddCondition("statuscode", ConditionOperator.Equal, (int)ReservationItemEnums.StatusCode.New);
            filterStatusCode.AddCondition("statuscode", ConditionOperator.Equal, (int)ReservationItemEnums.StatusCode.Noshow);
            expression.Criteria.AddFilter(filterStatusCode);
            expression.Criteria.AddCondition("rnt_reservationid", ConditionOperator.Equal, reservationId);
            expression.AddOrder("createdon", OrderType.Ascending);
            return this.Service.RetrieveMultiple(expression).Entities.ToList();
        }

        public List<Entity> getActiveReservationItemsByReservationId(Guid reservationId)
        {
            QueryExpression expression = new QueryExpression("rnt_reservationitem");
            expression.ColumnSet = new ColumnSet(true);
            //always new reservation item --> statuscode == 1 is 
            expression.Criteria.AddCondition("statuscode", ConditionOperator.Equal, (int)ReservationItemEnums.StatusCode.New);

            expression.Criteria.AddCondition("rnt_reservationid", ConditionOperator.Equal, reservationId);
            expression.AddOrder("createdon", OrderType.Ascending);
            return this.Service.RetrieveMultiple(expression).Entities.ToList();
        }

        public List<Entity> getActiveReservationItemsByReservationIdWithAdditionalProduct(Guid reservationId)
        {
            QueryExpression reservationItemQuery = new QueryExpression("rnt_reservationitem");
            reservationItemQuery.ColumnSet = new ColumnSet("rnt_reservationid", "rnt_itemno", "rnt_itemtypecode", "rnt_name", "rnt_baseprice", "rnt_netamount", "rnt_totalamount", "rnt_additionalproductid");
            reservationItemQuery.LinkEntities.Add(new LinkEntity("rnt_reservationitem", "rnt_additionalproduct", "rnt_additionalproductid", "rnt_additionalproductid", JoinOperator.LeftOuter));
            reservationItemQuery.LinkEntities[0].Columns.AddColumns("rnt_additionalproductid",
                                                                    "rnt_name",
                                                                    "rnt_additionalproducttype",
                                                                    "rnt_additionalproductcode",
                                                                    "rnt_maximumpieces",
                                                                    "rnt_showonweb",
                                                                    "rnt_showonwebsite",
                                                                    "rnt_webrank",
                                                                    "rnt_productdescription",
                                                                    "rnt_price",
                                                                    "rnt_pricecalculationtypecode",
                                                                    "rnt_monthlypackageprice");
            reservationItemQuery.LinkEntities[0].EntityAlias = "additionalProducts";
            reservationItemQuery.Criteria = new FilterExpression(LogicalOperator.And);
            reservationItemQuery.Criteria.AddCondition("rnt_reservationid", ConditionOperator.Equal, reservationId);
            reservationItemQuery.Criteria.AddCondition("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active);

            return this.Service.RetrieveMultiple(reservationItemQuery).Entities.ToList();
        }
        public List<Entity> getActiveReservationItemsByReservationIdByGivenColumns(Guid reservationId, string[] columns)
        {
            QueryExpression expression = new QueryExpression("rnt_reservationitem");
            expression.ColumnSet = new ColumnSet(columns);
            //always new reservation item --> statuscode == 1 is new 
            expression.Criteria.AddCondition("statuscode", ConditionOperator.Equal, (int)ReservationItemEnums.StatusCode.New);
            expression.Criteria.AddCondition("rnt_reservationid", ConditionOperator.Equal, reservationId);
            expression.AddOrder("createdon", OrderType.Ascending);
            return this.Service.RetrieveMultiple(expression).Entities.ToList();
        }

        public Entity getNewNoShowReservationItemEquipmentByGivenColumns(Guid reservationId, string[] columns)
        {
            QueryExpression expression = new QueryExpression("rnt_reservationitem");
            expression.ColumnSet = new ColumnSet(columns);
            //always new reservation item --> statuscode == 1 is new 
            FilterExpression filterStatusCode = new FilterExpression(LogicalOperator.Or);
            filterStatusCode.AddCondition("statuscode", ConditionOperator.Equal, (int)ReservationItemEnums.StatusCode.New);
            filterStatusCode.AddCondition("statuscode", ConditionOperator.Equal, (int)ReservationItemEnums.StatusCode.Noshow);
            expression.Criteria.AddFilter(filterStatusCode);
            expression.Criteria.AddCondition("rnt_reservationid", ConditionOperator.Equal, reservationId);
            expression.Criteria.AddCondition("rnt_itemtypecode", ConditionOperator.Equal, (int)ClassLibrary._Enums_1033.rnt_reservationitem_rnt_itemtypecode.Equipment);

            return this.Service.RetrieveMultiple(expression).Entities.FirstOrDefault();
        }

        public Entity getActiveReservationItemEquipmentByGivenColumns(Guid reservationId, string[] columns)
        {
            QueryExpression expression = new QueryExpression("rnt_reservationitem");
            expression.ColumnSet = new ColumnSet(columns);
            //always new reservation item --> statuscode == 1 is new 
            FilterExpression filterStatusCode = new FilterExpression(LogicalOperator.Or);
            filterStatusCode.AddCondition("statuscode", ConditionOperator.Equal, (int)ReservationItemEnums.StatusCode.New);
            filterStatusCode.AddCondition("statuscode", ConditionOperator.Equal, (int)ReservationItemEnums.StatusCode.Noshow);
            expression.Criteria.AddFilter(filterStatusCode);
            expression.Criteria.AddCondition("rnt_reservationid", ConditionOperator.Equal, reservationId);
            expression.Criteria.AddCondition("rnt_itemtypecode", ConditionOperator.Equal, (int)ClassLibrary._Enums_1033.rnt_reservationitem_rnt_itemtypecode.Equipment);

            return this.Service.RetrieveMultiple(expression).Entities.FirstOrDefault();
        }
        public Entity getActiveReservationItemEquipment(Guid reservationId)
        {
            QueryExpression expression = new QueryExpression("rnt_reservationitem");
            expression.ColumnSet = new ColumnSet(true);
            expression.Criteria.AddCondition("rnt_reservationid", ConditionOperator.Equal, reservationId);
            expression.Criteria.AddCondition("rnt_itemtypecode", ConditionOperator.Equal, (int)ClassLibrary._Enums_1033.rnt_reservationitem_rnt_itemtypecode.Equipment);

            return this.Service.RetrieveMultiple(expression).Entities.FirstOrDefault();
        }
        public Entity getFineReservationItemByAdditionalProductIdWithGivenColumns(Guid reservationId, Guid additionalProductId, string[] columns)
        {
            QueryExpression expression = new QueryExpression("rnt_reservationitem");
            expression.ColumnSet = new ColumnSet(columns);
            expression.Criteria.AddCondition("rnt_reservationid", ConditionOperator.Equal, reservationId);
            expression.Criteria.AddCondition("rnt_itemtypecode", ConditionOperator.Equal, (int)ClassLibrary._Enums_1033.rnt_reservationitem_rnt_itemtypecode.Fine);
            expression.Criteria.AddCondition("rnt_additionalproductid", ConditionOperator.Equal, additionalProductId);

            return this.Service.RetrieveMultiple(expression).Entities.FirstOrDefault();
        }
        public Entity getReservationItemByAdditionalProductIdWithGivenColumns(Guid reservationId, Guid additionalProductId, string[] columns)
        {
            QueryExpression expression = new QueryExpression("rnt_reservationitem");
            expression.ColumnSet = new ColumnSet(columns);
            expression.Criteria.AddCondition("rnt_reservationid", ConditionOperator.Equal, reservationId);
            expression.Criteria.AddCondition("rnt_additionalproductid", ConditionOperator.Equal, additionalProductId);

            return this.Service.RetrieveMultiple(expression).Entities.FirstOrDefault();
        }
        public List<Entity> getCompletedReservationItemsWithGivenColumns(Guid reservationId, string[] columns)
        {
            QueryExpression expression = new QueryExpression("rnt_reservationitem");
            expression.ColumnSet = new ColumnSet(columns);
            expression.Criteria.AddCondition("rnt_reservationid", ConditionOperator.Equal, reservationId);
            expression.Criteria.AddCondition("statuscode", ConditionOperator.Equal, (int)ReservationItemEnums.StatusCode.Completed);

            return this.Service.RetrieveMultiple(expression).Entities.ToList();
        }
        public Entity getCompletedEquipmentReservationItemsWithGivenColumns(Guid reservationId, string[] columns)
        {
            QueryExpression expression = new QueryExpression("rnt_reservationitem");
            expression.ColumnSet = new ColumnSet(columns);
            expression.Criteria.AddCondition("rnt_reservationid", ConditionOperator.Equal, reservationId);
            expression.Criteria.AddCondition("rnt_itemtypecode", ConditionOperator.Equal, rnt_reservationitem_rnt_itemtypecode.Equipment);
            expression.Criteria.AddCondition("statuscode", ConditionOperator.Equal, (int)rnt_reservationitem_StatusCode.Completed);

            return this.retrieveMultiple(expression).Entities.FirstOrDefault();
        }
        public List<Entity> getCompletedEquipmentandPriceDifferenceReservationItemsWithGivenColumns(Guid reservationId, string[] columns)
        {
            QueryExpression expression = new QueryExpression("rnt_reservationitem");
            expression.ColumnSet = new ColumnSet(columns);
            expression.Criteria.AddCondition("rnt_reservationid", ConditionOperator.Equal, reservationId);
            FilterExpression filterTypeCode = new FilterExpression(LogicalOperator.Or);
            filterTypeCode.AddCondition("rnt_itemtypecode", ConditionOperator.Equal, (int)rnt_reservationitem_rnt_itemtypecode.PriceDifference);
            filterTypeCode.AddCondition("rnt_itemtypecode", ConditionOperator.Equal, (int)rnt_reservationitem_rnt_itemtypecode.Equipment);
            expression.Criteria.AddFilter(filterTypeCode);
            expression.Criteria.AddCondition("statuscode", ConditionOperator.Equal, (int)rnt_reservationitem_StatusCode.Completed);

            return this.retrieveMultiple(expression).Entities.ToList();
        }
        public List<Entity> getCompletedReservationItems(string[] columns)
        {
            List<Entity> result = new List<Entity>();
            int queryCount = 5000;
            int pageNumber = 1;
            string pagingCookie = null;

            while (true)
            {


                QueryExpression query = new QueryExpression("rnt_reservationitem");
                query.PageInfo = new PagingInfo
                {
                    PageNumber = pageNumber,
                    Count = queryCount,
                    PagingCookie = pagingCookie
                };
                query.ColumnSet = new ColumnSet(columns);
                query.Criteria.AddCondition("statuscode", ConditionOperator.Equal, (int)ReservationItemEnums.StatusCode.Completed);

                var reservationItems = this.retrieveMultiple(query);
                result.AddRange(reservationItems.Entities.ToList());
                if (reservationItems.MoreRecords)
                {
                    pageNumber++;
                    pagingCookie = reservationItems.PagingCookie;
                }
                else
                {
                    break;
                }
            }

            return result;
        }
        public List<Entity> getCompletedReservationItemsByBranchId(List<object> pickupBranchId, string[] columns, DateTime startDate, DateTime endDate)
        {
            List<Entity> result = new List<Entity>();
            int queryCount = 5000;
            int pageNumber = 1;
            string pagingCookie = null;

            while (true)
            {
                LinkEntity invoiceItemLink = new LinkEntity();
                invoiceItemLink.EntityAlias = "invoiceItemAlias";
                invoiceItemLink.LinkFromAttributeName = "rnt_contractitemid";
                invoiceItemLink.LinkFromEntityName = "rnt_contractitem";
                invoiceItemLink.LinkToAttributeName = "rnt_contractitemid";
                invoiceItemLink.LinkToEntityName = "rnt_invoiceitem";
                invoiceItemLink.LinkCriteria.AddCondition("rnt_invoicedate", ConditionOperator.OnOrBefore, endDate.Date);
                invoiceItemLink.LinkCriteria.AddCondition("rnt_invoicedate", ConditionOperator.OnOrAfter, startDate.Date);

                LinkEntity contractItemLink = new LinkEntity();
                contractItemLink.EntityAlias = "contractItemAlias";
                contractItemLink.LinkFromAttributeName = "rnt_reservationitemid";
                contractItemLink.LinkFromEntityName = "rnt_reservationitem";
                contractItemLink.LinkToAttributeName = "rnt_reservationitemid";
                contractItemLink.LinkToEntityName = "rnt_contractitem";
                contractItemLink.LinkEntities.Add(invoiceItemLink);

                QueryExpression query = new QueryExpression("rnt_reservationitem");
                query.PageInfo = new PagingInfo
                {
                    PageNumber = pageNumber,
                    Count = queryCount,
                    PagingCookie = pagingCookie
                };
                query.ColumnSet = new ColumnSet(columns);
                query.Criteria.AddCondition("statuscode", ConditionOperator.Equal, (int)ReservationItemEnums.StatusCode.Completed);
                if (pickupBranchId.Count <= 5)
                {
                    query.Criteria.AddCondition(new ConditionExpression("rnt_pickupbranchid", ConditionOperator.In, pickupBranchId));
                }
                //query.LinkEntities.Add(new LinkEntity("rnt_reservationitem","rnt_reservation", "rnt_reservationid", "rnt_reservationid", JoinOperator.Inner));
                //query.LinkEntities[0].LinkCriteria.AddCondition(new ConditionExpression("rnt_pickupbranchid", ConditionOperator.In, pickupBranchId));
                query.LinkEntities.Add(contractItemLink);

                var reservationItems = this.retrieveMultiple(query);
                result.AddRange(reservationItems.Entities.ToList());
                if (reservationItems.MoreRecords)
                {
                    pageNumber++;
                    pagingCookie = reservationItems.PagingCookie;
                }
                else
                {
                    break;
                }
            }

            return result;
        }
        public List<Entity> getReservationItemsByXHours(string[] columns, int hours)
        {
            List<Entity> result = new List<Entity>();
            int queryCount = 5000;
            int pageNumber = 1;
            string pagingCookie = null;

            while (true)
            {
                QueryExpression query = new QueryExpression("rnt_reservationitem");
                query.PageInfo = new PagingInfo
                {
                    PageNumber = pageNumber,
                    Count = queryCount,
                    PagingCookie = pagingCookie
                };
                query.ColumnSet = new ColumnSet(columns);
                query.Criteria.AddCondition("modifiedon", ConditionOperator.LastXHours, hours);

                var reservationItems = this.retrieveMultiple(query);
                result.AddRange(reservationItems.Entities.ToList());
                if (reservationItems.MoreRecords)
                {
                    pageNumber++;
                    pagingCookie = reservationItems.PagingCookie;
                }
                else
                {
                    break;
                }
            }

            return result;
        }

        public EntityCollection getActiveReservationItemBetweenGivenDates(DateTime startDate, DateTime endDate, string[] columns)
        {

            EntityCollection reservationItems = new EntityCollection();
            int queryCount = 5000;
            // Initialize the page number.
            int pageNumber = 1;
            string pagingCookie = null;
            while (true)
            {
                LinkEntity reservationLinkEntity = new LinkEntity();
                reservationLinkEntity.LinkFromEntityName = "rnt_reservationitem";
                reservationLinkEntity.LinkFromAttributeName = "rnt_reservationid";
                reservationLinkEntity.LinkToEntityName = "rnt_reservation";
                reservationLinkEntity.LinkToAttributeName = "rnt_reservationid";
                reservationLinkEntity.LinkCriteria.AddCondition("statuscode", ConditionOperator.In, 1, 100000001, 100000002);

                QueryExpression getReservationItemQuery = new QueryExpression("rnt_reservationitem");
                getReservationItemQuery.ColumnSet = new ColumnSet(columns);
                getReservationItemQuery.Criteria.AddCondition("modifiedon", ConditionOperator.GreaterThan, startDate);
                getReservationItemQuery.Criteria.AddCondition("modifiedon", ConditionOperator.LessThan, endDate);
                getReservationItemQuery.Criteria.AddCondition("rnt_itemtypecode", ConditionOperator.In, 1, 5);
                getReservationItemQuery.Criteria.AddCondition("statecode", ConditionOperator.Equal, 0);
                getReservationItemQuery.LinkEntities.Add(reservationLinkEntity);


                getReservationItemQuery.PageInfo = new PagingInfo
                {
                    PageNumber = pageNumber,
                    Count = queryCount,
                    PagingCookie = pagingCookie
                };
                var l = this.Service.RetrieveMultiple(getReservationItemQuery);
                if (l.MoreRecords)
                {
                    reservationItems.Entities.AddRange(l.Entities.ToList());
                    pageNumber++;
                    pagingCookie = l.PagingCookie;
                }
                else
                {
                    reservationItems.Entities.AddRange(l.Entities.ToList());
                    break;
                }

            }
            return reservationItems;
        }

        public EntityCollection getDeactiveReservationItemBetweenGivenDates(DateTime startDate, DateTime endDate, string[] columns)
        {

            EntityCollection reservationItems = new EntityCollection();
            int queryCount = 5000;
            // Initialize the page number.
            int pageNumber = 1;
            string pagingCookie = null;
            while (true)
            {
                LinkEntity reservationLinkEntity = new LinkEntity();
                reservationLinkEntity.LinkFromEntityName = "rnt_reservationitem";
                reservationLinkEntity.LinkFromAttributeName = "rnt_reservationid";
                reservationLinkEntity.LinkToEntityName = "rnt_reservation";
                reservationLinkEntity.LinkToAttributeName = "rnt_reservationid";
                reservationLinkEntity.LinkCriteria.AddCondition("statuscode", ConditionOperator.In, 1, 100000001, 100000002);
                reservationLinkEntity.LinkCriteria.AddCondition("rnt_dropoffdatetime", ConditionOperator.GreaterThan, startDate);
                reservationLinkEntity.LinkCriteria.AddCondition("rnt_dropoffdatetime", ConditionOperator.LessThan, endDate);

                QueryExpression getReservationItemQuery = new QueryExpression("rnt_reservationitem");
                getReservationItemQuery.ColumnSet = new ColumnSet(columns);
                getReservationItemQuery.Criteria.AddCondition("rnt_itemtypecode", ConditionOperator.In, 1, 5);
                getReservationItemQuery.LinkEntities.Add(reservationLinkEntity);


                getReservationItemQuery.PageInfo = new PagingInfo
                {
                    PageNumber = pageNumber,
                    Count = queryCount,
                    PagingCookie = pagingCookie
                };
                var l = this.Service.RetrieveMultiple(getReservationItemQuery);
                if (l.MoreRecords)
                {
                    reservationItems.Entities.AddRange(l.Entities.ToList());
                    pageNumber++;
                    pagingCookie = l.PagingCookie;
                }
                else
                {
                    reservationItems.Entities.AddRange(l.Entities.ToList());
                    break;
                }

            }
            return reservationItems;
        }

        public EntityCollection getCompletedReservationItemBetweenGivenDates(DateTime startDate, DateTime endDate, string[] columns)
        {

            EntityCollection reservationItems = new EntityCollection();
            int queryCount = 5000;
            // Initialize the page number.
            int pageNumber = 1;
            string pagingCookie = null;
            while (true)
            {

                LinkEntity contractLinkEntity = new LinkEntity();
                contractLinkEntity.LinkFromEntityName = "rnt_reservation";
                contractLinkEntity.LinkFromAttributeName = "rnt_contractnumber";
                contractLinkEntity.LinkToEntityName = "rnt_contract";
                contractLinkEntity.LinkToAttributeName = "rnt_contractid";
                contractLinkEntity.LinkCriteria.AddCondition("statuscode", ConditionOperator.In, 100000001);
                contractLinkEntity.LinkCriteria.AddCondition("rnt_dropoffdatetime", ConditionOperator.GreaterThan, startDate);
                contractLinkEntity.LinkCriteria.AddCondition("rnt_dropoffdatetime", ConditionOperator.LessThan, endDate);

                LinkEntity reservationLinkEntity = new LinkEntity();
                reservationLinkEntity.LinkFromEntityName = "rnt_reservationitem";
                reservationLinkEntity.LinkFromAttributeName = "rnt_reservationid";
                reservationLinkEntity.LinkToEntityName = "rnt_reservation";
                reservationLinkEntity.LinkToAttributeName = "rnt_reservationid";
                reservationLinkEntity.LinkEntities.Add(contractLinkEntity);

                QueryExpression getReservationItemQuery = new QueryExpression("rnt_reservationitem");
                getReservationItemQuery.ColumnSet = new ColumnSet(columns);
                getReservationItemQuery.Criteria.AddCondition("rnt_itemtypecode", ConditionOperator.In, 1, 5);
                getReservationItemQuery.LinkEntities.Add(reservationLinkEntity);


                getReservationItemQuery.PageInfo = new PagingInfo
                {
                    PageNumber = pageNumber,
                    Count = queryCount,
                    PagingCookie = pagingCookie
                };
                var l = this.Service.RetrieveMultiple(getReservationItemQuery);
                if (l.MoreRecords)
                {
                    reservationItems.Entities.AddRange(l.Entities.ToList());
                    pageNumber++;
                    pagingCookie = l.PagingCookie;
                }
                else
                {
                    reservationItems.Entities.AddRange(l.Entities.ToList());
                    break;
                }

            }
            return reservationItems;
        }
    }
}
