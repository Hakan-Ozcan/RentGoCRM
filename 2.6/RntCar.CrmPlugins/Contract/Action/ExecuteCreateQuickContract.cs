using Microsoft.Xrm.Sdk;
using Newtonsoft.Json;
using RntCar.BusinessLibrary.Business;
using RntCar.BusinessLibrary.Helpers;
using RntCar.BusinessLibrary.Repository;
using RntCar.BusinessLibrary.Validations;
using RntCar.ClassLibrary;
using RntCar.ClassLibrary._Enums_1033;
using RntCar.ClassLibrary.MongoDB;
using RntCar.SDK.Common;
using RntCar.SDK.Mappers;
using System;
using System.Linq;

namespace RntCar.CrmPlugins.Contract.Action
{
    public class ExecuteCreateQuickContract : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            PluginInitializer initializer = new PluginInitializer(serviceProvider);

            try
            {
                #region Parameters
                string SelectedCustomer;
                initializer.PluginContext.GetContextParameter<string>("SelectedCustomer", out SelectedCustomer);

                string PriceParameters;
                initializer.PluginContext.GetContextParameter<string>("PriceParameters", out PriceParameters);

                string Currency;
                initializer.PluginContext.GetContextParameter<string>("Currency", out Currency);

                int LangId;
                initializer.PluginContext.GetContextParameter<int>("LangId", out LangId);

                string trackingNumber;
                initializer.PluginContext.GetContextParameter<string>("TrackingNumber", out trackingNumber);

                string reservationId;
                initializer.PluginContext.GetContextParameter<string>("ReservationId", out reservationId);

                string userInformation;
                initializer.PluginContext.GetContextParameter<string>("UserInformation", out userInformation);

                int channelCode;
                initializer.PluginContext.GetContextParameter<int>("channelCode", out channelCode);


                initializer.TraceMe("channelCode : " + channelCode);
                initializer.TraceMe("UserInformation : " + userInformation);
                initializer.TraceMe("ReservationId: " + reservationId);
                var selectedCustomerObject = JsonConvert.DeserializeObject<ContractCustomerParameters>(SelectedCustomer);
                var selectedPriceObject = JsonConvert.DeserializeObject<ContractPriceParameters>(PriceParameters);
                initializer.TraceMe("selectedCustomerObject : " + JsonConvert.SerializeObject(selectedCustomerObject));
                initializer.TraceMe("selectedPriceObject : " + JsonConvert.SerializeObject(selectedPriceObject));
                #endregion


                #region Validations
                if (selectedCustomerObject.invoiceAddress == null)
                {
                    XrmHelper xrmHelper = new XrmHelper(initializer.Service);
                    var message = xrmHelper.GetXmlTagContentByGivenLangId("MissingCustomerInfo", LangId);
                    initializer.PluginContext.ThrowException<InvalidPluginExecutionException>(message);
                }
                IndividualCustomerBL individualCustomerBL = new IndividualCustomerBL(initializer.Service, initializer.TracingService);
                var contactParameter = individualCustomerBL.getIndividualCustomerDetailContractRelationDataById(selectedCustomerObject.contactId);

                IndividualCustomerValidation individualCustomerValidation = new IndividualCustomerValidation(initializer.Service);
                var contactValidation = individualCustomerValidation.checkIndividualCustomerFieldsForQuickContract(contactParameter);
                if (!contactValidation)
                {
                    XrmHelper xrmHelper = new XrmHelper(initializer.Service);
                    var message = xrmHelper.GetXmlTagContentByGivenLangId("MissingCustomerInfo", LangId);
                    initializer.PluginContext.ThrowException<InvalidPluginExecutionException>(message);
                }

                BlackListBL blackListBL = new BlackListBL(initializer.Service);
                var blackListValidation = blackListBL.BlackListValidation(contactParameter.governmentId);

                if (blackListValidation.BlackList.IsInBlackList)
                {
                    XrmHelper xrmHelper = new XrmHelper(initializer.Service);
                    var message = xrmHelper.GetXmlTagContentByGivenLangId("BlackListValidation", LangId);
                    initializer.PluginContext.ThrowException<InvalidPluginExecutionException>(message);
                }
                #endregion

                initializer.TraceMe("i am here");
                ContractHelper contractHelper = new ContractHelper(initializer.Service, initializer.TracingService);
                //all rollup fields in reservation entity , will be handle by reservation item plugins
                ReservationRepository reservationRepository = new ReservationRepository(initializer.Service);
                var latestReservationEntity = reservationRepository.getReservationById(Guid.Parse(reservationId), new string[] {"rnt_reservationtypecode",
                                                                                                                                "rnt_totalamount",
                                                                                                                                "rnt_pnrnumber",
                                                                                                                                "rnt_paymentmethodcode",
                                                                                                                                "rnt_doublecreditcard",
                                                                                                                                "rnt_depositamount",
                                                                                                                                "rnt_paymentmethodcode",
                                                                                                                                "rnt_campaignid",
                                                                                                                                "createdon",
                                                                                                                                "rnt_ismonthly"});

                var corpContracts = contractHelper.checkMakePayment_CorporateContracts(latestReservationEntity.GetAttributeValue<OptionSetValue>("rnt_reservationtypecode").Value,
                                                                                       latestReservationEntity.GetAttributeValue<OptionSetValue>("rnt_paymentmethodcode").Value);

                initializer.TraceMe("corpContracts : " + corpContracts);

                if (!corpContracts)
                {
                    #region collect cards from parameters
                    if (latestReservationEntity.GetAttributeValue<Money>("rnt_depositamount").Value > 0)
                    {
                        var paymentCard = selectedPriceObject.creditCardData.Where(x => x.cardType == (int)PaymentEnums.PaymentTransactionType.SALE &&
                                                           (x.creditCardId != null || !string.IsNullOrEmpty(x.creditCardNumber))).FirstOrDefault();
                        var depositCard = selectedPriceObject.creditCardData.Where(x => x.cardType == (int)PaymentEnums.PaymentTransactionType.DEPOSIT &&
                                                                (x.creditCardId != null || !string.IsNullOrEmpty(x.creditCardNumber))).FirstOrDefault();

                        initializer.TraceMe("rnt_doublecreditcard : " + latestReservationEntity.GetAttributeValue<bool>("rnt_doublecreditcard"));

                        var resCard = contractHelper.checkPaymentCardandDepositCardIsSame(latestReservationEntity.GetAttributeValue<bool>("rnt_doublecreditcard"), new Guid(reservationId), paymentCard, depositCard, LangId);
                        if (!string.IsNullOrEmpty(resCard))
                        {
                            initializer.PluginContext.ThrowException<InvalidPluginExecutionException>(resCard);
                        }
                    }

                    #endregion
                }


                #region Create Contract Header Start
                initializer.TraceMe("create contract header start");
                ContractBL contractBL = new ContractBL(initializer.Service, initializer.TracingService);

                var camp = selectedPriceObject.campaignId.HasValue ? selectedPriceObject.campaignId.Value : Guid.Empty;
                //gridden rez arayıp gelirse kontrol et 
                if (camp == Guid.Empty)
                {
                    initializer.TraceMe("setting campaign");

                    if (latestReservationEntity.Contains("rnt_campaignid"))
                    {
                        camp = latestReservationEntity.GetAttributeValue<EntityReference>("rnt_campaignid").Id;
                    }
                }
                var createContractResponse = contractBL.createContractWithInitializeFromRequest(Guid.Parse(reservationId), latestReservationEntity.GetAttributeValue<string>("rnt_pnrnumber"), channelCode, camp);
                initializer.TraceMe("create contract header end");
                #endregion

                #region Update Document Plans

                if (latestReservationEntity.GetAttributeValue<bool>("rnt_ismonthly"))
                {
                    PaymentPlanRepository paymentPlanRepository = new PaymentPlanRepository(initializer.Service);
                    var plans = paymentPlanRepository.getPaymentPlansByReservationId(Guid.Parse(reservationId));

                    foreach (var item in plans)
                    {
                        Entity document = new Entity("rnt_documentpaymentplan");
                        document.Id = item.Id;
                        document["rnt_contractid"] = new EntityReference("rnt_contract", createContractResponse.contractId);
                        initializer.Service.Update(document);
                    }
                }

                #endregion

                #region Create Invoice
                InvoiceBL invoiceBL = new InvoiceBL(initializer.Service, initializer.TracingService);
                var invoiceId = invoiceBL.createInvoice(selectedCustomerObject.invoiceAddress,
                                                        null,
                                                        createContractResponse.contractId,
                                                        Guid.Parse(Currency));
                #endregion

                #region Create Contract Items
                initializer.TraceMe("create contract items start");
                ReservationItemBL reservationItemBL = new ReservationItemBL(initializer.Service);
                var reservationItems = reservationItemBL.getActiveReservationItemsGuidsByReservationId(Guid.Parse(reservationId));
                ContractItemBL contractItemBL = new ContractItemBL(initializer.Service, initializer.TracingService);

                Guid? userId = null;
                if (!string.IsNullOrEmpty(userInformation))
                {
                    var userInformationDeserialized = JsonConvert.DeserializeObject<RntCar.ClassLibrary._Tablet.UserInformation>(userInformation);
                    userId = userInformationDeserialized?.userId;
                }
                var createdContractItems = contractItemBL.createContractItemsWithInitializeFromRequest(reservationItems,
                                                                                                       createContractResponse.contractId,
                                                                                                       invoiceId,
                                                                                                       channelCode,
                                                                                                       userId);
                initializer.TraceMe("create contract items end");
                #endregion

                #region Update reservation payments
                contractBL.updateReservationPaymentsWithContract(createContractResponse.contractId, Guid.Parse(reservationId));
                #endregion

                #region Update Reservation Credit Card Slip With contract
                initializer.TraceMe("Update Reservation Credit Card Slip With contract start");
                contractBL.updateReservationCreditCardSlipsWithContract(createContractResponse.contractId, Guid.Parse(reservationId));
                initializer.TraceMe("Update Reservation Credit Card Slip With contract end");
                #endregion

                #region Complete reservation items and header
                ReservationBL reservationBL = new ReservationBL(initializer.Service, initializer.TracingService);
                reservationBL.completeReservationHeaderandItemsForContract(Guid.Parse(reservationId));
                #endregion

                #region Update Reservation Header with contract
                reservationBL.updateReservationHeaderForContract(Guid.Parse(reservationId), createContractResponse.contractId);
                #endregion

                #region Update reservation is walk-in
                reservationBL.updateReservationIsWalkin(latestReservationEntity);
                #endregion

                //check main price
                var finalRes = reservationRepository.getReservationById(Guid.Parse(reservationId), new string[] { "rnt_campaignid" });
                if (finalRes != null)
                {
                    ContractRepository contractRepository = new ContractRepository(initializer.Service);
                    var contract = contractRepository.getContractById(createContractResponse.contractId);
                    if (finalRes.Contains("rnt_campaignid") && !contract.Contains("rnt_campaignid"))
                    {
                        throw new Exception("Sözleşmenin kampanyası arka planda oluşturuluyor.Lütfen biraz bekledikten sonra tekrar deneyin");
                    }
                }

                if (!corpContracts)
                {
                    #region Payment

                    #region Credit Card Validation
                    if (latestReservationEntity.GetAttributeValue<Money>("rnt_depositamount").Value > 0)
                    {
                        CreditCardValidation creditCardValidation = new CreditCardValidation(initializer.Service, initializer.TracingService);
                        initializer.TraceMe("credit card validation start");
                        var res = creditCardValidation.checkCreditCard(selectedPriceObject.creditCardData.FirstOrDefault(), selectedCustomerObject.contactId, 1033);//todo langid will be added from actin parameters
                        if (!res.ResponseResult.Result)
                        {
                            initializer.PluginContext.ThrowException<InvalidPluginExecutionException>(res.ResponseResult.ExceptionDetail);
                        }

                    }
                    #endregion

                    initializer.TraceMe("make payment start1");

                    if (latestReservationEntity.GetAttributeValue<Money>("rnt_depositamount").Value > 0)
                    {

                        PaymentBL paymentBL = new PaymentBL(initializer.Service, initializer.TracingService);
                        var provider = paymentBL.getProviderCode(Guid.Empty, createContractResponse.contractId);
                        //todo response will handle
                        initializer.TraceMe("card data" + JsonConvert.SerializeObject(selectedPriceObject.creditCardData.FirstOrDefault()));
                        //initializer.TraceMe("card bin" + selectedPriceObject.creditCardData.FirstOrDefault().binNumber);

                        var vPosResponse = contractHelper.getVPosIdforGivenCardNumber(selectedPriceObject.creditCardData.FirstOrDefault(), provider);
                        if (!vPosResponse.ResponseResult.Result)
                        {
                            initializer.PluginContext.ThrowException<InvalidPluginExecutionException>(vPosResponse.ResponseResult.ExceptionDetail);
                        }
                        initializer.TraceMe("vPosResponse.virtualPosId" + vPosResponse.virtualPosId);

                        var createPaymentResponse = paymentBL.callMakePaymentAction(new CreatePaymentParameters
                        {
                            reservationId = Guid.Parse(reservationId),
                            contractId = createContractResponse.contractId,
                            transactionCurrencyId = new Guid(Currency),
                            individualCustomerId = selectedCustomerObject.contactId,
                            conversationId = createContractResponse.pnrNumber,
                            langId = LangId,
                            paymentTransactionType = (int)PaymentEnums.PaymentTransactionType.DEPOSIT,
                            creditCardData = selectedPriceObject.creditCardData.FirstOrDefault(),
                            installment = selectedPriceObject.installment,
                            paidAmount = latestReservationEntity.GetAttributeValue<Money>("rnt_depositamount").Value,
                            invoiceAddressData = selectedCustomerObject.invoiceAddress,
                            virtualPosId = vPosResponse.virtualPosId,
                            paymentChannelCode = PaymentMapper.mapDocumentChanneltoPaymentChannel(channelCode)
                        });
                        if (!createPaymentResponse.ResponseResult.Result)
                        {
                            initializer.PluginContext.ThrowException<InvalidPluginExecutionException>(createPaymentResponse.ResponseResult.ExceptionDetail);
                        }
                    }
                    #endregion
                }

                #region sending equipment type item to mongodb
                initializer.TraceMe("mongodb started");
                try
                {

                    var equipmentItem = createdContractItems.Where(p => p.itemTypeCode == (int)rnt_contractitem_rnt_itemtypecode.Equipment).FirstOrDefault();
                    initializer.RetryMethod<MongoDBResponse>(() => contractHelper.createContractItemInMongoDB(equipmentItem), StaticHelper.retryCount, StaticHelper.retrySleep);
                }
                catch (Exception ex)
                {
                    //will think a logic
                    initializer.TraceMe("mongodb integration error : " + ex.Message);
                }
                initializer.TraceMe("mongodb end");
                #endregion

                createContractResponse.ResponseResult = ClassLibrary.ResponseResult.ReturnSuccess();
                initializer.PluginContext.OutputParameters["QuickContractResponse"] = JsonConvert.SerializeObject(createContractResponse);
            }
            catch (Exception ex)
            {
                initializer.TraceMe("create contract error : " + ex.Message);
                initializer.TraceMe("create contract inner exception : " + ex.InnerException);
                initializer.PluginContext.ThrowException<InvalidPluginExecutionException>(ex.Message);
            }
        }
    }
}
