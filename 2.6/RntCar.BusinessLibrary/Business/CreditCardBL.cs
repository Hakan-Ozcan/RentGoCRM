using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Tooling.Connector;
using Newtonsoft.Json;
using RntCar.BusinessLibrary.Handlers;
using RntCar.BusinessLibrary.Repository;
using RntCar.ClassLibrary;
using RntCar.ClassLibrary._Enums_1033;
using RntCar.PaymentHelper;
using RntCar.PaymentHelper.iyzico;
using RntCar.PaymentHelper.NetTahsilat;
using RntCar.SDK.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using static RntCar.ClassLibrary.GlobalEnums;

namespace RntCar.BusinessLibrary.Business
{
    public class CreditCardBL : BusinessHandler
    {
        public CreditCardBL(IOrganizationService orgService) : base(orgService)
        {
        }

        public CreditCardBL(IOrganizationService orgService, ITracingService tracingService) : base(orgService, tracingService)
        {
        }

        public CreditCardBL(CrmServiceClient crmServiceClient) : base(crmServiceClient)
        {
        }

        public CreateCreditCardResponse createCreditCard(CreateCreditCardParameters createCreditCardParameters)
        {

            Guid _individualCustomerId = Guid.Empty;
            XrmHelper xrmHelper = new XrmHelper(this.OrgService);

            if (!Guid.TryParse(createCreditCardParameters.individualCustomerId, out _individualCustomerId))
            {
                return new CreateCreditCardResponse
                {
                    ResponseResult = ResponseResult.ReturnError(xrmHelper.GetXmlTagContentByGivenLangId("InvalidCustomerId",
                                                                                                        createCreditCardParameters.langId,
                                                                                                        this.paymentXmlPath))
                };
            }
            this.Trace(DateTime.Now + " individual retrieve start");
            IndividualCustomerRepository individualCustomerRepository = new IndividualCustomerRepository(this.OrgService);
            var result = individualCustomerRepository.getIndividualCustomerByIdWithGivenColumns(_individualCustomerId,
                                                                                                new string[] { "emailaddress1", "fullname", "rnt_customerexternalid" });
            this.Trace(DateTime.Now + " individual retrieve end");
            this.Trace("createCreditCardParameters.creditCardNumber : " + createCreditCardParameters.creditCardNumber);

            var formattedCard = CommonHelper.formatCreditCardNumber(createCreditCardParameters.creditCardNumber);
            this.Trace("credit card create started");
            //first create the card in draft status
            Entity e = new Entity("rnt_customercreditcard");
            e["rnt_contactid"] = new EntityReference("contact", _individualCustomerId);
            e["rnt_creditcardnumber"] = formattedCard;
            if (createCreditCardParameters.expireYear.ToString().Length == 2)
            {
                createCreditCardParameters.expireYear = Convert.ToInt32("20" + createCreditCardParameters.expireYear);
            }
            e["rnt_expireyear"] = new OptionSetValue((int)createCreditCardParameters.expireYear);
            e["rnt_expiremonthcode"] = new OptionSetValue((int)createCreditCardParameters.expireMonth);
            if (createCreditCardParameters.isHidden != 0)
                e["rnt_ishidden"] = new OptionSetValue(createCreditCardParameters.isHidden);
            e["rnt_name"] = result.GetAttributeValue<string>("fullname") + " - " + formattedCard;
            e["rnt_provider"] = new OptionSetValue((int)GlobalEnums.PaymentProvider.iyzico);
            var cardId = this.OrgService.Create(e);
            this.Trace("credit card create end with guid" + cardId);

            //set individual customer parameters
            createCreditCardParameters.email = result.GetAttributeValue<string>("emailaddress1");
            createCreditCardParameters.cardAlias = result.GetAttributeValue<string>("fullname");
            createCreditCardParameters.customerExternalId = result.GetAttributeValue<string>("rnt_customerexternalid");

            return new CreateCreditCardResponse()
            {
                ResponseResult = ResponseResult.ReturnSuccess(),
                creditCardId = cardId
            };

        }

