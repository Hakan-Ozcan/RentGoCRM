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
    public class CreditCardSlipRepository : RepositoryHandler
    {
        public CreditCardSlipRepository(IOrganizationService Service) : base(Service)
        {
        }

        public CreditCardSlipRepository(CrmServiceClient _crmServiceClient) : base(_crmServiceClient)
        {
        }

        public CreditCardSlipRepository(IOrganizationService Service, CrmServiceClient _crmServiceClient) : base(Service, _crmServiceClient)
        {
        }

        public Entity getCreditCardSlipByPaymentId(Guid paymentId)
        {
            QueryExpression expression = new QueryExpression("rnt_creditcardslip");
            expression.ColumnSet = new ColumnSet(true);
            expression.Criteria.AddCondition("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active);
            expression.Criteria.AddCondition("rnt_paymentid", ConditionOperator.Equal, paymentId);
            return this.retrieveMultiple(expression).Entities.FirstOrDefault();
        }
        public Entity getIntegratedWithLogoCreditCardSlipByIdByGivenColumns(Guid creditCardSlipId, string[] columns)
        {
            QueryExpression expression = new QueryExpression("rnt_creditcardslip");
            expression.ColumnSet = new ColumnSet(columns);
            expression.Criteria.AddCondition("statuscode", ConditionOperator.Equal, (int)rnt_creditcardslip_StatusCode.IntegratedWithLogo);
            expression.Criteria.AddCondition("rnt_creditcardslipid", ConditionOperator.Equal, creditCardSlipId);
            return this.retrieveMultiple(expression).Entities.FirstOrDefault();
        }
        public Entity getCreditCardSlipById(Guid creditCardSlipId)
        {
            return this.retrieveById("rnt_creditcardslip", creditCardSlipId);
        }
        public List<Entity> getCreditCardSlipsByReservationId(Guid reservationId, string[] columns)
        {
            QueryExpression expression = new QueryExpression("rnt_creditcardslip");
            expression.ColumnSet = new ColumnSet(columns);
            expression.Criteria.AddCondition("rnt_contractid", ConditionOperator.Null);
            expression.Criteria.AddCondition("rnt_reservationid", ConditionOperator.Equal, reservationId);
            return this.retrieveMultiple(expression).Entities.ToList();
        }
        public List<Entity> getFailedIntegrationCreditCardSlips(string[] columns)
        {
            QueryExpression expression = new QueryExpression("rnt_creditcardslip");
            expression.ColumnSet = new ColumnSet(columns);

            FilterExpression filterExpression = new FilterExpression(LogicalOperator.Or);
            filterExpression.AddCondition(new ConditionExpression("statuscode", ConditionOperator.Equal, (int)rnt_creditcardslip_StatusCode.InternalError));
            filterExpression.AddCondition(new ConditionExpression("statuscode", ConditionOperator.Equal, (int)rnt_creditcardslip_StatusCode.IntegrationError));

            expression.Criteria.AddFilter(filterExpression);
            return this.retrieveMultiple(expression).Entities.ToList();
        }
        public List<Entity> getFailedIntegrationCreditCardSlips_Reservation(Guid reservationId)
        {
            QueryExpression expression = new QueryExpression("rnt_creditcardslip");
            expression.ColumnSet = new ColumnSet(true);

            FilterExpression filterExpression = new FilterExpression(LogicalOperator.Or);
            filterExpression.AddCondition(new ConditionExpression("statuscode", ConditionOperator.Equal, (int)rnt_creditcardslip_StatusCode.InternalError));
            filterExpression.AddCondition(new ConditionExpression("statuscode", ConditionOperator.Equal, (int)rnt_creditcardslip_StatusCode.IntegrationError));
            expression.Criteria.AddCondition("rnt_reservationid", ConditionOperator.Equal, reservationId);

            expression.Criteria.AddFilter(filterExpression);
            return this.retrieveMultiple(expression).Entities.ToList();
        }
    }
}
