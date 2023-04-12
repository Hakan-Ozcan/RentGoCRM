using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Tooling.Connector;
using RntCar.BusinessLibrary.Handlers;
using RntCar.ClassLibrary;
using RntCar.SDK.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.BusinessLibrary.Repository
{
    public class CreditCardRepository : RepositoryHandler
    {
        public CreditCardRepository(IOrganizationService Service) : base(Service)
        {
        }

        public CreditCardRepository(CrmServiceClient _crmServiceClient) : base(_crmServiceClient)
        {
        }

        public CreditCardRepository(IOrganizationService Service, Guid UserId) : base(Service, UserId)
        {
        }

        public CreditCardRepository(IOrganizationService Service, CrmServiceClient _crmServiceClient) : base(Service, _crmServiceClient)
        {
        }

        public CreditCardRepository(IOrganizationService Service, Guid UserId, string OrganizationName) : base(Service, UserId, OrganizationName)
        {
        }

        public Entity getCreditCardByIdWithGivenColumns(Guid createCreditCardId, string[] columns)
        {
            return this.retrieveById("rnt_customercreditcard", createCreditCardId, columns);
        }
        public Entity getCreditCardById(Guid createCreditCardId)
        {
            return this.retrieveById("rnt_customercreditcard", createCreditCardId, true);
        }
        public List<Entity> getCreditCardsByCustomerIdWithGivenColumns(Guid customerId, int? provider, string[] columns)
        {
            QueryExpression queryExpression = new QueryExpression("rnt_customercreditcard");
            queryExpression.ColumnSet = new ColumnSet(columns);
            queryExpression.Criteria.AddCondition("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active);
            queryExpression.Criteria.AddCondition("rnt_contactid", ConditionOperator.Equal, customerId);
            if (provider.HasValue)
            {
                queryExpression.Criteria.AddCondition("rnt_provider", ConditionOperator.Equal, provider.Value);
            }
            return this.retrieveMultiple(queryExpression).Entities.ToList();
        }
        public Entity getCreditCardByUserKeyandCreditCardTokenByGivenColumns(string cardUserKey, string cardToken, string[] columns)
        {
            QueryExpression queryExpression = new QueryExpression("rnt_customercreditcard");
            queryExpression.ColumnSet = new ColumnSet(columns);
            queryExpression.Criteria.AddCondition("rnt_carduserkey", ConditionOperator.Equal, cardUserKey);
            queryExpression.Criteria.AddCondition("rnt_cardtoken", ConditionOperator.Equal, cardToken);
            return this.retrieveMultiple(queryExpression).Entities.FirstOrDefault();
        }
        public Entity getCreditCardByUserKeyandCreditCardTokenByGivenColumns(string cardUserKey, string cardToken)
        {
            QueryExpression queryExpression = new QueryExpression("rnt_customercreditcard");
            queryExpression.ColumnSet = new ColumnSet(true);
            queryExpression.Criteria.AddCondition("rnt_carduserkey", ConditionOperator.Equal, cardUserKey);
            queryExpression.Criteria.AddCondition("rnt_cardtoken", ConditionOperator.Equal, cardToken);
            return this.retrieveMultiple(queryExpression).Entities.FirstOrDefault();
        }
        public string getCreditCardTokenById(Guid creditCardId)
        {
            var creditCard = this.retrieveById("rnt_customercreditcard", creditCardId, new string[] { "rnt_cardtoken" });
            return creditCard.GetAttributeValue<string>("rnt_cardtoken");
        }
        public Entity getCreditCardByUserKey(string cardUserKey)
        {
            QueryExpression queryExpression = new QueryExpression("rnt_customercreditcard");
            queryExpression.ColumnSet = new ColumnSet(true);
            queryExpression.Criteria.AddCondition("rnt_carduserkey", ConditionOperator.Equal, cardUserKey);
            return this.retrieveMultiple(queryExpression).Entities.FirstOrDefault();
        }
        public List<Entity> getCreditCardsByCustomerId(Guid customerId, string[] columns)
        {
            QueryExpression queryExpression = new QueryExpression("rnt_customercreditcard");
            queryExpression.ColumnSet = new ColumnSet(columns);
            queryExpression.Criteria.AddCondition("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active);
            queryExpression.Criteria.AddCondition("rnt_expireyear", ConditionOperator.GreaterEqual, DateTime.Now.Year);
            queryExpression.Criteria.AddCondition("rnt_contactid", ConditionOperator.Equal, customerId);
            queryExpression.Criteria.AddCondition("rnt_ishidden", ConditionOperator.NotEqual, (int)GlobalEnums.CreditCardHidden.Yes);

            return this.retrieveMultiple(queryExpression).Entities.ToList();
        }
        public List<Entity> getCreditCardsByCustomerId(Guid customerId)
        {
            QueryExpression queryExpression = new QueryExpression("rnt_customercreditcard");
            queryExpression.ColumnSet = new ColumnSet(true);
            queryExpression.Criteria.AddCondition("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active);
            queryExpression.Criteria.AddCondition("rnt_contactid", ConditionOperator.Equal, customerId);
            return this.retrieveMultiple(queryExpression).Entities.ToList();
        }
        public Entity getCustomerCreditCardsByGivenParametersByGivenColumns(Guid customerId, string creditCardNumber, int year, int month, string[] columns)
        {
            QueryExpression queryExpression = new QueryExpression("rnt_customercreditcard");
            queryExpression.ColumnSet = new ColumnSet(columns);
            queryExpression.Criteria.AddCondition("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active);
            queryExpression.Criteria.AddCondition("rnt_contactid", ConditionOperator.Equal, customerId);
            queryExpression.Criteria.AddCondition("rnt_creditcardnumber", ConditionOperator.Equal, creditCardNumber);
            queryExpression.Criteria.AddCondition("rnt_expiremonthcode", ConditionOperator.Equal, month);
            queryExpression.Criteria.AddCondition("rnt_expireyear", ConditionOperator.Equal, year);
            return this.retrieveMultiple(queryExpression).Entities.FirstOrDefault();
        }

        public EntityCollection getCustomerCreditCardsListByGivenParametersByGivenColumns(Guid customerId, string creditCardNumber, int year, int month, string[] columns)
        {
            QueryExpression queryExpression = new QueryExpression("rnt_customercreditcard");
            queryExpression.ColumnSet = new ColumnSet(columns);
            queryExpression.Criteria.AddCondition("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active);
            queryExpression.Criteria.AddCondition("rnt_contactid", ConditionOperator.Equal, customerId);
            queryExpression.Criteria.AddCondition("rnt_creditcardnumber", ConditionOperator.Equal, creditCardNumber);
            queryExpression.Criteria.AddCondition("rnt_expiremonthcode", ConditionOperator.Equal, month);
            queryExpression.Criteria.AddCondition("rnt_expireyear", ConditionOperator.Equal, year);
            return this.retrieveMultiple(queryExpression);
        }

        public EntityCollection getCreditCardListByUserKeyandCreditCardTokenByGivenColumns(string cardUserKey, string cardToken, string[] columns)
        {
            QueryExpression queryExpression = new QueryExpression("rnt_customercreditcard");
            queryExpression.ColumnSet = new ColumnSet(columns);
            queryExpression.Criteria.AddCondition("rnt_carduserkey", ConditionOperator.Equal, cardUserKey);
            queryExpression.Criteria.AddCondition("rnt_cardtoken", ConditionOperator.Equal, cardToken);
            return this.retrieveMultiple(queryExpression);
        }
        public List<Entity> getCreditCardsByCustomerIdAndProviderWithGivenColumns(Guid customerId, int provider, string[] columns)
        {
            QueryExpression queryExpression = new QueryExpression("rnt_customercreditcard");
            queryExpression.ColumnSet = new ColumnSet(columns);
            queryExpression.Criteria.AddCondition("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active);
            queryExpression.Criteria.AddCondition("rnt_contactid", ConditionOperator.Equal, customerId);
            queryExpression.Criteria.AddCondition("rnt_provider", ConditionOperator.Equal, provider);
            return this.retrieveMultiple(queryExpression).Entities.ToList();
        }
        public void setStateCreditCard(Guid creditCardId, string exceptionCode)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(exceptionCode))
                {
                    Dictionary<string, int[]> exceptionMapList = mapToIyzicoToCreditCard();
                    int[] stateList = exceptionMapList.Where(x => x.Key == exceptionCode).Select(x => x.Value).FirstOrDefault();
                    //if (stateList == null || stateList.Count() == 0)
                    //{
                    //    stateList = exceptionMapList.Where(x => x.Key == "Default").Select(x => x.Value).FirstOrDefault();
                    //}
                    if (stateList != null)
                    {
                        SetStateRequest setStateRequest = new SetStateRequest()
                        {
                            EntityMoniker = new EntityReference("rnt_customercreditcard", creditCardId),
                            State = new OptionSetValue(stateList[0]),
                            Status = new OptionSetValue(stateList[1])
                        };
                        this.Service.Execute(setStateRequest);
                    }
                }
            }
            catch (Exception ex)
            {
            }
        }

        public Dictionary<string, int[]> mapToIyzicoToCreditCard()
        {
            ConfigurationRepository configurationRepository = new ConfigurationRepository(this.Service);
            string iyzicoExceptionCodeList = configurationRepository.GetConfigurationByKey("iyzicoCreditCardMap");

            string[] exceptionCodeRow = iyzicoExceptionCodeList.Split(';');
            Dictionary<string, int[]> exceptionMapList = new Dictionary<string, int[]>();
            foreach (var exceptionCode in exceptionCodeRow)
            {
                string[] temp = exceptionCode.Split(',');
                int[] tempState = new int[2];
                tempState[0] = Convert.ToInt32(temp[1]);
                tempState[1] = Convert.ToInt32(temp[2]);
                exceptionMapList.Add(temp[0], tempState);
            }
            return exceptionMapList;
        }
    }
}