        public CreateCreditCardResponse createProviderCreditCard(CreateCreditCardParameters createCreditCardParameters, Guid cardId, int provider)
        {
            IPaymentProvider paymentProvider = null;
            ConfigurationBL configurationBL = new ConfigurationBL(this.OrgService);
            this.Trace("createCreditCardParameters createProviderCreditCard" + JsonConvert.SerializeObject(createCreditCardParameters));
            if (provider == (int)GlobalEnums.PaymentProvider.iyzico)
            {
                paymentProvider = Activator.CreateInstance(typeof(IyzicoProvider)) as IPaymentProvider;
                var iyzicoInfo = configurationBL.GetConfigurationByName("iyzicoInformation");
                var configs = IyzicoHelper.setConfigurationValues(iyzicoInfo);
                //setting auth info
                createCreditCardParameters.baseurl = configs.iyzico_baseUrl;
                createCreditCardParameters.secretKey = configs.iyzico_secretKey;
                createCreditCardParameters.apikey = configs.iyzico_apiKey;
            }
            else if (provider == (int)GlobalEnums.PaymentProvider.nettahsilat)
            {
                paymentProvider = Activator.CreateInstance(typeof(NetTahsilatProvider)) as IPaymentProvider;
                var netTahsilatInfo = configurationBL.GetConfigurationByName("nettahsilatconfiguration");
                var configs = NetTahsilatHelper.setConfigurationValues(netTahsilatInfo);

                createCreditCardParameters.userName = configs.nettahsilat_username;
                createCreditCardParameters.password = configs.nettahsilat_password;
                createCreditCardParameters.baseurl = configs.nettahsilat_url;
                createCreditCardParameters.vendorCode = configs.nettahsilat_vendorcode;
                createCreditCardParameters.vendorId = configs.nettahsilat_vendorid;
            }

            var responseCard = paymentProvider.createCreditCard(createCreditCardParameters);
            this.Trace("create credit card end" + JsonConvert.SerializeObject(responseCard));

            if (!responseCard.ResponseResult.Result)
            {
                //roll back everthing we need to throw exception
                throw new Exception(responseCard.ResponseResult.ExceptionDetail);
            }
            this.Trace("update started");
            string binNumber = !string.IsNullOrWhiteSpace(createCreditCardParameters.creditCardNumber) ? createCreditCardParameters.creditCardNumber.Substring(0, 6) : string.Empty;
            Entity updateEntity = new Entity("rnt_customercreditcard");
            updateEntity["rnt_binnumber"] = !string.IsNullOrWhiteSpace(responseCard.binNumber) ? responseCard.binNumber : binNumber;
            updateEntity["rnt_bank"] = responseCard.bankName;
            if (!string.IsNullOrEmpty(responseCard.cardFamily))
                updateEntity["rnt_cardprogramcode"] = new OptionSetValue(decideCardProgram(responseCard.cardFamily));
            if (!string.IsNullOrEmpty(responseCard.cardAssociation))
                updateEntity["rnt_cardtypecode"] = new OptionSetValue(decideCardType(responseCard.cardAssociation));
            if (!string.IsNullOrEmpty(responseCard.cardType))
            {
                var cardOrganizationType = decideCardOrganizationType(responseCard.cardType);
                updateEntity["rnt_cardorganizationtype"] = cardOrganizationType.HasValue ? new OptionSetValue(cardOrganizationType.Value) : null;
            }

            updateEntity["rnt_conversationid"] = responseCard.conversationId;
            updateEntity["rnt_carduserkey"] = responseCard.cardUserKey;
            updateEntity["rnt_cardtoken"] = responseCard.cardToken;
            updateEntity["rnt_externalid"] = responseCard.externalId;
            updateEntity["rnt_bankcode"] = Convert.ToString(responseCard.bankCode);
            updateEntity["rnt_provider"] = new OptionSetValue(provider);
            updateEntity.Id = cardId;
            this.OrgService.Update(updateEntity);
            this.Trace("update end");

            responseCard.creditCardId = cardId;
            responseCard.isHidden = createCreditCardParameters.isHidden;
            responseCard.emailAddress = createCreditCardParameters.email;
            return responseCard;
        }

        public CreateCreditCardResponse createCreditCard_Iyzco(CreateCreditCardParameters createCreditCardParameters, Guid cardId)
        {
            ConfigurationBL configurationBL = new ConfigurationBL(this.OrgService);
            var iyzicoInfo = configurationBL.GetConfigurationByName("iyzicoInformation");

            var splittedInfo = iyzicoInfo.Split(';');
            this.Trace("iyzico_baseUrl" + splittedInfo[0]);
            this.Trace("iyzico_apiKey" + splittedInfo[1]);
            this.Trace("iyzico_secretKey" + splittedInfo[2]);
            this.Trace("create credit card stared");
            IyzicoHelper iyzicoHelper = new IyzicoHelper(new IyzicoConfiguration
            {
                iyzico_apiKey = splittedInfo[1],
                iyzico_baseUrl = splittedInfo[0],
                iyzico_secretKey = splittedInfo[2]
            });
            this.Trace("before create credit card in iyzico : " + JsonConvert.SerializeObject(createCreditCardParameters));
            var responseCard = iyzicoHelper.createCreditCard(createCreditCardParameters);
            this.Trace("create credit card end" + JsonConvert.SerializeObject(responseCard));

            if (!responseCard.ResponseResult.Result)
            {
                //roll back everthing we need to throw exception
                throw new Exception(responseCard.ResponseResult.ExceptionDetail);
            }
            this.Trace("update started");

            Entity updateEntity = new Entity("rnt_customercreditcard");
            updateEntity["rnt_binnumber"] = responseCard.binNumber;
            updateEntity["rnt_bank"] = responseCard.bankName;
            if (!string.IsNullOrEmpty(responseCard.cardFamily))
                updateEntity["rnt_cardprogramcode"] = new OptionSetValue(decideCardProgram(responseCard.cardFamily));
            if (!string.IsNullOrEmpty(responseCard.cardAssociation))
                updateEntity["rnt_cardtypecode"] = new OptionSetValue(decideCardType(responseCard.cardAssociation));
            if (!string.IsNullOrEmpty(responseCard.cardType))
            {
                var cardOrganizationType = decideCardOrganizationType(responseCard.cardType);
                updateEntity["rnt_cardorganizationtype"] = cardOrganizationType.HasValue ? new OptionSetValue(cardOrganizationType.Value) : null;
            }

            updateEntity["rnt_conversationid"] = responseCard.conversationId;
            updateEntity["rnt_carduserkey"] = responseCard.cardUserKey;
            updateEntity["rnt_cardtoken"] = responseCard.cardToken;
            updateEntity["rnt_externalid"] = responseCard.externalId;
            updateEntity["rnt_bankcode"] = Convert.ToString(responseCard.bankCode);
            updateEntity["rnt_provider"] = new OptionSetValue((int)GlobalEnums.PaymentProvider.iyzico);
            updateEntity.Id = cardId;
            this.OrgService.Update(updateEntity);
            this.Trace("update end");

            responseCard.creditCardId = cardId;
            responseCard.isHidden = createCreditCardParameters.isHidden;
            responseCard.emailAddress = createCreditCardParameters.email;
            return responseCard;
        }

