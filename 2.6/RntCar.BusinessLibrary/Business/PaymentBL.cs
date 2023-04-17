using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Newtonsoft.Json;
using RntCar.BusinessLibrary.Handlers;
using RntCar.BusinessLibrary.Helpers;
using RntCar.BusinessLibrary.Repository;
using RntCar.ClassLibrary;
using RntCar.ClassLibrary._Enums_1033;
using RntCar.ClassLibrary.Payment;
using RntCar.PaymentHelper;
using RntCar.PaymentHelper.iyzico;
using RntCar.PaymentHelper.NetTahsilat;
using RntCar.SDK.Common;
using RntCar.SDK.Mappers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Numerics;
using System.Security.Cryptography;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Shapes;
using static Microsoft.IdentityModel.Protocols.WSFederation.WSFederationConstants;

namespace RntCar.BusinessLibrary.Business
{
    public class PaymentBL : BusinessHandler
    {
        bool isCompleted = false;
        private static string currentUrl = "";

        public PaymentBL(IOrganizationService orgService) : base(orgService)
        {
        }

        public PaymentBL(GlobalEnums.ConnectionType enums = GlobalEnums.ConnectionType.default_Service, string UserId = "") : base(enums, UserId)
        {
        }

        public PaymentBL(IOrganizationService orgService, ITracingService tracingService) : base(orgService, tracingService)
        {
        }

        public PaymentBL(IOrganizationService orgService, Guid userId) : base(orgService, userId)
        {
        }

        public PaymentBL(PluginInitializer initializer, IOrganizationService orgService, ITracingService tracingService) : base(initializer, orgService, tracingService)
        {
        }

        public PaymentBL(IOrganizationService orgService, Guid userId, string orgName) : base(orgService, userId, orgName)
        {
        }

        public CreatePaymentResponse makePayment(CreatePaymentParameters param)
        {
            this.Trace("CreatePaymentParameters" + JsonConvert.SerializeObject(param));

            var createPaymentResponse = new CreatePaymentResponse();
            try
            {
                #region Provider Choosing
                var provider = getProviderCode(param.reservationId, param.contractId);
                this.Trace("paymentprovider_namespace : " + provider);
                ConfigurationBL configurationBL = new ConfigurationBL(this.OrgService);
                IPaymentProvider paymentProvider = null;
                if (provider == (int)GlobalEnums.PaymentProvider.iyzico)
                {
                    paymentProvider = Activator.CreateInstance(typeof(IyzicoProvider)) as IPaymentProvider;
                    var iyzicoInfo = configurationBL.GetConfigurationByName("iyzicoInformation");
                    var configs = IyzicoHelper.setConfigurationValues(iyzicoInfo);
                    //setting auth info
                    param.baseurl = configs.iyzico_baseUrl;
                    param.secretKey = configs.iyzico_secretKey;
                    param.apikey = configs.iyzico_apiKey;
                }
                else if (provider == (int)GlobalEnums.PaymentProvider.nettahsilat)
                {
                    paymentProvider = Activator.CreateInstance(typeof(NetTahsilatProvider)) as IPaymentProvider;
                    var netTahsilatInfo = configurationBL.GetConfigurationByName("nettahsilatconfiguration");
                    var configs = NetTahsilatHelper.setConfigurationValues(netTahsilatInfo);

                    param.userName = configs.nettahsilat_username;
                    param.password = configs.nettahsilat_password;
                    param.baseurl = configs.nettahsilat_url;
                    param.vendorCode = configs.nettahsilat_vendorcode;
                    param.vendorId = configs.nettahsilat_vendorid;
                }
                #endregion

                #region Credit Card Create/Update/Check 3D Secure
                this.Trace("Credit Card Create/Update/Check 3D Secure Start");
                CreditCardBL creditCardBL = new CreditCardBL(this.OrgService, this.TracingService);
                CreditCardRepository creditCardRepository = new CreditCardRepository(this.OrgService);
                param = CreateOrGetCreditCardIdForPayment(param, provider, creditCardBL, creditCardRepository);

                this.Trace("Credit Card Create/Update/Check 3D Secure End");
                #endregion

                #region Installment


                if (param.installment == 0)
                {
                    param.installment = 1;
                }

                param.installmentRatio = 0;

                RntCar.BusinessLibrary.Helpers.PaymentHelper paymentHelper = new BusinessLibrary.Helpers.PaymentHelper(this.OrgService, this.TracingService);
                if (param.installment != 1)
                {
                    this.Trace("Installment Start");
                    var binNumber = param.creditCardData.binNumber;
                    var ratio = paymentHelper.getInstallmentRatio(binNumber, param.installment, provider);
                    param.installmentRatio = ratio;
                    this.Trace("Installment End");
                }

                #endregion

                #region Create Payment In Dynamics And Create Relations Credit Card

                this.Trace("Create Payment In Dynamics And Create Relations Credit Card Start");
                var id = this.createPayment(param, provider);
                this.Trace("payment created id : " + id);

                createPaymentResponse.paymentId = Convert.ToString(id);
                updatePaymentCreditCard(id, param.creditCardData.creditCardId.Value);

                this.Trace("Create Payment In Dynamics And Create Relations Credit Card End");
                #endregion

                #region Payment And Credit Card Update 3D Secure Status

                if (param.use3DSecure)
                {
                    this.Trace("Payment And Credit Card Update 3D Secure Status Start");
                    creditCardBL.setCreditCardStatus(param.creditCardData.creditCardId.Value, (int)rnt_customercreditcard_StatusCode.WaitingFor3D);
                    setPaymentTransactionResultState(id, (int)rnt_payment_rnt_transactionresult.waitingFor3D);
                    this.Trace("Payment And Credit Card Update 3D Secure Status End");
                }
                #endregion

                #region Payment Invoice Information Fill

                this.Trace("Payment Invoice Information Fill Start");
                if (param.invoiceAddressData == null || (param.invoiceAddressData != null && !param.invoiceAddressData.invoiceAddressId.HasValue))
                {
                    this.Trace("param.contractId " + param.contractId);
                    if (param.contractId.HasValue)
                    {
                        InvoiceRepository invoiceRepository = new InvoiceRepository(this.OrgService);
                        var invoice = invoiceRepository.getInvoicesByContractId(param.contractId.Value);

                        if (invoice.FirstOrDefault() != null)
                        {
                            param.invoiceAddressData = InvoiceHelper.buildInvoiceAddressDataFromInvoiceEntity(invoice.FirstOrDefault());

                            this.Trace("address data getting completed invoice info");
                        }
                        //means invoice is  completed
                        else
                        {
                            invoice = invoiceRepository.getFirstActiveInvoiceByContractId(param.contractId.Value);
                            param.invoiceAddressData = InvoiceHelper.buildInvoiceAddressDataFromInvoiceEntity(invoice.FirstOrDefault());

                            this.Trace("address data getting default invoice info");
                        }

                        this.Trace("address data : " + JsonConvert.SerializeObject(param.invoiceAddressData));
                    }
                    else if (param.reservationId.HasValue)
                    {
                        InvoiceRepository invoiceRepository = new InvoiceRepository(this.OrgService);
                        var invoice = invoiceRepository.getInvoiceByReservationId(param.contractId.Value);
                        param.invoiceAddressData = InvoiceHelper.buildInvoiceAddressDataFromInvoiceEntity(invoice);
                    }
                }
                this.Trace(param.invoiceAddressData == null ? "param.invoiceAddressData is null" : "param.invoiceAddressData is not null");
                if (string.IsNullOrEmpty(param.invoiceAddressData.email))
                {
                    param.invoiceAddressData.email = "pay@rentgo.com";
                }
                if (string.IsNullOrEmpty(param.invoiceAddressData.addressCityName))
                {
                    param.invoiceAddressData.addressCityName = "Rentgo Merkez";
                }

                this.Trace("Payment Invoice Information Fill End");
                #endregion


                this.Trace("makePayment Start");
                var paymentResponse = paymentProvider.makePayment(param);

                this.Trace("makePayment End");
                var paymentFormattedResponse = (PaymentResponse)paymentResponse;

                this.Trace("paymentFormattedResponse.status" + paymentFormattedResponse.status);
                if (paymentFormattedResponse.status)
                {
                    createPaymentResponse.externalPaymentId = paymentFormattedResponse.paymentId;
                    createPaymentResponse.externalPaymentTransactionId = paymentFormattedResponse.paymentTransactionId;
                    createPaymentResponse.use3DSecure = paymentFormattedResponse.use3DSecure;
                    if (createPaymentResponse.use3DSecure && provider == (int)GlobalEnums.PaymentProvider.nettahsilat)
                    {
                        //var webBrowser = new WebBrowser();
                        //webBrowser.Navigate(paymentFormattedResponse.htmlContent);
                        //webBrowser.DocumentCompleted += WebBrowser_DocumentCompleted;
                        //webBrowser.ScriptErrorsSuppressed = true;
                        string result = null;
                        using (HttpClient client = new HttpClient())
                        {
                            using (HttpResponseMessage response = client.GetAsync(paymentFormattedResponse.htmlContent).Result)
                            {
                                using (HttpContent content = response.Content)
                                {
                                    result = content.ReadAsStringAsync().Result;
                                }
                            }
                        }
                        //do
                        //{
                        //    Thread.Sleep(10);
                        //    Application.DoEvents();
                        //} while (!isCompleted);

                        paymentFormattedResponse.htmlContent = result;
                    }
                    createPaymentResponse.htmlContent = paymentFormattedResponse.htmlContent;


                    try
                    {
                        this.updatePaymentResult(id, paymentFormattedResponse);
                    }
                    catch (Exception ex)
                    {
                        this.Trace("internal exception" + ex.Message);
                        //update record with internal Message
                        this.updatePaymentRecordInternalError(id, ex.Message);
                    }
                    createPaymentResponse.ResponseResult = ResponseResult.ReturnSuccess();
                }
                else
                {
                    //will read from xml
                    if (paymentResponse.errorMessage == "errFailNotFoundErrorMessage")
                    {
                        paymentResponse.errorMessage = StaticHelper.paymentNotFoundReplacement;
                    }
                    this.updatePaymentResult(id, paymentResponse);
                    this.Trace("payment error" + paymentResponse.errorMessage);
                    throw new Exception("Kart-Banka Ödeme Hatası:" + paymentResponse.errorMessage);
                }
            }
            catch (Exception ex)
            {
                this.Trace("makePayment ex.Message" + ex.Message);
                throw new Exception(ex.Message);
            }
            return createPaymentResponse;
        }

