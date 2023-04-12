using Microsoft.Xrm.Sdk;
using Newtonsoft.Json;
using RntCar.BusinessLibrary.Business;
using RntCar.BusinessLibrary.Helpers;
using RntCar.BusinessLibrary.Repository;
using RntCar.BusinessLibrary.Validations;
using RntCar.ClassLibrary;
using RntCar.ClassLibrary._Enums_1033;
using RntCar.SDK.Common;
using RntCar.SDK.Mappers;
using System;
using System.Collections.Generic;

namespace RntCar.CrmPlugins.ManualPayment.Actions
{
    public class ExecuteManualPayment : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            PluginInitializer initializer = new PluginInitializer(serviceProvider);
            List<Guid> contractItems = new List<Guid>();

            #region  getting Parameters
            string manualPaymentParameters;
            int manuelPaymentType = 0;
            initializer.PluginContext.GetContextParameter<string>("ManualPaymentParameters", out manualPaymentParameters);
            initializer.TraceMe("manualPaymentParameters" + manualPaymentParameters);
            var parameters = JsonConvert.DeserializeObject<ManualPaymentParameters>(manualPaymentParameters);

            initializer.TraceMe("entity primary id : " + parameters.entityId);
            #endregion

            try
            {
                #region Definitions

                ManualPaymentBL manualPaymentBl = new ManualPaymentBL(initializer.Service, initializer.TracingService);
                PaymentBL paymentBL = new PaymentBL(initializer.Service, initializer.TracingService);
                var provider = paymentBL.getProviderCode(parameters.reservationId, parameters.contractId);

                CreditCardData creditCardData = new CreditCardData();
                CreatePaymentParameters paymentParameters = new CreatePaymentParameters();
                Entity contract = new Entity();
                Entity reservation = new Entity();
                if (parameters.contractId.HasValue)
                {
                    ContractRepository contractRepository = new ContractRepository(initializer.Service);
                    contract = contractRepository.getContractById(parameters.contractId.Value, new string[] { "transactioncurrencyid", "rnt_customerid", "rnt_pnrnumber", "statuscode", "rnt_corporateid", "rnt_contracttypecode", "rnt_debitamount", "rnt_paymentmethodcode", "rnt_generaltotalamount", "rnt_netpayment", "rnt_totalamount", "rnt_depositamount" });
                }
                else if (parameters.reservationId.HasValue)
                {
                    ReservationRepository reservationRepository = new ReservationRepository(initializer.Service);
                    reservation = reservationRepository.getReservationById(parameters.reservationId.Value, new string[] { "rnt_netpayment", "rnt_totalamount", "rnt_depositamount", "rnt_customerid", "rnt_pnrnumber", });
                }
                #endregion

                #region Validations

                string manuelPaymentName = string.Empty;
                var paymentId = string.Empty;
                var totalAmount = decimal.Zero;
                totalAmount += parameters.paymentAmount.HasValue ? parameters.paymentAmount.Value : decimal.Zero;
                totalAmount += parameters.amount.HasValue ? parameters.amount.Value : decimal.Zero;
                totalAmount += parameters.amount2.HasValue ? parameters.amount2.Value : decimal.Zero;
                totalAmount += parameters.amount3.HasValue ? parameters.amount3.Value : decimal.Zero;

                initializer.TraceMe("total Amount :  " + totalAmount);
                ManualPaymentValidations manualPaymentValidations = new ManualPaymentValidations(initializer.Service, initializer.TracingService);
                var responseValidation = manualPaymentValidations.checkBeforeManualPayment(parameters, totalAmount, contract, reservation);
                if (!responseValidation.ResponseResult.Result)
                {
                    initializer.PluginContext.ThrowException<InvalidPluginExecutionException>(responseValidation.ResponseResult.ExceptionDetail);
                }
                #endregion

                ManualPaymentResponse response = new ManualPaymentResponse();
                #region Only Refund
                if (parameters.manualPaymentType == (int)ClassLibrary._Enums_1033.rnt_ManualPaymentTypeCode.OnlyRefund)
                {
                    initializer.TraceMe("refund amount" + parameters.paymentAmount);
                    manuelPaymentType = (int)GlobalEnums.SmsContentCode.ManuelPaymentRefund;

                    response.ResponseResult = ResponseResult.ReturnSuccess();
                    try
                    {
                        paymentBL.createRefund(new CreateRefundParameters
                        {
                            isDepositRefund = false,
                            refundAmount = parameters.paymentAmount.Value,
                            contractId = parameters.contractId,
                            reservationId = parameters.reservationId
                        });
                    }
                    catch (Exception ex)
                    {
                        response.ResponseResult = ResponseResult.ReturnError(ex.Message);
                    }
                    manuelPaymentName = "iade";
                    initializer.TraceMe("Result" + response.ResponseResult.Result);
                    initializer.PluginContext.OutputParameters["ManualPaymentResponse"] = JsonConvert.SerializeObject(response);
                }
                #endregion

                #region Only Payment
                else if (parameters.manualPaymentType == (int)ClassLibrary._Enums_1033.rnt_ManualPaymentTypeCode.OnlyPayment)
                {
                    initializer.TraceMe("payment amount" + parameters.paymentAmount);
                    manuelPaymentType = (int)GlobalEnums.SmsContentCode.ManuelPaymentPayment;

                    paymentParameters.paidAmount = parameters.paymentAmount.Value;
                    paymentParameters.paymentChannelCode = PaymentMapper.mapDocumentChanneltoPaymentChannel((int)RntCar.ClassLibrary._Enums_1033.rnt_ReservationChannel.Branch);

                    paymentParameters.creditCardData = buildPaymentCard(parameters, initializer);
                    parameters.amount = parameters.paymentAmount.Value;
                    paymentParameters = manualPaymentBl.CreateContractPaymentParameters(parameters, paymentParameters.creditCardData, contract);
                    initializer.TraceMe("paymentParameters.creditCardData : " + JsonConvert.SerializeObject(paymentParameters.creditCardData));

                    CreditCardBL creditCardBL = new CreditCardBL(initializer.Service);
                    var vPosResponse = creditCardBL.retrieveVirtualPosIdforGivenCard(new RetrieveVirtualPosIdParameters
                    {
                        cardBin = paymentParameters.creditCardData.binNumber,
                        provider = provider
                    });

                    initializer.TraceMe("vPosResponse.ResponseResult.Result : " + JsonConvert.SerializeObject(vPosResponse.ResponseResult.Result));

                    if (!vPosResponse.ResponseResult.Result)
                    {
                        initializer.PluginContext.ThrowException<InvalidPluginExecutionException>(vPosResponse.ResponseResult.ExceptionDetail);
                    }
                    initializer.TraceMe("CallMakePaymentAction part start");
                    paymentParameters.virtualPosId = vPosResponse.virtualPosId;
                    initializer.TraceMe("paymentParameters.virtualPosId" + paymentParameters.virtualPosId);

                    initializer.TraceMe("Manual Payment parameters : " + JsonConvert.SerializeObject(paymentParameters));

                    initializer.TraceMe("Call make payment action");
                    paymentParameters.rollbackOperation = false;
                    var createPaymentResponse = paymentBL.callMakePaymentAction(paymentParameters);


                    if (createPaymentResponse.ResponseResult.Result)
                    {
                        paymentId = createPaymentResponse.paymentId;
                        initializer.TraceMe("description: " + parameters.description);
                        paymentBL.updateDescription(new Guid(paymentId), parameters.description);

                        response.ResponseResult = ResponseResult.ReturnSuccess();

                        manuelPaymentName = "çekim";
                        initializer.TraceMe("Result" + response.ResponseResult.Result);
                        initializer.PluginContext.OutputParameters["ManualPaymentResponse"] = JsonConvert.SerializeObject(response);
                    }
                    else
                    {
                        initializer.TraceMe("CallMakePaymentAction false");
                        response.ResponseResult = ResponseResult.ReturnError(createPaymentResponse.ResponseResult.ExceptionDetail);

                        initializer.TraceMe("createPaymentResponse.ResponseResult.ExceptionDetail : " + createPaymentResponse.ResponseResult.ExceptionDetail);
                        initializer.TraceMe("Result" + response.ResponseResult.Result);
                        initializer.PluginContext.OutputParameters["ManualPaymentResponse"] = JsonConvert.SerializeObject(response);
                    }

                }
                #endregion

                #region AddAdditionalProductWithPayment
                else if (parameters.manualPaymentType == (int)ClassLibrary._Enums_1033.rnt_ManualPaymentTypeCode.AddAdditionalProductWithPayment)
                {
                    OptionSetValue statusCode = contract.GetAttributeValue<OptionSetValue>("statuscode");

                    decimal amount = decimal.Zero;
                    manuelPaymentType = (int)GlobalEnums.SmsContentCode.ManuelPaymentAdditionalProduct;
                    contractItems.Add(manualPaymentBl.CreateContractItem((Guid)parameters.contractId, parameters.amount.Value, (Guid)parameters.additionalProductId, parameters.channelCode));

                    ConfigurationRepository configurationRepository = new ConfigurationRepository(initializer.Service);
                    AdditionalProductRepository additionalProductRepository = new AdditionalProductRepository(initializer.Service);

                    var hgsCode = configurationRepository.GetConfigurationByKey("additionalProduct_HGS");
                    var hgsAdditionalProduct = additionalProductRepository.getAdditionalProductByProductCode(hgsCode);

                    amount += parameters.amount.Value;
                    initializer.TraceMe("amount" + amount);
                    AdditionalProductHelper additionalProductHelper = new AdditionalProductHelper(null, initializer.Service, initializer.TracingService);
                    var fineProduct = additionalProductHelper.getFineAdditionalProduct();
                    initializer.TraceMe("fineProduct.Id" + fineProduct.Id);
                    initializer.TraceMe("parameters.additionalProductId.Value" + parameters.additionalProductId.Value);
                    var serviceContractItem = additionalProductHelper.getAdditionalProductService_Contract(parameters.additionalProductId.Value, parameters.contractId.Value);
                    if (serviceContractItem.subProduct != null && serviceContractItem.serviceItem == null)
                    {
                        //If Additional Product Is Not HGS, Process Is Continue
                        //If Additional Product Is HGS, Contract Status Haven't WaitingForDelivery And Rental
                        if (hgsAdditionalProduct.Id != parameters.additionalProductId.Value || (statusCode.Value != (int)rnt_contract_StatusCode.WaitingForDelivery && statusCode.Value != (int)rnt_contract_StatusCode.Rental))
                        {

                            initializer.TraceMe("serviceContractItem.subProduct value" + serviceContractItem.subProduct.GetAttributeValue<Money>("rnt_price").Value);

                            var subAmount = additionalProductHelper.calculateFineProductServicePrice(fineProduct.Id, parameters.additionalProductId.Value, parameters.amount.Value, serviceContractItem.subProduct.GetAttributeValue<Money>("rnt_price").Value);

                            contractItems.Add(manualPaymentBl.CreateContractItem((Guid)parameters.contractId,
                                                                                 subAmount,
                                                                                 serviceContractItem.subProduct.Id,
                                                                                 parameters.channelCode));
                            amount += subAmount;
                            initializer.TraceMe("amount" + amount);
                        }

                    }
                    if (parameters.additionalProductId2.HasValue)
                    {
                        amount += parameters.amount2.Value;
                        initializer.TraceMe("amount" + amount);
                        initializer.TraceMe("parameters.additionalProductId2.HasValue true");
                        initializer.TraceMe("parameters.additionalProductId2" + parameters.additionalProductId2.Value);
                        contractItems.Add(manualPaymentBl.CreateContractItem((Guid)parameters.contractId, parameters.amount2.Value, (Guid)parameters.additionalProductId2, parameters.channelCode));
                        serviceContractItem = additionalProductHelper.getAdditionalProductService_Contract(parameters.additionalProductId2.Value, parameters.contractId.Value);
                        if (serviceContractItem.subProduct != null && serviceContractItem.serviceItem == null)
                        {
                            //If Additional Product Is Not HGS, Process Is Continue
                            //If Additional Product Is HGS, Contract Status Haven't WaitingForDelivery And Rental
                            if (hgsAdditionalProduct.Id != parameters.additionalProductId2.Value || (statusCode.Value != (int)rnt_contract_StatusCode.WaitingForDelivery && statusCode.Value != (int)rnt_contract_StatusCode.Rental))
                            {
                                var subAmount = additionalProductHelper.calculateFineProductServicePrice(fineProduct.Id, parameters.additionalProductId2.Value, parameters.amount2.Value, serviceContractItem.subProduct.GetAttributeValue<Money>("rnt_price").Value);
                                contractItems.Add(manualPaymentBl.CreateContractItem((Guid)parameters.contractId,
                                                                                     subAmount,
                                                                                     serviceContractItem.subProduct.Id,
                                                                                     parameters.channelCode));
                                amount += subAmount;
                                initializer.TraceMe("amount" + amount);
                            }
                        }
                    }
                    if (parameters.additionalProductId3.HasValue)
                    {
                        amount += parameters.amount3.Value;
                        initializer.TraceMe("amount" + amount);
                        initializer.TraceMe("parameters.additionalProductId3.HasValue true");
                        initializer.TraceMe("parameters.additionalProductId3" + parameters.additionalProductId3.Value);
                        contractItems.Add(manualPaymentBl.CreateContractItem((Guid)parameters.contractId, parameters.amount3.Value, (Guid)parameters.additionalProductId3, parameters.channelCode));
                        serviceContractItem = additionalProductHelper.getAdditionalProductService_Contract(parameters.additionalProductId3.Value, parameters.contractId.Value);
                        if (serviceContractItem.subProduct != null && serviceContractItem.serviceItem == null)
                        {
                            //If Additional Product Is Not HGS, Process Is Continue
                            //If Additional Product Is HGS, Contract Status Haven't WaitingForDelivery And Rental
                            if (hgsAdditionalProduct.Id != parameters.additionalProductId3.Value || (statusCode.Value != (int)rnt_contract_StatusCode.WaitingForDelivery && statusCode.Value != (int)rnt_contract_StatusCode.Rental))
                            {
                                var subAmount = additionalProductHelper.calculateFineProductServicePrice(fineProduct.Id, parameters.additionalProductId3.Value, parameters.amount3.Value, serviceContractItem.subProduct.GetAttributeValue<Money>("rnt_price").Value);
                                contractItems.Add(manualPaymentBl.CreateContractItem((Guid)parameters.contractId,
                                                                                     subAmount,
                                                                                     serviceContractItem.subProduct.Id,
                                                                                     parameters.channelCode));
                                amount += subAmount;
                                initializer.TraceMe("amount" + amount);
                            }
                        }
                    }

                    paymentParameters = manualPaymentBl.CreateContractPaymentParameters(parameters, creditCardData, contract);
                    paymentParameters.paidAmount = amount;


                    ContractRepository contractRepository = new ContractRepository(initializer.Service);
                    var p = contract.GetAttributeValue<OptionSetValue>("rnt_paymentmethodcode").Value;
                    var isCorpCurrentContract = p == (int)RntCar.ClassLibrary._Enums_1033.rnt_PaymentMethodCode.FullCredit ||
                                                p == (int)RntCar.ClassLibrary._Enums_1033.rnt_PaymentMethodCode.Current;

                    Guid corporateId = Guid.Empty;

                    if (contract.Contains("rnt_corporateid"))
                    {
                        corporateId = contract.GetAttributeValue<EntityReference>("rnt_corporateid").Id;
                    }

                    initializer.TraceMe("corporateId : " + corporateId);
                    initializer.TraceMe("isCorpCurrentContract : " + isCorpCurrentContract);
                    ContractHelper contractHelper = new ContractHelper(initializer.Service);
                    if (paymentParameters.paidAmount >= 0 && !isCorpCurrentContract)
                    {
                        paymentParameters.creditCardData = buildPaymentCard(parameters, initializer);

                        initializer.TraceMe("paymentParameters.creditCardData : " + JsonConvert.SerializeObject(paymentParameters.creditCardData));
                        var vPosResponse = contractHelper.getVPosIdforGivenCardNumber(paymentParameters.creditCardData, provider);
                        initializer.TraceMe("vPosResponse.ResponseResult.Result : " + JsonConvert.SerializeObject(vPosResponse.ResponseResult.Result));

                        if (!vPosResponse.ResponseResult.Result)
                        {
                            initializer.PluginContext.ThrowException<InvalidPluginExecutionException>(vPosResponse.ResponseResult.ExceptionDetail);
                        }
                        initializer.TraceMe("CallMakePaymentAction part start");
                        paymentParameters.virtualPosId = vPosResponse.virtualPosId;
                        initializer.TraceMe("paymentParameters.virtualPosId" + paymentParameters.virtualPosId);

                        InvoiceHelper invoiceHelper = new InvoiceHelper(initializer.Service, initializer.TracingService);
                        Guid? newInvoiceId = null;
                        if ((corporateId == Guid.Empty && parameters.contractId.HasValue && parameters.contractId != Guid.Empty && parameters.contractId != null) ||
                            contract.GetAttributeValue<OptionSetValue>("rnt_contracttypecode").Value == (int)rnt_ReservationTypeCode.Broker ||
                            p == (int)rnt_PaymentMethodCode.LimitedCredit ||
                            (contract.GetAttributeValue<OptionSetValue>("rnt_contracttypecode").Value == (int)rnt_ReservationTypeCode.Acente && p == (int)rnt_PaymentMethodCode.CreditCard))
                        {
                            initializer.TraceMe("indidividual invoicing");
                            if (!parameters.invoiceId.HasValue)
                            {
                                newInvoiceId = invoiceHelper.createInvoiceFromInvoiceAddress(parameters.contractId.Value, parameters.invoiceAddressId);
                            }
                            else
                            {
                                newInvoiceId = invoiceHelper.createInvoiceFromInvoice(parameters.contractId.Value, parameters.invoiceId);
                            }
                            if (newInvoiceId != null)
                            {
                                InvoiceRepository invoiceRepository = new InvoiceRepository(initializer.Service);
                                var invoice = invoiceRepository.getInvoiceById(newInvoiceId.Value);
                                paymentParameters.invoiceAddressData = InvoiceHelper.buildInvoiceAddressDataFromInvoiceEntity(invoice);
                            }
                        }
                        //kurumsal -- KK senaryosu
                        else if (corporateId != Guid.Empty)
                        {
                            initializer.TraceMe("corporate invoicing");
                            var invoice = invoiceHelper.invoiceOperationsCorporate(parameters.contractId.Value, corporateId);
                            initializer.TraceMe(JsonConvert.SerializeObject(invoice));
                            newInvoiceId = invoice.invoiceId;
                            paymentParameters.invoiceAddressData = invoice.invoiceAddressData;
                        }
                        paymentParameters.invoiceId = newInvoiceId;
                        initializer.TraceMe("Manual Payment parameters : " + JsonConvert.SerializeObject(paymentParameters));

                        paymentParameters.paymentChannelCode = PaymentMapper.mapDocumentChanneltoPaymentChannel((int)RntCar.ClassLibrary._Enums_1033.rnt_ReservationChannel.Branch);

                        var responsePaymentAction = new CreatePaymentResponse();
                        try
                        {
                            paymentParameters.rollbackOperation = !parameters.isDebt;
                            initializer.TraceMe("Call make payment action");
                            responsePaymentAction = paymentBL.callMakePaymentAction(paymentParameters);
                            initializer.TraceMe("responsePaymentAction: " + JsonConvert.SerializeObject(responsePaymentAction));
                            if (responsePaymentAction.ResponseResult.Result)
                            {
                                paymentId = responsePaymentAction.paymentId;
                                initializer.TraceMe("description: " + parameters.description);
                                paymentBL.updateDescription(new Guid(paymentId), parameters.description);
                                initializer.TraceMe("CallMakePaymentAction part finish");
                            }
                        }
                        catch (Exception ex)
                        {
                            responsePaymentAction.ResponseResult = new ResponseResult { Result = false };
                            initializer.TraceMe("ex.exception ManualPaymentAddAdditionalProduct  :" + ex.Message);
                        }


                        try
                        {
                            //if it is contract manual payment
                            if (parameters.contractId.HasValue && parameters.contractId != Guid.Empty && parameters.contractId != null)
                            {
                                initializer.TraceMe("sending logo start");
                                invoiceHelper.manualPaymentInvoiceOperations(parameters.contractId.Value,
                                                                             contractItems,
                                                                             newInvoiceId);
                                initializer.TraceMe("sending logo end");
                            }
                        }
                        catch (Exception ex)
                        {
                            initializer.TraceMe("send logo error" + ex.Message);
                        }

                        if (responsePaymentAction.ResponseResult.Result)
                        {

                            initializer.TraceMe("CallMakePaymentAction true");
                            response.ResponseResult = ResponseResult.ReturnSuccess();

                            initializer.TraceMe("Result" + response.ResponseResult.Result);
                            initializer.PluginContext.OutputParameters["ManualPaymentResponse"] = JsonConvert.SerializeObject(response);
                        }
                        else
                        {
                            initializer.TraceMe("CallMakePaymentAction false");
                            response.ResponseResult = ResponseResult.ReturnError(responsePaymentAction.ResponseResult.ExceptionDetail);

                            initializer.TraceMe("Result" + response.ResponseResult.Result);
                            initializer.PluginContext.OutputParameters["ManualPaymentResponse"] = JsonConvert.SerializeObject(response);
                        }

                        initializer.TraceMe("AddAdditionalProductWithPayment finished");
                    }
                    else if (isCorpCurrentContract)
                    {
                        initializer.TraceMe("corp current operation");
                        InvoiceHelper invoiceHelper = new InvoiceHelper(initializer.Service, initializer.TracingService);
                        var invoice = invoiceHelper.invoiceOperationsCorporate(parameters.contractId.Value, corporateId);
                        initializer.TraceMe("isCorpCurrentContract operation");
                        initializer.TraceMe(JsonConvert.SerializeObject(invoice));

                        invoiceHelper.manualPaymentInvoiceOperations(parameters.contractId.Value,
                                                                     contractItems,
                                                                     invoice.invoiceId);

                        response.ResponseResult = ResponseResult.ReturnSuccess();
                        initializer.PluginContext.OutputParameters["ManualPaymentResponse"] = JsonConvert.SerializeObject(response);
                    }

                    manuelPaymentName = "kalem ekleyerek çekim";
                }
                #endregion

                if (response.ResponseResult.Result)
                {
                    #region add annotation
                    initializer.TraceMe("add annotation start");
                    AnnotationBL annotationBL = new AnnotationBL(initializer.Service);
                    var annotationId = annotationBL.createNewAnnotation(new ClassLibrary.AnnotationData
                    {
                        Subject = "Manuel Payment ",
                        NoteText = DateTime.Today.ToString() + " - " +
                                initializer.InitiatingUserId.ToString() + " - " + totalAmount + " - " + manuelPaymentName + " - " + parameters.description,
                        ObjectId = parameters.contractId.HasValue ? parameters.contractId.Value : parameters.reservationId.Value,
                        ObjectName = parameters.contractId.HasValue ? "rnt_contract" : "rnt_reservation"
                    });
                    initializer.TraceMe("add annotation end");
                    #endregion

                    if (parameters.contractId.HasValue)
                    {
                        ContractHelper contractHelper = new ContractHelper(initializer.Service);
                        contractHelper.calculateDebitAmount(parameters.contractId.Value);
                    }

                    EntityReference customerRef = new EntityReference();
                    string pnrNumber = string.Empty;
                    int langId = 1055;

                    if (parameters.contractId.HasValue)
                    {
                        customerRef = contract.GetAttributeValue<EntityReference>("rnt_customerid");
                        pnrNumber = contract.GetAttributeValue<string>("rnt_pnrnumber");
                    }
                    else
                    {
                        customerRef = reservation.GetAttributeValue<EntityReference>("rnt_customerid");
                        pnrNumber = reservation.GetAttributeValue<string>("rnt_pnrnumber");
                    }

                    if (customerRef.Id != Guid.Empty && customerRef.LogicalName.ToLower() == "contact")
                    {
                        string additionalProductName = string.Empty;
                        IndividualCustomerRepository individualCustomerRepository = new IndividualCustomerRepository(initializer.Service);
                        var ind = individualCustomerRepository.getIndividualCustomerById(customerRef.Id);

                        if (!string.IsNullOrEmpty(ind.GetAttributeValue<string>("mobilephone")))
                        {
                            if (parameters.manualPaymentType == (int)ClassLibrary._Enums_1033.rnt_ManualPaymentTypeCode.AddAdditionalProductWithPayment)
                            {
                                AdditionalProductRepository additionalProductRepository = new AdditionalProductRepository(initializer.Service);
                                var additionalProduct = additionalProductRepository.getAdditionalProductById(parameters.additionalProductId.Value, new string[] { "rnt_name" });
                                additionalProductName = additionalProduct.GetAttributeValue<string>("rnt_name");
                                if (parameters.additionalProductId2.HasValue)
                                {
                                    var additionalProduct2 = additionalProductRepository.getAdditionalProductById(parameters.additionalProductId2.Value, new string[] { "rnt_name" });
                                    additionalProductName = additionalProductName + "," + additionalProduct2.GetAttributeValue<string>("rnt_name");
                                }
                                if (parameters.additionalProductId3.HasValue)
                                {
                                    var additionalProduct3 = additionalProductRepository.getAdditionalProductById(parameters.additionalProductId3.Value, new string[] { "rnt_name" });
                                    additionalProductName = additionalProductName + "," + additionalProduct3.GetAttributeValue<string>("rnt_name");
                                }
                            }
                            string dailCode = ind.GetAttributeValue<string>("rnt_dialcode");
                            string mobilePhone = ind.GetAttributeValue<string>("mobilephone");
                            string mobilePhoneWithDialCode = dailCode + mobilePhone;
                            OrganizationRequest organizationRequest = new OrganizationRequest("rnt_GenerateAndSendSMS");
                            organizationRequest["FirstName"] = ind.GetAttributeValue<string>("firstname");
                            organizationRequest["LastName"] = ind.GetAttributeValue<string>("lastname");
                            organizationRequest["MobilePhone"] = mobilePhoneWithDialCode;
                            organizationRequest["LangId"] = langId;
                            organizationRequest["amount"] = Convert.ToString(Math.Round(totalAmount, 2));
                            organizationRequest["SMSContentCode"] = manuelPaymentType;
                            organizationRequest["PnrNumber"] = pnrNumber;
                            organizationRequest["AdditionalProductName"] = additionalProductName;

                            var res = initializer.Service.Execute(organizationRequest);
                            initializer.TraceMe("sms send end");

                        }
                    }
                    initializer.TraceMe("manual payment operations finished in peace");
                }
            }
            catch (Exception ex)
            {
                initializer.TraceMe("ex.exception  :" + ex.Message);

                if (!parameters.isDebt && parameters.manualPaymentType == (int)ClassLibrary._Enums_1033.rnt_ManualPaymentTypeCode.AddAdditionalProductWithPayment)
                {
                    XrmHelper xrmHelper = new XrmHelper(initializer.Service);
                    var message = xrmHelper.GetXmlTagContentByGivenLangId("ManualPaymentError", parameters.langId);
                    initializer.TraceMe("!parameters.isDebt && parameters.manualPaymentType == (int)ClassLibrary._Enums_1033.rnt_ManualPaymentTypeCode.AddAdditionalProductWithPayment");
                    initializer.PluginContext.ThrowException<InvalidPluginExecutionException>(message);
                }
                initializer.TraceMe("general exception handling" + ex.Message);
            }
        }
        private CreditCardData buildPaymentCard(ManualPaymentParameters parameters, PluginInitializer initializer)
        {
            ManualPaymentBL manualPaymentBL = new ManualPaymentBL(initializer.Service, initializer.TracingService);
            CreditCardData creditCardData = new CreditCardData();
            //new credit card
            if (parameters.creditCardId == null || parameters.creditCardId == Guid.Empty)
            {
                initializer.TraceMe("CreditCardType : new start");
                creditCardData = manualPaymentBL.FillNewCreditCardData(parameters);
                initializer.TraceMe("CreditCardType : new finish");
            }
            //existing credit card
            else
            {
                initializer.TraceMe("CreditCardType : existing start");
                creditCardData = manualPaymentBL.FillExistingCreditCardData(parameters);
                initializer.TraceMe("CreditCardType : existing finish");
            }
            return creditCardData;
        }
    }
}