        public Guid createCreditCard_nettahsilat(CreateCreditCardParameters createCreditCardParameters,
                                                 PaymentResponse paymentResponse = null)
        {
            this.Trace("createCreditCardParameters : " + JsonConvert.SerializeObject(createCreditCardParameters));
            this.Trace("paymentResponse : " + JsonConvert.SerializeObject(paymentResponse));

            var formattedCard = CommonHelper.formatCreditCardNumber(createCreditCardParameters.creditCardNumber);
            this.Trace("credit card create started with formatted value : " + formattedCard);
            //first create the card in draft status
            Entity e = new Entity("rnt_customercreditcard");
            this.Trace("createCreditCardParameters.individualCustomerId : " + createCreditCardParameters.individualCustomerId);

            e["rnt_contactid"] = new EntityReference("contact", new Guid(createCreditCardParameters.individualCustomerId));
            e["rnt_creditcardnumber"] = formattedCard;

            this.Trace("createCreditCardParameters.expireYear : " + createCreditCardParameters.expireYear);
            if (createCreditCardParameters.expireYear.ToString().Length == 2)
            {
                createCreditCardParameters.expireYear = Convert.ToInt32("20" + createCreditCardParameters.expireYear);
            }
            this.Trace("createCreditCardParameters.expireMonth : " + createCreditCardParameters.expireMonth);

            e["rnt_expireyear"] = new OptionSetValue((int)createCreditCardParameters.expireYear);
            e["rnt_expiremonthcode"] = new OptionSetValue((int)createCreditCardParameters.expireMonth);
            e["rnt_name"] = formattedCard;
            if (paymentResponse != null)
            {
                e["rnt_binnumber"] = paymentResponse.cardBin;
                e["rnt_bank"] = paymentResponse.cardGroup;

                this.Trace("paymentResponse.cardType : " + paymentResponse.cardType);
                if (!string.IsNullOrEmpty(paymentResponse.cardType))
                {
                    e["rnt_cardtypecode"] = new OptionSetValue(decideCardType_nettahsilat(paymentResponse.cardType));
                }
                this.Trace("paymentResponse.cardOrganizationType : " + paymentResponse.cardOrganizationType);

                if (!string.IsNullOrEmpty(paymentResponse.cardOrganizationType))
                {
                    var cardOrganizationType = decideCardOrganizationType_nettahsilat(paymentResponse.cardOrganizationType);
                    e["rnt_cardorganizationtype"] = cardOrganizationType.HasValue ? new OptionSetValue(cardOrganizationType.Value) : null;
                }

                e["rnt_carduserkey"] = paymentResponse.merchantSafeKey;
                e["rnt_cardtoken"] = paymentResponse.merchantSafeKey;
            }
            e["rnt_provider"] = new OptionSetValue((int)GlobalEnums.PaymentProvider.nettahsilat);
            return this.OrgService.Create(e);

        }