        public CreatePaymentParameters CreateOrGetCreditCardIdForPayment(CreatePaymentParameters param, int provider, CreditCardBL creditCardBL = null, CreditCardRepository creditCardRepository = null)
        {
            this.Trace("CreateOrGetCreditCardIdForPayment Start");

            this.Trace("CreatePaymentParameters" + JsonConvert.SerializeObject(param));

            if (creditCardBL == null)
            {
                creditCardBL = new CreditCardBL(this.OrgService, this.TracingService);
            }
            if (creditCardRepository == null)
            {
                creditCardRepository = new CreditCardRepository(this.OrgService);
            }

            string _expireYear = FormatExpireYear(param);
            Entity card = creditCardBL.GetCreditCardWithProvider(param, provider, creditCardRepository, _expireYear);

            if (provider == (int)GlobalEnums.PaymentProvider.iyzico)
                param.use3DSecure = check3DSecure(card, (int)param.paymentChannelCode);
            else
                param.use3DSecure = false;

            this.Trace("param.creditCardData.cardUserKey" + JsonConvert.SerializeObject(card));
            if (card == null || card.Id == Guid.Empty)
            {
                card = new Entity();
                IndividualCustomerRepository individualCustomerRepository = new IndividualCustomerRepository(this.OrgService);
                var customer = individualCustomerRepository.getIndividualCustomerByIdWithGivenColumns(param.individualCustomerId, new string[] { "rnt_customerexternalid" });
                var creditCardParams = new CreateCreditCardParameters
                {
                    cardAlias = param.creditCardData.creditCardNumber,
                    cardHolderName = param.creditCardData.cardHolderName,
                    creditCardNumber = param.creditCardData.creditCardNumber.removeEmptyCharacters(),
                    cvc = param.creditCardData.cvc,
                    expireMonth = param.creditCardData.expireMonth,
                    expireYear = Convert.ToInt32(_expireYear),
                    individualCustomerId = param.individualCustomerId.ToString(),
                    langId = param.langId,
                    customerExternalId = customer.GetAttributeValue<string>("rnt_customerexternalid")
                };

                this.Trace("creditCardParams " + Convert.ToString(JsonConvert.SerializeObject(creditCardParams)));
                OrganizationRequest organizationRequest = new OrganizationRequest("rnt_CreateCustomerCreditCard");
                organizationRequest["creditCardParameters"] = Convert.ToString(JsonConvert.SerializeObject(creditCardParams));
                var actionResponse = this.OrgService.Execute(organizationRequest);
                var parsedResponse = JsonConvert.DeserializeObject<CreateCreditCardResponse>(Convert.ToString(actionResponse.Results["ExecutionResult"]));

                param.creditCardData.creditCardId = parsedResponse.creditCardId;

                var providerCreditCard = creditCardBL.createProviderCreditCard(creditCardParams, parsedResponse.creditCardId, provider);

                param.creditCardData.cardUserKey = providerCreditCard.cardUserKey;
                param.creditCardData.cardToken = providerCreditCard.cardToken;
                param.creditCardData.binNumber = !string.IsNullOrWhiteSpace(parsedResponse.binNumber) ? parsedResponse.binNumber : param.creditCardData.creditCardNumber.removeEmptyCharacters().Substring(0, 6);
                this.Trace("param.creditCardData.cardUserKey" + param.creditCardData.cardUserKey);
                this.Trace("param.creditCardData.cardToken" + param.creditCardData.cardToken);
                this.Trace("param.creditCardData.binNumber" + param.creditCardData.binNumber);
                if (param.invoiceAddressData != null)
                {
                    param.invoiceAddressData.email = providerCreditCard.emailAddress;
                }
            }
            else
            {
                string cardUserKey = card.GetAttributeValue<string>("rnt_carduserkey");
                string cardToken = card.GetAttributeValue<string>("rnt_cardtoken");
                if (string.IsNullOrWhiteSpace(cardUserKey) && string.IsNullOrWhiteSpace(cardToken))
                {
                    IndividualCustomerRepository individualCustomerRepository = new IndividualCustomerRepository(this.OrgService);
                    var customer = individualCustomerRepository.getIndividualCustomerByIdWithGivenColumns(param.individualCustomerId, new string[] { "rnt_customerexternalid" });

                    var creditCardParams = new CreateCreditCardParameters
                    {
                        cardAlias = param.creditCardData.creditCardNumber,
                        cardHolderName = param.creditCardData.cardHolderName,
                        creditCardNumber = param.creditCardData.creditCardNumber.removeEmptyCharacters(),
                        cvc = param.creditCardData.cvc,
                        expireMonth = param.creditCardData.expireMonth,
                        expireYear = Convert.ToInt32(_expireYear),
                        individualCustomerId = param.individualCustomerId.ToString(),
                        langId = param.langId,
                        customerExternalId = customer.GetAttributeValue<string>("rnt_customerexternalid")
                    };
                    param.creditCardData.creditCardId = card.Id;

                    var providerCreditCard = creditCardBL.createProviderCreditCard(creditCardParams, card.Id, provider);

                    param.creditCardData.cardUserKey = providerCreditCard.cardUserKey;
                    param.creditCardData.cardToken = providerCreditCard.cardToken;
                    param.creditCardData.binNumber = providerCreditCard.binNumber;
                    this.Trace("param.creditCardData.cardUserKey" + param.creditCardData.cardUserKey);
                    this.Trace("param.creditCardData.cardToken" + param.creditCardData.cardToken);
                    if (param.invoiceAddressData != null)
                    {
                        param.invoiceAddressData.email = providerCreditCard.emailAddress;
                    }
                }
                else
                {
                    param.creditCardData.cardUserKey = card.GetAttributeValue<string>("rnt_carduserkey");
                    param.creditCardData.cardToken = card.GetAttributeValue<string>("rnt_cardtoken");
                    param.creditCardData.creditCardId = card.Id;
                    param.creditCardData.binNumber = card.GetAttributeValue<string>("rnt_binnumber");
                }
            }
            this.Trace("CreatePaymentParameters" + JsonConvert.SerializeObject(card));
            this.Trace("CreateOrGetCreditCardIdForPayment End");
            return param;
        }

        private static string FormatExpireYear(CreatePaymentParameters param)
        {
            string _expireYear = string.Empty;
            if (param.creditCardData.expireYear.HasValue)
            {
                if (param.creditCardData.expireYear.Value.ToString().Length == 2)
                {
                    _expireYear = "20" + param.creditCardData.expireYear.Value;
                }
                else
                {
                    _expireYear = param.creditCardData.expireYear.Value.ToString();
                }
            }

            return _expireYear;
        }

        public void setName(Entity entity, Entity PreImageEntity)
        {
            string name = string.Empty;
            #region contract or reservation
            if (entity.Contains("rnt_contractid"))
            {
                this.Trace("entity rnt_contractid : " + entity.GetAttributeValue<EntityReference>("rnt_contractid").Name);
                name += entity.GetAttributeValue<EntityReference>("rnt_contractid").Name;
            }
            else if (PreImageEntity.Contains("rnt_contractid"))
            {
                this.Trace("PreImageEntity rnt_contractid : " + PreImageEntity.GetAttributeValue<EntityReference>("rnt_contractid").Name);
                name += PreImageEntity.GetAttributeValue<EntityReference>("rnt_contractid").Name;
            }
            else if (entity.Contains("rnt_reservationid"))
            {
                this.Trace("entity rnt_reservationid : " + entity.GetAttributeValue<EntityReference>("rnt_reservationid").Name);
                name += entity.GetAttributeValue<EntityReference>("rnt_reservationid").Name;
            }
            else if (PreImageEntity.Contains("rnt_reservationid"))
            {
                this.Trace("PreImageEntity rnt_reservationid : " + PreImageEntity.GetAttributeValue<EntityReference>("rnt_reservationid").Name);
                name += PreImageEntity.GetAttributeValue<EntityReference>("rnt_reservationid").Name;
            }
            #endregion

            #region Payment Transaction Code
            if (entity.Contains("rnt_paymenttransactionid"))
            {
                name += " - " + entity.GetAttributeValue<string>("rnt_paymenttransactionid");
            }
            else if (PreImageEntity.Contains("rnt_paymenttransactionid"))
            {
                name += " - " + PreImageEntity.GetAttributeValue<string>("rnt_paymenttransactionid");
            }
            #endregion

            #region Transaction Type code
            var v = int.MinValue;
            if (entity.Contains("rnt_transactiontypecode"))
            {
                v = entity.GetAttributeValue<OptionSetValue>("rnt_transactiontypecode").Value;
            }
            else if (PreImageEntity.Contains("rnt_transactiontypecode"))
            {
                v = PreImageEntity.GetAttributeValue<OptionSetValue>("rnt_transactiontypecode").Value;
            }
            if (v != int.MinValue)
            {
                if ((int)RntCar.ClassLibrary._Enums_1033.rnt_TransactionTypeCode.Deposit == v)
                {
                    name += " - Teminat";
                }
                else if ((int)RntCar.ClassLibrary._Enums_1033.rnt_TransactionTypeCode.Sale == v)
                {
                    name += " - Satış";
                }
                else if ((int)RntCar.ClassLibrary._Enums_1033.rnt_TransactionTypeCode.Refund == v)
                {
                    name += " - Iade";
                }
            }
            #endregion

            entity["rnt_name"] = name;

        }
        public Guid createPayment(CreatePaymentParameters createPaymentParameters, int provider)
        {
            LogoBankCodeMappingRepository logoBankCodeMappingRepository = new LogoBankCodeMappingRepository(this.OrgService);
            Entity e = new Entity("rnt_payment");
            if (createPaymentParameters.reservationId.HasValue && createPaymentParameters.contractId.HasValue)
            {
                e["rnt_reservationid"] = new EntityReference("rnt_reservation", createPaymentParameters.reservationId.Value);
                e["rnt_contractid"] = new EntityReference("rnt_contract", createPaymentParameters.contractId.Value);
            }
            else if (createPaymentParameters.reservationId.HasValue)
            {
                e["rnt_reservationid"] = new EntityReference("rnt_reservation", createPaymentParameters.reservationId.Value);
            }
            else if (createPaymentParameters.contractId.HasValue)
            {
                e["rnt_contractid"] = new EntityReference("rnt_contract", createPaymentParameters.contractId.Value);
            }
            if (createPaymentParameters.creditCardData.creditCardId.HasValue)
            {
                e["rnt_customercreditcardid"] = new EntityReference("rnt_customercreditcard", createPaymentParameters.creditCardData.creditCardId.Value);
            }
            createPaymentParameters.installment = createPaymentParameters.installment == 0 ? 1 : createPaymentParameters.installment;
            e["rnt_contactid"] = new EntityReference("contact", createPaymentParameters.individualCustomerId);
            e["rnt_installment"] = createPaymentParameters.installment;
            e["rnt_conversationid"] = createPaymentParameters.conversationId;
            e["transactioncurrencyid"] = new EntityReference("transactioncurrency", createPaymentParameters.transactionCurrencyId);
            e["rnt_transactiontypecode"] = new OptionSetValue((int)createPaymentParameters.paymentTransactionType);
            e["rnt_paymentchannelcode"] = new OptionSetValue((int)createPaymentParameters.paymentChannelCode);
            e["rnt_transactionresult"] = new OptionSetValue((int)PaymentEnums.PaymentTransactionResult.Draft);
            e["rnt_paymentprovider"] = new OptionSetValue(provider);
            if (provider == (int)GlobalEnums.PaymentProvider.iyzico)
            {
                Entity logoBankCodeEntity = logoBankCodeMappingRepository.getIyzicoVirtualPosId();
                e["rnt_virtualposid"] = logoBankCodeEntity.GetAttributeValue<string>("rnt_name");
            }
            else if (provider == (int)GlobalEnums.PaymentProvider.nettahsilat)
            {
                if (createPaymentParameters.virtualPosId.HasValue)
                    e["rnt_virtualposid"] = Convert.ToString(createPaymentParameters.virtualPosId.Value);
                else
                {
                    Entity logoBankCodeEntity = logoBankCodeMappingRepository.getNetTahsilatVirtualPosId();
                    e["rnt_virtualposid"] = logoBankCodeEntity.GetAttributeValue<string>("rnt_name");
                }
                //Else seçeneği yok, eğer gelmiyorsa sistem atama yapsın hatlı kalsın olarak iletildi.
            }
            e["rnt_amount"] = new Money(createPaymentParameters.paidAmount);
            e["rnt_netamount"] = new Money(createPaymentParameters.paidAmount);
            if (createPaymentParameters.installmentAmount.HasValue)
            {
                e["rnt_installmentamount"] = new Money(createPaymentParameters.installmentAmount.Value / createPaymentParameters.installment);
            }
            else
            {
                e["rnt_installmentamount"] = new Money(createPaymentParameters.paidAmount / createPaymentParameters.installment);
            }
            e["rnt_installmentratio"] = createPaymentParameters.installmentRatio;
            if (createPaymentParameters.use3DSecure)
            {
                e["rnt_transactionresult"] = new OptionSetValue((int)rnt_payment_rnt_transactionresult.waitingFor3D);
            }
            return this.OrgService.Create(e);

        }
        public void updatePaymentResult(Guid paymentId, PaymentResponse paymentResponse)
        {
            Entity e = new Entity("rnt_payment");
            e.Id = paymentId;
            if (paymentResponse.status)
            {
                e["rnt_transactionresult"] = new OptionSetValue((int)PaymentEnums.PaymentTransactionResult.Success);
                e["rnt_paymenttransactionid"] = paymentResponse.paymentTransactionId;
                e["rnt_paymentresultid"] = paymentResponse.paymentId;
                e["rnt_conversationid"] = paymentResponse.clientReferenceCode;
                e["rnt_iyzicocomission"] = new Money(paymentResponse.providerComission);
            }
            else
            {
                e["rnt_transactionresult"] = new OptionSetValue((int)PaymentEnums.PaymentTransactionResult.Error);
                e["rnt_errorcode"] = paymentResponse.errorCode;
                e["rnt_errorgroup"] = paymentResponse.errorGroup;
                e["rnt_errormessage"] = paymentResponse.errorMessage;
            }
            this.OrgService.Update(e);
        }
        public void updatePaymentResult(Guid paymentId, Reservation3dPaymentReturnResponse reservation3DPaymentReturn)
        {
            Entity e = new Entity("rnt_payment");
            e.Id = paymentId;
            if (reservation3DPaymentReturn.status)
            {
                e["rnt_transactionresult"] = new OptionSetValue((int)PaymentEnums.PaymentTransactionResult.Success);
                e["rnt_paymenttransactionid"] = reservation3DPaymentReturn.paymentTransactionId;
                e["rnt_paymentresultid"] = reservation3DPaymentReturn.paymentId;
                e["rnt_conversationid"] = reservation3DPaymentReturn.clientReferenceCode;
                e["rnt_iyzicocomission"] = new Money(reservation3DPaymentReturn.providerComission);
            }
            else
            {
                e["rnt_transactionresult"] = new OptionSetValue((int)PaymentEnums.PaymentTransactionResult.Error);
                e["rnt_errorcode"] = reservation3DPaymentReturn.errorCode;
                e["rnt_errorgroup"] = reservation3DPaymentReturn.errorGroup;
                e["rnt_errormessage"] = reservation3DPaymentReturn.errorMessage;
            }
            this.OrgService.Update(e);
        }
        public void updateRefundResult(Guid paymentId, RefundResponse refundResponse)
        {
            Entity e = new Entity("rnt_payment");
            e.Id = paymentId;
            if (refundResponse.status)
            {
                e["rnt_transactionresult"] = new OptionSetValue((int)PaymentEnums.PaymentTransactionResult.Success);
            }
            else
            {
                e["rnt_transactionresult"] = new OptionSetValue((int)PaymentEnums.PaymentTransactionResult.Error);
                e["rnt_errorcode"] = refundResponse.errorCode;
                e["rnt_errorgroup"] = refundResponse.errorGroup;
                e["rnt_errormessage"] = refundResponse.errorMessage;
            }
            this.OrgService.Update(e);
        }
        public void updatePaymentRecordInternalError(Guid paymentId, string message)
        {
            Entity e = new Entity("rnt_payment");
            e.Id = paymentId;
            e["rnt_internalmessage"] = message;
            e["rnt_transactionresult"] = new OptionSetValue((int)PaymentEnums.PaymentTransactionResult.Success_WithoutPaymentTransactions);
            this.OrgService.Update(e);
        }
        public void updatePaymentCreditCard(Guid paymentId, Guid creditCardId)
        {
            Entity e = new Entity("rnt_payment");
            e.Id = paymentId;
            e["rnt_customercreditcardid"] = new EntityReference("rnt_customercreditcard", creditCardId);
            this.OrgService.Update(e);
        }
        public void updateDescription(Guid paymentId, string description)
        {
            Entity e = new Entity("rnt_payment");
            e.Id = paymentId;
            e["rnt_description"] = description;
            this.OrgService.Update(e);
        }

