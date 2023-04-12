using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Newtonsoft.Json;
using RntCar.BusinessLibrary.Business;
using RntCar.BusinessLibrary.Helpers;
using RntCar.BusinessLibrary.Repository;
using RntCar.ClassLibrary;
using RntCar.ClassLibrary._Enums_1033;
using RntCar.Logger;
using RntCar.SDK.Common;
using System;
using System.Linq;

namespace RntCar.MakePaymentDebtContracts
{
    class Program
    {
        static LoggerHelper loggerHelper = new LoggerHelper();
        static CrmServiceHelper crmServiceHelper = new CrmServiceHelper();
        static void Main(string[] args)
        {
            //get the completed contracts for last 30 days
            ContractRepository contractRepository = new ContractRepository(crmServiceHelper.IOrganizationService);
            string contractLastX = StaticHelper.GetConfiguration("contractLastX");
            var results = contractRepository.getCompletedDepthContractsByXlastDays(Convert.ToInt32(contractLastX));
            ContractBL contractBL = new ContractBL(crmServiceHelper.IOrganizationService);
            InvoiceRepository invoiceRepository = new InvoiceRepository(crmServiceHelper.IOrganizationService);
            ContractHelper contractHelper = new ContractHelper(crmServiceHelper.IOrganizationService);

            foreach (var item in results)
            {
                var p = item.GetAttributeValue<OptionSetValue>("rnt_paymentmethodcode").Value;
                if (p == (int)rnt_PaymentMethodCode.FullCredit || p == (int)rnt_PaymentMethodCode.Current)
                {
                    loggerHelper.traceInfo("contractid : " + item.Id);
                    loggerHelper.traceInfo("cari ve fullcreditte çekim olmaz");
                    continue;
                }
                if (item.GetAttributeValue<DateTime>("rnt_dropoffdatetime").AddDays(3) <= DateTime.UtcNow.AddMinutes(StaticHelper.offset))
                {
                    try
                    {
                        var invoiceAdress = new InvoiceAddressData();
                        var invoice = invoiceRepository.getFirstActiveInvoiceByContractId(item.Id);
                        if (invoice.Count == 0)
                        {
                            if (item.GetAttributeValue<OptionSetValue>("rnt_contracttypecode").Value == (int)rnt_ReservationTypeCode.Broker ||
                               item.GetAttributeValue<OptionSetValue>("rnt_contracttypecode").Value == (int)rnt_ReservationTypeCode.Acente)
                            {
                                InvoiceAddressRepository invoiceAddressRepository = new InvoiceAddressRepository(crmServiceHelper.IOrganizationService);
                                var address = invoiceAddressRepository.getIndividualInvoiceAddressByCustomerIdByGivenColumns(item.GetAttributeValue<EntityReference>("rnt_customerid").Id).FirstOrDefault();
                                invoiceAdress = InvoiceHelper.buildInvoiceAddressDataFromInvoiceAddressEntity(address);
                            }
                        }
                        else
                        {
                            invoiceAdress = InvoiceHelper.buildInvoiceAddressDataFromInvoiceEntity(invoice.FirstOrDefault());
                        }


                        var d = item.GetAttributeValue<Money>("rnt_totalamount").Value - item.GetAttributeValue<Money>("rnt_netpayment").Value;
                        if (d > StaticHelper._one_)
                        {
                            var a = item.GetAttributeValue<string>("rnt_contractnumber");
                            if (!item.GetAttributeValue<bool>("rnt_debitmailsent"))
                            {
                                SMSContentRepository SMSContentRepository = new SMSContentRepository(crmServiceHelper.IOrganizationService);
                                var content = SMSContentRepository.getSMSContentByCodeandLangId(1000, 1055);

                                var message = content.GetAttributeValue<string>("rnt_message");

                                IndividualCustomerRepository individualCustomerRepository = new IndividualCustomerRepository(crmServiceHelper.IOrganizationService);
                                var ind = individualCustomerRepository.getIndividualCustomerByIdWithGivenColumns(item.GetAttributeValue<EntityReference>("rnt_customerid").Id, new string[] { "firstname", "lastname", "emailaddress1" });

                                message = message.Replace("@firstName", ind.GetAttributeValue<string>("firstname"));
                                message = message.Replace("@lastName", ind.GetAttributeValue<string>("lastname"));
                                message = message.Replace("@pnrnumber", item.GetAttributeValue<string>("rnt_pnrnumber"));
                                message = message.Replace("@amount", d.ToString("0.##"));

                                Entity fromActivityParty = new Entity("activityparty");

                                ConfigurationRepository configurationRepository = new ConfigurationRepository(crmServiceHelper.IOrganizationService);
                                fromActivityParty["partyid"] = new EntityReference("systemuser", new Guid(configurationRepository.GetConfigurationByKey("CrmAdminGuid")));

                                var recipients = new EntityCollection();
                                recipients.Entities.Add(new Entity("activityparty")
                                {
                                    ["partyid"] = new EntityReference("contact", ind.Id)
                                });                                

                                Entity createemail = new Entity("email");
                                createemail["regardingobjectid"] = new EntityReference("rnt_contract", item.Id);
                                createemail["from"] = new Entity[] { fromActivityParty };
                                createemail["to"] = recipients;


                                createemail["subject"] = "Borcunuz Hakkında";
                                createemail["description"] = message;
                                createemail["directioncode"] = true;
                                Guid emailId = crmServiceHelper.IOrganizationService.Create(createemail);

                                SendEmailRequest sendEmailRequest = new SendEmailRequest
                                {
                                    EmailId = emailId,
                                    TrackingToken = "",
                                    IssueSend = true
                                };

                                SendEmailResponse sendEmailresp = (SendEmailResponse)crmServiceHelper.IOrganizationService.Execute(sendEmailRequest);
                                Entity e = new Entity("rnt_contract");
                                e.Id = item.Id;
                                e["rnt_debitmailsent"] = true;
                                crmServiceHelper.IOrganizationService.Update(e);
                            }
                            loggerHelper.traceInfo("contract : " + a);
                            loggerHelper.traceInfo("for amount : + " + d);
                            try
                            {
                                CreditCardRepository creditCardRepository = new CreditCardRepository(crmServiceHelper.IOrganizationService);
                                var cards = creditCardRepository.getCreditCardsByCustomerId(item.GetAttributeValue<EntityReference>("rnt_customerid").Id);
                                //cards.Reverse();
                                loggerHelper.traceInfo("cards found : " + cards.Count);
                                var remainingAmount = d;
                                var paidAmount = decimal.Zero;
                                var response = new CreatePaymentResponse
                                {
                                    ResponseResult = ResponseResult.ReturnSuccess()
                                };
                                foreach (var card in cards)
                                {
                                    var count = 1;
                                    remainingAmount = d - paidAmount;// reset reamining amont for new card. Set debt amount - paidAmount to remainingAmount when old card paid 
                                    do
                                    {
                                        try
                                        {
                                            count++;
                                            response = MakePayment(item, card, invoiceAdress, remainingAmount);
                                            loggerHelper.traceInfo("response : " + JsonConvert.SerializeObject(response));
                                            if (response.ResponseResult.Result)
                                            {
                                                paidAmount += response.paidAmount;
                                                remainingAmount = d - response.paidAmount;
                                                loggerHelper.traceInfo("payment successful , braking foreach");
                                                contractHelper.calculateDebitAmount(item.Id);
                                                break;
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            loggerHelper.traceInfo("remainingAmount is unsuccesful : " + remainingAmount);
                                            loggerHelper.traceInfo("exception : " + ex.Message);
                                            if (ex.Message != "CustomErrorMessagefinder: Kart limiti yetersiz, yetersiz bakiye")
                                            {
                                                break;
                                            }
                                            //if payment fail try debt amount 1/2 or 1/3 or 1/4
                                            remainingAmount = (d / count) > 1 ? (d / count) : decimal.Zero;
                                            loggerHelper.traceInfo("will try remainingAmount : " + remainingAmount);
                                            continue;
                                        }
                                    } while (remainingAmount != decimal.Zero && count < 5);

                                    if (response.ResponseResult.Result && paidAmount == d)// if paid amount eq debt
                                        break;

                                }
                            }
                            catch (Exception ex)
                            {
                                loggerHelper.traceInfo("exception : " + ex.Message);
                                continue;
                            }

                        }
                    }
                    catch (Exception ex)
                    {
                        loggerHelper.traceInfo(ex.Message);
                        continue;
                    }
                }
            }

            loggerHelper.traceInfo("all operations finished in peace");
        }

        static CreatePaymentResponse MakePayment(Entity contract, Entity card, InvoiceAddressData invoiceAddress, decimal paidAmount)
        {
            var creditCardData = new CreditCardData
            {
                cardToken = card.GetAttributeValue<string>("rnt_cardtoken"),
                cardUserKey = card.GetAttributeValue<string>("rnt_carduserkey"),
            };
            loggerHelper.traceInfo("cardToken : " + card.GetAttributeValue<string>("rnt_cardtoken"));
            var createPaymentParameters = new CreatePaymentParameters
            {
                contractId = contract.Id,
                transactionCurrencyId = new Guid("036024DD-E8A5-E911-A847-000D3A2BD64E"),
                individualCustomerId = contract.GetAttributeValue<EntityReference>("rnt_customerid").Id,
                conversationId = contract.GetAttributeValue<string>("rnt_pnrnumber"),
                langId = 1055,
                paymentTransactionType = (int)PaymentEnums.PaymentTransactionType.SALE,
                creditCardData = creditCardData,
                installment = 1, // installment can not be selected during contract creation
                paidAmount = paidAmount,//remainingAmount == decimal.Zero ? d : remainingAmount,
                invoiceAddressData = invoiceAddress,
                virtualPosId = 0,
                paymentChannelCode = rnt_PaymentChannelCode.BRANCH
            };

            var paymentBL = new PaymentBL(crmServiceHelper.IOrganizationService);
            var response = paymentBL.callMakePaymentAction(createPaymentParameters);
            response.paidAmount = paidAmount;
            return response;
        }
    }
}