        public InstallmentResponse retrieveInstallmentforGivenCard(RetrieveInstallmentParameters retrieveInstallmentParameters)
        {
            SystemParameterBL systemParameterBL = new SystemParameterBL(this.OrgService);
            ConfigurationBL configurationBL = new ConfigurationBL(this.OrgService);
            IPaymentProvider paymentProvider = null;
            var provider = retrieveInstallmentParameters.provider.HasValue ? retrieveInstallmentParameters.provider.Value : systemParameterBL.getProvider();
            //todo will handle from enum
            if (provider == (int)GlobalEnums.PaymentProvider.iyzico)
            {
                this.Trace("iyzico instance");
                var iyzicoInfo = configurationBL.GetConfigurationByName("iyzicoInformation");
                var configs = IyzicoHelper.setConfigurationValues(iyzicoInfo);
                retrieveInstallmentParameters.apikey = configs.iyzico_apiKey;
                retrieveInstallmentParameters.baseurl = configs.iyzico_baseUrl;
                retrieveInstallmentParameters.secretKey = configs.iyzico_secretKey;
                paymentProvider = Activator.CreateInstance(typeof(IyzicoProvider)) as IPaymentProvider;
            }
            else if (provider == (int)GlobalEnums.PaymentProvider.nettahsilat)
            {
                this.Trace("net tahsilat instance");
                var netTahsilatInfo = configurationBL.GetConfigurationByName("nettahsilatconfiguration");
                var configs = NetTahsilatHelper.setConfigurationValues(netTahsilatInfo);
                retrieveInstallmentParameters.baseurl = configs.nettahsilat_url;
                retrieveInstallmentParameters.userName = configs.nettahsilat_username;
                retrieveInstallmentParameters.password = configs.nettahsilat_password;
                retrieveInstallmentParameters.vendorCode = configs.nettahsilat_vendorcode;
                retrieveInstallmentParameters.vendorId = configs.nettahsilat_vendorid;
                paymentProvider = Activator.CreateInstance(typeof(NetTahsilatProvider)) as IPaymentProvider;
            }
            this.Trace("before retrieve installment");
            var r = paymentProvider.retrieveInstallment(retrieveInstallmentParameters);

            if (!systemParameterBL.getInstallmentEnabled())
            {
                List<List<InstallmentData>> data = new List<List<InstallmentData>>();
                data.Add(r.installmentData.FirstOrDefault().Where(p => p.installmentNumber == 1).ToList());
                r.installmentData = data;
            }

            this.Trace("end retrieve installment");
            return r;

        }

        public VirtualPosResponse retrieveVirtualPosIdforGivenCard(RetrieveVirtualPosIdParameters retrieveVirtualPosIdParameters)
        {
            ConfigurationBL configurationBL = new ConfigurationBL(this.OrgService);
            SystemParameterBL systemParameterBL = new SystemParameterBL(this.OrgService);
            IPaymentProvider paymentProvider = null;
            var provider = retrieveVirtualPosIdParameters.provider.HasValue ? retrieveVirtualPosIdParameters.provider.Value : systemParameterBL.getProvider();
            //todo will handle from enum
            if (provider == (int)GlobalEnums.PaymentProvider.iyzico)
            {
                LogoBankCodeMappingRepository logoBankCodeMappingRepository = new LogoBankCodeMappingRepository(this.OrgService);
                Entity logoBankCodeEntity = logoBankCodeMappingRepository.getIyzicoVirtualPosId();
                return new VirtualPosResponse
                {
                    virtualPosId = Convert.ToInt32(logoBankCodeEntity.GetAttributeValue<string>("rnt_name")),
                    ResponseResult = ResponseResult.ReturnSuccess()
                };
            }
            else if (provider == (int)GlobalEnums.PaymentProvider.nettahsilat)
            {
                this.Trace("net tahsilat instance");
                var netTahsilatInfo = configurationBL.GetConfigurationByName("nettahsilatconfiguration");
                var configs = NetTahsilatHelper.setConfigurationValues(netTahsilatInfo);
                retrieveVirtualPosIdParameters.userName = configs.nettahsilat_username;
                retrieveVirtualPosIdParameters.password = configs.nettahsilat_password;
                retrieveVirtualPosIdParameters.baseurl = configs.nettahsilat_url;
                retrieveVirtualPosIdParameters.vendorCode = configs.nettahsilat_vendorcode;
                retrieveVirtualPosIdParameters.vendorId = configs.nettahsilat_vendorid;
                paymentProvider = Activator.CreateInstance(typeof(NetTahsilatProvider)) as IPaymentProvider;
            }
            return paymentProvider.retrieveDefaultPosIdforGivenCard(retrieveVirtualPosIdParameters);
        }
        public GetCustomerCreditCardsResponse getCustomerCreditCards(string customerId,
                                                                     Guid? reservationId,
                                                                     Guid? contractId,
                                                                     Guid? pickupBranchId)
        {
            string[] columns = new string[] { "rnt_binnumber",
                                              "rnt_conversationid",
                                              "rnt_externalid",
                                              "rnt_carduserkey",
                                              "rnt_cardtoken",
                                              "rnt_creditcardnumber",
                                              "rnt_expireyear",
                                              "rnt_expiremonthcode",
                                              "rnt_cardtypecode",
                                              "rnt_name"
                                            };
            CreditCardRepository creditCardRepository = new CreditCardRepository(this.OrgService, this.CrmServiceClient);
            PaymentBL paymentBL = new PaymentBL(this.OrgService);
            int? provider = null;
            if (pickupBranchId.HasValue && pickupBranchId.Value != Guid.Empty)
            {
                this.Trace("pickupBranchId.HasValue");
                provider = paymentBL.getProviderCode(pickupBranchId.Value);
            }
            else if ((reservationId.HasValue && reservationId != Guid.Empty) || (contractId.HasValue && contractId.Value != Guid.Empty))
            {
                this.Trace("else pickupBranchId.HasValue");
                provider = paymentBL.getProviderCode(reservationId, contractId);
            }
            var result = creditCardRepository.getCreditCardsByCustomerIdWithGivenColumns(new Guid(customerId), provider, columns);
            List<CreditCardData> creditCardDatas = new List<CreditCardData>();

            var selectedCreditCardId = string.Empty;

            PaymentRepository paymentRepository = new PaymentRepository(this.OrgService);
            if (reservationId.HasValue && reservationId != Guid.Empty)
            {
                var lastPayment = paymentRepository.getLastPayment_Reservation(reservationId.Value);
                if (lastPayment != null)
                    selectedCreditCardId = lastPayment.GetAttributeValue<EntityReference>("rnt_customercreditcardid")?.Id.ToString();
            }
            else if (contractId.HasValue && contractId != Guid.Empty)
            {
                var lastPayment = paymentRepository.getLastPayment_Contract(contractId.Value);
                this.Trace("lastPayment : " + lastPayment);
                if (lastPayment != null)
                    selectedCreditCardId = lastPayment.GetAttributeValue<EntityReference>("rnt_customercreditcardid")?.Id.ToString();
            }

            foreach (var item in result)
            {
                creditCardDatas.Add(new CreditCardData
                {
                    cardHolderName = item.GetAttributeValue<string>("rnt_name"),
                    binNumber = item.GetAttributeValue<string>("rnt_binnumber"),
                    conversationId = item.GetAttributeValue<string>("rnt_conversationid"),
                    externalId = item.GetAttributeValue<string>("rnt_externalid"),
                    cardUserKey = item.GetAttributeValue<string>("rnt_carduserkey"),
                    cardToken = item.GetAttributeValue<string>("rnt_cardtoken"),
                    creditCardId = item.Id,
                    cardType = item.Attributes.Contains("rnt_cardtypecode") ? item.GetAttributeValue<OptionSetValue>("rnt_cardtypecode").Value : 0,
                    creditCardNumber = item.GetAttributeValue<string>("rnt_creditcardnumber"),
                    expireMonth = item.GetAttributeValue<OptionSetValue>("rnt_expiremonthcode").Value,
                    expireYear = item.GetAttributeValue<OptionSetValue>("rnt_expireyear").Value
                });
            }
            return new GetCustomerCreditCardsResponse
            {
                ResponseResult = ResponseResult.ReturnSuccess(),
                selectedCreditCardId = selectedCreditCardId,
                creditCards = creditCardDatas
            };
        }