        public CreatePaymentResponse callMakePaymentAction(CreatePaymentParameters createPaymentParameters)
        {
            this.Trace("start payment action");
            OrganizationRequest request = new OrganizationRequest("rnt_MakePayment");
            request["paymentParameters"] = JsonConvert.SerializeObject(createPaymentParameters);
            var response = this.OrgService.Execute(request);
            this.Trace("make payment action response" + Convert.ToString(response.Results["ExecutionResult"]));
            if (string.IsNullOrEmpty(Convert.ToString(response.Results["ExecutionResult"])))
            {
                return JsonConvert.DeserializeObject<CreatePaymentResponse>(Convert.ToString(response.Results["paymentResponse"]));

            }
            return new CreatePaymentResponse
            {
                ResponseResult = ResponseResult.ReturnError(Convert.ToString(response.Results["ExecutionResult"]))
            };
        }
        public void callCreatePaymentRecordService(CreatePaymentWithServiceParameters createPaymentWithServiceParameters)
        {
            ConfigurationRepository configurationRepository = new ConfigurationRepository(this.OrgService);
            var baseUrl = configurationRepository.GetConfigurationByKey("ExternalCrmWebApiUrl");
            RestSharpHelper restSharpHelper = new RestSharpHelper(baseUrl, "createPaymentRecord", RestSharp.Method.POST);
            this.Trace(" base url --> callCreatePaymentRecordService " + baseUrl);
            this.Trace("createPaymentWithServiceParameters " + JsonConvert.SerializeObject(createPaymentWithServiceParameters));
            restSharpHelper.AddJsonParameter<CreatePaymentWithServiceParameters>(createPaymentWithServiceParameters);
            restSharpHelper.ExecuteAsync<CreatePaymentWithServiceResponse>();

        }
        public void callCreateRefundRecordService(CreatePaymentWithServiceParameters createPaymentWithServiceParameters)
        {
            ConfigurationRepository configurationRepository = new ConfigurationRepository(this.OrgService);
            var baseUrl = configurationRepository.GetConfigurationByKey("ExternalCrmWebApiUrl");
            RestSharpHelper restSharpHelper = new RestSharpHelper(baseUrl, "createRefundRecord", RestSharp.Method.POST);
            this.Trace(" base url --> callCreatePaymentRecordService " + baseUrl);
            this.Trace("createPaymentWithServiceParameters " + JsonConvert.SerializeObject(createPaymentWithServiceParameters));
            restSharpHelper.AddJsonParameter<CreatePaymentWithServiceParameters>(createPaymentWithServiceParameters);
            restSharpHelper.ExecuteAsync<CreatePaymentWithServiceResponse>();

        }
        public Guid createRefundEntity(RefundData refundData)
        {
            Entity e = new Entity("rnt_payment");
            if (refundData.reservationId.HasValue)
            {
                e["rnt_reservationid"] = new EntityReference("rnt_reservation", refundData.reservationId.Value);
            }
            else if (refundData.contractId.HasValue)
            {
                e["rnt_contractid"] = new EntityReference("rnt_contract", refundData.contractId.Value);
            }
            if (refundData.customerCreditCard.HasValue)
                e["rnt_customercreditcardid"] = new EntityReference("rnt_customercreditcard", refundData.customerCreditCard.Value);
            if (refundData.parentPaymentId.HasValue)
                e["rnt_parentpaymentid"] = new EntityReference("rnt_payment", refundData.parentPaymentId.Value);
            if (refundData.contactId.HasValue)
                e["rnt_contactid"] = new EntityReference("contact", refundData.contactId.Value);

            e["rnt_refundamount"] = new Money(refundData.refundAmount);
            e["rnt_amount"] = new Money(refundData.refundAmount);
            e["rnt_transactionresult"] = new OptionSetValue((int)rnt_payment_rnt_transactionresult.Draft);
            e["rnt_paymentresultid"] = refundData.paymentResultId;
            e["rnt_paymenttransactionid"] = refundData.paymentTransactionId;
            e["rnt_paymentchannelcode"] = new OptionSetValue((int)refundData.paymentChannelCode);
            e["transactioncurrencyid"] = new EntityReference("transactioncurrency", refundData.transactionCurrencyId);
            //e["rnt_refundstatuscode"] = new OptionSetValue(refundData.refundStatus);
            if (refundData.parentPaymentId.HasValue)
            {
                PaymentRepository paymentRepository = new PaymentRepository(this.OrgService);
                Entity parentPayment = paymentRepository.getPaymentByIdByGivenColumns(refundData.parentPaymentId.Value, new string[] { "rnt_virtualposid" });
                e["rnt_virtualposid"] = parentPayment.GetAttributeValue<string>("rnt_virtualposid");
            }

            e["rnt_transactiontypecode"] = new OptionSetValue((int)rnt_TransactionTypeCode.Refund);
            return this.OrgService.Create(e);

        }
        public void updatePaymentPartiallyRefund(Guid paymentId, decimal amount)
        {
            Entity e = new Entity("rnt_payment");
            e.Id = paymentId;
            e["rnt_refundamount"] = new Money(amount);
            e["rnt_refundstatuscode"] = new OptionSetValue((int)PaymentEnums.RefundStatus.Partially_Refund);
            this.OrgService.Update(e);
        }
        public void updatePaymentTotallyRefund(Guid paymentId, decimal amount)
        {
            Entity e = new Entity("rnt_payment");
            e.Id = paymentId;
            e["rnt_refundamount"] = new Money(amount);
            e["rnt_refundstatuscode"] = new OptionSetValue((int)PaymentEnums.RefundStatus.Totally_Refund);
            this.OrgService.Update(e);
        }
        public List<Entity> createRefund(CreateRefundParameters createRefundParameters)
        {
            var tempEntityList = new List<Entity>();
            this.Trace("createRefundParameters " + JsonConvert.SerializeObject(createRefundParameters));
            if (createRefundParameters.reservationId.HasValue || createRefundParameters.contractId.HasValue)
            {
                var transactionType = createRefundParameters.isDepositRefund ? PaymentEnums.PaymentTransactionType.DEPOSIT : PaymentEnums.PaymentTransactionType.SALE;
                PaymentRepository paymentRepository = new PaymentRepository(this.OrgService);
                var records = createRefundParameters.reservationId.HasValue ?
                                paymentRepository.getNotRefundedPayments_Reservation(createRefundParameters.reservationId.Value)
                              : createRefundParameters.contractId.HasValue ?
                                    paymentRepository.getNotRefundedPayments_Contract(createRefundParameters.contractId.Value, transactionType)
                                    :
                                    null;
                if (records != null)
                {
                    var amounttobeRefund = createRefundParameters.refundAmount;
                    this.Trace("amounttobeRefund : " + amounttobeRefund);
                    var totalPayment = records.Sum(p => p.GetAttributeValue<Money>("rnt_amount").Value);

                    this.Trace("totalPayment : " + totalPayment);

                    //todo will read from xml
                    if (amounttobeRefund > totalPayment && transactionType == PaymentEnums.PaymentTransactionType.SALE)
                    {
                        throw new Exception("refund amount can not be bigger than total amount");
                    }
                    List<PaymentObject> paymentObjects = new List<PaymentObject>();
                    #region iterate exisiting payment records and decide they will be partially refund or totally refund
                    foreach (var item in records)
                    {

                        var paymentAmount = decimal.Zero;
                        if (item.Attributes.Contains("rnt_refundamount"))
                        {
                            paymentAmount = item.GetAttributeValue<Money>("rnt_amount").Value - item.GetAttributeValue<Money>("rnt_refundamount").Value;
                        }
                        else
                        {
                            paymentAmount = item.GetAttributeValue<Money>("rnt_amount").Value;
                        }
                        var refundAmount = decimal.Zero;
                        //partially refund 
                        if (paymentAmount > amounttobeRefund)
                        {
                            this.Trace("payment is bigger than amounttobeRefund , payment amount : " + paymentAmount);
                            refundAmount = amounttobeRefund;
                            paymentObjects.Add(new PaymentObject
                            {
                                refundStatus = (int)ClassLibrary._Enums_1033.rnt_RefundStatus.PartiallyRefund,
                                paymentId = item.Id,
                                refundAmount = refundAmount
                            });

                        }
                        //totally refund
                        else if (paymentAmount == amounttobeRefund)
                        {
                            this.Trace("payment is equal amounttobeRefund");
                            refundAmount = amounttobeRefund;
                            paymentObjects.Add(new PaymentObject
                            {
                                refundStatus = (int)ClassLibrary._Enums_1033.rnt_RefundStatus.TotallyRefund,
                                paymentId = item.Id,
                                refundAmount = refundAmount
                            });
                        }
                        //if amount to be refund is bigger than the payment we need to iterate more
                        else if (paymentAmount < amounttobeRefund)
                        {
                            this.Trace("payment is less than amounttobeRefund");
                            refundAmount = paymentAmount;
                            paymentObjects.Add(new PaymentObject
                            {
                                refundStatus = (int)ClassLibrary._Enums_1033.rnt_RefundStatus.TotallyRefund,
                                paymentId = item.Id,
                                refundAmount = refundAmount
                            });
                        }
                        //create the refud type payment records
                        this.Trace("refund record created start");

                        var paymentCopy = item;
                        paymentCopy.Attributes.Remove("ownerid");
                        paymentCopy.Attributes.Remove("rnt_paymentid");
                        paymentCopy.Attributes.Remove("rnt_refundstatuscode");
                        paymentCopy.Attributes.Remove("rnt_creditcardslipid");

                        paymentCopy["rnt_amount"] = new Money(refundAmount);
                        paymentCopy["rnt_comission"] = null;
                        paymentCopy["rnt_creditcardslipid"] = null;
                        paymentCopy["rnt_transactiontypecode"] = new OptionSetValue((int)PaymentEnums.PaymentTransactionType.REFUND);
                        paymentCopy["rnt_transactionresult"] = new OptionSetValue((int)PaymentEnums.PaymentTransactionResult.Draft);
                        paymentCopy["rnt_parentpaymentid"] = new EntityReference("rnt_payment", item.Id);
                        paymentCopy.Id = Guid.NewGuid();
                        var refundId = this.OrgService.Create(paymentCopy);

                        this.Trace("refund record created end with id: " + refundId);
                        tempEntityList.Add(paymentCopy);
                        amounttobeRefund = amounttobeRefund - paymentAmount;
                        if (amounttobeRefund <= 0)
                        {
                            break;
                        }
                    }
                    #endregion

                    #region Getting parameters and create instance wiht activator
                    ConfigurationBL configurationBL = new ConfigurationBL(this.OrgService);

                    

                    this.Trace("instance ready");

                    #endregion

                    //now lets start with iyzico integration
                    foreach (var item in tempEntityList)
                    {
                        IPaymentProvider paymentProvider = null;
                        var configrefundParams = new CreateRefundParameters();
                        var provider = item.GetAttributeValue<OptionSetValue>("rnt_paymentprovider");
                        int providerCode = 0;
                        if (provider != null)
                        {
                            providerCode = provider.Value;
                        }
                        else
                        {
                            providerCode = getProviderCode(createRefundParameters.reservationId, createRefundParameters.contractId);
                        }
                        if (providerCode == (int)GlobalEnums.PaymentProvider.nettahsilat)
                        {
                            this.Trace("instance --> nettahsilat");
                            paymentProvider = Activator.CreateInstance(typeof(NetTahsilatProvider)) as IPaymentProvider;

                            var netTahsilatInfo = configurationBL.GetConfigurationByName("nettahsilatconfiguration");
                            var configs = NetTahsilatHelper.setConfigurationValues(netTahsilatInfo);

                            configrefundParams.userName = configs.nettahsilat_username;
                            configrefundParams.password = configs.nettahsilat_password;
                            configrefundParams.baseurl = configs.nettahsilat_url;
                            configrefundParams.vendorCode = configs.nettahsilat_vendorcode;
                            configrefundParams.vendorId = configs.nettahsilat_vendorid;
                        }
                        else if (providerCode == (int)GlobalEnums.PaymentProvider.iyzico)
                        {
                            this.Trace("instance --> iyzico");
                            paymentProvider = Activator.CreateInstance(typeof(IyzicoProvider)) as IPaymentProvider;

                            var iyzicoInfo = configurationBL.GetConfigurationByName("iyzicoInformation");
                            var configs = IyzicoHelper.setConfigurationValues(iyzicoInfo);
                            //setting auth info
                            configrefundParams.baseurl = configs.iyzico_baseUrl;
                            configrefundParams.secretKey = configs.iyzico_secretKey;
                            configrefundParams.apikey = configs.iyzico_apiKey;

                        }

                        #region Refund operations
                        var refundParams = new CreateRefundParameters
                        {
                            reservationId = createRefundParameters.reservationId,
                            paymentTransactionId = item.GetAttributeValue<string>("rnt_paymenttransactionid"),
                            conversationId = item.GetAttributeValue<string>("rnt_conversationid"),
                            refundAmount = item.GetAttributeValue<Money>("rnt_amount").Value,
                            langId = createRefundParameters.langId
                        };
                        refundParams.userName = configrefundParams.userName;
                        refundParams.password = configrefundParams.password;
                        refundParams.baseurl = configrefundParams.baseurl;
                        refundParams.secretKey = configrefundParams.secretKey;
                        refundParams.apikey = configrefundParams.apikey;
                        refundParams.vendorCode = configrefundParams.vendorCode;
                        refundParams.vendorId = configrefundParams.vendorId;

                        this.Trace("refundParams  " + JsonConvert.SerializeObject(refundParams));
                        var response = paymentProvider.makeRefund(refundParams);
                        if (response.errorMessage == "errFailNotFoundErrorMessage")
                        {
                            response.errorMessage = StaticHelper.paymentNotFoundReplacement;
                        }
                        this.Trace("refund response " + JsonConvert.SerializeObject(response));
                        #endregion

                        //todo success will handle from enum
                        this.updateRefundResult(item.Id, response);
                        if (response.status)
                        {
                            item["rnt_transactionresult"] = new OptionSetValue((int)PaymentEnums.PaymentTransactionResult.Success);
                        }
                        else
                        {
                            item["rnt_transactionresult"] = new OptionSetValue((int)PaymentEnums.PaymentTransactionResult.Error);
                        }
                        if (response.status)
                        {
                            #region Update payment records partially refund or totally refund
                            var pay = paymentObjects.Where(p => p.paymentId.Equals(item.GetAttributeValue<EntityReference>("rnt_parentpaymentid").Id)).FirstOrDefault();
                            if (pay?.refundStatus == (int)ClassLibrary._Enums_1033.rnt_RefundStatus.PartiallyRefund)
                            {
                                if (!item.Attributes.Contains("rnt_refundamount"))
                                {
                                    item.Attributes.Add("rnt_refundamount", new Money(pay.refundAmount));
                                }

                                if (!item.Attributes.Contains("rnt_refundstatuscode"))
                                {
                                    item.Attributes.Add("rnt_refundstatuscode", new OptionSetValue((int)PaymentEnums.RefundStatus.Partially_Refund));
                                }

                                this.updatePaymentPartiallyRefund(pay.paymentId, pay.refundAmount);
                            }
                            else if (pay?.refundStatus == (int)ClassLibrary._Enums_1033.rnt_RefundStatus.TotallyRefund)
                            {
                                if (!item.Attributes.Contains("rnt_refundamount"))
                                {
                                    item.Attributes.Add("rnt_refundamount", new Money(pay.refundAmount));
                                }

                                if (!item.Attributes.Contains("rnt_refundstatuscode"))
                                {
                                    item.Attributes.Add("rnt_refundstatuscode", new OptionSetValue((int)PaymentEnums.RefundStatus.Totally_Refund));
                                }

                                this.updatePaymentTotallyRefund(pay.paymentId, pay.refundAmount);
                            }
                            #endregion
                        }
                        // todo failure will create payment activity
                    }
                }
            }

            return tempEntityList;
        }
        public void createRefundForDeposit(CreateRefundParameters createRefundParameters)
        {
            var tempEntityList = new List<Entity>();

            if (createRefundParameters.contractId.HasValue)
            {
                PaymentRepository paymentRepository = new PaymentRepository(this.OrgService);
                var record = paymentRepository.getNotRefundedDepositPayment_Contract(createRefundParameters.contractId.Value);
                if (record != null)
                {
                    this.Trace("available payment record found for deposit");

                    var amounttobeRefund = createRefundParameters.refundAmount;

                    this.Trace("amounttobeRefund : " + amounttobeRefund);

                    var depositPayment = record.Attributes.Contains("rnt_amount") ? record.GetAttributeValue<Money>("rnt_amount").Value : decimal.Zero;
                    //todo will read from xml
                    if (amounttobeRefund > depositPayment)
                    {
                        throw new Exception("refund amount can not be bigger than total amount");
                    }
                    List<PaymentObject> paymentObjects = new List<PaymentObject>();
                    #region iterate exisiting payment records and decide they will be partially refund or totally refund

                    var refundAmount = decimal.Zero;
                    //partially refund 
                    if (depositPayment > amounttobeRefund)
                    {
                        this.Trace("payment is bigger than amounttobeRefund , payment amount : " + depositPayment);
                        refundAmount = amounttobeRefund;
                        paymentObjects.Add(new PaymentObject
                        {
                            refundStatus = (int)ClassLibrary._Enums_1033.rnt_RefundStatus.PartiallyRefund,
                            paymentId = record.Id,
                            refundAmount = refundAmount
                        });

                    }
                    //totally refund
                    else if (depositPayment == amounttobeRefund)
                    {
                        this.Trace("payment is equal amounttobeRefund");
                        refundAmount = amounttobeRefund;
                        paymentObjects.Add(new PaymentObject
                        {
                            refundStatus = (int)ClassLibrary._Enums_1033.rnt_RefundStatus.TotallyRefund,
                            paymentId = record.Id,
                            refundAmount = refundAmount
                        });
                    }
                    //if amount to be refund is bigger than the payment we need to iterate more
                    else if (depositPayment < amounttobeRefund)
                    {
                        this.Trace("payment is less than amounttobeRefund");
                        refundAmount = depositPayment;
                        paymentObjects.Add(new PaymentObject
                        {
                            refundStatus = (int)ClassLibrary._Enums_1033.rnt_RefundStatus.TotallyRefund,
                            paymentId = record.Id,
                            refundAmount = refundAmount
                        });
                    }
                    //create the refud type payment records
                    this.Trace("refund record created start");

                    var paymentCopy = record;
                    paymentCopy.Attributes.Remove("ownerid");
                    paymentCopy.Attributes.Remove("rnt_paymentid");
                    paymentCopy.Attributes.Remove("rnt_refundstatuscode");
                    paymentCopy["rnt_amount"] = new Money(refundAmount);
                    paymentCopy["rnt_comission"] = null;
                    paymentCopy["rnt_transactiontypecode"] = new OptionSetValue((int)PaymentEnums.PaymentTransactionType.REFUND);
                    paymentCopy["rnt_transactionresult"] = new OptionSetValue((int)PaymentEnums.PaymentTransactionResult.Draft);
                    paymentCopy["rnt_parentpaymentid"] = new EntityReference("rnt_payment", record.Id);
                    paymentCopy.Id = Guid.NewGuid();
                    var refundId = this.OrgService.Create(paymentCopy);

                    this.Trace("refund record created end with id: " + refundId);

                    tempEntityList.Add(paymentCopy);
                    #endregion

                    #region Getting parameters and create instance wiht activator
                    ConfigurationBL configurationBL = new ConfigurationBL(this.OrgService);
                    var provider = ((GlobalEnums.PaymentProvider)record.GetAttributeValue<OptionSetValue>("rnt_paymentprovider")?.Value).ToString();

                    IPaymentProvider paymentProvider = null;
                    var configrefundParams = new CreateRefundParameters();
                    if (provider == GlobalEnums.PaymentProvider.iyzico.ToString())
                    {
                        this.Trace("instance --> iyzico");
                        paymentProvider = Activator.CreateInstance(typeof(IyzicoProvider)) as IPaymentProvider;

                        var iyzicoInfo = configurationBL.GetConfigurationByName("iyzicoInformation");
                        var configs = IyzicoHelper.setConfigurationValues(iyzicoInfo);
                        //setting auth info
                        configrefundParams.baseurl = configs.iyzico_baseUrl;
                        configrefundParams.secretKey = configs.iyzico_secretKey;
                        configrefundParams.apikey = configs.iyzico_apiKey;

                    }
                    else if (provider == GlobalEnums.PaymentProvider.nettahsilat.ToString())
                    {
                        this.Trace("instance --> nettahsilat");
                        var netTahsilatInfo = configurationBL.GetConfigurationByName("nettahsilatconfiguration");
                        var configs = NetTahsilatHelper.setConfigurationValues(netTahsilatInfo);

                        configrefundParams.userName = configs.nettahsilat_username;
                        configrefundParams.password = configs.nettahsilat_password;
                        configrefundParams.baseurl = configs.nettahsilat_url;
                        configrefundParams.vendorCode = configs.nettahsilat_vendorcode;
                        configrefundParams.vendorId = configs.nettahsilat_vendorid;

                        paymentProvider = Activator.CreateInstance(typeof(NetTahsilatProvider)) as IPaymentProvider;
                    }
                    this.Trace("instance ready");
                    #endregion
                    //todo if iyzico will implement in future , this code block needs to change

                    //now lets start with iyzico integration
                    foreach (var item in tempEntityList)
                    {
                        #region Iyzico refund operations
                        var refundParams = new CreateRefundParameters
                        {
                            reservationId = createRefundParameters.reservationId,
                            paymentTransactionId = item.GetAttributeValue<string>("rnt_paymenttransactionid"),
                            conversationId = item.GetAttributeValue<string>("rnt_conversationid"),
                            refundAmount = item.GetAttributeValue<Money>("rnt_amount").Value,
                            langId = createRefundParameters.langId
                        };
                        refundParams.userName = configrefundParams.userName;
                        refundParams.password = configrefundParams.password;
                        refundParams.baseurl = configrefundParams.baseurl;
                        refundParams.secretKey = configrefundParams.secretKey;
                        refundParams.apikey = configrefundParams.apikey;
                        this.Trace("refundParams  " + JsonConvert.SerializeObject(refundParams));
                        var response = paymentProvider.makeRefund(refundParams);
                        this.Trace("iyzico refund response " + response.status);
                        #endregion

                        //todo success will handle from enum
                        this.updateRefundResult(item.Id, response);

                        if (response.status)
                        {
                            #region Update payment records partially refund or totally refund
                            var pay = paymentObjects.Where(p => p.paymentId.Equals(item.GetAttributeValue<EntityReference>("rnt_parentpaymentid").Id)).FirstOrDefault();
                            if (pay?.refundStatus == (int)ClassLibrary._Enums_1033.rnt_RefundStatus.PartiallyRefund)
                            {
                                this.updatePaymentPartiallyRefund(pay.paymentId, pay.refundAmount);
                            }
                            else if (pay?.refundStatus == (int)ClassLibrary._Enums_1033.rnt_RefundStatus.TotallyRefund)
                            {
                                this.updatePaymentTotallyRefund(pay.paymentId, pay.refundAmount);
                            }
                            #endregion
                        }
                        // todo failure will create payment activity
                    }
                }
            }
        }
        public void calculateRollupRelatedFields(Entity payment)
        {
            if (payment.Attributes.Contains("rnt_contractid") && payment.Attributes.Contains("rnt_transactiontypecode") &&
                  (payment.GetAttributeValue<OptionSetValue>("rnt_transactionresult").Value == (int)PaymentEnums.PaymentTransactionResult.Success ||
                  payment.GetAttributeValue<OptionSetValue>("rnt_transactionresult").Value == (int)PaymentEnums.PaymentTransactionResult.Success_WithoutPaymentTransactions))
            {
                ContractBL contractBl = new ContractBL(this.OrgService, this.TracingService);
                this.Trace("rollup field calculation started for contract");
                this.Trace("res Id  : " + payment);
                this.Trace("rnt_transactiontypecode  : " + payment.GetAttributeValue<OptionSetValue>("rnt_transactiontypecode").Value);

                contractBl.calcultePaymentRelatedRollupFields(payment.GetAttributeValue<EntityReference>("rnt_contractid").Id,
                                                                 payment.GetAttributeValue<OptionSetValue>("rnt_transactiontypecode").Value);
                this.Trace("rollup field calculation end for contract");
            }
            else if (payment.Attributes.Contains("rnt_reservationid") && payment.Attributes.Contains("rnt_transactiontypecode") &&
               (payment.GetAttributeValue<OptionSetValue>("rnt_transactionresult").Value == (int)PaymentEnums.PaymentTransactionResult.Success ||
                payment.GetAttributeValue<OptionSetValue>("rnt_transactionresult").Value == (int)PaymentEnums.PaymentTransactionResult.Success_WithoutPaymentTransactions))
            {
                ReservationBL reservationBL = new ReservationBL(this.OrgService, this.TracingService);
                this.Trace("rollup field calculation started for reservation");
                this.Trace("res Id  : " + payment.Id);
                this.Trace("rnt_transactiontypecode  : " + payment.GetAttributeValue<OptionSetValue>("rnt_transactiontypecode").Value);

                reservationBL.calcultePaymentRelatedRollupFields(payment.GetAttributeValue<EntityReference>("rnt_reservationid").Id,
                                                                 payment.GetAttributeValue<OptionSetValue>("rnt_transactiontypecode").Value);
                this.Trace("rollup field calculation end for reservation");

            }
        }
        public void checkAndCreatePaymentForReverseIntegration(IyzicoTransactionObject iyzicoTransactionObject)
        {
            Entity paymentTransaction = new Entity("rnt_payment");

            LinkEntity linkIyziLink = new LinkEntity();
            linkIyziLink.EntityAlias = "iyziLinkAlias";
            linkIyziLink.LinkFromAttributeName = "rnt_paymentid";
            linkIyziLink.LinkFromEntityName = "rnt_paymnent";
            linkIyziLink.LinkToAttributeName = "rnt_paymentid";
            linkIyziLink.LinkToEntityName = "rnt_iyzilinktransaction";
            linkIyziLink.LinkCriteria.AddCondition("rnt_conversationid", ConditionOperator.Equal, iyzicoTransactionObject.ConversationId);
            linkIyziLink.JoinOperator = JoinOperator.LeftOuter;

            LinkEntity linkContract = new LinkEntity();
            linkContract.EntityAlias = "contractAlias";
            linkContract.LinkFromAttributeName = "rnt_contractid";
            linkContract.LinkFromEntityName = "rnt_payment";
            linkContract.LinkToAttributeName = "rnt_contractid";
            linkContract.LinkToEntityName = "rnt_contract";
            linkContract.LinkCriteria.AddCondition("rnt_pnrnumber", ConditionOperator.Equal, iyzicoTransactionObject.ConversationId);
            linkContract.JoinOperator = JoinOperator.LeftOuter;

            LinkEntity linkReservation = new LinkEntity();
            linkReservation.EntityAlias = "reservastionAlias";
            linkReservation.LinkFromAttributeName = "rnt_reservationid";
            linkReservation.LinkFromEntityName = "rnt_payment";
            linkReservation.LinkToAttributeName = "rnt_reservationid";
            linkReservation.LinkToEntityName = "rnt_reservation";
            linkReservation.LinkCriteria.AddCondition("rnt_pnrnumber", ConditionOperator.Equal, iyzicoTransactionObject.ConversationId);
            linkReservation.JoinOperator = JoinOperator.LeftOuter;


            QueryExpression getPayment = new QueryExpression("rnt_payment");
            getPayment.ColumnSet = new ColumnSet(true);
            getPayment.Criteria.AddCondition("rnt_paymenttransactionid", ConditionOperator.Equal, Convert.ToString(iyzicoTransactionObject.PaymentTransactionId));
            getPayment.Criteria.AddCondition("rnt_paymentresultid", ConditionOperator.Equal, Convert.ToString(iyzicoTransactionObject.PaymentId));
            if (iyzicoTransactionObject.TransactionType == "PAYMENT")
            {
                getPayment.Criteria.AddCondition("rnt_transactiontypecode", ConditionOperator.NotEqual, (int)PaymentEnums.PaymentTransactionType.REFUND);
            }
            else
            {
                getPayment.Criteria.AddCondition("rnt_transactiontypecode", ConditionOperator.Equal, (int)PaymentEnums.PaymentTransactionType.REFUND);
            }
            getPayment.LinkEntities.Add(linkContract);
            getPayment.LinkEntities.Add(linkReservation);
            getPayment.LinkEntities.Add(linkIyziLink);
            EntityCollection paymentList = this.OrgService.RetrieveMultiple(getPayment);

            if (paymentList.Entities.Count > 0)
            {
                Entity temp = paymentList.Entities[0];
                string transactionid = temp.GetAttributeValue<string>("rnt_transactionid");
                if (string.IsNullOrWhiteSpace(transactionid))
                {
                    paymentTransaction.Id = temp.Id;
                    paymentTransaction.Attributes["rnt_transactionid"] = Convert.ToString(iyzicoTransactionObject.TransactionId);
                    this.OrgService.Update(paymentTransaction);
                }

            }
            else
            {
                Entity updateIyziLink = new Entity();
                int paymentType = (int)PaymentEnums.PaymentTransactionType.SALE;
                if (iyzicoTransactionObject.TransactionType != "PAYMENT")
                {
                    paymentType = (int)PaymentEnums.PaymentTransactionType.REFUND;
                }

                paymentTransaction.Attributes["rnt_paymenttransactionid"] = Convert.ToString(iyzicoTransactionObject.PaymentTransactionId);
                paymentTransaction.Attributes["rnt_paymentresultid"] = Convert.ToString(iyzicoTransactionObject.PaymentId);
                paymentTransaction.Attributes["rnt_transactiontypecode"] = new OptionSetValue(paymentType);
                paymentTransaction.Attributes["rnt_transactionresult"] = new OptionSetValue((int)rnt_payment_rnt_transactionresult.Success);
                paymentTransaction.Attributes["rnt_transactionid"] = Convert.ToString(iyzicoTransactionObject.TransactionId);
                paymentTransaction.Attributes["rnt_conversationid"] = Convert.ToString(iyzicoTransactionObject.ConversationId);
                paymentTransaction.Attributes["rnt_paymentchannelcode"] = new OptionSetValue((int)rnt_PaymentChannelCode.BATCH);
                paymentTransaction.Attributes["rnt_netamount"] = new Money(Convert.ToDecimal(iyzicoTransactionObject.Price));
                paymentTransaction.Attributes["rnt_amount"] = new Money(Convert.ToDecimal(iyzicoTransactionObject.Price));
                paymentTransaction.Attributes["rnt_iyzicocomission"] = new Money(Convert.ToDecimal(iyzicoTransactionObject.IyzicoCommission));
                paymentTransaction.Attributes["rnt_installment"] = Convert.ToInt32(iyzicoTransactionObject.Installment);
                paymentTransaction.Attributes["rnt_installmentamount"] = new Money(Convert.ToDecimal(iyzicoTransactionObject.Price));
                paymentTransaction.Attributes["rnt_name"] = $"IyziLink Payment - {Convert.ToDateTime(iyzicoTransactionObject.TransactionDate).ToString("yyyy-MM-dd")}";

                LinkEntity linkContractForLink = new LinkEntity();
                linkContractForLink.EntityAlias = "contractAlias";
                linkContractForLink.LinkFromAttributeName = "rnt_contractid";
                linkContractForLink.LinkFromEntityName = "rnt_iyzilinktransaction";
                linkContractForLink.LinkToAttributeName = "rnt_contractid";
                linkContractForLink.LinkToEntityName = "rnt_contract";
                linkContractForLink.Columns = new ColumnSet("rnt_customerid");
                linkContractForLink.JoinOperator = JoinOperator.LeftOuter;

                QueryExpression getIyziLinkQuery = new QueryExpression("rnt_iyzilinktransaction");
                getIyziLinkQuery.ColumnSet = new ColumnSet(true);
                getIyziLinkQuery.Criteria.AddCondition("rnt_conversationid", ConditionOperator.Equal, iyzicoTransactionObject.ConversationId);
                getIyziLinkQuery.Criteria.AddCondition("rnt_paymentid", ConditionOperator.Null);
                getIyziLinkQuery.LinkEntities.Add(linkContractForLink);
                EntityCollection iyziLinkList = this.OrgService.RetrieveMultiple(getIyziLinkQuery);
                if (iyziLinkList.Entities.Count > 0)
                {
                    Entity iyziLink = iyziLinkList.Entities[0];

                    updateIyziLink = new Entity(iyziLink.LogicalName, iyziLink.Id);
                    EntityReference contractRef = iyziLink.GetAttributeValue<EntityReference>("rnt_contractid");
                    if (contractRef != null)
                    {
                        paymentTransaction.Attributes["rnt_contractid"] = new EntityReference(contractRef.LogicalName, contractRef.Id);
                    }

                    AliasedValue customerAlias = iyziLink.GetAttributeValue<AliasedValue>("contractAlias.rnt_customerid");
                    if (customerAlias != null && customerAlias.Value != null)
                    {
                        EntityReference customerRef = (EntityReference)customerAlias.Value;
                        if (customerRef.LogicalName.ToLower() == "contact")
                        {
                            paymentTransaction.Attributes["rnt_contactid"] = new EntityReference(customerRef.LogicalName, customerRef.Id);
                        }
                    }

                }

                paymentTransaction.Id = this.OrgService.Create(paymentTransaction);


                if (updateIyziLink.Id != Guid.Empty)
                {
                    updateIyziLink.Attributes["rnt_paymentid"] = new EntityReference(paymentTransaction.LogicalName, paymentTransaction.Id);
                    updateIyziLink.Attributes["rnt_paymentdate"] = Convert.ToDateTime(iyzicoTransactionObject.TransactionDate);
                    this.OrgService.Update(updateIyziLink);
                }
            }
        }
        public bool Payment3dReturn(Payment3dReturnParameters payment3DReturnParameters)
        {
            this.Trace($"payment 3d return parameters : {JsonConvert.SerializeObject(payment3DReturnParameters)}");

            bool result = false;
            if (!payment3DReturnParameters.reservationId.HasValue)
                return result;

            ReservationRepository reservationRepository = new ReservationRepository(this.OrgService);
            var reservation = reservationRepository.getReservationById(payment3DReturnParameters.reservationId.Value, new string[] { "rnt_pnrnumber" });

            QueryExpression getPaymentItems = new QueryExpression("rnt_payment");
            getPaymentItems.ColumnSet = new ColumnSet(true);
            getPaymentItems.Criteria.AddCondition("rnt_reservationid", ConditionOperator.Equal, payment3DReturnParameters.reservationId.Value);
            getPaymentItems.Criteria.AddCondition("rnt_transactionresult", ConditionOperator.Equal, (int)PaymentEnums.PaymentTransactionResult.WaitingFor3D);
            var paymentItems = this.OrgService.RetrieveMultiple(getPaymentItems);

            Reservation3dPaymentReturnResponse paymentResponse = new Reservation3dPaymentReturnResponse();
            if (payment3DReturnParameters.success)
            {
                ConfigurationBL configurationBL = new ConfigurationBL(this.OrgService);
                var provider = getProviderCode(payment3DReturnParameters.reservationId, payment3DReturnParameters.contractId);
                this.Trace($"Provider code:{provider}");

                if (provider == (int)GlobalEnums.PaymentProvider.iyzico)
                {
                    this.Trace("Iyzico");
                    #region Iyzico Config Info
                    var paymentProvider = Activator.CreateInstance(typeof(IyzicoProvider)) as IPaymentProvider;
                    var iyzicoInfo = configurationBL.GetConfigurationByName("iyzicoInformation");
                    var configs = IyzicoHelper.setConfigurationValues(iyzicoInfo);
                    var Iyzico3DReturnParameter = new Iyzico3DReturnParameter()
                    {
                        ConversationData = payment3DReturnParameters.conversationData,
                        PaymentId = payment3DReturnParameters.providerPaymentId,
                        ConversationId = payment3DReturnParameters.conversationId

                    };
                    Iyzico3DReturnParameter.baseurl = configs.iyzico_baseUrl;
                    Iyzico3DReturnParameter.secretKey = configs.iyzico_secretKey;
                    Iyzico3DReturnParameter.apikey = configs.iyzico_apiKey;
                    #endregion

                    this.Trace($"Iyzico3DReturnParameter : {JsonConvert.SerializeObject(Iyzico3DReturnParameter)}");
                    paymentResponse = paymentProvider.Payment3dReturn(Iyzico3DReturnParameter);
                    result = paymentResponse.status;
                    this.Trace($"Payment3dReturn result: {result}");

                }
                else if (provider == (int)GlobalEnums.PaymentProvider.nettahsilat)
                {
                    //TODO Net Tahsilat
                }

            }

            if (result)
            {
                var reservationStatusCode = (int)ReservationEnums.StatusCode.New;

                this.Trace("reservation set state");
                ReservationBL reservationBL = new ReservationBL(this.OrgService);
                reservationBL.setReservationState(payment3DReturnParameters.reservationId.Value, reservationStatusCode);

                this.Trace("payment set state");
                CreditCardBL creditCardBL = new CreditCardBL(this.OrgService, this.TracingService);
                var paymentItem = paymentItems.Entities.FirstOrDefault();

                this.Trace($"payment item : {paymentItem.Id}, status:{payment3DReturnParameters.success.ToString()}");
                var creditCard = paymentItem.GetAttributeValue<EntityReference>("rnt_customercreditcardid");
                this.Trace("updating payment, card state");
                setPaymentTransactionResultState(paymentItem.Id, (int)rnt_payment_rnt_transactionresult.Success);
                this.Trace($"creditCardId: {creditCard.Id}");
                creditCardBL.setCreditCardStatus(creditCard.Id, (int)rnt_customercreditcard_StatusCode.Active);
                updatePaymentResult(paymentItem.Id, paymentResponse);

            }
            else
            {
                OrganizationRequest organizationRequest = new OrganizationRequest("rnt_CancelReservation");
                organizationRequest["langId"] = 1055;
                organizationRequest["reservationId"] = payment3DReturnParameters.reservationId.ToString();
                organizationRequest["pnrNumber"] = reservation.GetAttributeValue<string>("rnt_pnrnumber");
                organizationRequest["cancellationReason"] = (int)rnt_reservation_StatusCode.CancelledByRentgo;
                organizationRequest["cancellationDescription"] = "Müşteri tarafından iptal - 3D Başarısız";
                organizationRequest["cancellationSubReason"] = 100000009;
                var response = this.OrgService.Execute(organizationRequest);
            }

            return result;

        }

