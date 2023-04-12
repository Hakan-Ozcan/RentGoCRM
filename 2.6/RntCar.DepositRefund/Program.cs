using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Newtonsoft.Json;
using RntCar.BusinessLibrary.Business;
using RntCar.BusinessLibrary.Helpers;
using RntCar.BusinessLibrary.Repository;
using RntCar.ClassLibrary;
using RntCar.ClassLibrary._Enums_1033;
using RntCar.ClassLibrary.SMS;
using RntCar.Logger;
using RntCar.SDK.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RntCar.DepositRefund
{
    class Program
    {
        static void Main(string[] args)
        {
            LoggerHelper loggerHelper = new LoggerHelper();
            CrmServiceHelper crmServiceHelper = new CrmServiceHelper();
            ContractHelper contractHelper = new ContractHelper(crmServiceHelper.IOrganizationService);
            List<Entity> contracts = new List<Entity>();
            //string contractLastX = StaticHelper.GetConfiguration("contractLastX");
            // Set the number of records per page to retrieve.
            int queryCount = 5000;
            // Initialize the page number.
            int pageNumber = 1;
            string pagingCookie = null;
            int contractDuration = Convert.ToInt32(StaticHelper.GetConfiguration("contractduration"));
            contractDuration = -1 * contractDuration;
            while (true)
            {
                LinkEntity contactEnt = new LinkEntity();
                contactEnt.LinkFromEntityName = "rnt_contract";
                contactEnt.LinkFromAttributeName = "rnt_customerid";
                contactEnt.LinkToEntityName = "contact";
                contactEnt.LinkToAttributeName = "contactid";
                contactEnt.LinkCriteria.AddCondition("rnt_debitamount", ConditionOperator.LessThan, 0);

                QueryExpression queryExpression = new QueryExpression("rnt_contract");
                queryExpression.ColumnSet = new ColumnSet(new string[] { "rnt_dropoffdatetime", "rnt_customerid", "rnt_pnrnumber", "rnt_contractnumber", "rnt_totalamount", "rnt_netpayment", "rnt_depositblockage" });
                queryExpression.Criteria.AddCondition("rnt_debitamount", ConditionOperator.LessThan, -1);
                queryExpression.Criteria.AddCondition("rnt_depositblockage", ConditionOperator.Equal, false);
                queryExpression.Criteria.AddCondition("statuscode", ConditionOperator.Equal, (int)rnt_contract_StatusCode.Completed);
                queryExpression.Criteria.AddCondition("rnt_dropoffdatetime", ConditionOperator.OnOrAfter, new DateTime(2021, 09, 01));
                queryExpression.Criteria.AddCondition("rnt_dropoffdatetime", ConditionOperator.OnOrBefore, DateTime.Now.AddDays(contractDuration));
                queryExpression.AddOrder("rnt_dropoffdatetime", OrderType.Descending);
                queryExpression.PageInfo = new PagingInfo
                {
                    PageNumber = pageNumber,
                    Count = queryCount,
                    PagingCookie = pagingCookie
                };
                var l = crmServiceHelper.IOrganizationService.RetrieveMultiple(queryExpression);
                if (l.MoreRecords)
                {
                    contracts.AddRange(l.Entities.ToList());
                    pageNumber++;
                    pagingCookie = l.PagingCookie;
                }
                else
                {
                    contracts.AddRange(l.Entities.ToList());
                    break;
                }

            }
            ConfigurationBL crmConfigurationBL = new ConfigurationBL(crmServiceHelper.IOrganizationService);
            var config = crmConfigurationBL.GetConfigurationByName("TurkeyGuid");

            PaymentBL paymentBL = new PaymentBL(crmServiceHelper.IOrganizationService);
            int i = 0;
            int j = 0;
            foreach (var contract in contracts)
            {
                i++;
                loggerHelper.traceInfo("contractnumber : " + contract.GetAttributeValue<string>("rnt_contractnumber"));
                var totalAmount = contract.GetAttributeValue<Money>("rnt_totalamount").Value;
                var netPayment = contract.GetAttributeValue<Money>("rnt_netpayment").Value;
                var diff = totalAmount - netPayment;
                if (diff < -1)
                {
                    j++;
                    loggerHelper.traceInfo(string.Format("Deposit amount will be refund {0}", diff));
                    try
                    {
                        var ent = paymentBL.createRefund(new CreateRefundParameters
                        {
                            isDepositRefund = true,
                            refundAmount = ((decimal)-1 * diff),
                            contractId = contract.Id
                        });

                        contractHelper.calculateDebitAmount(contract.Id);
                        if (ent.Count > 0 &&
                           ent.FirstOrDefault().GetAttributeValue<OptionSetValue>("rnt_transactionresult").Value == (int)RntCar.ClassLibrary._Enums_1033.rnt_payment_rnt_transactionresult.Success)
                        {
                            Console.WriteLine("contractnumber : " + contract.GetAttributeValue<string>("rnt_contractnumber") + " işlem yapıldı");
                            sendSMS(loggerHelper, crmServiceHelper.IOrganizationService, contract, config, diff * -1, contract.GetAttributeValue<string>("rnt_pnrnumber"));
                            loggerHelper.traceInfo("deposit refunded successfully");
                        }
                    }
                    catch (Exception ex)
                    {
                        loggerHelper.traceInfo("deposit refund error : " + ex.Message);
                    }
                }

                Console.WriteLine("contractnumber : " + contract.GetAttributeValue<string>("rnt_contractnumber") + " kayıt sayısı: " + i + " iadeye uygun olan: " + j);
            }


        }

        private static void sendSMS(LoggerHelper loggerHelper, IOrganizationService organizationService, Entity contract, string config, decimal amount, string pnrnumber)
        {
            IndividualCustomerRepository individualCustomerRepository = new IndividualCustomerRepository(organizationService);
            var ind = individualCustomerRepository.getIndividualCustomerByIdWithGivenColumns(contract.GetAttributeValue<EntityReference>("rnt_customerid").Id,
                                                                                             new string[] { "firstname", "lastname", "mobilephone", "rnt_citizenshipid" });


            var langId = 1055;
            if (ind.GetAttributeValue<EntityReference>("rnt_citizenshipid").Id != new Guid(config.Split(';')[0]))
            {
                langId = 1033;
            }
            try
            {
                if (!string.IsNullOrEmpty(ind.GetAttributeValue<string>("mobilephone")))
                {
                    OrganizationRequest organizationRequest = new OrganizationRequest("rnt_GenerateAndSendSMS");
                    organizationRequest["FirstName"] = ind.GetAttributeValue<string>("firstname");
                    organizationRequest["LastName"] = ind.GetAttributeValue<string>("lastname");
                    organizationRequest["MobilePhone"] = ind.GetAttributeValue<string>("mobilephone");
                    organizationRequest["LangId"] = langId;
                    organizationRequest["amount"] = amount.ToString();
                    organizationRequest["SMSContentCode"] = (int)GlobalEnums.SmsContentCode.DepositRefund;
                    organizationRequest["PnrNumber"] = pnrnumber;


                    var res = organizationService.Execute(organizationRequest);
                    loggerHelper.traceInfo("res " + res.Results["GenerateAndSendSMSResponse"]);
                }
                else
                {
                    loggerHelper.traceInfo("mobile phone is null " + contract.Id);
                }
            }
            catch (Exception ex)
            {
                loggerHelper.traceInfo("sms hata : " + ex.Message);
            }


        }
    }
}