        public CreditCardData getCustomerCreditCardByCreditCardId(Guid creditCardId)
        {
            string[] columns = new string[] { "rnt_binnumber",
                                              "rnt_conversationid",
                                              "rnt_externalid",
                                              "rnt_carduserkey",
                                              "rnt_cardtoken",
                                              "rnt_creditcardnumber",
                                              "rnt_expireyear",
                                              "rnt_expiremonthcode",
                                              "rnt_cardtypecode"
                                            };
            CreditCardRepository creditCardRepository = new CreditCardRepository(this.OrgService, this.CrmServiceClient);
            var result = creditCardRepository.getCreditCardByIdWithGivenColumns(creditCardId, columns);
            return new CreditCardData
            {

                binNumber = result.GetAttributeValue<string>("rnt_binnumber"),
                conversationId = result.GetAttributeValue<string>("rnt_conversationid"),
                externalId = result.GetAttributeValue<string>("rnt_externalid"),
                cardUserKey = result.GetAttributeValue<string>("rnt_carduserkey"),
                cardToken = result.GetAttributeValue<string>("rnt_cardtoken"),
                creditCardId = result.Id,
                cardType = result.Attributes.Contains("rnt_cardtypecode") ? result.GetAttributeValue<OptionSetValue>("rnt_cardtypecode").Value : 0,
                creditCardNumber = result.GetAttributeValue<string>("rnt_creditcardnumber"),
                expireMonth = result.GetAttributeValue<OptionSetValue>("rnt_expiremonthcode").Value,
                expireYear = result.GetAttributeValue<OptionSetValue>("rnt_expireyear").Value
            };
        }