        public int getProviderCode(Guid? reservastionId, Guid? contractId)
        {
            SystemParameterBL systemParameterBL = new SystemParameterBL(this.OrgService);
            int providerCode = systemParameterBL.getProvider();

            LinkEntity linkEntity = new LinkEntity();
            linkEntity.EntityAlias = "linkEntity";
            linkEntity.LinkFromAttributeName = "rnt_branchid";
            linkEntity.LinkFromEntityName = "rnt_branch";
            if (contractId.HasValue && contractId.Value != Guid.Empty)
            {
                linkEntity.LinkToEntityName = "rnt_contract";
                linkEntity.LinkToAttributeName = "rnt_pickupbranchid";
                linkEntity.LinkCriteria.AddCondition("rnt_contractid", ConditionOperator.Equal, contractId.Value);
            }
            else if (reservastionId.HasValue && reservastionId.Value != Guid.Empty)
            {
                linkEntity.LinkToEntityName = "rnt_reservation";
                linkEntity.LinkToAttributeName = "rnt_pickupbranchid";
                linkEntity.LinkCriteria.AddCondition("rnt_reservationid", ConditionOperator.Equal, reservastionId.Value);
            }

            QueryExpression branchQuery = new QueryExpression("rnt_branch");
            branchQuery.ColumnSet = new ColumnSet("rnt_paymentprovider");
            branchQuery.LinkEntities.Add(linkEntity);
            EntityCollection branchList = this.OrgService.RetrieveMultiple(branchQuery);
            if (branchList.Entities.Count > 0)
            {
                OptionSetValue paymentProvider = branchList.Entities[0].GetAttributeValue<OptionSetValue>("rnt_paymentprovider");
                if (paymentProvider != null)
                {
                    providerCode = paymentProvider.Value;
                }
            }


            return providerCode;
        }

