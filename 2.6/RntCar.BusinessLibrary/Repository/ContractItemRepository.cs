using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Tooling.Connector;
using RntCar.BusinessLibrary.Business;
using RntCar.BusinessLibrary.Handlers;
using RntCar.BusinessLibrary.Repository;
using RntCar.ClassLibrary;
using RntCar.ClassLibrary._Enums_1033;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.BusinessLibrary
{
    public class ContractItemRepository : RepositoryHandler
    {
        public ContractItemRepository(IOrganizationService Service) : base(Service)
        {
        }

        public ContractItemRepository(IOrganizationService Service, Guid UserId) : base(Service, UserId)
        {
        }

        public ContractItemRepository(IOrganizationService Service, Guid UserId, string OrganizationName) : base(Service, UserId, OrganizationName)
        {
        }

        public decimal getSumEquipmentContractItems(Guid contractItemId, Guid contractId)
        {
            //get the payment not totally refunded and int success status
            var fetchXml = string.Format(@"<fetch distinct='false' mapping='logical' aggregate='true'>   
                                            <entity name='rnt_contractitem'>
                                             <attribute name='rnt_totalamount' aggregate='sum' alias='sumofamounts'/>   
                                                 <filter type='and'>                                                 
                                                      <condition attribute='rnt_contractitemid' operator='ne' uitype='rnt_contractitem' value='{0}' />
                                                      <condition attribute='rnt_contractid' operator='eq' uitype='rnt_contract' value='{1}' />
                                                      <condition attribute='rnt_itemtypecode' operator='eq' value='1' />
                                                      <condition attribute='statuscode' operator='not-in'>
                                                        <value>100000008</value>
                                                        <value>100000009</value>
                                                        <value>2</value>
                                                        <value>100000000</value>
                                                      </condition>
                                                  </filter>                                          
                                               </entity>   
                                            </fetch>", contractItemId, contractId);

            var record = this.retrieveMultiple(new FetchExpression(fetchXml)).Entities.FirstOrDefault();
            if (record == null)
                return decimal.Zero;

            var aliasedValue = record.GetAttributeValue<AliasedValue>("sumofamounts").Value;
            if (aliasedValue == null)
            {
                return decimal.Zero;
            }
            return ((Money)aliasedValue).Value;
        }
        public decimal getSumCompletedEquipmentContractItems(Guid contractId)
        {
            //get the payment not totally refunded and int success status
            var fetchXml = string.Format(@"<fetch distinct='false' mapping='logical' aggregate='true'>   
                                            <entity name='rnt_contractitem'>
                                             <attribute name='rnt_totalamount' aggregate='sum' alias='sumofamounts'/>   
                                                 <filter type='and'>                                                
                                                      <condition attribute='rnt_contractid' operator='eq' uitype='rnt_contract' value='{0}' />
                                                      <condition attribute='rnt_itemtypecode' operator='eq' value='1' />
                                                      <condition attribute='statuscode' operator='eq' value='100000003' />                                                        
                                                  </filter>                                          
                                               </entity>   
                                            </fetch>", contractId);

            var record = this.retrieveMultiple(new FetchExpression(fetchXml)).Entities.FirstOrDefault();
            if (record == null)
                return decimal.Zero;

            var aliasedValue = record.GetAttributeValue<AliasedValue>("sumofamounts").Value;
            if (aliasedValue == null)
            {
                return decimal.Zero;
            }
            return ((Money)aliasedValue).Value;
        }
        public ContractItemRepository(IOrganizationService Service, CrmServiceClient _crmServiceClient) : base(Service, _crmServiceClient)
        {
        }

        public ContractItemRepository(CrmServiceClient _crmServiceClient) : base(_crmServiceClient)
        {
        }
        public List<Entity> getActiveContractItems(Guid contractId)
        {
            QueryExpression queryExpression = new QueryExpression("rnt_contractitem");
            queryExpression.ColumnSet = new ColumnSet(true);
            queryExpression.Criteria.AddCondition("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active);
            queryExpression.Criteria.AddCondition("rnt_contractid", ConditionOperator.Equal, contractId);
            return this.retrieveMultiple(queryExpression).Entities.ToList();
        }
        public List<Entity> getContractItemsByContractId(string contractId, string[] columns)
        {
            QueryExpression queryExpression = new QueryExpression("rnt_contractitem");
            queryExpression.ColumnSet = new ColumnSet(columns);
            queryExpression.Criteria.AddCondition("rnt_contractid", ConditionOperator.Equal, contractId);
            return this.retrieveMultiple(queryExpression).Entities.ToList();
        }

        public List<Entity> getCompletedContractEquipmentByContractId(string contractId, string[] columns)
        {
            QueryExpression queryExpression = new QueryExpression("rnt_contractitem");
            queryExpression.ColumnSet = new ColumnSet(columns);
            queryExpression.Criteria.AddCondition("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active);
            queryExpression.Criteria.AddCondition("statuscode", ConditionOperator.Equal, (int)RntCar.ClassLibrary._Enums_1033.rnt_contractitem_StatusCode.Completed);
            queryExpression.Criteria.AddCondition("rnt_contractid", ConditionOperator.Equal, contractId);
            return this.retrieveMultiple(queryExpression).Entities.ToList();
        }

        public Entity getPriceFactorDifference(Guid contractId, string[] columns)
        {
            ConfigurationBL configurationBL = new ConfigurationBL(this.Service);
            var differenceAdditionalCode = configurationBL.GetConfigurationByName("additionalProduct_priceDifference");

            AdditionalProductRepository additionalProductRepository = new AdditionalProductRepository(this.Service);
            var additionalProduct = additionalProductRepository.getAdditionalProductByProductCode(differenceAdditionalCode);

            QueryExpression queryExpression = new QueryExpression("rnt_contractitem");
            queryExpression.ColumnSet = new ColumnSet(columns);
            queryExpression.Criteria.AddCondition("rnt_additionalproductid", ConditionOperator.Equal, additionalProduct.Id);
            queryExpression.Criteria.AddCondition("rnt_contractid", ConditionOperator.Equal, contractId);

            return this.retrieveMultiple(queryExpression).Entities.FirstOrDefault();
        }
        public List<Entity> getAllCompletedContractItems_billingTypeCorporate(Guid contractId, string[] columns)
        {
            QueryExpression queryExpression = new QueryExpression("rnt_contractitem");
            queryExpression.ColumnSet = new ColumnSet(columns);
            queryExpression.Criteria.AddCondition("rnt_contractid", ConditionOperator.Equal, contractId);
            queryExpression.Criteria.AddCondition("rnt_billingtype", ConditionOperator.Equal, (int)rnt_BillingTypeCode.Corporate);
            queryExpression.Criteria.AddCondition("statuscode", ConditionOperator.Equal, (int)rnt_contractitem_StatusCode.Completed);

            return this.retrieveMultiple(queryExpression).Entities.ToList();
        }
        public List<Entity> getAllCompletedContractItems(string[] columns)
        {
            List<Entity> result = new List<Entity>();
            int queryCount = 5000;
            int pageNumber = 1;
            string pagingCookie = null;

            while (true)
            {
                QueryExpression query = new QueryExpression("rnt_contractitem");
                query.PageInfo = new PagingInfo
                {
                    PageNumber = pageNumber,
                    Count = queryCount,
                    PagingCookie = pagingCookie
                };
                query.ColumnSet = new ColumnSet(columns);
                query.Criteria.AddCondition("statuscode", ConditionOperator.Equal, (int)rnt_contractitem_StatusCode.Completed);
                query.LinkEntities.Add(new LinkEntity("rnt_contractitem", "rnt_contract", "rnt_contractid", "rnt_contractid", JoinOperator.Inner));
                query.LinkEntities[0].EntityAlias = "contracts";
                query.LinkEntities[0].Columns.AddColumns("rnt_pickupbranchid", "rnt_contractnumber");
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
        public List<Entity> getBrokerCompletedContractItemsByXlastDays(int days)
        {
            List<Entity> result = new List<Entity>();
            int queryCount = 5000;
            int pageNumber = 1;
            string pagingCookie = null;

            while (true)
            {
                QueryExpression queryExpression = new QueryExpression("rnt_contractitem");
                queryExpression.ColumnSet = new ColumnSet(true);
                //FilterExpression filterExpression = new FilterExpression(LogicalOperator.Or);
                //filterExpression.AddCondition(new ConditionExpression("statuscode", ConditionOperator.Equal, (int)rnt_contractitem_StatusCode.InactiveNoShow));
                //filterExpression.AddCondition(new ConditionExpression("statuscode", ConditionOperator.Equal, (int)rnt_contractitem_StatusCode.Inactive));
                //filterExpression.AddCondition(new ConditionExpression("statuscode", ConditionOperator.Equal, (int)rnt_contractitem_StatusCode.Completed));
                //queryExpression.Criteria.AddFilter(filterExpression);
                queryExpression.Criteria.AddCondition("statuscode", ConditionOperator.In, (int)rnt_contractitem_StatusCode.InactiveNoShow, (int)rnt_contractitem_StatusCode.Inactive, (int)rnt_contractitem_StatusCode.Completed);
                queryExpression.Criteria.AddCondition("rnt_dropoffdatetime", ConditionOperator.LastXDays, days);
                queryExpression.Criteria.AddCondition("rnt_changereason", ConditionOperator.Null);

                queryExpression.Criteria.AddCondition("rnt_itemtypecode", ConditionOperator.Equal, (int)rnt_contractitem_rnt_itemtypecode.Equipment);

                queryExpression.LinkEntities.Add(new LinkEntity("rnt_contractitem", "rnt_contract", "rnt_contractid", "rnt_contractid", JoinOperator.Inner));
                queryExpression.LinkEntities[0].EntityAlias = "contracts";
                queryExpression.LinkEntities[0].LinkCriteria.AddCondition("statuscode", ConditionOperator.Equal, (int)rnt_contract_StatusCode.Completed);
                queryExpression.LinkEntities[0].LinkCriteria.AddCondition("rnt_contracttypecode", ConditionOperator.Equal, (int)rnt_ReservationTypeCode.Broker);
                //queryExpression.LinkEntities[0].LinkCriteria.AddCondition("rnt_ismonthly", ConditionOperator.Equal, false);
                queryExpression.PageInfo = new PagingInfo
                {
                    PageNumber = pageNumber,
                    Count = queryCount,
                    PagingCookie = pagingCookie
                };

                var items = this.retrieveMultiple(queryExpression);
                if (items.MoreRecords)
                {
                    result.AddRange(items.Entities.ToList());
                    pageNumber++;
                    pagingCookie = items.PagingCookie;
                }
                else
                {
                    result.AddRange(items.Entities.ToList());
                    break;
                }
            }

            return result;

        }
        public List<Entity> getCompletedContractItemPriceDifferenceByXlastDays(int days)
        {
            List<Entity> result = new List<Entity>();
            int queryCount = 5000;
            int pageNumber = 1;
            string pagingCookie = null;

            while (true)
            {
                QueryExpression queryExpression = new QueryExpression("rnt_contractitem");
                queryExpression.ColumnSet = new ColumnSet(true);
                queryExpression.Criteria.AddCondition("rnt_dropoffdatetime", ConditionOperator.LastXDays, days);
                queryExpression.Criteria.AddCondition("statuscode", ConditionOperator.Equal, (int)rnt_contractitem_StatusCode.Completed);
                queryExpression.Criteria.AddCondition("rnt_itemtypecode", ConditionOperator.Equal, (int)rnt_contractitem_rnt_itemtypecode.PriceDifference);

                queryExpression.LinkEntities.Add(new LinkEntity("rnt_contractitem", "rnt_contract", "rnt_contractid", "rnt_contractid", JoinOperator.Inner));
                queryExpression.LinkEntities[0].EntityAlias = "contracts";
                queryExpression.LinkEntities[0].LinkCriteria.AddCondition("statuscode", ConditionOperator.Equal, (int)rnt_contract_StatusCode.Completed);
                //queryExpression.LinkEntities[0].LinkCriteria.AddCondition("rnt_ismonthly", ConditionOperator.Equal, false);
                queryExpression.PageInfo = new PagingInfo
                {
                    PageNumber = pageNumber,
                    Count = queryCount,
                    PagingCookie = pagingCookie
                };

                var items = this.retrieveMultiple(queryExpression);
                result.AddRange(items.Entities.ToList());
                if (items.MoreRecords)
                {
                    pageNumber++;
                    pagingCookie = items.PagingCookie;
                }
                else
                {
                    result.AddRange(items.Entities.ToList());
                    break;
                }
            }

            return result;

        }
        public List<Entity> getCompletedContractItemsByGivenDays()
        {
            List<Entity> result = new List<Entity>();
            int queryCount = 5000;
            int pageNumber = 1;
            string pagingCookie = null;

            while (true)
            {
                QueryExpression queryExpression = new QueryExpression("rnt_contractitem");
                queryExpression.ColumnSet = new ColumnSet(true);
                //queryExpression.Criteria.AddCondition("rnt_contractid", ConditionOperator.Equal, "dd0e266d-b517-ec11-b6e6-6045bd8af5b6");
                queryExpression.Criteria.AddCondition("statuscode", ConditionOperator.In, new object[] { (int)rnt_contractitem_StatusCode.Completed, 100000010 });
                queryExpression.Criteria.AddCondition("rnt_itemtypecode", ConditionOperator.In, new object[] { (int)rnt_contractitem_rnt_itemtypecode.Equipment, (int)rnt_contractitem_rnt_itemtypecode.PriceDifference });
                queryExpression.AddOrder("rnt_dropoffdatetime", OrderType.Descending);

                queryExpression.LinkEntities.Add(new LinkEntity("rnt_contractitem", "rnt_contract", "rnt_contractid", "rnt_contractid", JoinOperator.LeftOuter));
                queryExpression.LinkEntities[0].Columns = new ColumnSet("rnt_pnrnumber");
                queryExpression.LinkEntities[0].EntityAlias = "contract";

                queryExpression.PageInfo = new PagingInfo
                {
                    PageNumber = pageNumber,
                    Count = queryCount,
                    PagingCookie = pagingCookie
                };

                var items = this.retrieveMultiple(queryExpression);
                result.AddRange(items.Entities.ToList());
                if (items.MoreRecords)
                {
                    pageNumber++;
                    pagingCookie = items.PagingCookie;
                }
                else
                {
                    result.AddRange(items.Entities.ToList());
                    break;
                }
            }

            return result;



        }
        public List<Entity> getCompletedContractItemsByGivenDays(string startDate, string endDate)
        {
            List<Entity> result = new List<Entity>();
            int queryCount = 5000;
            int pageNumber = 1;
            string pagingCookie = null;

            while (true)
            {
                QueryExpression queryExpression = new QueryExpression("rnt_contractitem");
                queryExpression.ColumnSet = new ColumnSet(true);
                queryExpression.Criteria.AddCondition("rnt_dropoffdatetime", ConditionOperator.OnOrAfter, startDate);
                queryExpression.Criteria.AddCondition("rnt_dropoffdatetime", ConditionOperator.OnOrBefore, endDate);
                //queryExpression.Criteria.AddCondition("rnt_contractid", ConditionOperator.Equal, "dd0e266d-b517-ec11-b6e6-6045bd8af5b6");
                queryExpression.Criteria.AddCondition("statuscode", ConditionOperator.In, new object[] { (int)rnt_contractitem_StatusCode.Completed, 100000010 });
                queryExpression.Criteria.AddCondition("rnt_itemtypecode", ConditionOperator.In, new object[] { (int)rnt_contractitem_rnt_itemtypecode.Equipment, (int)rnt_contractitem_rnt_itemtypecode.PriceDifference });
                queryExpression.AddOrder("rnt_dropoffdatetime", OrderType.Descending);

                queryExpression.LinkEntities.Add(new LinkEntity("rnt_contractitem", "rnt_contract", "rnt_contractid", "rnt_contractid", JoinOperator.LeftOuter));
                queryExpression.LinkEntities[0].Columns = new ColumnSet("rnt_pnrnumber");
                queryExpression.LinkEntities[0].EntityAlias = "contract";

                queryExpression.PageInfo = new PagingInfo
                {
                    PageNumber = pageNumber,
                    Count = queryCount,
                    PagingCookie = pagingCookie
                };

                var items = this.retrieveMultiple(queryExpression);
                result.AddRange(items.Entities.ToList());
                if (items.MoreRecords)
                {
                    pageNumber++;
                    pagingCookie = items.PagingCookie;
                }
                else
                {
                    result.AddRange(items.Entities.ToList());
                    break;
                }
            }

            return result;



        }
        public List<Entity> getCompletedContractItemEquipmentsByXlastDays(int days)
        {
            List<Entity> result = new List<Entity>();
            int queryCount = 5000;
            int pageNumber = 1;
            string pagingCookie = null;

            while (true)
            {
                QueryExpression queryExpression = new QueryExpression("rnt_contractitem");
                queryExpression.ColumnSet = new ColumnSet(true);
                queryExpression.Criteria.AddCondition("rnt_dropoffdatetime", ConditionOperator.LastXDays, days);
                queryExpression.Criteria.AddCondition("statuscode", ConditionOperator.Equal, (int)rnt_contractitem_StatusCode.Completed);
                queryExpression.Criteria.AddCondition("rnt_itemtypecode", ConditionOperator.Equal, (int)rnt_contractitem_rnt_itemtypecode.Equipment);

                //queryExpression.LinkEntities.Add(new LinkEntity("rnt_contractitem", "rnt_contract", "rnt_contractid", "rnt_contractid", JoinOperator.Inner));
                //queryExpression.LinkEntities[0].LinkCriteria.AddCondition("rnt_ismonthly", ConditionOperator.Equal, false);

                queryExpression.PageInfo = new PagingInfo
                {
                    PageNumber = pageNumber,
                    Count = queryCount,
                    PagingCookie = pagingCookie
                };

                var items = this.retrieveMultiple(queryExpression);
                result.AddRange(items.Entities.ToList());
                if (items.MoreRecords)
                {
                    pageNumber++;
                    pagingCookie = items.PagingCookie;
                }
                else
                {
                    result.AddRange(items.Entities.ToList());
                    break;
                }
            }

            return result;



        }
        public List<Entity> getContractItemsWithoutAnyColumnsByContractId(Guid contractId)
        {
            QueryExpression queryExpression = new QueryExpression("rnt_contractitem");
            queryExpression.Criteria.AddCondition("rnt_contractid", ConditionOperator.Equal, contractId);
            queryExpression.Criteria.AddCondition("statuscode", ConditionOperator.Equal, (int)ContractItemEnums.StatusCode.WaitingforDelivery);
            return this.retrieveMultiple(queryExpression).Entities.ToList();
        }
        public List<Entity> getContractItemsByGivenList(List<Guid> contractItems)
        {
            QueryExpression queryExpression = new QueryExpression("rnt_contractitem");
            queryExpression.ColumnSet = new ColumnSet(true);
            queryExpression.Criteria.AddCondition("rnt_contractitemid", ConditionOperator.In, contractItems.ToArray());
            return this.retrieveMultiple(queryExpression).Entities.ToList();
        }
        public List<Entity> getWaitingforDeliveryContractAdditionalProductsByContractId(Guid contractId, string[] columns)
        {
            QueryExpression queryExpression = new QueryExpression("rnt_contractitem");
            queryExpression.ColumnSet = new ColumnSet(columns);
            queryExpression.Criteria.AddCondition("rnt_contractid", ConditionOperator.Equal, contractId);
            queryExpression.Criteria.AddCondition("statuscode", ConditionOperator.Equal, (int)ClassLibrary._Enums_1033.rnt_contractitem_StatusCode.WaitingForDelivery);
            queryExpression.Criteria.AddCondition("rnt_itemtypecode", ConditionOperator.Equal, (int)ClassLibrary._Enums_1033.rnt_contractitem_rnt_itemtypecode.AdditionalProduct);

            return this.retrieveMultiple(queryExpression).Entities.ToList();
        }
        public List<Entity> getWaitingforDeliveryContractItemsByContractIdByGivenColumns(Guid contractId, string[] columns)
        {
            QueryExpression queryExpression = new QueryExpression("rnt_contractitem");
            queryExpression.ColumnSet = new ColumnSet(columns);
            queryExpression.Criteria.AddCondition("rnt_contractid", ConditionOperator.Equal, contractId);
            queryExpression.Criteria.AddCondition("statuscode", ConditionOperator.Equal, (int)ContractItemEnums.StatusCode.WaitingforDelivery);
            return this.retrieveMultiple(queryExpression).Entities.ToList();
        }
        public List<Entity> getRentalContractItemsByContractIdByGivenColumns(Guid contractId, string[] columns)
        {
            QueryExpression queryExpression = new QueryExpression("rnt_contractitem");
            queryExpression.ColumnSet = new ColumnSet(columns);
            queryExpression.Criteria.AddCondition("rnt_contractid", ConditionOperator.Equal, contractId);
            queryExpression.Criteria.AddCondition("statuscode", ConditionOperator.Equal, (int)ContractItemEnums.StatusCode.Rental);
            return this.retrieveMultiple(queryExpression).Entities.ToList();
        }
        public List<Entity> getActiveContractEquipments(Guid contractId, string[] columns)
        {
            QueryExpression queryExpression = new QueryExpression("rnt_contractitem");
            queryExpression.ColumnSet = new ColumnSet(columns);
            queryExpression.Criteria.AddCondition("rnt_contractid", ConditionOperator.Equal, contractId);
            queryExpression.Criteria.AddCondition("rnt_itemtypecode", ConditionOperator.Equal, (int)rnt_contractitem_rnt_itemtypecode.Equipment);
            queryExpression.Criteria.AddCondition("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active);

            return this.retrieveMultiple(queryExpression).Entities.ToList();
        }
        public List<Entity> getContractAdditionalandFineProducts(Guid contractId)
        {
            QueryExpression queryExpression = new QueryExpression("rnt_contractitem");
            queryExpression.ColumnSet = new ColumnSet(true);
            queryExpression.Criteria.AddCondition("rnt_contractid", ConditionOperator.Equal, contractId);
            queryExpression.Criteria.AddCondition("rnt_itemtypecode", ConditionOperator.In, new object[] { (int)rnt_contractitem_rnt_itemtypecode.Fine, (int)rnt_contractitem_rnt_itemtypecode.AdditionalProduct });
            queryExpression.Criteria.AddCondition("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active);

            return this.retrieveMultiple(queryExpression).Entities.ToList();
        }
        public List<Entity> getActiveContractEquipments(Guid contractId)
        {
            QueryExpression queryExpression = new QueryExpression("rnt_contractitem");
            queryExpression.ColumnSet = new ColumnSet(true);
            queryExpression.Criteria.AddCondition("rnt_contractid", ConditionOperator.Equal, contractId);
            queryExpression.Criteria.AddCondition("rnt_itemtypecode", ConditionOperator.Equal, (int)rnt_contractitem_rnt_itemtypecode.Equipment);
            // queryExpression.Criteria.AddCondition("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active);

            return this.retrieveMultiple(queryExpression).Entities.ToList();
        }
        public List<Entity> getActiveContractEquipmentsandPriceDifferences(Guid contractId)
        {
            QueryExpression queryExpression = new QueryExpression("rnt_contractitem");
            queryExpression.ColumnSet = new ColumnSet(true);
            queryExpression.Criteria.AddCondition("rnt_contractid", ConditionOperator.Equal, contractId);
            queryExpression.Criteria.AddCondition("rnt_itemtypecode", ConditionOperator.In, new object[] { (int)rnt_contractitem_rnt_itemtypecode.Equipment, (int)rnt_contractitem_rnt_itemtypecode.PriceDifference });
            queryExpression.Criteria.AddCondition("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active);

            return this.retrieveMultiple(queryExpression).Entities.ToList();
        }
        public List<Entity> getRentalandWaitingforDeliveryEquipmentContractItemsByContractId(Guid contractId, string[] columns)
        {
            QueryExpression queryExpression = new QueryExpression("rnt_contractitem");
            queryExpression.ColumnSet = new ColumnSet(columns);
            queryExpression.Criteria.AddCondition("rnt_contractid", ConditionOperator.Equal, contractId);
            queryExpression.Criteria.AddCondition("rnt_itemtypecode", ConditionOperator.Equal, (int)rnt_contractitem_rnt_itemtypecode.Equipment);

            FilterExpression filterExpression = new FilterExpression(LogicalOperator.Or);
            filterExpression.AddCondition(new ConditionExpression("statuscode", ConditionOperator.Equal, (int)rnt_contractitem_StatusCode.Rental));
            filterExpression.AddCondition(new ConditionExpression("statuscode", ConditionOperator.Equal, (int)rnt_contractitem_StatusCode.WaitingForDelivery));

            queryExpression.Criteria.AddFilter(filterExpression);
            return this.retrieveMultiple(queryExpression).Entities.ToList();
        }
        public List<Entity> getRentalContractItemsByContractId(Guid contractId)
        {
            QueryExpression queryExpression = new QueryExpression("rnt_contractitem");
            queryExpression.ColumnSet = new ColumnSet(true);
            queryExpression.Criteria.AddCondition("rnt_contractid", ConditionOperator.Equal, contractId);
            queryExpression.Criteria.AddCondition("statuscode", ConditionOperator.Equal, (int)rnt_contractitem_StatusCode.Rental);
            return this.retrieveMultiple(queryExpression).Entities.ToList();
        }
        public List<Entity> getRentalContractItemAdditionalProductsByContractIdByGivenColumns(Guid contractId, string[] columns)
        {
            QueryExpression queryExpression = new QueryExpression("rnt_contractitem");
            queryExpression.ColumnSet = new ColumnSet(columns);
            queryExpression.Criteria.AddCondition("rnt_contractid", ConditionOperator.Equal, contractId);
            queryExpression.Criteria.AddCondition("statuscode", ConditionOperator.Equal, (int)rnt_contractitem_StatusCode.Rental);
            queryExpression.Criteria.AddCondition("rnt_itemtypecode", ConditionOperator.Equal, (int)rnt_contractitem_rnt_itemtypecode.AdditionalProduct);

            return this.retrieveMultiple(queryExpression).Entities.ToList();
        }
        public Entity getUpselledItem(Guid contractId, string[] columns)
        {
            QueryExpression queryExpression = new QueryExpression("rnt_contractitem");
            queryExpression.ColumnSet = new ColumnSet(columns);
            queryExpression.Criteria.AddCondition("rnt_contractid", ConditionOperator.Equal, contractId);
            queryExpression.Criteria.AddCondition("rnt_itemtypecode", ConditionOperator.Equal, (int)ContractItemEnums.ItemTypeCode.Equipment);
            queryExpression.Criteria.AddCondition("statecode", ConditionOperator.Equal, 1);
            queryExpression.Criteria.AddCondition("rnt_changetype", ConditionOperator.Equal, (int)rnt_ChangeType.Upsell);

            return this.retrieveMultiple(queryExpression).Entities.FirstOrDefault();
        }
        public Entity getContractEquipmentByGivenColumns(Guid contractId, string[] columns)
        {
            QueryExpression queryExpression = new QueryExpression("rnt_contractitem");
            queryExpression.ColumnSet = new ColumnSet(columns);
            queryExpression.Criteria.AddCondition("rnt_contractid", ConditionOperator.Equal, contractId);
            queryExpression.Criteria.AddCondition("rnt_itemtypecode", ConditionOperator.Equal, (int)ContractItemEnums.ItemTypeCode.Equipment);

            return this.retrieveMultiple(queryExpression).Entities.FirstOrDefault();
        }
        public List<Entity> getContractEquipmentsByGivenColumns(Guid contractId, string[] columns)
        {
            QueryExpression queryExpression = new QueryExpression("rnt_contractitem");
            queryExpression.ColumnSet = new ColumnSet(columns);
            queryExpression.Criteria.AddCondition("rnt_contractid", ConditionOperator.Equal, contractId);
            queryExpression.Criteria.AddCondition("rnt_itemtypecode", ConditionOperator.Equal, (int)ContractItemEnums.ItemTypeCode.Equipment);

            return this.retrieveMultiple(queryExpression).Entities.ToList();
        }
        public List<Entity> getWaitingForDeliveryContractItemsByContractId(Guid contractId)
        {
            QueryExpression queryExpression = new QueryExpression("rnt_contractitem");
            queryExpression.ColumnSet = new ColumnSet(true);
            queryExpression.Criteria.AddCondition("rnt_contractid", ConditionOperator.Equal, contractId);

            queryExpression.Criteria.AddCondition("statuscode", ConditionOperator.Equal, (int)ContractItemEnums.StatusCode.WaitingforDelivery);
            return this.retrieveMultiple(queryExpression).Entities.ToList();
        }
        public List<Entity> getContractItemAdditionalProductsByContractIdByAdditionalProductIds(Guid contractId, string[] additionalProductsIds)
        {
            QueryExpression queryExpression = new QueryExpression("rnt_contractitem");
            queryExpression.ColumnSet = new ColumnSet(true);
            queryExpression.Criteria.AddCondition("rnt_contractid", ConditionOperator.Equal, contractId);
            queryExpression.Criteria.AddCondition("rnt_itemtypecode", ConditionOperator.Equal, (int)ContractItemEnums.ItemTypeCode.AdditionalProduct);
            queryExpression.Criteria.AddCondition("rnt_additionalproductid", ConditionOperator.In, additionalProductsIds);
            return this.retrieveMultiple(queryExpression).Entities.ToList();
        }
        public Entity getContractItemByReservationItemId(Guid reservationItemId)
        {
            QueryExpression queryExpression = new QueryExpression("rnt_contractitem");
            queryExpression.ColumnSet = new ColumnSet(true);
            queryExpression.Criteria.AddCondition("rnt_reservationitemid", ConditionOperator.Equal, reservationItemId);

            return this.retrieveMultiple(queryExpression).Entities.FirstOrDefault();
        }
        public List<Entity> getCompletedContractItemsByContractId(Guid contractId)
        {
            QueryExpression queryExpression = new QueryExpression("rnt_contractitem");
            queryExpression.ColumnSet = new ColumnSet(true);
            queryExpression.Criteria.AddCondition("rnt_contractid", ConditionOperator.Equal, contractId);
            queryExpression.Criteria.AddCondition("statuscode", ConditionOperator.Equal, (int)ContractItemEnums.StatusCode.Completed);
            return this.retrieveMultiple(queryExpression).Entities.ToList();
        }
        public List<Entity> getCompletedContractItemsByContractIdWithGivenColmuns(Guid contractId, string[] columns)
        {
            QueryExpression queryExpression = new QueryExpression("rnt_contractitem");
            queryExpression.ColumnSet = new ColumnSet(columns);
            queryExpression.Criteria.AddCondition("rnt_contractid", ConditionOperator.Equal, contractId);
            queryExpression.Criteria.AddCondition("statuscode", ConditionOperator.Equal, (int)ContractItemEnums.StatusCode.Completed);
            return this.retrieveMultiple(queryExpression).Entities.ToList();
        }
        public Entity getContractItemByAdditionalProductIdandContractId(Guid contractId, Guid additionalProductId)
        {
            QueryExpression queryExpression = new QueryExpression("rnt_contractitem");
            queryExpression.ColumnSet = new ColumnSet(true);
            queryExpression.Criteria.AddCondition("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active);
            queryExpression.Criteria.AddCondition("rnt_additionalproductid", ConditionOperator.Equal, additionalProductId);
            queryExpression.Criteria.AddCondition("rnt_itemtypecode", ConditionOperator.Equal, (int)ContractItemEnums.ItemTypeCode.AdditionalProduct);
            queryExpression.Criteria.AddCondition("rnt_contractid", ConditionOperator.Equal, contractId);
            return this.retrieveMultiple(queryExpression).Entities.FirstOrDefault();
        }
        public Entity getActiveContractItemByAdditionalProductIdandContractId(Guid contractId, Guid additionalProductId)
        {
            QueryExpression queryExpression = new QueryExpression("rnt_contractitem");
            queryExpression.ColumnSet = new ColumnSet(true);
            queryExpression.Criteria.AddCondition("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active);
            queryExpression.Criteria.AddCondition("statuscode", ConditionOperator.NotIn, new object[] { (int)rnt_contractitem_StatusCode.CanceledByRentGo, (int)rnt_contractitem_StatusCode.CanceledByCustomer });
            queryExpression.Criteria.AddCondition("rnt_additionalproductid", ConditionOperator.Equal, additionalProductId);
            queryExpression.Criteria.AddCondition("rnt_itemtypecode", ConditionOperator.Equal, (int)ContractItemEnums.ItemTypeCode.AdditionalProduct);
            queryExpression.Criteria.AddCondition("rnt_contractid", ConditionOperator.Equal, contractId);
            return this.retrieveMultiple(queryExpression).Entities.FirstOrDefault();
        }
        public Entity getRentalEquipmentContractItemByGivenColumns(Guid contractId, string[] columns)
        {
            QueryExpression queryExpression = new QueryExpression("rnt_contractitem");
            queryExpression.ColumnSet = new ColumnSet(columns);
            queryExpression.Criteria.AddCondition("statuscode", ConditionOperator.Equal, (int)rnt_contractitem_StatusCode.Rental);
            queryExpression.Criteria.AddCondition("rnt_itemtypecode", ConditionOperator.Equal, (int)rnt_contractitem_rnt_itemtypecode.Equipment);
            queryExpression.Criteria.AddCondition("rnt_contractid", ConditionOperator.Equal, contractId);
            return this.retrieveMultiple(queryExpression).Entities.FirstOrDefault();
        }
        public Entity getContractItemIdByGivenColumns(Guid contractItemId, string[] columns)
        {
            QueryExpression queryExpression = new QueryExpression("rnt_contractitem");
            queryExpression.ColumnSet = new ColumnSet(columns);
            queryExpression.Criteria.AddCondition("rnt_contractitemid", ConditionOperator.Equal, contractItemId);
            return this.retrieveMultiple(queryExpression).Entities.FirstOrDefault();
        }
        public Entity getContractItemId(Guid contractItemId)
        {
            QueryExpression queryExpression = new QueryExpression("rnt_contractitem");
            queryExpression.ColumnSet = new ColumnSet(true);
            queryExpression.Criteria.AddCondition("rnt_contractitemid", ConditionOperator.Equal, contractItemId);
            return this.retrieveMultiple(queryExpression).Entities.FirstOrDefault();
        }
        public Entity getContractItemAdditionalProductByContractIdandAdditionalProductIdByGivenColumns(Guid contractId, Guid additonalProductId, string[] columns)
        {
            QueryExpression queryExpression = new QueryExpression("rnt_contractitem");
            queryExpression.ColumnSet = new ColumnSet(columns);
            queryExpression.Criteria.AddCondition("rnt_contractid", ConditionOperator.Equal, contractId);
            queryExpression.Criteria.AddCondition("rnt_additionalproductid", ConditionOperator.Equal, additonalProductId);
            queryExpression.Criteria.AddCondition("rnt_itemtypecode", ConditionOperator.Equal, (int)rnt_contractitem_rnt_itemtypecode.AdditionalProduct);

            return this.retrieveMultiple(queryExpression).Entities.FirstOrDefault();
        }
        public Entity getContractItemFineAdditionalProductByContractIdandAdditionalProductIdByGivenColumns(Guid contractId, Guid additonalProductId, string[] columns)
        {
            QueryExpression queryExpression = new QueryExpression("rnt_contractitem");
            queryExpression.ColumnSet = new ColumnSet(columns);
            queryExpression.Criteria.AddCondition("rnt_contractid", ConditionOperator.Equal, contractId);
            queryExpression.Criteria.AddCondition("rnt_additionalproductid", ConditionOperator.Equal, additonalProductId);
            queryExpression.Criteria.AddCondition("rnt_itemtypecode", ConditionOperator.Equal, (int)rnt_contractitem_rnt_itemtypecode.Fine);

            return this.retrieveMultiple(queryExpression).Entities.FirstOrDefault();
        }

        public EntityCollection getContractItemFineAdditionalProductsByContractIdandAdditionalProductIdByGivenColumns(Guid contractId, Guid additonalProductId, string[] columns)
        {
            QueryExpression queryExpression = new QueryExpression("rnt_contractitem");
            queryExpression.ColumnSet = new ColumnSet(columns);
            queryExpression.Criteria.AddCondition("rnt_contractid", ConditionOperator.Equal, contractId);
            queryExpression.Criteria.AddCondition("rnt_additionalproductid", ConditionOperator.Equal, additonalProductId);
            queryExpression.Criteria.AddCondition("rnt_itemtypecode", ConditionOperator.Equal, (int)rnt_contractitem_rnt_itemtypecode.Fine);

            return this.retrieveMultiple(queryExpression);
        }
        public Entity getCompletedEquipmentContractItemByContractandEquipmentId(Guid contractId, Guid equipmentId, string[] columns)
        {
            QueryExpression queryExpression = new QueryExpression("rnt_contractitem");
            queryExpression.ColumnSet = new ColumnSet(columns);
            queryExpression.Criteria.AddCondition("rnt_contractid", ConditionOperator.Equal, contractId);
            queryExpression.Criteria.AddCondition("rnt_equipment", ConditionOperator.Equal, equipmentId);
            queryExpression.Criteria.AddCondition("statuscode", ConditionOperator.Equal, (int)rnt_contractitem_StatusCode.Completed);
            return this.retrieveMultiple(queryExpression).Entities.FirstOrDefault();
        }
        public List<Entity> getRentalFineItems(Guid contractId)
        {
            QueryExpression queryExpression = new QueryExpression("rnt_contractitem");
            queryExpression.ColumnSet = new ColumnSet(true);
            queryExpression.Criteria.AddCondition("rnt_contractid", ConditionOperator.Equal, contractId);
            queryExpression.Criteria.AddCondition("statuscode", ConditionOperator.Equal, (int)rnt_contractitem_StatusCode.Rental);
            queryExpression.Criteria.AddCondition("rnt_itemtypecode", ConditionOperator.Equal, (int)rnt_contractitem_rnt_itemtypecode.Fine);
            return this.retrieveMultiple(queryExpression).Entities.ToList();
        }
        public List<Entity> getNotCancelledEquipmentContractItemByEquipmentId(Guid equipmentId, DateTime fineDateTime)
        {
            QueryExpression queryExpression = new QueryExpression("rnt_contractitem");
            queryExpression.ColumnSet = new ColumnSet(true);
            queryExpression.Criteria.AddCondition("rnt_equipment", ConditionOperator.Equal, equipmentId);
            queryExpression.Criteria.AddCondition("rnt_pickupdatetime", ConditionOperator.OnOrBefore, fineDateTime);
            queryExpression.Criteria.AddCondition("rnt_dropoffdatetime", ConditionOperator.OnOrAfter, fineDateTime);
            FilterExpression filterExpression = new FilterExpression(LogicalOperator.And);
            filterExpression.AddCondition(new ConditionExpression("statuscode", ConditionOperator.NotEqual, (int)rnt_contractitem_StatusCode.CanceledByCustomer));
            filterExpression.AddCondition(new ConditionExpression("statuscode", ConditionOperator.NotEqual, (int)rnt_contractitem_StatusCode.CanceledByRentGo));
            filterExpression.AddCondition(new ConditionExpression("statuscode", ConditionOperator.NotEqual, (int)rnt_contractitem_StatusCode.Inactive));

            queryExpression.Criteria.AddFilter(filterExpression);
            return this.retrieveMultiple(queryExpression).Entities.ToList();
        }
        public Entity getRentalEquipment(Guid equipmentId)
        {
            QueryExpression queryExpression = new QueryExpression("rnt_contractitem");
            queryExpression.ColumnSet = new ColumnSet(true);
            queryExpression.Criteria.AddCondition("rnt_equipment", ConditionOperator.Equal, equipmentId);
            queryExpression.Criteria.AddCondition("statuscode", ConditionOperator.Equal, (int)rnt_contractitem_StatusCode.Rental);
            return this.retrieveMultiple(queryExpression).Entities.FirstOrDefault();
        }
        public List<Entity> getCompletedContractItemAdditionalProductByBranchIdBetweenGivenDates(Guid branchId, DateTime startDate, DateTime endDate, string[] columns)
        {
            QueryExpression queryExpression = new QueryExpression("rnt_contractitem");
            queryExpression.ColumnSet = new ColumnSet(columns);
            queryExpression.Criteria.AddCondition("rnt_pickupbranchid", ConditionOperator.Equal, branchId);
            queryExpression.Criteria.AddCondition("rnt_itemtypecode", ConditionOperator.Equal, (int)rnt_contractitem_rnt_itemtypecode.AdditionalProduct);
            queryExpression.Criteria.AddCondition("statuscode", ConditionOperator.Equal, (int)rnt_contractitem_StatusCode.Completed);
            queryExpression.Criteria.AddCondition("rnt_pickupdatetime", ConditionOperator.GreaterThan, startDate);
            queryExpression.Criteria.AddCondition("rnt_pickupdatetime", ConditionOperator.LessThan, endDate);

            return this.retrieveMultiple(queryExpression).Entities.ToList();
        }
        public Entity getDiscountContractItem(Guid contractId, string[] columns)
        {
            ConfigurationBL configurationBL = new ConfigurationBL(this.Service);
            var manualDiscountProduct = configurationBL.GetConfigurationByName("additionalProducts_manuelDiscount");

            AdditionalProductRepository additionalProductRepository = new AdditionalProductRepository(this.Service);
            var additionalProduct = additionalProductRepository.getAdditionalProductByProductCode(manualDiscountProduct);

            QueryExpression queryExpression = new QueryExpression("rnt_contractitem");
            queryExpression.ColumnSet = new ColumnSet(columns);
            queryExpression.Criteria.AddCondition("rnt_additionalproductid", ConditionOperator.Equal, additionalProduct.Id);
            queryExpression.Criteria.AddCondition("rnt_contractid", ConditionOperator.Equal, contractId);

            return this.retrieveMultiple(queryExpression).Entities.FirstOrDefault();
        }

        public List<Entity> getAdditionalProductsByGivenDateByBranch(Guid branchId, string lastDay, string month, string year)
        {
            if (lastDay.Length == 1)
            {
                lastDay = "0" + lastDay;
            }
            if (month.Length == 1)
            {
                month = "0" + month;
            }
            string fetch = string.Format(@"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                                              <entity name='rnt_contractitem'>
                                                <attribute name='rnt_contractitemid' />
                                                <attribute name='rnt_totalamount' />
                                                <attribute name='statuscode' />        
                                                <attribute name='rnt_pickupdatetime' />
                                                <attribute name='rnt_pickupbranchid' />
                                                <attribute name='rnt_groupcodeinformations' />
                                                <attribute name='rnt_equipment' />
                                                <attribute name='rnt_dropoffdatetime' />
                                                <attribute name='rnt_dropoffbranch' />               
                                                <attribute name='rnt_additionalproductid' />               
                                                <attribute name='createdby' /> 
                                                <filter type='and'>
                                                  <condition attribute='rnt_itemtypecode' operator='eq' value='2' />
                                                  <condition attribute='rnt_pickupbranchid' operator='eq'  uitype='rnt_branch' value='{0}' />
                                                  <condition attribute='rnt_dropoffdatetime' operator='on-or-before' value='{1}-{2}-{3}' />
                                                  <condition attribute='rnt_dropoffdatetime' operator='on-or-after' value='{1}-{2}-01' />
                                                  <condition attribute='rnt_externalusercreatedbyid' operator='null' />
                                                 <condition attribute='statuscode' operator='in'>
                                                        <value>100000003</value>
                                                        <value>100000001</value>
                                                        <value>100000002</value>
                                                 </condition>
                                                </filter>
                                                 <link-entity name='rnt_reservationitem' from='rnt_reservationitemid' to='rnt_reservationitemid' visible='false' link-type='outer' alias='a_a715b173c7a8e911a840000d3a2ddc03'>
                                                      <attribute name='createdby' />
                                                      <attribute name='rnt_channelcode' />
                                                 </link-entity>
                                                <link-entity name='rnt_additionalproduct' from='rnt_additionalproductid' to='rnt_additionalproductid' link-type='inner' alias='ac'>
                                                  <filter type='and'>
                                                    <condition attribute='rnt_bonuscheckcode' operator='eq' value='1' />
                                                  </filter>
                                                </link-entity>
                                              </entity>
                                            </fetch >", branchId, year, month, lastDay);
            return this.retrieveMultiple(new FetchExpression(fetch)).Entities.ToList();
        }

        public EntityCollection getActiveContractItemBetweenGivenDates(DateTime startDate, DateTime endDate, string[] columns)
        {
            EntityCollection contractItems = new EntityCollection();
            int queryCount = 5000;
            // Initialize the page number.
            int pageNumber = 1;
            string pagingCookie = null;
            while (true)
            {
                LinkEntity contractLinkEntity = new LinkEntity();
                contractLinkEntity.EntityAlias = "contractLink";
                contractLinkEntity.LinkFromEntityName = "rnt_contractitem";
                contractLinkEntity.LinkFromAttributeName = "rnt_contractid";
                contractLinkEntity.LinkToEntityName = "rnt_contract";
                contractLinkEntity.LinkToAttributeName = "rnt_contractid";
                contractLinkEntity.LinkCriteria.AddCondition("statuscode", ConditionOperator.In, 1, 100000000, 100000006, 100000007, 100000008);
                contractLinkEntity.Columns = new ColumnSet("rnt_pricinggroupcodeid");

                QueryExpression getContractItemQuery = new QueryExpression("rnt_contractitem");
                getContractItemQuery.ColumnSet = new ColumnSet(columns);
                getContractItemQuery.Criteria.AddCondition("modifiedon", ConditionOperator.OnOrAfter, startDate);
                getContractItemQuery.Criteria.AddCondition("modifiedon", ConditionOperator.OnOrBefore, endDate);
                getContractItemQuery.Criteria.AddCondition("rnt_itemtypecode", ConditionOperator.In, 1, 5);
                getContractItemQuery.Criteria.AddCondition("statecode", ConditionOperator.Equal, 0);
                getContractItemQuery.AddOrder("rnt_dropoffdatetime", OrderType.Ascending);
                getContractItemQuery.LinkEntities.Add(contractLinkEntity);


                getContractItemQuery.PageInfo = new PagingInfo
                {
                    PageNumber = pageNumber,
                    Count = queryCount,
                    PagingCookie = pagingCookie
                };
                var l = this.Service.RetrieveMultiple(getContractItemQuery);
                if (l.MoreRecords)
                {
                    contractItems.Entities.AddRange(l.Entities.ToList());
                    pageNumber++;
                    pagingCookie = l.PagingCookie;
                }
                else
                {
                    contractItems.Entities.AddRange(l.Entities.ToList());
                    break;
                }

            }
            return contractItems;
        }


        public EntityCollection getActiveContractItemBetweenGivenDates(DateTime startDate, string[] columns)
        {
            EntityCollection contractItems = new EntityCollection();
            int queryCount = 5000;
            // Initialize the page number.
            int pageNumber = 1;
            string pagingCookie = null;
            while (true)
            {
                LinkEntity contractLinkEntity = new LinkEntity();
                contractLinkEntity.EntityAlias = "contractLink";
                contractLinkEntity.LinkFromEntityName = "rnt_contractitem";
                contractLinkEntity.LinkFromAttributeName = "rnt_contractid";
                contractLinkEntity.LinkToEntityName = "rnt_contract";
                contractLinkEntity.LinkToAttributeName = "rnt_contractid";
                contractLinkEntity.LinkCriteria.AddCondition("statuscode", ConditionOperator.In, 1, 100000000, 100000006, 100000007, 100000008);
                contractLinkEntity.Columns = new ColumnSet("rnt_pricinggroupcodeid");

                QueryExpression getContractItemQuery = new QueryExpression("rnt_contractitem");
                getContractItemQuery.ColumnSet = new ColumnSet(columns);
                getContractItemQuery.Criteria.AddCondition("rnt_dropoffdatetime", ConditionOperator.OnOrAfter, startDate);
                getContractItemQuery.Criteria.AddCondition("rnt_itemtypecode", ConditionOperator.Equal, 1);
                getContractItemQuery.Criteria.AddCondition("statecode", ConditionOperator.Equal, 0);
                getContractItemQuery.AddOrder("rnt_dropoffdatetime", OrderType.Ascending);
                getContractItemQuery.LinkEntities.Add(contractLinkEntity);


                getContractItemQuery.PageInfo = new PagingInfo
                {
                    PageNumber = pageNumber,
                    Count = queryCount,
                    PagingCookie = pagingCookie
                };
                var l = this.Service.RetrieveMultiple(getContractItemQuery);
                if (l.MoreRecords)
                {
                    contractItems.Entities.AddRange(l.Entities.ToList());
                    pageNumber++;
                    pagingCookie = l.PagingCookie;
                }
                else
                {
                    contractItems.Entities.AddRange(l.Entities.ToList());
                    break;
                }

            }
            return contractItems;
        }

        public EntityCollection getCompletedContractItemBetweenGivenDates(DateTime startDate, DateTime endDate, string[] columns)
        {
            EntityCollection contractItems = new EntityCollection();
            int queryCount = 5000;
            // Initialize the page number.
            int pageNumber = 1;
            string pagingCookie = null;
            while (true)
            {
                LinkEntity contractLinkEntity = new LinkEntity();
                contractLinkEntity.LinkFromEntityName = "rnt_contractitem";
                contractLinkEntity.LinkFromAttributeName = "rnt_contractid";
                contractLinkEntity.LinkToEntityName = "rnt_contract";
                contractLinkEntity.LinkToAttributeName = "rnt_contractid";
                contractLinkEntity.LinkCriteria.AddCondition("statuscode", ConditionOperator.In, 100000001);
                contractLinkEntity.LinkCriteria.AddCondition("rnt_dropoffdatetime", ConditionOperator.GreaterThan, startDate);
                contractLinkEntity.LinkCriteria.AddCondition("rnt_dropoffdatetime", ConditionOperator.LessThan, endDate);

                QueryExpression getContractItemQuery = new QueryExpression("rnt_contractitem");
                getContractItemQuery.ColumnSet = new ColumnSet(columns);
                getContractItemQuery.Criteria.AddCondition("rnt_itemtypecode", ConditionOperator.In, 1, 5);
                getContractItemQuery.LinkEntities.Add(contractLinkEntity);


                getContractItemQuery.PageInfo = new PagingInfo
                {
                    PageNumber = pageNumber,
                    Count = queryCount,
                    PagingCookie = pagingCookie
                };
                var l = this.Service.RetrieveMultiple(getContractItemQuery);
                if (l.MoreRecords)
                {
                    contractItems.Entities.AddRange(l.Entities.ToList());
                    pageNumber++;
                    pagingCookie = l.PagingCookie;
                }
                else
                {
                    contractItems.Entities.AddRange(l.Entities.ToList());
                    break;
                }

            }
            return contractItems;
        }

        public EntityCollection getDeactiveContractItemBetweenGivenDates(DateTime startDate, DateTime endDate, string[] columns)
        {
            EntityCollection contractItems = new EntityCollection();
            int queryCount = 5000;
            // Initialize the page number.
            int pageNumber = 1;
            string pagingCookie = null;
            while (true)
            {
                LinkEntity contractLinkEntity = new LinkEntity();
                contractLinkEntity.LinkFromEntityName = "rnt_contractitem";
                contractLinkEntity.LinkFromAttributeName = "rnt_contractid";
                contractLinkEntity.LinkToEntityName = "rnt_contract";
                contractLinkEntity.LinkToAttributeName = "rnt_contractid";
                contractLinkEntity.LinkCriteria.AddCondition("statuscode", ConditionOperator.In, 100000003, 100000004, 100000005);
                contractLinkEntity.LinkCriteria.AddCondition("rnt_dropoffdatetime", ConditionOperator.GreaterThan, startDate);
                contractLinkEntity.LinkCriteria.AddCondition("rnt_dropoffdatetime", ConditionOperator.LessThan, endDate);


                QueryExpression getContractItemQuery = new QueryExpression("rnt_contractitem");
                getContractItemQuery.ColumnSet = new ColumnSet(columns);
                getContractItemQuery.Criteria.AddCondition("rnt_itemtypecode", ConditionOperator.In, 1, 5);
                getContractItemQuery.LinkEntities.Add(contractLinkEntity);


                getContractItemQuery.PageInfo = new PagingInfo
                {
                    PageNumber = pageNumber,
                    Count = queryCount,
                    PagingCookie = pagingCookie
                };
                var l = this.Service.RetrieveMultiple(getContractItemQuery);
                if (l.MoreRecords)
                {
                    contractItems.Entities.AddRange(l.Entities.ToList());
                    pageNumber++;
                    pagingCookie = l.PagingCookie;
                }
                else
                {
                    contractItems.Entities.AddRange(l.Entities.ToList());
                    break;
                }

            }
            return contractItems;
        }
    }
}