        public GetCustomerCreditCardsResponse getCustomerCreditCardsWithProvider(string customerId, int provider,
                                                                     string reservationId = null,
                                                                     string contractId = null)
        {
            string[] columns = new string[] { "rnt_binnumber",
                                              "rnt_conversationid",
                                              "rnt_externalid",
                                              "rnt_carduserkey",
                                              "rnt_cardtoken",
                                              "rnt_creditcardnumber",
                                              "rnt_expireyear",
                                              "rnt_expiremonthcode",
                                              "rnt_cardtypecode",
                                              "rnt_name"
                                            };
            CreditCardRepository creditCardRepository = new CreditCardRepository(this.OrgService, this.CrmServiceClient);
            var result = creditCardRepository.getCreditCardsByCustomerIdAndProviderWithGivenColumns(new Guid(customerId), provider, columns);
            List<CreditCardData> creditCardDatas = new List<CreditCardData>();

            var selectedCreditCardId = string.Empty;

            PaymentRepository paymentRepository = new PaymentRepository(this.OrgService);
            if (!string.IsNullOrEmpty(reservationId))
            {
                var lastPayment = paymentRepository.getLastPayment_Reservation(Guid.Parse(reservationId));
                if (lastPayment != null)
                    selectedCreditCardId = lastPayment.GetAttributeValue<EntityReference>("rnt_customercreditcardid")?.Id.ToString();
            }
            else if (!string.IsNullOrEmpty(contractId))
            {
                var lastPayment = paymentRepository.getLastPayment_Contract(Guid.Parse(contractId));
                this.Trace("lastPayment : " + lastPayment);
                if (lastPayment != null)
                    selectedCreditCardId = lastPayment.GetAttributeValue<EntityReference>("rnt_customercreditcardid")?.Id.ToString();
            }

            foreach (var item in result)
            {
                creditCardDatas.Add(new CreditCardData
                {
                    cardHolderName = item.GetAttributeValue<string>("rnt_name"),
                    binNumber = item.GetAttributeValue<string>("rnt_binnumber"),
                    conversationId = item.GetAttributeValue<string>("rnt_conversationid"),
                    externalId = item.GetAttributeValue<string>("rnt_externalid"),
                    cardUserKey = item.GetAttributeValue<string>("rnt_carduserkey"),
                    cardToken = item.GetAttributeValue<string>("rnt_cardtoken"),
                    creditCardId = item.Id,
                    cardType = item.Attributes.Contains("rnt_cardtypecode") ? item.GetAttributeValue<OptionSetValue>("rnt_cardtypecode").Value : 0,
                    creditCardNumber = item.GetAttributeValue<string>("rnt_creditcardnumber"),
                    expireMonth = item.GetAttributeValue<OptionSetValue>("rnt_expiremonthcode").Value,
                    expireYear = item.GetAttributeValue<OptionSetValue>("rnt_expireyear").Value
                });
            }
            return new GetCustomerCreditCardsResponse
            {
                ResponseResult = ResponseResult.ReturnSuccess(),
                selectedCreditCardId = selectedCreditCardId,
                creditCards = creditCardDatas
            };
        }

        public void checkFraudCustomerControl(Guid customerId, int provider)
        {
            string[] columns = new string[] { "rnt_provider",
                                              "rnt_name"
                                            };

            CreditCardRepository creditCardRepository = new CreditCardRepository(this.OrgService, this.CrmServiceClient);
            var result = creditCardRepository.getCreditCardsByCustomerIdAndProviderWithGivenColumns(customerId, provider, columns);
            this.Trace("result.Count : " + result.Count);
            SystemParameterBL systemParameterBL = new SystemParameterBL(this.OrgService);
            int maxCount = systemParameterBL.getCustomerCreditCardLimit();
            if (maxCount != 0 && result.Count > maxCount)
            {
                SetStateRequest setStateRequest = new SetStateRequest()
                {
                    EntityMoniker = new EntityReference("contact", customerId),
                    State = new OptionSetValue((int)StateCode.Active),
                    Status = new OptionSetValue((int)Contact_StatusCode.FraudForCreditCard)
                };
                this.OrgService.Execute(setStateRequest);
            }
        }
        private int decideCardProgram(string cardFamily)
        {
            cardFamily = cardFamily.Replace("&", "_");
            //7 is equal unknown value
            var optionSetValue = 7;
            if (cardFamily.ToLower() == PaymentEnums.CreditCardFamily.Axess.ToString().ToLower())
            {
                optionSetValue = (int)PaymentEnums.CreditCardFamily.Axess;
            }
            else if (cardFamily.ToLower() == PaymentEnums.CreditCardFamily.Bonus.ToString().ToLower())
            {
                optionSetValue = (int)PaymentEnums.CreditCardFamily.Bonus;
            }
            else if (cardFamily.ToLower() == PaymentEnums.CreditCardFamily.World.ToString().ToLower())
            {
                optionSetValue = (int)PaymentEnums.CreditCardFamily.World;
            }
            else if (cardFamily.ToLower() == PaymentEnums.CreditCardFamily.Maximum.ToString().ToLower())
            {
                optionSetValue = (int)PaymentEnums.CreditCardFamily.Maximum;
            }
            else if (cardFamily.ToLower() == PaymentEnums.CreditCardFamily.Paraf.ToString().ToLower())
            {
                optionSetValue = (int)PaymentEnums.CreditCardFamily.Paraf;
            }
            else if (cardFamily.ToLower() == PaymentEnums.CreditCardFamily.CardFinans.ToString().ToLower())
            {
                optionSetValue = (int)PaymentEnums.CreditCardFamily.CardFinans;
            }
            else if (cardFamily.ToLower() == PaymentEnums.CreditCardFamily.Miles_Smiles.ToString().ToLower())
            {
                optionSetValue = (int)PaymentEnums.CreditCardFamily.Miles_Smiles;
            }
            return optionSetValue;
        }
        //private int decideCardProgram_nettahsilat(string cardFamily)
        //{
        //    cardFamily = cardFamily.Replace("&", "_");
        //    //7 is equal unknown value
        //    var optionSetValue = 7;
        //    if (cardFamily.ToLower() == PaymentEnums.CreditCardFamily.Axess.ToString().ToLower())
        //    {
        //        optionSetValue = (int)PaymentEnums.CreditCardFamily.Axess;
        //    }
        //    else if (cardFamily.ToLower() == PaymentEnums.CreditCardFamily.Bonus.ToString().ToLower())
        //    {
        //        optionSetValue = (int)PaymentEnums.CreditCardFamily.Bonus;
        //    }
        //    else if (cardFamily.ToLower() == PaymentEnums.CreditCardFamily.World.ToString().ToLower())
        //    {
        //        optionSetValue = (int)PaymentEnums.CreditCardFamily.World;
        //    }
        //    else if (cardFamily.ToLower() == PaymentEnums.CreditCardFamily.Maximum.ToString().ToLower())
        //    {
        //        optionSetValue = (int)PaymentEnums.CreditCardFamily.Maximum;
        //    }
        //    else if (cardFamily.ToLower() == PaymentEnums.CreditCardFamily.Paraf.ToString().ToLower())
        //    {
        //        optionSetValue = (int)PaymentEnums.CreditCardFamily.Paraf;
        //    }
        //    else if (cardFamily.ToLower() == PaymentEnums.CreditCardFamily.CardFinans.ToString().ToLower())
        //    {
        //        optionSetValue = (int)PaymentEnums.CreditCardFamily.CardFinans;
        //    }
        //    else if (cardFamily.ToLower() == PaymentEnums.CreditCardFamily.Miles_Smiles.ToString().ToLower())
        //    {
        //        optionSetValue = (int)PaymentEnums.CreditCardFamily.Miles_Smiles;
        //    }
        //    return optionSetValue;
        //}
        private int decideCardType(string cardType)
        {
            //50 is equal unknown value
            var optionSetValue = (int)PaymentEnums.CreditCardType.UNKNOWN;
            if (cardType == PaymentEnums.CreditCardType.MASTER_CARD.ToString())
            {
                optionSetValue = (int)PaymentEnums.CreditCardType.MASTER_CARD;
            }
            else if (cardType == PaymentEnums.CreditCardType.VISA.ToString())
            {
                optionSetValue = (int)PaymentEnums.CreditCardType.VISA;
            }
            else if (cardType == PaymentEnums.CreditCardType.AMERICAN_EXPRESS.ToString())
            {
                optionSetValue = (int)PaymentEnums.CreditCardType.AMERICAN_EXPRESS;
            }
            else if (cardType == PaymentEnums.CreditCardType.TROY.ToString())
            {
                optionSetValue = (int)PaymentEnums.CreditCardType.TROY;
            }
            return optionSetValue;
        }