        public int getProviderCode(Guid pickupBranchId)//Bu metod, verilen pickupBranchId göre, ödeme sağlayıcısını belirlemek için kullanılır.
        {
            SystemParameterBL systemParameterBL = new SystemParameterBL(this.OrgService);
            int providerCode = systemParameterBL.getProvider();//pickupBranchId'ye özel bir ödeme sağlayıcısı belirtilmemişse sistem parametresi kullanılır

            Entity branch = this.OrgService.Retrieve("rnt_branch", pickupBranchId, new ColumnSet("rnt_paymentprovider"));
            if (branch != null && branch.Id != Guid.Empty)//pickupBranchId'ye özel bir ödeme sağlayıcısı belirtilmişse if bloğuna girer
            {
                OptionSetValue paymentProvider = branch.GetAttributeValue<OptionSetValue>("rnt_paymentprovider");
                if (paymentProvider != null)
                {
                    providerCode = paymentProvider.Value;
                }
            }


            return providerCode;
        }
        public EntityCollection getPaymentForReverseIntegration(DateTime processDate)
        {
            int queryCount = 5000;
            int pageNumber = 1;
            string pagingCookie = null;
            EntityCollection paymentList = new EntityCollection();
            while (true)
            {
                LinkEntity linkIyziLink = new LinkEntity();
                linkIyziLink.EntityAlias = "iyziLinkAlias";
                linkIyziLink.LinkFromAttributeName = "rnt_paymentid";
                linkIyziLink.LinkFromEntityName = "rnt_paymnent";
                linkIyziLink.LinkToAttributeName = "rnt_paymentid";
                linkIyziLink.LinkToEntityName = "rnt_iyzilinktransaction";
                linkIyziLink.Columns = new ColumnSet("rnt_conversationid");
                linkIyziLink.JoinOperator = JoinOperator.LeftOuter;

                LinkEntity linkContract = new LinkEntity();
                linkContract.EntityAlias = "contractAlias";
                linkContract.LinkFromAttributeName = "rnt_contractid";
                linkContract.LinkFromEntityName = "rnt_payment";
                linkContract.LinkToAttributeName = "rnt_contractid";
                linkContract.LinkToEntityName = "rnt_contract";
                linkContract.Columns = new ColumnSet("rnt_pnrnumber");
                linkContract.JoinOperator = JoinOperator.LeftOuter;

                LinkEntity linkReservation = new LinkEntity();
                linkReservation.EntityAlias = "reservastionAlias";
                linkReservation.LinkFromAttributeName = "rnt_reservationid";
                linkReservation.LinkFromEntityName = "rnt_payment";
                linkReservation.LinkToAttributeName = "rnt_reservationid";
                linkReservation.LinkToEntityName = "rnt_reservation";
                linkReservation.Columns = new ColumnSet("rnt_pnrnumber");
                linkReservation.JoinOperator = JoinOperator.LeftOuter;


                QueryExpression getPaymentQuery = new QueryExpression("rnt_payment");
                getPaymentQuery.ColumnSet = new ColumnSet(true);
                getPaymentQuery.Criteria.AddCondition("createdon", ConditionOperator.OnOrAfter, processDate.AddDays(-1));
                getPaymentQuery.Criteria.AddCondition("createdon", ConditionOperator.OnOrBefore, processDate.AddDays(1));
                getPaymentQuery.LinkEntities.Add(linkContract);
                getPaymentQuery.LinkEntities.Add(linkReservation);
                getPaymentQuery.LinkEntities.Add(linkIyziLink);
                getPaymentQuery.PageInfo = new PagingInfo
                {
                    PageNumber = pageNumber,
                    Count = queryCount,
                    PagingCookie = pagingCookie
                };
                EntityCollection results = this.OrgService.RetrieveMultiple(getPaymentQuery);
                paymentList.Entities.AddRange(results.Entities);

                if (results.MoreRecords)
                {
                    pageNumber++;
                    pagingCookie = results.PagingCookie;
                }
                else
                {
                    break;
                }
            }
            return paymentList;
        }

