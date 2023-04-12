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
    public class InvoiceRepository : RepositoryHandler
    {
        public InvoiceRepository(IOrganizationService Service) : base(Service)
        {
        }

        public InvoiceRepository(IOrganizationService Service, CrmServiceClient _crmServiceClient) : base(Service, _crmServiceClient)
        {
        }

        public List<Entity> getCustomerInvoices(Guid individualCustomerId)
        {
            QueryExpression invoiceQuery = new QueryExpression("rnt_invoice");
            invoiceQuery.ColumnSet = new ColumnSet(true);
            invoiceQuery.LinkEntities.Add(new LinkEntity("rnt_invoice", "rnt_contract", "rnt_contractid", "rnt_contractid", JoinOperator.Inner));
            invoiceQuery.LinkEntities[0].Columns.AddColumns("rnt_name","rnt_dropoffdatetime","rnt_pnrnumber");
            invoiceQuery.LinkEntities[0].EntityAlias = "contract";
            invoiceQuery.Criteria = new FilterExpression(LogicalOperator.And);
            invoiceQuery.Criteria.AddCondition("statuscode", ConditionOperator.Equal, (int)rnt_invoice_StatusCode.IntegratedWithLogo);
            invoiceQuery.LinkEntities[0].LinkCriteria.AddCondition("rnt_customerid", ConditionOperator.Equal, individualCustomerId);
            return this.retrieveMultiple(invoiceQuery).Entities.ToList();
        }
        public List<Entity> getInvoicesByInvoiceItemId(Guid invoiceItemId)
        {
            QueryExpression query = new QueryExpression("rnt_invoice");
            query.ColumnSet = new ColumnSet(true);
            query.Criteria.AddCondition("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active);
            query.Criteria.AddCondition("rnt_invoiceid", ConditionOperator.Equal, invoiceItemId);
            return this.retrieveMultiple(query).Entities.ToList();
        }
        public Entity getInvoiceById(Guid invoiceId)
        {
            QueryExpression query = new QueryExpression("rnt_invoice");
            query.ColumnSet = new ColumnSet(true);
            query.Criteria.AddCondition("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active);
            query.Criteria.AddCondition("rnt_invoiceid", ConditionOperator.Equal, invoiceId);
            return this.retrieveMultiple(query).Entities.FirstOrDefault();
        }
        public List<Entity> getInvoicesByContractIdExceptByGivenTaxNumber(Guid contractId, string taxnumber)
        {
            QueryExpression query = new QueryExpression("rnt_invoice");
            query.ColumnSet = new ColumnSet(true);
            query.Criteria.AddCondition("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active);
            query.Criteria.AddCondition("rnt_contractid", ConditionOperator.Equal, contractId);
            query.Criteria.AddCondition("rnt_taxnumber", ConditionOperator.NotEqual, taxnumber);
            query.Orders.Add(new OrderExpression("createdon", OrderType.Descending));
            return this.retrieveMultiple(query).Entities.ToList();
        }
        public List<Entity> getInvoicesByContractId(Guid contractId)
        {
            QueryExpression query = new QueryExpression("rnt_invoice");
            query.ColumnSet = new ColumnSet(true);
            query.Criteria.AddCondition("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active);
            query.Criteria.AddCondition("rnt_contractid", ConditionOperator.Equal, contractId);
            FilterExpression filter = new FilterExpression(LogicalOperator.Or);
            filter.AddCondition("statuscode", ConditionOperator.Equal, (int)ClassLibrary._Enums_1033.rnt_invoice_StatusCode.Draft);
            filter.AddCondition("statuscode", ConditionOperator.Equal, (int)ClassLibrary._Enums_1033.rnt_invoice_StatusCode.IntegrationError);
            query.Criteria.AddFilter(filter);
            query.Orders.Add(new OrderExpression("createdon", OrderType.Descending));
            return this.retrieveMultiple(query).Entities.ToList();
        }
        public Entity getLastIntegratedInvoice(Guid contractId)
        {
            QueryExpression query = new QueryExpression("rnt_invoice");
            query.ColumnSet = new ColumnSet(true);
            query.Criteria.AddCondition("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active);
            query.Criteria.AddCondition("rnt_contractid", ConditionOperator.Equal, contractId);
            FilterExpression filter = new FilterExpression(LogicalOperator.Or);
            filter.AddCondition("statuscode", ConditionOperator.Equal, (int)rnt_invoice_StatusCode.IntegratedWithLogo);
            filter.AddCondition("statuscode", ConditionOperator.Equal, (int)rnt_invoice_StatusCode.DealerInvoicing);
            query.Criteria.AddFilter(filter);
            query.Orders.Add(new OrderExpression("createdon", OrderType.Descending));
            return this.retrieveMultiple(query).Entities.FirstOrDefault();
        }
        public Entity getFirstInvoiceByContractId(Guid contractId)
        {
            QueryExpression query = new QueryExpression("rnt_invoice");
            query.ColumnSet = new ColumnSet(true);
            query.Criteria.AddCondition("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active);
            query.Criteria.AddCondition("rnt_contractid", ConditionOperator.Equal, contractId);
            FilterExpression filter = new FilterExpression(LogicalOperator.Or);
            filter.AddCondition("statuscode", ConditionOperator.Equal, (int)rnt_invoice_StatusCode.Draft);
            filter.AddCondition("statuscode", ConditionOperator.Equal, (int)rnt_invoice_StatusCode.IntegrationError);
            query.Criteria.AddFilter(filter);
            query.Orders.Add(new OrderExpression("createdon", OrderType.Descending));
            return this.retrieveMultiple(query).Entities.FirstOrDefault();
        }
        public Entity getCorporateNotIntegratedInvoiceByContractId(Guid contractId, bool isDefault = true)
        {
            QueryExpression query = new QueryExpression("rnt_invoice");
            query.ColumnSet = new ColumnSet(true);
            FilterExpression filter = new FilterExpression(LogicalOperator.Or);
            filter.AddCondition("statuscode", ConditionOperator.Equal, (int)rnt_invoice_StatusCode.Draft);
            filter.AddCondition("statuscode", ConditionOperator.Equal, (int)rnt_invoice_StatusCode.IntegrationError);
            query.Criteria.AddFilter(filter);
            query.Criteria.AddCondition("rnt_defaultinvoice", ConditionOperator.Equal, isDefault);
            query.Criteria.AddCondition("rnt_contractid", ConditionOperator.Equal, contractId);
            query.Criteria.AddCondition("rnt_invoicetypecode", ConditionOperator.Equal, (int)rnt_invoice_rnt_invoicetypecode.Corporate);
            query.Orders.Add(new OrderExpression("createdon", OrderType.Descending));
            return this.retrieveMultiple(query).Entities.FirstOrDefault();
        }
        public List<Entity> getErrorInvoices()
        {
            QueryExpression query = new QueryExpression("rnt_invoice");
            query.ColumnSet = new ColumnSet(true);
            FilterExpression filter = new FilterExpression(LogicalOperator.Or);
            filter.AddCondition("statuscode", ConditionOperator.Equal, (int)rnt_invoice_StatusCode.InternalError);
            filter.AddCondition("statuscode", ConditionOperator.Equal, (int)rnt_invoice_StatusCode.IntegrationError);
            query.Criteria.AddFilter(filter);
            return this.retrieveMultiple(query).Entities.ToList();
        }
        public List<Entity> getFirstActiveInvoiceByContractId(Guid contractId)
        {
            QueryExpression query = new QueryExpression("rnt_invoice");
            query.ColumnSet = new ColumnSet(true);
            query.Criteria.AddCondition("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active);
            query.Criteria.AddCondition("rnt_contractid", ConditionOperator.Equal, contractId);
            query.Criteria.AddCondition("rnt_defaultinvoice", ConditionOperator.Equal, true);
            query.Orders.Add(new OrderExpression("createdon", OrderType.Descending));
            return this.retrieveMultiple(query).Entities.ToList();
        }

        public List<Entity> getFirstAvailableInvoiceByContractId(Guid contractId, bool isDefault = true)
        {
            QueryExpression query = new QueryExpression("rnt_invoice");
            query.ColumnSet = new ColumnSet(true);
            query.Criteria.AddCondition("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active);
            query.Criteria.AddCondition("statuscode", ConditionOperator.In, (int)rnt_invoice_StatusCode.Draft, (int)rnt_invoice_StatusCode.IntegrationError, (int)rnt_invoice_StatusCode.InternalError);
            query.Criteria.AddCondition("rnt_contractid", ConditionOperator.Equal, contractId);
            query.Criteria.AddCondition("rnt_defaultinvoice", ConditionOperator.Equal, isDefault);
            query.Orders.Add(new OrderExpression("createdon", OrderType.Descending));
            return this.retrieveMultiple(query).Entities.ToList();
        }


        public List<Entity> getDraftInvoicesByReservationId(Guid reservationId)
        {
            QueryExpression query = new QueryExpression("rnt_invoice");
            query.ColumnSet = new ColumnSet(true);
            query.Criteria.AddCondition("statuscode", ConditionOperator.Equal, 1);
            query.Criteria.AddCondition("rnt_reservationid", ConditionOperator.Equal, reservationId);
            
            return this.retrieveMultiple(query).Entities.ToList();
        }
        public List<Entity> getFirstActiveInvoiceByReservationId(Guid contractId)
        {
            QueryExpression query = new QueryExpression("rnt_invoice");
            query.ColumnSet = new ColumnSet(true);
            query.Criteria.AddCondition("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active);
            query.Criteria.AddCondition("rnt_reservationid", ConditionOperator.Equal, contractId);
            query.Orders.Add(new OrderExpression("createdon", OrderType.Descending));
            return this.retrieveMultiple(query).Entities.ToList();
        }
        public Entity getInvoiceByReservationId(Guid reservationid)
        {
            QueryExpression query = new QueryExpression("rnt_invoice");
            query.ColumnSet = new ColumnSet(true);
            query.Criteria.AddCondition("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active);
            query.Criteria.AddCondition("rnt_reservationid", ConditionOperator.Equal, reservationid);
            FilterExpression filter = new FilterExpression(LogicalOperator.Or);
            filter.AddCondition("statuscode", ConditionOperator.Equal, (int)ClassLibrary._Enums_1033.rnt_invoice_StatusCode.Draft);
            filter.AddCondition("statuscode", ConditionOperator.Equal, (int)ClassLibrary._Enums_1033.rnt_invoice_StatusCode.IntegrationError);
            query.Criteria.AddFilter(filter);
            return this.retrieveMultiple(query).Entities.FirstOrDefault();
        }
        public Entity getInvoiceByLogoInvoiceNumber(string logoInvoiceNumber)
        {
            QueryExpression query = new QueryExpression("rnt_invoice");
            query.ColumnSet = new ColumnSet(true);
            query.Criteria.AddCondition("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active);
            query.Criteria.AddCondition("rnt_logoinvoicenumber", ConditionOperator.Equal, logoInvoiceNumber);
            return this.retrieveMultiple(query).Entities.FirstOrDefault();
        }

        public List<Entity> getDraftInvoicesCompletedContracts()
        {
            var fetch = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                          <entity name='rnt_invoice'>
                            <attribute name='rnt_invoiceid' />   
                            <filter type='and'>
                              <condition attribute='statuscode' operator='eq' value='1' />
                              <condition attribute='rnt_totalamount' operator='gt' value='0' />
                            </filter>   
                            <link-entity name='rnt_contract' from='rnt_contractid' to='rnt_contractid' link-type='inner' alias='a_cea99d67c7a8e911a840000d3a2ddc03'>      
                              <attribute name='statuscode' />
                              <filter type='and'>
                                <condition attribute='statuscode' operator='eq' value='100000001' />
                              </filter>
                            </link-entity>
                          </entity>
                        </fetch>";
            return this.retrieveMultiple(new FetchExpression(fetch)).Entities.ToList();

        }
    }
}
