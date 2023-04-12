using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Tooling.Connector;
using RntCar.BusinessLibrary.Handlers;
using RntCar.ClassLibrary;
using RntCar.ClassLibrary._Enums_1033;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RntCar.BusinessLibrary.Repository
{
    public class CorporateCustomerRepository : RepositoryHandler
    {
        public CorporateCustomerRepository(IOrganizationService Service) : base(Service)
        {
        }

        public CorporateCustomerRepository(IOrganizationService Service, Guid UserId) : base(Service, UserId)
        {
        }

        public CorporateCustomerRepository(IOrganizationService Service, Guid UserId, string OrganizationName) : base(Service, UserId, OrganizationName)
        {
        }

        public CorporateCustomerRepository(CrmServiceClient _crmServiceClient) : base(_crmServiceClient)
        {
        }

        public CorporateCustomerRepository(IOrganizationService Service, CrmServiceClient _crmServiceClient) : base(Service, _crmServiceClient)
        {
        }
        public Entity getAgencyByUserNamePassword(string userName,string password)
        {
            QueryExpression queryExpression = new QueryExpression("account");
            queryExpression.ColumnSet = new ColumnSet(true);
            queryExpression.Criteria.AddCondition("description", ConditionOperator.Equal, userName);
            queryExpression.Criteria.AddCondition("stockexchange", ConditionOperator.Equal, password);
            
            return this.retrieveMultiple(queryExpression).Entities.FirstOrDefault();
        }
        public Entity getCorporateCustomerById(Guid corporateCustomerId, string[] columns)
        {
            QueryExpression queryExpression = new QueryExpression("account");
            queryExpression.ColumnSet = new ColumnSet(columns);
            queryExpression.Criteria.AddCondition("accountid", ConditionOperator.Equal, corporateCustomerId);
            return this.retrieveMultiple(queryExpression).Entities.FirstOrDefault();
        }
        public Entity getCorporateCustomerById(Guid corporateCustomerId)
        {
            QueryExpression queryExpression = new QueryExpression("account");
            queryExpression.ColumnSet = new ColumnSet(true);
            queryExpression.Criteria.AddCondition("accountid", ConditionOperator.Equal, corporateCustomerId);
            return this.retrieveMultiple(queryExpression).Entities.FirstOrDefault();
        }
        public Entity getCorporateCustomerByBrokerCode(string brokerCode, string[] columns)
        {
            QueryExpression queryExpression = new QueryExpression("account");
            queryExpression.ColumnSet = new ColumnSet(columns);
            queryExpression.Criteria.AddCondition("rnt_accounttypecode", ConditionOperator.Equal, (int)rnt_AccountTypeCode.Broker);
            queryExpression.Criteria.AddCondition("rnt_brokercode", ConditionOperator.Equal, brokerCode);
            return this.retrieveMultiple(queryExpression).Entities.FirstOrDefault();
        }
        public List<Entity> getAgencyAndBrokerCorporateCustomers(string[] columns)
        {
            QueryExpression queryExpression = new QueryExpression("account");
            queryExpression.ColumnSet = new ColumnSet(columns);
            var filter = new FilterExpression(LogicalOperator.Or);
            filter.AddCondition("rnt_accounttypecode", ConditionOperator.Equal, (int)rnt_AccountTypeCode.Broker);
            filter.AddCondition("rnt_accounttypecode", ConditionOperator.Equal, (int)rnt_AccountTypeCode.Agency);
            queryExpression.Criteria.AddFilter(filter);
            queryExpression.Criteria.AddCondition("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active);

            return this.retrieveMultiple(queryExpression).Entities.ToList();
        }
        public List<Entity> getCorporateCustomers(string[] columns)
        {
            QueryExpression queryExpression = new QueryExpression("account");
            queryExpression.ColumnSet = new ColumnSet(columns);
            queryExpression.Criteria.AddCondition("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active);

            return this.retrieveMultiple(queryExpression).Entities.ToList();
        }
        public EntityCollection getCorporateCustomersByFetchXML(string criteria)
        {
            var fetchXml = string.Format(@"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                              <entity name='account'>
                                <attribute name='name' />
                                <attribute name='rnt_taxnumber' />
                                <attribute name='telephone1' />
                                <attribute name='rnt_taxoffice' />
                                <order attribute='name' descending='false' />
                                <filter type='and'>
                                  <filter type='and'>
                                    <condition attribute='rnt_accounttypecode' operator='eq' value='10' />
                                    <filter type='or'>
                                      <condition attribute='rnt_taxnumber' operator='like' value='%{0}%' />
                                      <condition attribute='name' operator='like' value='%{0}%' />
                                    </filter>
                                  </filter>
                                </filter>
                              </entity>
                            </fetch>", criteria);

            return this.Service.RetrieveMultiple(new FetchExpression(fetchXml));

        }
        public EntityCollection getCorporateCustomersByTaxNumberandCustomerType(string taxNo, int customerType)
        {
            QueryExpression query = new QueryExpression("account");
            query.ColumnSet = new ColumnSet("accountid");
            query.Criteria = new FilterExpression(LogicalOperator.And);
            query.Criteria.AddCondition("rnt_accounttypecode", ConditionOperator.Equal, customerType);
            query.Criteria.AddCondition("rnt_taxnumber", ConditionOperator.Equal, taxNo);

            return this.Service.RetrieveMultiple(query);
        }
        public EntityCollection getCorporateCustomersByTaxNumberandCustomerTypeForUpdate(string taxNo, int customerType, Guid accountid)
        {
            // get customers does not eq to given account id 
            // this method for check duplicate customer on update message
            QueryExpression query = new QueryExpression("account");
            query.ColumnSet = new ColumnSet("accountid");
            query.Criteria = new FilterExpression(LogicalOperator.And);
            query.Criteria.AddCondition("rnt_accounttypecode", ConditionOperator.Equal, customerType);
            query.Criteria.AddCondition("rnt_taxnumber", ConditionOperator.Equal, taxNo);
            query.Criteria.AddCondition("accountid", ConditionOperator.NotEqual, accountid);
            return this.Service.RetrieveMultiple(query);
        }
        public Entity getCorporateCustomersByTaxNumber(string taxNo)
        {
            QueryExpression query = new QueryExpression("account");
            query.ColumnSet = new ColumnSet(true);
            query.Criteria = new FilterExpression(LogicalOperator.And);
            query.Criteria.AddCondition("rnt_taxnumber", ConditionOperator.Equal, taxNo);
            query.Criteria.AddCondition("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active);
            return this.retrieveMultiple(query).Entities.FirstOrDefault();
        }
        public Guid getCorporateCustomerPriceList(Guid accountId)
        {
            QueryExpression queryExpression = new QueryExpression("account");
            queryExpression.ColumnSet = new ColumnSet("rnt_pricelistid");
            queryExpression.Criteria.AddCondition("accountid", ConditionOperator.Equal, accountId);
            var res = this.Service.RetrieveMultiple(queryExpression).Entities.FirstOrDefault();
            return res == null ? Guid.Empty :
                          res.GetAttributeValue<EntityReference>("rnt_pricelistid") == null ? Guid.Empty :
                          res.GetAttributeValue<EntityReference>("rnt_pricelistid").Id;
        }
        public Guid getCorporateCustomerPriceCode(Guid accountId)
        {
            QueryExpression queryExpression = new QueryExpression("account");
            queryExpression.ColumnSet = new ColumnSet("rnt_pricecodeid");
            queryExpression.Criteria.AddCondition("accountid", ConditionOperator.Equal, accountId);
            var res = this.Service.RetrieveMultiple(queryExpression).Entities.FirstOrDefault();
            return res == null ? Guid.Empty :
                          res.GetAttributeValue<EntityReference>("rnt_pricecodeid") == null ? Guid.Empty :
                          res.GetAttributeValue<EntityReference>("rnt_pricecodeid").Id;
        }
        public Guid getCorporateCustomerMonthlyPriceCode(Guid accountId)
        {
            QueryExpression queryExpression = new QueryExpression("account");
            queryExpression.ColumnSet = new ColumnSet("rnt_monthlypricecodeid");
            queryExpression.Criteria.AddCondition("accountid", ConditionOperator.Equal, accountId);
            var res = this.Service.RetrieveMultiple(queryExpression).Entities.FirstOrDefault();
            return res == null ? Guid.Empty :
                          res.GetAttributeValue<EntityReference>("rnt_monthlypricecodeid") == null ? Guid.Empty :
                          res.GetAttributeValue<EntityReference>("rnt_monthlypricecodeid").Id;
        }
    }
}