        public EntityCollection getIyzilinkTransaction()
        {
            LinkEntity linkContractForLink = new LinkEntity();
            linkContractForLink.EntityAlias = "contractAlias";
            linkContractForLink.LinkFromAttributeName = "rnt_contractid";
            linkContractForLink.LinkFromEntityName = "rnt_iyzilinktransaction";
            linkContractForLink.LinkToAttributeName = "rnt_contractid";
            linkContractForLink.LinkToEntityName = "rnt_contract";
            linkContractForLink.Columns = new ColumnSet("rnt_customerid");
            linkContractForLink.JoinOperator = JoinOperator.LeftOuter;

            QueryExpression getIyziLinkQuery = new QueryExpression("rnt_iyzilinktransaction");
            getIyziLinkQuery.ColumnSet = new ColumnSet(true);
            getIyziLinkQuery.Criteria.AddCondition("rnt_paymentid", ConditionOperator.Null);
            getIyziLinkQuery.LinkEntities.Add(linkContractForLink);
            EntityCollection iyziLinkList = this.OrgService.RetrieveMultiple(getIyziLinkQuery);
            return iyziLinkList;
        }

        public void checkAndCreatePaymentForReverseIntegration(EntityCollection paymentList, IyzicoTransactionObject iyzicoTransactionObject)
        {
            Entity paymentTransaction = new Entity("rnt_payment");
            List<Entity> tempList = paymentList.Entities.Where(x => x.GetAttributeValue<string>("rnt_paymenttransactionid") == Convert.ToString(iyzicoTransactionObject.PaymentTransactionId) && x.GetAttributeValue<string>("rnt_paymentresultid") == Convert.ToString(iyzicoTransactionObject.PaymentId)).ToList();
            if (tempList.Count > 0)
            {
                foreach (var temp in tempList)
                {
                    OptionSetValue transactionType = temp.GetAttributeValue<OptionSetValue>("rnt_transactiontypecode");
                    string transactionid = temp.GetAttributeValue<string>("rnt_transactionid");
                    if (string.IsNullOrWhiteSpace(transactionid))
                    {
                        if (iyzicoTransactionObject.TransactionType != "PAYMENT" && transactionType.Value == (int)PaymentEnums.PaymentTransactionType.REFUND)
                        {
                            paymentTransaction.Id = temp.Id;
                            paymentTransaction.Attributes["rnt_transactionid"] = Convert.ToString(iyzicoTransactionObject.TransactionId);
                            this.OrgService.Update(paymentTransaction);
                            break;
                        }
                        else if (iyzicoTransactionObject.TransactionType == "PAYMENT" && transactionType.Value != (int)PaymentEnums.PaymentTransactionType.REFUND)
                        {
                            paymentTransaction.Id = temp.Id;
                            paymentTransaction.Attributes["rnt_transactionid"] = Convert.ToString(iyzicoTransactionObject.TransactionId);
                            this.OrgService.Update(paymentTransaction);
                            break;
                        }
                    }
                }
            }
            else
            {
                Entity updateIyziLink = new Entity();
                int paymentType = (int)PaymentEnums.PaymentTransactionType.SALE;
                if (iyzicoTransactionObject.TransactionType != "PAYMENT")
                {
                    paymentType = (int)PaymentEnums.PaymentTransactionType.REFUND;
                }
                ReservationRepository reservationRepository = new ReservationRepository(this.OrgService);
                Entity reservation = reservationRepository.getReservationByPnrNumber(Convert.ToString(iyzicoTransactionObject.ConversationId));
                if (reservation != null)
                {
                    paymentTransaction.Attributes["rnt_reservationid"] = new EntityReference(reservation.LogicalName, reservation.Id);
                    EntityReference contractRef = reservation.GetAttributeValue<EntityReference>("rnt_contractnumber");
                    EntityReference contactRef = reservation.GetAttributeValue<EntityReference>("rnt_customerid");
                    if (contractRef != null && contractRef.Id != Guid.Empty)
                    {
                        paymentTransaction.Attributes["rnt_contractid"] = contractRef;
                    }
                    if (contactRef != null && contactRef.Id != Guid.Empty)
                    {
                        paymentTransaction.Attributes["rnt_contactid"] = contactRef;
                    }
                }
                paymentTransaction.Attributes["rnt_paymenttransactionid"] = Convert.ToString(iyzicoTransactionObject.PaymentTransactionId);
                paymentTransaction.Attributes["rnt_paymentresultid"] = Convert.ToString(iyzicoTransactionObject.PaymentId);
                paymentTransaction.Attributes["rnt_transactiontypecode"] = new OptionSetValue(paymentType);
                paymentTransaction.Attributes["rnt_transactionresult"] = new OptionSetValue((int)rnt_payment_rnt_transactionresult.Success);
                paymentTransaction.Attributes["rnt_transactionid"] = Convert.ToString(iyzicoTransactionObject.TransactionId);
                paymentTransaction.Attributes["rnt_conversationid"] = Convert.ToString(iyzicoTransactionObject.ConversationId);
                paymentTransaction.Attributes["rnt_paymentchannelcode"] = new OptionSetValue((int)rnt_PaymentChannelCode.BATCH);
                paymentTransaction.Attributes["rnt_netamount"] = new Money(Convert.ToDecimal(iyzicoTransactionObject.Price, CultureInfo.GetCultureInfo("en-US")));
                paymentTransaction.Attributes["rnt_amount"] = new Money(Convert.ToDecimal(iyzicoTransactionObject.Price, CultureInfo.GetCultureInfo("en-US")));
                paymentTransaction.Attributes["rnt_iyzicocomission"] = new Money(Convert.ToDecimal(iyzicoTransactionObject.IyzicoCommission, CultureInfo.GetCultureInfo("en-US")));
                paymentTransaction.Attributes["rnt_installment"] = Convert.ToInt32(iyzicoTransactionObject.Installment);
                paymentTransaction.Attributes["rnt_installmentamount"] = new Money(Convert.ToDecimal(iyzicoTransactionObject.Price, CultureInfo.GetCultureInfo("en-US")));
                paymentTransaction.Attributes["rnt_paymentprovider"] = new OptionSetValue((int)GlobalEnums.PaymentProvider.iyzico);
                paymentTransaction.Attributes["rnt_name"] = $"Iyzico Payment - {Convert.ToDateTime(iyzicoTransactionObject.TransactionDate).ToString("yyyy-MM-dd")}";

                LinkEntity linkContractForLink = new LinkEntity();
                linkContractForLink.EntityAlias = "contractAlias";
                linkContractForLink.LinkFromAttributeName = "rnt_contractid";
                linkContractForLink.LinkFromEntityName = "rnt_iyzilinktransaction";
                linkContractForLink.LinkToAttributeName = "rnt_contractid";
                linkContractForLink.LinkToEntityName = "rnt_contract";
                linkContractForLink.Columns = new ColumnSet("rnt_customerid");
                linkContractForLink.JoinOperator = JoinOperator.LeftOuter;

                QueryExpression getIyziLinkQuery = new QueryExpression("rnt_iyzilinktransaction");
                getIyziLinkQuery.ColumnSet = new ColumnSet(true);
                getIyziLinkQuery.Criteria.AddCondition("rnt_conversationid", ConditionOperator.Equal, iyzicoTransactionObject.ConversationId);
                getIyziLinkQuery.Criteria.AddCondition("rnt_paymentid", ConditionOperator.Null);
                getIyziLinkQuery.LinkEntities.Add(linkContractForLink);
                EntityCollection iyziLinkList = this.OrgService.RetrieveMultiple(getIyziLinkQuery);
                if (iyziLinkList.Entities.Count > 0)
                {
                    Entity iyziLink = iyziLinkList.Entities[0];

                    updateIyziLink = new Entity(iyziLink.LogicalName, iyziLink.Id);
                    EntityReference contractRef = iyziLink.GetAttributeValue<EntityReference>("rnt_contractid");
                    if (contractRef != null)
                    {
                        paymentTransaction.Attributes["rnt_contractid"] = new EntityReference(contractRef.LogicalName, contractRef.Id);
                    }

                    AliasedValue customerAlias = iyziLink.GetAttributeValue<AliasedValue>("contractAlias.rnt_customerid");
                    if (customerAlias != null && customerAlias.Value != null)
                    {
                        EntityReference customerRef = (EntityReference)customerAlias.Value;
                        if (customerRef.LogicalName.ToLower() == "contact")
                        {
                            paymentTransaction.Attributes["rnt_contactid"] = new EntityReference(customerRef.LogicalName, customerRef.Id);
                        }
                    }

                }

                paymentTransaction.Id = this.OrgService.Create(paymentTransaction);


                if (updateIyziLink.Id != Guid.Empty)
                {
                    updateIyziLink.Attributes["rnt_paymentid"] = new EntityReference(paymentTransaction.LogicalName, paymentTransaction.Id);
                    updateIyziLink.Attributes["rnt_paymentdate"] = Convert.ToDateTime(iyzicoTransactionObject.TransactionDate);
                    this.OrgService.Update(updateIyziLink);
                }
            }
        }
        //Aşağıdaki metot, Iyzico ödeme sağlayıcısından gelen bir ödeme işlemi için bir RNT (Reservation and Ticketing System) ödeme kaydı oluşturur ve Iyzilink varlığına ödeme kaydının Id'si ve tarihi ekler.
        public void checkIyzilinkPayment(EntityCollection iyzilinkList, IyzicoTransactionObject iyzicoTransactionObject)
        {

            Entity iyziLink = iyzilinkList.Entities.Where(x => x.GetAttributeValue<string>("rnt_conversationid") == Convert.ToString(iyzicoTransactionObject.ConversationId)).FirstOrDefault();//Bu satır, Iyzico ile yapılan ödemenin konuşma kimliğinin iyzilinkList içinde var olup olmadığını kontrol eder ve sonuç olarak iyziLink adlı bir Entity nesnesi döndürür.

            if (iyziLink != null && iyziLink.Id != Guid.Empty)
            {
                // iyziLink varlığı null değilse ve Id'si boş değilse, eşleşme var demektir ve ödeme işlemi için bir RNT ödeme kaydı oluşturulması gerekir.
                Entity updateIyziLink = new Entity(iyziLink.LogicalName, iyziLink.Id);//iyziLink varlığından updateIyziLink adlı yeni bir Entity nesnesi oluşturulur. Bu, varlık güncelleneceği zaman kullanılır.
                Entity paymentTransaction = new Entity("rnt_payment"); //paymentTransaction adlı yeni bir RNT ödeme kaydı oluşturulur.
                EntityReference contractRef = iyziLink.GetAttributeValue<EntityReference>("rnt_contractid");//: iyziLink varlığından rnt_contractid alanı alınır.
                if (contractRef != null)
                {//: contractRef null değilse, rnt_contractid alanı paymentTransaction nesnesine eklenir.
                    paymentTransaction.Attributes["rnt_contractid"] = new EntityReference(contractRef.LogicalName, contractRef.Id);
                }

                AliasedValue customerAlias = iyziLink.GetAttributeValue<AliasedValue>("contractAlias.rnt_customerid");//: iyziLink varlığından contractAlias.rnt_customerid alanı alınır.
                if (customerAlias != null && customerAlias.Value != null)
                {//: customerAlias null değilse ve Value özelliği de null değilse, customerRef adlı yeni bir EntityReference nesnesi oluşturulur.
                    EntityReference customerRef = (EntityReference)customerAlias.Value;
                    if (customerRef.LogicalName.ToLower() == "contact")
                    {//: customerRef nesnesinin LogicalName özelliği "contact" ise, rnt_contactid alanı paymentTransaction nesnesine eklenir.
                        paymentTransaction.Attributes["rnt_contactid"] = new EntityReference(customerRef.LogicalName, customerRef.Id);
                    }
                }

                int paymentType = (int)PaymentEnums.PaymentTransactionType.SALE;
                if (iyzicoTransactionObject.TransactionType != "PAYMENT")
                {//iyzico ödeme işlemlerinin geri ödeme işlemi mi yoksa ödeme işlemi mi olduğunu kontrol eder ve buna göre işlemin tipini belirler.
                    paymentType = (int)PaymentEnums.PaymentTransactionType.REFUND;
                }
                ReservationRepository reservationRepository = new ReservationRepository(this.OrgService);
                Entity reservation = reservationRepository.getReservationByPnrNumber(Convert.ToString(iyzicoTransactionObject.ConversationId));
                if (reservation != null)
                {
                    paymentTransaction.Attributes["rnt_reservationid"] = new EntityReference(reservation.LogicalName, reservation.Id);
                    if (contractRef == null || contractRef.Id == Guid.Empty)
                    {
                        contractRef = reservation.GetAttributeValue<EntityReference>("rnt_contractnumber");
                    }
                    EntityReference contactRef = reservation.GetAttributeValue<EntityReference>("rnt_customerid");
                    if (contractRef != null && contractRef.Id != Guid.Empty)
                    {
                        paymentTransaction.Attributes["rnt_contractid"] = contractRef;
                    }
                    if (contactRef != null && contactRef.Id != Guid.Empty)
                    {
                        paymentTransaction.Attributes["rnt_contactid"] = contactRef;
                    }
                }
                paymentTransaction.Attributes["rnt_paymenttransactionid"] = Convert.ToString(iyzicoTransactionObject.PaymentTransactionId);
                paymentTransaction.Attributes["rnt_paymentresultid"] = Convert.ToString(iyzicoTransactionObject.PaymentId);
                paymentTransaction.Attributes["rnt_transactiontypecode"] = new OptionSetValue(paymentType);
                paymentTransaction.Attributes["rnt_transactionresult"] = new OptionSetValue((int)rnt_payment_rnt_transactionresult.Success);
                paymentTransaction.Attributes["rnt_transactionid"] = Convert.ToString(iyzicoTransactionObject.TransactionId);
                paymentTransaction.Attributes["rnt_conversationid"] = Convert.ToString(iyzicoTransactionObject.ConversationId);
                paymentTransaction.Attributes["rnt_paymentchannelcode"] = new OptionSetValue((int)rnt_PaymentChannelCode.BATCH);
                paymentTransaction.Attributes["rnt_netamount"] = new Money(Convert.ToDecimal(iyzicoTransactionObject.Price, CultureInfo.GetCultureInfo("en-US")));
                paymentTransaction.Attributes["rnt_amount"] = new Money(Convert.ToDecimal(iyzicoTransactionObject.Price, CultureInfo.GetCultureInfo("en-US")));
                paymentTransaction.Attributes["rnt_iyzicocomission"] = new Money(Convert.ToDecimal(iyzicoTransactionObject.IyzicoCommission, CultureInfo.GetCultureInfo("en-US")));
                paymentTransaction.Attributes["rnt_installment"] = Convert.ToInt32(iyzicoTransactionObject.Installment);
                paymentTransaction.Attributes["rnt_installmentamount"] = new Money(Convert.ToDecimal(iyzicoTransactionObject.Price, CultureInfo.GetCultureInfo("en-US")));
                paymentTransaction.Attributes["rnt_paymentprovider"] = new OptionSetValue((int)GlobalEnums.PaymentProvider.iyzico);
                paymentTransaction.Attributes["rnt_name"] = $"Iyzico Payment - {Convert.ToDateTime(iyzicoTransactionObject.TransactionDate).ToString("yyyy-MM-dd")}";

                paymentTransaction.Id = this.OrgService.Create(paymentTransaction);

                updateIyziLink.Attributes["rnt_paymentid"] = new EntityReference(paymentTransaction.LogicalName, paymentTransaction.Id);
                updateIyziLink.Attributes["rnt_paymentdate"] = Convert.ToDateTime(iyzicoTransactionObject.TransactionDate);
                this.OrgService.Update(updateIyziLink);
            }
        }

