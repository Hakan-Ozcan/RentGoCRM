using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
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
    public class InvoiceAddressRepository : RepositoryHandler
    {
        public InvoiceAddressRepository(IOrganizationService Service) : base(Service)
        {
        }

        public InvoiceAddressRepository(IOrganizationService Service, Guid UserId) : base(Service, UserId)
        {
        }

        public InvoiceAddressRepository(IOrganizationService Service, Guid UserId, string OrganizationName) : base(Service, UserId, OrganizationName)
        {
        }
        public List<Entity> getFirstInvoiceAddressByCustomerIdByGivenColumns(Guid individualCustomerId)
        {
            QueryExpression expression = new QueryExpression("rnt_invoiceaddress");
            expression.ColumnSet = new ColumnSet(true);
            expression.Criteria.AddCondition("rnt_contactid", ConditionOperator.Equal, individualCustomerId);
            expression.Criteria.AddCondition("rnt_invoicetypecode", ConditionOperator.Equal, (int)rnt_invoice_rnt_invoicetypecode.Individual);
            expression.Criteria.AddCondition("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active);
            return this.retrieveMultiple(expression).Entities.ToList();
        }
        public List<Entity> getIndividualInvoiceAddressByCustomerIdByGivenColumns(Guid individualCustomerId)
        {
            QueryExpression expression = new QueryExpression("rnt_invoiceaddress");
            expression.ColumnSet = new ColumnSet(true);
            expression.Criteria.AddCondition("rnt_contactid", ConditionOperator.Equal, individualCustomerId);
            expression.Criteria.AddCondition("rnt_invoicetypecode", ConditionOperator.Equal, (int)rnt_invoice_rnt_invoicetypecode.Individual);
            expression.Criteria.AddCondition("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active);
            return this.retrieveMultiple(expression).Entities.ToList();
        }
        public List<Entity> getInvoiceAddressByCustomerIdByGivenColumns(Guid individualCustomerId, string[] columns)
        {
            QueryExpression expression = new QueryExpression("rnt_invoiceaddress");
            expression.ColumnSet = new ColumnSet(columns);
            expression.Criteria.AddCondition("rnt_contactid", ConditionOperator.Equal, individualCustomerId);
            expression.Criteria.AddCondition("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active);
            return this.retrieveMultiple(expression).Entities.ToList();
        }
        public List<Entity> getInvoiceAddressByCustomerIdByGivenColumns(Guid individualCustomerId)
        {
            QueryExpression expression = new QueryExpression("rnt_invoiceaddress");
            expression.ColumnSet = new ColumnSet(true);
            expression.Criteria.AddCondition("rnt_contactid", ConditionOperator.Equal, individualCustomerId);
            expression.Criteria.AddCondition("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active);
            return this.retrieveMultiple(expression).Entities.ToList();
        }
        public Entity getInvoiceAddressById(Guid invoiceAddressId)
        {
            QueryExpression expression = new QueryExpression("rnt_invoiceaddress");
            expression.ColumnSet = new ColumnSet(true);
            expression.Criteria.AddCondition("rnt_invoiceaddressid", ConditionOperator.Equal, invoiceAddressId);
            expression.Criteria.AddCondition("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active);
            return this.retrieveMultiple(expression).Entities.FirstOrDefault();
        }
        public List<Entity> getInvoiceAddressByGovermentIdOrByTaxNumber(string key)
        {
            QueryExpression expression = new QueryExpression("rnt_invoiceaddress");
            expression.ColumnSet = new ColumnSet(true);
            expression.Criteria.AddCondition("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active);
            FilterExpression idCondition = new FilterExpression(LogicalOperator.Or);
            idCondition.AddCondition("rnt_government", ConditionOperator.Equal, key);
            idCondition.AddCondition("rnt_taxnumber", ConditionOperator.Equal, key);
            expression.Criteria.AddFilter(idCondition);

            return this.retrieveMultiple(expression).Entities.ToList();
        }
    }
}