        private int decideCardType_nettahsilat(string cardType)
        {
            //50 is equal unknown value
            var optionSetValue = (int)PaymentEnums.CreditCardType.UNKNOWN;
            if (cardType.ToLower() == "master")
            {
                optionSetValue = (int)PaymentEnums.CreditCardType.MASTER_CARD;
            }
            else if (cardType.ToLower() == "visa")
            {
                optionSetValue = (int)PaymentEnums.CreditCardType.VISA;
            }
            //todo will get from net tahsilat
            else if (cardType.ToLower() == PaymentEnums.CreditCardType.AMERICAN_EXPRESS.ToString())
            {
                optionSetValue = (int)PaymentEnums.CreditCardType.AMERICAN_EXPRESS;
            }
            else if (cardType.ToLower() == PaymentEnums.CreditCardType.TROY.ToString())
            {
                optionSetValue = (int)PaymentEnums.CreditCardType.TROY;
            }
            return optionSetValue;
        }

        private int? decideCardOrganizationType(string cardAssociation)
        {
            int? optionSetValue = null;
            if (cardAssociation == PaymentEnums.CreditCardOrganizationType.CREDIT_CARD.ToString())
            {
                optionSetValue = (int)PaymentEnums.CreditCardOrganizationType.CREDIT_CARD;
            }
            else if (cardAssociation == PaymentEnums.CreditCardOrganizationType.DEBIT_CARD.ToString())
            {
                optionSetValue = (int)PaymentEnums.CreditCardOrganizationType.DEBIT_CARD;
            }
            else if (cardAssociation == PaymentEnums.CreditCardType.AMERICAN_EXPRESS.ToString())
            {
                optionSetValue = (int)PaymentEnums.CreditCardType.AMERICAN_EXPRESS;
            }
            else if (cardAssociation == PaymentEnums.CreditCardType.TROY.ToString())
            {
                optionSetValue = (int)PaymentEnums.CreditCardType.TROY;
            }
            return optionSetValue;
        }

        private int? decideCardOrganizationType_nettahsilat(string cardAssociation)
        {
            int? optionSetValue = null;
            if (cardAssociation.ToLower() == "credit")
            {
                optionSetValue = (int)PaymentEnums.CreditCardOrganizationType.CREDIT_CARD;
            }
            else if (cardAssociation.ToLower() == "debit")
            {
                optionSetValue = (int)PaymentEnums.CreditCardOrganizationType.DEBIT_CARD;
            }
            else if (cardAssociation.ToLower() == "amex")
            {
                optionSetValue = (int)PaymentEnums.CreditCardType.AMERICAN_EXPRESS;
            }
            else if (cardAssociation.ToLower() == "troy")
            {
                optionSetValue = (int)PaymentEnums.CreditCardType.TROY;
            }
            return optionSetValue;
        }