        public void cancelPaymentsForWaiting3d(Guid reservationId)
        {
            //Bu metot, ödeme işlemi sonucu "WaitingFor3D" olan kayıtların durumlarını hızlıca değiştirmek için kullanılabilir. Örneğin, müşteri 3D doğrulama işlemini tamamlayamamış olabilir ve rezervasyon işlemi devam edemiyor olabilir. Bu durumda, bu metot çağrılarak tüm ödeme işlemi kayıtları "Error" durumuna güncellenebilir ve müşteriye yeni bir ödeme işlemi başlatması için fırsat tanınabilir.
            QueryExpression getPaymentItems = new QueryExpression("rnt_payment");//rnt_payment" varlığına ait kayıtları sorgulamak için bir QueryExpression nesnesi oluşturduk
         
            getPaymentItems.ColumnSet = new ColumnSet(true);
            getPaymentItems.Criteria.AddCondition("rnt_reservationid", ConditionOperator.Equal, reservationId.ToString());
            //Bu sorgu, reservationId parametresiyle belirtilen rezervasyona ait olan ve ödeme işlemi durumu "WaitingFor3D" olan kayıtları getirir.
            getPaymentItems.Criteria.AddCondition("rnt_transactionresult", ConditionOperator.Equal, (int)PaymentEnums.PaymentTransactionResult.WaitingFor3D);
            var paymentItems = this.OrgService.RetrieveMultiple(getPaymentItems);//RetrieveMultiple() metodu çağrılarak sorgudan elde edilen kayıtların tümü alınır ve foreach döngüsü kullanılarak her bir kaydın durumu "Error" olarak ayarlanır. Bunun için, setPaymentTransactionResultState() metodu çağrılır ve ödeme kaydının Id'si ve durum kodu (rnt_payment_rnt_transactionresult.Error) parametre olarak verilir.

            foreach (var paymentItem in paymentItems.Entities)
            {
                setPaymentTransactionResultState(paymentItem.Id, (int)rnt_payment_rnt_transactionresult.Error);
            }
        }
        //Aşağıdaki metot, bir Microsoft Dynamics 365 (eski adıyla Dynamics CRM) örneğinde "rnt_payment" adlı bir özel varlık için ödeme işlemi sonucu durumunu ayarlamak için kullanılmaktadır.
        public void setPaymentTransactionResultState(Guid paymentId, int statusCode)
        {
            //Bu metot, "rnt_payment" varlığına ait bir kaydın "rnt_transactionresult" alanına, statusCode parametresinde belirtilen değeri atayarak ödeme işlemi sonucu durumunu günceller.
            Entity e = new Entity("rnt_payment");//"Entity" sınıfı, Dynamics 365/CRM varlıklarının temsil edilmesi için kullanılan bir sınıftır.
            e.Id = paymentId;// yeni bir "rnt_payment" varlığı örneği oluşturulur ve paymentId parametresiyle belirtilen GUID değeri ile eşleştirilir.
            e["rnt_transactionresult"] = new OptionSetValue(statusCode);//"rnt_transactionresult" alanı yeni bir OptionSetValue nesnesi ile ayarlanır
            this.OrgService.Update(e);//değişiklikler kaydedilir.
        }
        //Aşağıdaki fonksiyon, bir kredi kartı varlığı ve bir kanal kodu parametresi alır ve üç boyutlu güvenlik (3D Secure) kontrolünün gerekip gerekmediğini belirlemek için kullanılır.
        public bool check3DSecure(Entity card, int channelCode)
        {
            //İlk olarak, "SystemParameterRepository" sınıfından bir nesne oluşturulur ve "getExists3dForChannel" yöntemi kullanılarak belirli bir kanal için 3D Secure ayarları alınır.
            bool result = false;
            SystemParameterRepository systemParameterRepository = new SystemParameterRepository(this.OrgService);
            //Ardından, "channelCodes" değişkenine, belirli kanal koduna sahip olan 3D Secure ayarları atanır.
            var channelCodes = systemParameterRepository.getExists3dForChannel().Where(c => c.Value == channelCode).ToList();

            if (channelCodes.Count > 0)
            {
                //"channelCodes" dizisi boş değilse, fonksiyon devam eder ve kredi kartı bilgileri kontrol edilir. Eğer kredi kartı yoksa veya "rnt_carduserkey" ve "rnt_cardtoken" alanları boşsa, fonksiyon "true" döner, yani 3D Secure gereksinimi yoktur.
                if (card == null ||
                   (string.IsNullOrEmpty(card.GetAttributeValue<string>("rnt_carduserkey")) && string.IsNullOrEmpty(card.GetAttributeValue<string>("rnt_cardtoken"))))
                {
                    result = true;
                }
                //Eğer kart varsa ve "statuscode" alanı "WaitingFor3D" durumunda ise, fonksiyon yine "true" döner, yani 3D Secure gereksinimi vardır.
                else if (card.GetAttributeValue<OptionSetValue>("statuscode").Value == (int)ClassLibrary._Enums_1033.rnt_customercreditcard_StatusCode.WaitingFor3D)
                {
                    result = true;
                }
            }
            //Fonksiyon, 3D Secure gereksinimine göre sonuç döndürür. "true" değeri, 3D Secure gereksiniminin olmadığını, "false" değeri ise 3D Secure gereksiniminin olduğunu belirtir.

            return result;
        }
        //Aşağıdaki metot bir "WebBrowser" nesnesi belge yükleme işlemi tamamlandığında kullanılabilir. Yönetici tarafından ayarlanan "currentUrl" ve "isCompleted" değişkenleri, daha sonra kodda kullanılabilir ve yüklü belge hakkında bilgi sağlayabilir veya belge yüklenene kadar diğer işlemlerin devam etmesini engelleyebilir.
        private void WebBrowser_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            currentUrl = ((WebBrowser)sender).Url.ToString();
            isCompleted = true;
        }
    }
}
