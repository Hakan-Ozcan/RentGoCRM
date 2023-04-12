using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using RntCar.BusinessLibrary.Handlers;
using RntCar.ClassLibrary;
using RntCar.SDK.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RntCar.BusinessLibrary.Repository
{
    public class ContractInvoiceDateRepository : RepositoryHandler
    {
        public ContractInvoiceDateRepository(IOrganizationService Service) : base(Service)
        {
        }

        public ContractInvoiceDateRepository(IOrganizationService Service, Guid UserId) : base(Service, UserId)
        {
        }
        public List<Entity> getContractALLInvoiceDates(string contractId)
        {
            QueryExpression queryExpression = new QueryExpression("rnt_contractinvoicedate");
            queryExpression.ColumnSet = new ColumnSet(true);
            queryExpression.Criteria.AddCondition("rnt_contractid", ConditionOperator.Equal, contractId);
            queryExpression.Criteria.AddCondition("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active);

            return this.retrieveMultiple(queryExpression).Entities.ToList();
        }
        public List<Entity> getContractMonthlyInvoiceDates(string contractId)
        {
            QueryExpression queryExpression = new QueryExpression("rnt_contractinvoicedate");
            queryExpression.ColumnSet = new ColumnSet(true);
            queryExpression.Criteria.AddCondition("rnt_contractid", ConditionOperator.Equal, contractId);
            queryExpression.Criteria.AddCondition("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active);
            queryExpression.Criteria.AddCondition("rnt_extensiontypecode", ConditionOperator.Equal, 1);
            return this.retrieveMultiple(queryExpression).Entities.ToList();
        }
        public List<Entity> getChargedContractInvoicesByContractId(string contractId)
        {
            QueryExpression queryExpression = new QueryExpression("rnt_contractinvoicedate");
            queryExpression.ColumnSet = new ColumnSet(true);
            queryExpression.Criteria.AddCondition("rnt_invoicedate", ConditionOperator.LessEqual, DateTime.UtcNow.AddMinutes(StaticHelper.offset));
            queryExpression.Criteria.AddCondition("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active);
            queryExpression.Criteria.AddCondition("rnt_ischarged", ConditionOperator.Equal, true);
            queryExpression.Criteria.AddCondition("rnt_contractid", ConditionOperator.Equal, contractId);

            return this.retrieveMultiple(queryExpression).Entities.ToList();
        }
        public Entity getLastInvoiceofMonth_Monthly(string contractId)
        {
            QueryExpression queryExpression = new QueryExpression("rnt_contractinvoicedate");
            queryExpression.ColumnSet = new ColumnSet(true);
            queryExpression.Criteria.AddCondition("rnt_contractid", ConditionOperator.Equal, contractId);
            queryExpression.Criteria.AddCondition("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active);
            //queryExpression.Criteria.AddCondition("rnt_extensiontypecode", ConditionOperator.Equal, 1);

            var entities = this.retrieveMultiple(queryExpression).Entities.ToList();
            return entities.OrderByDescending(t => t.GetAttributeValue<DateTime>("rnt_dropoffdatetime")).FirstOrDefault();

        }
        public Entity getLastInvoiceofMonth_notMonthly(string contractId, DateTime relatedDate)
        {
            QueryExpression queryExpression = new QueryExpression("rnt_contractinvoicedate");
            queryExpression.ColumnSet = new ColumnSet(true);
            queryExpression.Criteria.AddCondition("rnt_contractid", ConditionOperator.Equal, contractId);
            queryExpression.Criteria.AddCondition("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active);
            queryExpression.Criteria.AddCondition("rnt_extensiontypecode", ConditionOperator.Equal, 2);
            queryExpression.Criteria.AddCondition("rnt_invoicedate", ConditionOperator.Equal, StaticHelper.GetLastDayOfMonth(relatedDate));
            return this.retrieveMultiple(queryExpression).Entities.FirstOrDefault();
        }
        public Entity getLastInvoiceofMonth_notMonthly_notCharged(string contractId)
        {
            QueryExpression queryExpression = new QueryExpression("rnt_contractinvoicedate");
            queryExpression.ColumnSet = new ColumnSet(true);
            queryExpression.Criteria.AddCondition("rnt_contractid", ConditionOperator.Equal, contractId);
            queryExpression.Criteria.AddCondition("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active);
            queryExpression.Criteria.AddCondition("rnt_extensiontypecode", ConditionOperator.Equal, 2);
            queryExpression.Criteria.AddCondition("rnt_ischarged", ConditionOperator.Equal, false);
            return this.retrieveMultiple(queryExpression).Entities.FirstOrDefault();
        }
        public List<Entity> getAllContractInvoicesExceptItSelf(string contractId, string contractinvoicedateid)
        {
            QueryExpression queryExpression = new QueryExpression("rnt_contractinvoicedate");
            queryExpression.ColumnSet = new ColumnSet(true);
            queryExpression.Criteria.AddCondition("rnt_contractid", ConditionOperator.Equal, contractId);
            queryExpression.Criteria.AddCondition("rnt_contractinvoicedateid", ConditionOperator.NotEqual, contractinvoicedateid);
            queryExpression.Criteria.AddCondition("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active);

            return this.retrieveMultiple(queryExpression).Entities.ToList();
        }
        public List<Entity> getTodayContractInvoices()
        {
            QueryExpression queryExpression = new QueryExpression("rnt_contractinvoicedate");
            queryExpression.ColumnSet = new ColumnSet(true);
            queryExpression.Criteria.AddCondition("rnt_invoicedate", ConditionOperator.LessEqual, DateTime.UtcNow.AddMinutes(StaticHelper.offset));
            queryExpression.Criteria.AddCondition("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active);
            queryExpression.Criteria.AddCondition("rnt_ischarged", ConditionOperator.Equal, false);
            return this.retrieveMultiple(queryExpression).Entities.ToList();
        }
        public List<Entity> getContratcInvoicesLessThanGivenDay(int days)
        {
            QueryExpression queryExpression = new QueryExpression("rnt_contractinvoicedate");
            queryExpression.ColumnSet = new ColumnSet(true);
            queryExpression.Criteria.AddCondition("rnt_invoicedate", ConditionOperator.OnOrBefore, DateTime.UtcNow.Date.AddDays(-days));
            queryExpression.Criteria.AddCondition("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active);
            queryExpression.Criteria.AddCondition("rnt_ischarged", ConditionOperator.Equal, false);
            queryExpression.AddOrder("rnt_amount", OrderType.Ascending);
            return this.retrieveMultiple(queryExpression).Entities.ToList();
        }
        public List<Entity> getContratcInvoices(Guid contractId)
        {
            QueryExpression queryExpression = new QueryExpression("rnt_contractinvoicedate");
            queryExpression.ColumnSet = new ColumnSet(true);
            //ed7d2893-092b-eb11-a813-000d3a38a128
            queryExpression.Criteria.AddCondition("rnt_contractid", ConditionOperator.Equal, contractId);
            queryExpression.Criteria.AddCondition("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active);
            queryExpression.Criteria.AddCondition("rnt_ischarged", ConditionOperator.Equal, true);
            return this.retrieveMultiple(queryExpression).Entities.ToList();
        }
        public List<Entity> getContractInvoicesByGivenDates(DateTime startDate, DateTime endDate)
        {
            var str = string.Format(@"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                          <entity name='rnt_contractinvoicedate'>
                            <attribute name='rnt_contractinvoicedateid' />
                            <attribute name='rnt_pickupdatetime' />
                            <attribute name='rnt_dropoffdatetime' />
                            <attribute name='rnt_contractid' />
                            <attribute name='rnt_name' />                         
                            <attribute name='rnt_amount' />
                            <filter type='and'>
                              <condition attribute='rnt_dropoffdatetime' operator='on-or-before' value='{1}' />
                              <condition attribute='rnt_ischarged' operator='eq' value='1' />
                              <condition attribute='rnt_dropoffdatetime' operator='on-or-after' value='{0}' />
                            </filter>
                            <link-entity name='rnt_contract' from='rnt_contractid' to='rnt_contractid' link-type='inner' alias='af'>
                                    <attribute name='rnt_pickupbranchid' />
                              <filter type='and'>
                                <condition attribute='statuscode' operator='eq' value='100000000' />
                              </filter>
                            </link-entity>
                          </entity>
                        </fetch>", startDate.ToString("yyyy-MM-dd"), endDate.ToString("yyyy-MM-dd"));
            return this.retrieveMultiple(new FetchExpression(str)).Entities.ToList();
        }

        public List<Entity> getContractInvoicesDateByDropoffDate(DateTime startDate, DateTime endDate) 
        {
          var fetchXml = "<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>" +
                          "<entity name='rnt_contractinvoicedate'>" +
                            "<attribute name='rnt_contractinvoicedateid' />" +
                            "<attribute name='rnt_name' />" +
                            "<attribute name='createdon' />" +
                            "<attribute name='rnt_pickupdatetime' />" +
                            "<attribute name='rnt_ischarged' />" +
                            "<attribute name='rnt_dropoffdatetime' />" +
                            "<attribute name='rnt_contractid' />" +
                            "<order attribute='rnt_name' descending='false' />" +
                            "<filter type='and'>" +
                              "<condition attribute='rnt_dropoffdatetime' operator='on-or-after' value='" + startDate + "' />" +
                              "<condition attribute='rnt_dropoffdatetime' operator='on-or-before' value='" + endDate + "' />" +
                              "<condition attribute='statecode' operator='eq' value='0' />" +
                            "</filter>" +
                          "</entity>" +
                        "</fetch>";

            return this.retrieveMultiple(new FetchExpression(fetchXml)).Entities.ToList();

        }
    }
}