        public void setCreditCardStatus(Guid creditCardId, int statusCode)
        {
            Entity e = new Entity("rnt_customercreditcard");
            e.Id = creditCardId;
            e["statuscode"] = new OptionSetValue(statusCode);
            this.OrgService.Update(e);

        }

        public void updateCreditCard_nettahsilat(Guid creditCartId, PaymentResponse paymentResponse)
        {
            this.Trace("paymentResponse : " + JsonConvert.SerializeObject(paymentResponse));
            //first create the card in draft status
            Entity e = new Entity("rnt_customercreditcard", creditCartId);
            e["rnt_binnumber"] = paymentResponse.cardBin;
            e["rnt_bank"] = paymentResponse.cardGroup;
            this.Trace("paymentResponse.cardType : " + paymentResponse.cardType);
            if (!string.IsNullOrEmpty(paymentResponse.cardType))
            {
                e["rnt_cardtypecode"] = new OptionSetValue(decideCardType_nettahsilat(paymentResponse.cardType));
            }
            this.Trace("paymentResponse.cardOrganizationType : " + paymentResponse.cardOrganizationType);
            if (!string.IsNullOrEmpty(paymentResponse.cardOrganizationType))
            {
                var cardOrganizationType = decideCardOrganizationType_nettahsilat(paymentResponse.cardOrganizationType);
                e["rnt_cardorganizationtype"] = cardOrganizationType.HasValue ? new OptionSetValue(cardOrganizationType.Value) : null;
            }
            e["rnt_carduserkey"] = paymentResponse.merchantSafeKey;
            e["rnt_cardtoken"] = paymentResponse.merchantSafeKey;
            this.OrgService.Update(e);
        }

        public Entity GetCreditCardWithProvider(CreatePaymentParameters param, int provider, CreditCardRepository creditCardRepository, string _expireYear)
        {
            Entity card = new Entity();
            if (param.creditCardData.creditCardId.HasValue && !string.IsNullOrWhiteSpace(param.creditCardData.creditCardNumber))
            {
                card = creditCardRepository.getCreditCardByIdWithGivenColumns(param.creditCardData.creditCardId.Value, new string[] { "rnt_carduserkey", "rnt_cardtoken", "rnt_provider", "rnt_binnumber", "statuscode" });
                if (card.GetAttributeValue<OptionSetValue>("rnt_provider").Value == provider)
                {
                    return card;
                }
                else
                {
                    XrmHelper xrmHelper = new XrmHelper(this.OrgService);
                    throw new Exception(xrmHelper.GetXmlTagContentByGivenLangId("InvalidProviderCreditCard", param.langId, this.paymentXmlPath));
                }
            }
            else if (string.IsNullOrEmpty(param.creditCardData.cardToken) && string.IsNullOrEmpty(param.creditCardData.cardUserKey))
            {
                EntityCollection cardList = creditCardRepository.getCustomerCreditCardsListByGivenParametersByGivenColumns(param.individualCustomerId,
                                                                                                         CommonHelper.formatCreditCardNumber(param.creditCardData.creditCardNumber.removeEmptyCharacters()),
                                                                                                         Convert.ToInt32(_expireYear),
                                                                                                         param.creditCardData.expireMonth.Value,
                                                                                                         new string[] { "rnt_carduserkey", "rnt_cardtoken", "rnt_provider", "rnt_binnumber", "statuscode" });
                card = cardList.Entities.Where(x => x.GetAttributeValue<OptionSetValue>("rnt_provider") != null && x.GetAttributeValue<OptionSetValue>("rnt_provider").Value == provider).FirstOrDefault();
                if (card == null)
                {
                    card = cardList.Entities.Where(x => x.GetAttributeValue<OptionSetValue>("rnt_provider") == null && string.IsNullOrEmpty(x.GetAttributeValue<string>("rnt_carduserkey")) && string.IsNullOrEmpty(x.GetAttributeValue<string>("rnt_cardtoken"))).FirstOrDefault();
                }
            }
            else
            {
                EntityCollection cardList = creditCardRepository.getCreditCardListByUserKeyandCreditCardTokenByGivenColumns(param.creditCardData.cardUserKey, param.creditCardData.cardToken, new string[] { "rnt_carduserkey", "rnt_cardtoken", "rnt_provider", "rnt_binnumber", "statuscode" });
                card = cardList.Entities.Where(x => x.GetAttributeValue<OptionSetValue>("rnt_provider") != null && x.GetAttributeValue<OptionSetValue>("rnt_provider").Value == provider).FirstOrDefault();
                if (card == null)
                {
                    card = cardList.Entities.Where(x => x.GetAttributeValue<OptionSetValue>("rnt_provider") == null).FirstOrDefault();
                }
            }

            return card;
        }

    }
}
