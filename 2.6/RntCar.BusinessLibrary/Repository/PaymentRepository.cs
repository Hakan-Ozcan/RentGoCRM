using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using RntCar.BusinessLibrary.Handlers;
using RntCar.ClassLibrary;
using RntCar.ClassLibrary._Enums_1033;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RntCar.BusinessLibrary.Repository
{
    public class PaymentRepository : RepositoryHandler
    {
        public PaymentRepository(IOrganizationService Service) : base(Service)
        {
        }

        public PaymentRepository(IOrganizationService Service, Guid UserId) : base(Service, UserId)
        {
        }

        public PaymentRepository(IOrganizationService Service, Guid UserId, string OrganizationName) : base(Service, UserId, OrganizationName)
        {
        }

        public int getCountofNotRefundedPayments_Reservation(Guid reservationId)
        {
            //get the payment not totally refunded and int success status
            var fetchXml = string.Format(@"<fetch distinct='false' mapping='logical' aggregate='true'>   
                           <entity name='rnt_payment'>   
                              <attribute name='rnt_paymentid' aggregate='count' alias='countofpayments'/>   
                             <filter type='and'>
                                  <condition attribute='statecode' operator='eq' value='0' />
                                  <condition attribute='rnt_transactionresult' operator='eq' value='{0}' />
                                  <condition attribute='rnt_refundstatuscode' operator='ne' value='{1}' />
                                  <condition attribute='rnt_reservationid' operator='eq' value='{2}' />
                                  <condition attribute='rnt_transactiontypecode' operator='eq' value='{3}' />
                             </filter>
                            </entity>   
                           </fetch>", (int)PaymentEnums.PaymentTransactionResult.Success,
                           (int)PaymentEnums.RefundStatus.Totally_Refund,//not equal totallyrefund
                           reservationId,
                           (int)PaymentEnums.PaymentTransactionType.SALE);

            var record = this.Service.RetrieveMultiple(new FetchExpression(fetchXml)).Entities.FirstOrDefault();
            return Convert.ToInt32(record.GetAttributeValue<AliasedValue>("countofpayments").Value);
        }

        public int getCountofNotRefundedPayments_Contract(Guid contractId)
        {
            //get the payment not totally refunded and int success status
            var fetchXml = string.Format(@"<fetch distinct='false' mapping='logical' aggregate='true'>   
                           <entity name='rnt_payment'>   
                              <attribute name='rnt_paymentid' aggregate='count' alias='countofpayments'/>   
                             <filter type='and'>
                                  <condition attribute='statecode' operator='eq' value='0' />
                                  <condition attribute='rnt_transactionresult' operator='eq' value='{0}' />
                                  <condition attribute='rnt_refundstatuscode' operator='ne' value='{1}' />
                                  <condition attribute='rnt_contractid' operator='eq' value='{2}' />
                                  <condition attribute='rnt_transactiontypecode' operator='eq' value='{3}' />
                             </filter>
                            </entity>   
                           </fetch>", (int)PaymentEnums.PaymentTransactionResult.Success,
                           (int)PaymentEnums.RefundStatus.Totally_Refund,//not equal totallyrefund
                           contractId,
                           (int)PaymentEnums.PaymentTransactionType.SALE);

            var record = this.Service.RetrieveMultiple(new FetchExpression(fetchXml)).Entities.FirstOrDefault();
            return Convert.ToInt32(record.GetAttributeValue<AliasedValue>("countofpayments").Value);
        }
        public List<Entity> getNotRefundedPayments_Reservation(Guid reservationId)
        {
            QueryExpression queryExpression = new QueryExpression("rnt_payment");
            queryExpression.ColumnSet = new ColumnSet(true);
            queryExpression.Criteria.AddCondition("rnt_transactiontypecode", ConditionOperator.Equal, (int)PaymentEnums.PaymentTransactionType.SALE);
            queryExpression.Criteria.AddCondition("rnt_transactionresult", ConditionOperator.Equal, (int)PaymentEnums.PaymentTransactionResult.Success);
            queryExpression.Criteria.AddCondition("rnt_refundstatuscode", ConditionOperator.NotEqual, (int)PaymentEnums.RefundStatus.Totally_Refund);
            queryExpression.Criteria.AddCondition("rnt_reservationid", ConditionOperator.Equal, reservationId.ToString());
            queryExpression.AddOrder("createdon", OrderType.Descending);
            return this.Service.RetrieveMultiple(queryExpression).Entities.ToList();
        }
        public List<Entity> getNotRefundedPayments_Contract(Guid contractId, PaymentEnums.PaymentTransactionType paymentTransactionType)
        {

            QueryExpression queryExpression = new QueryExpression("rnt_payment");
            queryExpression.ColumnSet = new ColumnSet(true);
            queryExpression.Criteria.AddCondition("rnt_transactiontypecode", ConditionOperator.Equal, (int)paymentTransactionType);
            queryExpression.Criteria.AddCondition("rnt_transactionresult", ConditionOperator.Equal, (int)PaymentEnums.PaymentTransactionResult.Success);
            queryExpression.Criteria.AddCondition("rnt_refundstatuscode", ConditionOperator.NotEqual, (int)PaymentEnums.RefundStatus.Totally_Refund);
            queryExpression.Criteria.AddCondition("rnt_contractid", ConditionOperator.Equal, contractId.ToString());
            queryExpression.Criteria.AddCondition("rnt_transfertypecode", ConditionOperator.NotEqual, (int)rnt_payment_rnt_transfertypecode.MoneyOrder);
            queryExpression.AddOrder("createdon", OrderType.Descending);
            return this.Service.RetrieveMultiple(queryExpression).Entities.ToList();
        }
        public Entity getNotRefundedDepositPayment_Contract(Guid contractId)
        {
            QueryExpression queryExpression = new QueryExpression("rnt_payment");
            queryExpression.ColumnSet = new ColumnSet(true);
            queryExpression.Criteria.AddCondition("rnt_transactiontypecode", ConditionOperator.Equal, (int)PaymentEnums.PaymentTransactionType.DEPOSIT);
            queryExpression.Criteria.AddCondition("rnt_transactionresult", ConditionOperator.Equal, (int)PaymentEnums.PaymentTransactionResult.Success);
            queryExpression.Criteria.AddCondition("rnt_refundstatuscode", ConditionOperator.NotEqual, (int)PaymentEnums.RefundStatus.Totally_Refund);
            queryExpression.Criteria.AddCondition("rnt_contractid", ConditionOperator.Equal, contractId.ToString());
            queryExpression.AddOrder("createdon", OrderType.Descending);
            return this.Service.RetrieveMultiple(queryExpression).Entities.FirstOrDefault();
        }
        public List<Entity> getNotRefundedSalePayment_Reservation(Guid reservationId)
        {
            QueryExpression queryExpression = new QueryExpression("rnt_payment");
            queryExpression.ColumnSet = new ColumnSet(true);
            queryExpression.Criteria.AddCondition("rnt_transactiontypecode", ConditionOperator.Equal, (int)PaymentEnums.PaymentTransactionType.SALE);
            queryExpression.Criteria.AddCondition("rnt_transactionresult", ConditionOperator.Equal, (int)PaymentEnums.PaymentTransactionResult.Success);
            queryExpression.Criteria.AddCondition("rnt_refundstatuscode", ConditionOperator.NotEqual, (int)PaymentEnums.RefundStatus.Totally_Refund);
            queryExpression.Criteria.AddCondition("rnt_reservationid", ConditionOperator.Equal, reservationId.ToString());
            queryExpression.AddOrder("createdon", OrderType.Descending);
            return this.Service.RetrieveMultiple(queryExpression).Entities.ToList();
        }
        public List<Entity> getNotRefundedSalePayment_Contract(Guid contractId)
        {
            QueryExpression queryExpression = new QueryExpression("rnt_payment");
            queryExpression.ColumnSet = new ColumnSet(true);
            queryExpression.Criteria.AddCondition("rnt_transactiontypecode", ConditionOperator.Equal, (int)PaymentEnums.PaymentTransactionType.SALE);
            queryExpression.Criteria.AddCondition("rnt_transactionresult", ConditionOperator.Equal, (int)PaymentEnums.PaymentTransactionResult.Success);
            queryExpression.Criteria.AddCondition("rnt_refundstatuscode", ConditionOperator.NotEqual, (int)PaymentEnums.RefundStatus.Totally_Refund);
            queryExpression.Criteria.AddCondition("rnt_contractid", ConditionOperator.Equal, contractId.ToString());
            queryExpression.AddOrder("createdon", OrderType.Descending);
            return this.Service.RetrieveMultiple(queryExpression).Entities.ToList();
        }
        public List<Entity> getAllPaymentandRefund_Reservation(Guid reservationId)
        {
            QueryExpression queryExpression = new QueryExpression("rnt_payment");
            queryExpression.ColumnSet = new ColumnSet(true);
            queryExpression.Criteria.AddCondition("rnt_transactionresult", ConditionOperator.Equal, (int)PaymentEnums.PaymentTransactionResult.Success);
            queryExpression.Criteria.AddCondition("rnt_reservationid", ConditionOperator.Equal, reservationId.ToString());
            queryExpression.AddOrder("createdon", OrderType.Descending);
            return this.Service.RetrieveMultiple(queryExpression).Entities.ToList();
        }
        public Entity getLastPayment_Reservation(Guid reservationId)
        {
            QueryExpression queryExpression = new QueryExpression("rnt_payment");
            queryExpression.ColumnSet = new ColumnSet(true);
            queryExpression.Criteria.AddCondition("rnt_transactiontypecode", ConditionOperator.Equal, (int)PaymentEnums.PaymentTransactionType.SALE);
            queryExpression.Criteria.AddCondition("rnt_transactionresult", ConditionOperator.Equal, (int)PaymentEnums.PaymentTransactionResult.Success);
            queryExpression.Criteria.AddCondition("rnt_reservationid", ConditionOperator.Equal, reservationId.ToString());
            queryExpression.AddOrder("createdon", OrderType.Descending);
            return this.Service.RetrieveMultiple(queryExpression).Entities.FirstOrDefault();
        }
        public Entity getLastPayment_Reservation_CreditCardIsNotEmpty(Guid reservationId)
        {
            QueryExpression queryExpression = new QueryExpression("rnt_payment");
            queryExpression.ColumnSet = new ColumnSet(true);
            queryExpression.Criteria.AddCondition("rnt_transactiontypecode", ConditionOperator.Equal, (int)PaymentEnums.PaymentTransactionType.SALE);
            queryExpression.Criteria.AddCondition("rnt_transactionresult", ConditionOperator.Equal, (int)PaymentEnums.PaymentTransactionResult.Success);
            queryExpression.Criteria.AddCondition("rnt_reservationid", ConditionOperator.Equal, reservationId.ToString());
            queryExpression.Criteria.AddCondition("rnt_customercreditcardid", ConditionOperator.NotNull);
            queryExpression.AddOrder("createdon", OrderType.Descending);
            return this.Service.RetrieveMultiple(queryExpression).Entities.FirstOrDefault();
        }
        public Entity getLastPayment_Contract(Guid contractId)
        {
            QueryExpression queryExpression = new QueryExpression("rnt_payment");
            queryExpression.ColumnSet = new ColumnSet(true);
            queryExpression.Criteria.AddCondition("rnt_transactiontypecode", ConditionOperator.Equal, (int)PaymentEnums.PaymentTransactionType.SALE);
            queryExpression.Criteria.AddCondition("rnt_transactionresult", ConditionOperator.Equal, (int)PaymentEnums.PaymentTransactionResult.Success);
            queryExpression.Criteria.AddCondition("rnt_contractid", ConditionOperator.Equal, contractId.ToString());
            queryExpression.Criteria.AddCondition("rnt_customercreditcardid", ConditionOperator.NotNull);
            queryExpression.AddOrder("createdon", OrderType.Descending);
            return this.Service.RetrieveMultiple(queryExpression).Entities.FirstOrDefault();
        }
        public Entity getLastPayment_Contract_CreditCardIsNotEmpty(Guid contractId)
        {
            QueryExpression queryExpression = new QueryExpression("rnt_payment");
            queryExpression.ColumnSet = new ColumnSet(true);
            queryExpression.Criteria.AddCondition("rnt_transactiontypecode", ConditionOperator.Equal, (int)PaymentEnums.PaymentTransactionType.SALE);
            queryExpression.Criteria.AddCondition("rnt_transactionresult", ConditionOperator.Equal, (int)PaymentEnums.PaymentTransactionResult.Success);
            queryExpression.Criteria.AddCondition("rnt_contractid", ConditionOperator.Equal, contractId.ToString());
            queryExpression.Criteria.AddCondition("rnt_customercreditcardid", ConditionOperator.NotNull);
            queryExpression.AddOrder("createdon", OrderType.Descending);
            return this.Service.RetrieveMultiple(queryExpression).Entities.FirstOrDefault();
        }
        public Entity getDeposit_Contract(Guid contractId)
        {
            QueryExpression queryExpression = new QueryExpression("rnt_payment");
            queryExpression.ColumnSet = new ColumnSet(true);
            queryExpression.Criteria.AddCondition("rnt_transactiontypecode", ConditionOperator.Equal, (int)PaymentEnums.PaymentTransactionType.DEPOSIT);
            queryExpression.Criteria.AddCondition("rnt_transactionresult", ConditionOperator.Equal, (int)PaymentEnums.PaymentTransactionResult.Success);
            queryExpression.Criteria.AddCondition("rnt_contractid", ConditionOperator.Equal, contractId.ToString());
            queryExpression.AddOrder("createdon", OrderType.Descending);
            return this.Service.RetrieveMultiple(queryExpression).Entities.FirstOrDefault();
        }
        public Entity getDepositRefund(Guid? reservationId, Guid? contractId, string[] columns)
        {
            QueryExpression queryExpression = new QueryExpression("rnt_payment");
            queryExpression.ColumnSet = new ColumnSet(columns);

            queryExpression.Criteria.AddCondition("rnt_transactionresult", ConditionOperator.Equal, (int)rnt_payment_rnt_transactionresult.Success);
            if (contractId.HasValue)
            {
                queryExpression.Criteria.AddCondition("rnt_contractid", ConditionOperator.Equal, contractId.Value.ToString());
            }
            else if (reservationId.HasValue)
            {
                queryExpression.Criteria.AddCondition("rnt_reservationid", ConditionOperator.Equal, reservationId.Value.ToString());
            }
            queryExpression.Criteria.AddCondition("rnt_transactiontypecode", ConditionOperator.Equal, (int)rnt_TransactionTypeCode.Refund);
            queryExpression.Criteria.AddCondition("rnt_parentpaymentid", ConditionOperator.NotNull);
            queryExpression.LinkEntities.Add(new LinkEntity("rnt_payment", "rnt_payment", "rnt_parentpaymentid", "rnt_paymentid", JoinOperator.Inner));
            queryExpression.LinkEntities[0].EntityAlias = "parentPayment";
            queryExpression.LinkEntities[0].LinkCriteria.AddCondition("rnt_transactiontypecode", ConditionOperator.Equal, (int)rnt_TransactionTypeCode.Deposit);

            return this.Service.RetrieveMultiple(queryExpression).Entities.FirstOrDefault();
        }
        public List<Entity> getAllPaymentWithGivenColumns_Contract(Guid contractId, string[] columns)
        {
            QueryExpression queryExpression = new QueryExpression("rnt_payment");
            queryExpression.ColumnSet = new ColumnSet(columns);
            var mainFilter = new FilterExpression(LogicalOperator.And);

            var typeFiter = new FilterExpression(LogicalOperator.Or);
            typeFiter.AddCondition(new ConditionExpression("rnt_transactiontypecode", ConditionOperator.Equal, (int)PaymentEnums.PaymentTransactionType.SALE));
            typeFiter.AddCondition(new ConditionExpression("rnt_transactiontypecode", ConditionOperator.Equal, (int)PaymentEnums.PaymentTransactionType.DEPOSIT));

            mainFilter.AddCondition("rnt_transactionresult", ConditionOperator.Equal, (int)PaymentEnums.PaymentTransactionResult.Success);
            mainFilter.AddCondition("rnt_contractid", ConditionOperator.Equal, contractId.ToString());
            mainFilter.AddFilter(typeFiter);

            queryExpression.Criteria = mainFilter;
            return this.Service.RetrieveMultiple(queryExpression).Entities.ToList();
        }
        public Entity getPaymentById(Guid paymentId)
        {
            return this.retrieveById("rnt_payment", paymentId, true);
        }
        public Entity getPaymentByIdByGivenColumns(Guid paymentId, string[] columns)
        {
            return this.retrieveById("rnt_payment", paymentId, columns);
        }
        public List<Entity> getAllPaymentsByGivenMonth(string[] columns)
        {
            QueryExpression queryExpression = new QueryExpression("rnt_payment");
            queryExpression.ColumnSet = new ColumnSet(columns);
            queryExpression.Criteria.AddCondition("rnt_creditcardslipid", ConditionOperator.NotNull);
            return this.retrieveMultiple(queryExpression).Entities.ToList();
        }
        public List<Entity> getAllPayments(string[] columns)
        {
            QueryExpression queryExpression = new QueryExpression("rnt_payment");
            queryExpression.ColumnSet = new ColumnSet(columns);
            queryExpression.Criteria.AddCondition("rnt_creditcardslipid", ConditionOperator.NotNull);
            return this.retrieveMultiple(queryExpression).Entities.ToList();
        }
        public List<Entity> getPaymentsCreditCardSlipsNotIntegratedWithLogo()
        {
            QueryExpression queryExpression = new QueryExpression("rnt_payment");
            queryExpression.ColumnSet = new ColumnSet(true);
            queryExpression.Criteria.AddCondition("rnt_transactionresult", ConditionOperator.In, new object[] { (int)rnt_payment_rnt_transactionresult.Success, (int)rnt_payment_rnt_transactionresult.Success_WithoutPaymentTransaction });
            LinkEntity creditCardSlipLink = new LinkEntity("rnt_payment", "rnt_creditcardslip", "rnt_creditcardslipid", "rnt_creditcardslipid", JoinOperator.Inner);
            FilterExpression filter1 = new FilterExpression(LogicalOperator.And);

            filter1.Conditions.Add(new ConditionExpression("statuscode", ConditionOperator.NotIn, new object[] { (int)rnt_creditcardslip_StatusCode.IntegratedWithLogo, (int)rnt_creditcardslip_StatusCode.Draft}));
            creditCardSlipLink.Columns = new ColumnSet(true);
            creditCardSlipLink.LinkCriteria.AddFilter(filter1);

            creditCardSlipLink.EntityAlias = "creditcardSlips";

            queryExpression.LinkEntities.Add(creditCardSlipLink);

            return this.retrieveMultiple(queryExpression).Entities.ToList();
        }
    }
}
