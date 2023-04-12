using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using RntCar.BusinessLibrary.Handlers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RntCar.BusinessLibrary.Repository
{
    public class PaymentPlanRepository : RepositoryHandler
    {
        public PaymentPlanRepository(IOrganizationService Service) : base(Service)
        {
        }

        public PaymentPlanRepository(IOrganizationService Service, Guid UserId) : base(Service, UserId)
        {
        }

        public PaymentPlanRepository(IOrganizationService Service, Guid UserId, string OrganizationName) : base(Service, UserId, OrganizationName)
        {
        }

        public List<Entity> getPaymentPlansByReservationId(Guid reservationId)
        {
            QueryExpression queryExpression = new QueryExpression("rnt_documentpaymentplan");
            queryExpression.ColumnSet = new ColumnSet(true);
            queryExpression.Criteria.AddCondition("rnt_reservation", ConditionOperator.Equal, reservationId);

            return this.retrieveMultiple(queryExpression).Entities.ToList();
        }
        public List<Entity> getPaymentPlansByContractId(Guid contractId)
        {
            QueryExpression queryExpression = new QueryExpression("rnt_documentpaymentplan");
            queryExpression.ColumnSet = new ColumnSet(true);
            queryExpression.Criteria.AddCondition("rnt_contractid", ConditionOperator.Equal, contractId);

            return this.retrieveMultiple(queryExpression).Entities.ToList();
        }
        public Entity getFirstPaymentPlanByContractId(Guid contractId)
        {
            QueryExpression queryExpression = new QueryExpression("rnt_documentpaymentplan");
            queryExpression.ColumnSet = new ColumnSet(true);
            queryExpression.Criteria.AddCondition("rnt_contractid", ConditionOperator.Equal, contractId);

            var entities = this.retrieveMultiple(queryExpression).Entities.ToList();

            return entities.OrderBy(p => p.GetAttributeValue<OptionSetValue>("rnt_month").Value).FirstOrDefault();
        }
        public Entity getLastPaymentPlanByContractId(Guid contractId)
        {
            QueryExpression queryExpression = new QueryExpression("rnt_documentpaymentplan");
            queryExpression.ColumnSet = new ColumnSet(true);
            queryExpression.Criteria.AddCondition("rnt_contractid", ConditionOperator.Equal, contractId);

            var entities = this.retrieveMultiple(queryExpression).Entities.ToList();

            return entities.OrderByDescending(p => p.GetAttributeValue<OptionSetValue>("rnt_month").Value).FirstOrDefault();
        }

        public Entity getPaymentPlans(Guid contractId, int monthValue)
        {
            QueryExpression documentPaymentPlanQuery = new QueryExpression("rnt_documentpaymentplan");
            documentPaymentPlanQuery.ColumnSet = new ColumnSet("rnt_amount");
            documentPaymentPlanQuery.Criteria.AddCondition("rnt_contractid", ConditionOperator.Equal, contractId);
            documentPaymentPlanQuery.Criteria.AddCondition("rnt_month", ConditionOperator.Equal, monthValue);
            return this.retrieveMultiple(documentPaymentPlanQuery).Entities.FirstOrDefault();
        }
    }
}
