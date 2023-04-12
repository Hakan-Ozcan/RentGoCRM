using Microsoft.Xrm.Sdk;
using Newtonsoft.Json;
using RntCar.BusinessLibrary.Business;
using RntCar.BusinessLibrary.Handlers;
using RntCar.BusinessLibrary.Repository;
using RntCar.BusinessLibrary.Validations;
using RntCar.ClassLibrary;
using RntCar.ClassLibrary._Enums_1033;
using RntCar.ClassLibrary.MongoDB;
using RntCar.ClassLibrary.Reservation;
using RntCar.SDK.Common;
using RntCar.SDK.Mappers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RntCar.CrmPlugins.Reservation.Actions
{
    public class ExecuteCreateReservation : HandlerBase, IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            PluginInitializer initializer = new PluginInitializer(serviceProvider);
            ConfigurationBL configurationBL = new ConfigurationBL(initializer.Service);
            PaymentBL paymentBL = new PaymentBL(initializer.Service, initializer.TracingService);
            try
            {
                initializer.TraceMe("create reservation start");

                string SelectedCustomer;
                initializer.PluginContext.GetContextParameter<string>("SelectedCustomer", out SelectedCustomer);

                string SelectedDateAndBranch;
                initializer.PluginContext.GetContextParameter<string>("SelectedDateAndBranch", out SelectedDateAndBranch);

                string SelectedEquipment;
                initializer.PluginContext.GetContextParameter<string>("SelectedEquipment", out SelectedEquipment);

                string PriceParameters;
                initializer.PluginContext.GetContextParameter<string>("PriceParameters", out PriceParameters);

                string CouponCode;
                initializer.PluginContext.GetContextParameter<string>("CouponCode", out CouponCode);

                string SelectedAdditionalProducts;
                initializer.PluginContext.GetContextParameter<string>("SelectedAdditionalProducts", out SelectedAdditionalProducts);

                int ReservationChannel;
                initializer.PluginContext.GetContextParameter<int>("ReservationChannel", out ReservationChannel);

                int ReservationTypeCode;
                initializer.PluginContext.GetContextParameter<int>("ReservationTypeCode", out ReservationTypeCode);

                string Currency;
                initializer.PluginContext.GetContextParameter<string>("Currency", out Currency);

                int TotalDuration;
                initializer.PluginContext.GetContextParameter<int>("TotalDuration", out TotalDuration);

                int langId;
                initializer.PluginContext.GetContextParameter<int>("LangId", out langId);

                string trackingNumber;
                initializer.PluginContext.GetContextParameter<string>("TrackingNumber", out trackingNumber);

                initializer.TracingService.Trace("selected customer : " + SelectedCustomer);
                initializer.TracingService.Trace("SelectedDateAndBranch : " + SelectedDateAndBranch);
                initializer.TracingService.Trace("SelectedEquipment : " + SelectedEquipment);
                initializer.TracingService.Trace("PriceObject : " + PriceParameters);
                initializer.TracingService.Trace("SelectedAdditionalProducts : " + SelectedAdditionalProducts);
                initializer.TracingService.Trace("ReservationChannel : " + ReservationChannel);
                initializer.TracingService.Trace("ReservationTypeCode : " + ReservationTypeCode);
                initializer.TracingService.Trace("Currency : " + Currency);
                initializer.TracingService.Trace("TrackingNumber : " + trackingNumber);
                initializer.TracingService.Trace("langId : " + langId);
                initializer.TracingService.Trace("TotalDuration : " + TotalDuration);

                #region Parameters
                var selectedCustomerObject = JsonConvert.DeserializeObject<ReservationCustomerParameters>(SelectedCustomer);
                var selectedDateandBranchObject = JsonConvert.DeserializeObject<ReservationDateandBranchParameters>(SelectedDateAndBranch);
                var selectedEquipmentObject = JsonConvert.DeserializeObject<ReservationEquipmentParameters>(SelectedEquipment);
                var selectedPriceObject = JsonConvert.DeserializeObject<ReservationPriceParameters>(PriceParameters);
                var selectedAdditionalProducts = JsonConvert.DeserializeObject<List<ReservationAdditionalProductParameters>>(SelectedAdditionalProducts);

                ReservationRelatedParameters reservationRelatedParameter = new ReservationRelatedParameters
                {
                    reservationChannel = ReservationChannel,
                    reservationTypeCode = ReservationTypeCode,
                    trackingNumber = trackingNumber
                };
                #endregion


                ReservationBL reservationBL = new ReservationBL(initializer.Service, initializer.TracingService);

                #region Validations

                //check date and branch inputs
                #region Reservation Date and Branch Inpputs Validation
                ReservationCreationValidation reservationCreationValidation = new ReservationCreationValidation(initializer.Service);
                var dateandBranchValidation = reservationCreationValidation.checkReservationDateandBranch(selectedDateandBranchObject.pickupDate, selectedDateandBranchObject.dropoffDate,
                                                                            selectedDateandBranchObject.pickupBranchId, selectedDateandBranchObject.dropoffBranchId);
                if (!dateandBranchValidation)
                {
                    XrmHelper xrmHelper = new XrmHelper(initializer.Service);
                    var message = xrmHelper.GetXmlTagContentByGivenLangId("DateandBranchNullCheck", langId);
                    initializer.PluginContext.ThrowException<InvalidPluginExecutionException>(message);
                }
                #endregion

                // check branchs working hours
                #region Branchs Working Hour Validations
                WorkingHourValidation workingHourValidation = new WorkingHourValidation(initializer.Service, initializer.TracingService);
                var wokingHourValidationResponse = workingHourValidation.checkBranchWorkingHour(selectedDateandBranchObject.pickupBranchId,
                                                                                                selectedDateandBranchObject.dropoffBranchId,
                                                                                                selectedDateandBranchObject.pickupDate.AddMinutes(StaticHelper.offset),
                                                                                                selectedDateandBranchObject.dropoffDate.AddMinutes(StaticHelper.offset),
                                                                                                langId,
                                                                                                reservationRelatedParameter.reservationChannel);
                if (!string.IsNullOrEmpty(wokingHourValidationResponse))
                {
                    initializer.PluginContext.ThrowException<InvalidPluginExecutionException>(wokingHourValidationResponse);
                }

                #endregion

                #region Retrieve and Convert Group Code Information
                initializer.TraceMe("groupcodeInfo retrieve start");
                GroupCodeInformationBL groupCodeInformationBL = new GroupCodeInformationBL(initializer.Service);
                var groupCodeInformationForDocument = groupCodeInformationBL.getGroupCodeInformationDetailForDocument(selectedEquipmentObject.groupCodeInformationId);
                var groupCodeData = new GroupCodeInformationCRMData
                {
                    rnt_youngdriverlicence = groupCodeInformationForDocument.youngDriverLicense,
                    rnt_youngdriverage = groupCodeInformationForDocument.youngDriverAge,
                    rnt_minimumage = groupCodeInformationForDocument.minimumAge,
                };
                initializer.TraceMe("groupcodeInfo retrieve end");
                #endregion

                #region Customer Control
                initializer.TracingService.Trace("selectedCustomerObject.contactId: " + selectedCustomerObject.contactId);

                if (selectedCustomerObject.contactId != Guid.Empty)
                {
                    #region Existing Reservations and Contracts Validation
                    SystemParameterBL systemParameterBL = new SystemParameterBL(initializer.Service);
                    var reservationRelatedSystemParameters = systemParameterBL.GetSystemParameters();
                    if (reservationRelatedSystemParameters.isReservationandContractCheckEnabled)
                    {
                        initializer.TraceMe("checkExistingReservationsDateTime start");
                        var validationResponse = reservationCreationValidation.checkExistingReservationsDateTime(selectedCustomerObject.contactId,
                                                                                                                 null,
                                                                                                                 selectedDateandBranchObject.pickupDate.AddMinutes(StaticHelper.offset),
                                                                                                                 selectedDateandBranchObject.dropoffDate.AddMinutes(StaticHelper.offset),
                                                                                                                 langId);
                        if (!validationResponse.ResponseResult.Result)
                        {
                            initializer.PluginContext.ThrowException<InvalidPluginExecutionException>(validationResponse.ResponseResult.ExceptionDetail);
                        }

                        initializer.TraceMe("checkExistingReservationsDateTime end");
                        ContractCreationValidation contractCreationValidation = new ContractCreationValidation(initializer.Service);

                        initializer.TraceMe("checkExistingContractsDateTime start");
                        validationResponse = contractCreationValidation.checkExistingContractsDateTime(selectedCustomerObject.contactId,
                                                                                                       selectedDateandBranchObject.pickupDate.AddMinutes(StaticHelper.offset),
                                                                                                       selectedDateandBranchObject.dropoffDate.AddMinutes(StaticHelper.offset),
                                                                                                       langId);
                        if (!validationResponse.ResponseResult.Result)
                        {
                            initializer.PluginContext.ThrowException<InvalidPluginExecutionException>(validationResponse.ResponseResult.ExceptionDetail);
                        }

                        initializer.TraceMe("checkExistingContractsDateTime end");
                    }
                    #endregion

                    #region Retrieve Individual Customer Detail Data
                    initializer.TraceMe("individualCustomer retrieve start");
                    IndividualCustomerBL individualCustomerBL = new IndividualCustomerBL(initializer.Service);
                    var individualCustomer = individualCustomerBL.getIndividualCustomerInformationForValidation(selectedCustomerObject.contactId);

                    var customerValidationResponse = individualCustomerBL.checkContractStatus(selectedCustomerObject.contactId, langId);
                    if (!customerValidationResponse.ResponseResult.Result)
                    {
                        initializer.TraceMe("customerValidationResponse Status");
                        initializer.PluginContext.ThrowException<InvalidPluginExecutionException>(customerValidationResponse.ResponseResult.ExceptionDetail);
                    }

                    rnt_CustomerSegment individualCustomerSegment = (rnt_CustomerSegment)individualCustomer.customerSegment;
                    initializer.TraceMe("individualCustomer retrieve end");
                    #endregion

                    // check individual customer black list
                    #region Black List Validation
                    BlackListBL blackListBL = new BlackListBL(initializer.Service);
                    var blackListValidation = blackListBL.BlackListValidation(individualCustomer.governmentId);
                    if (blackListValidation.BlackList.IsInBlackList)
                    {
                        XrmHelper xrmHelper = new XrmHelper(initializer.Service);
                        var message = xrmHelper.GetXmlTagContentByGivenLangId("BlackListValidation", langId);

                        if (ReservationChannel == (int)rnt_ReservationChannel.Web)
                        {
                            message = xrmHelper.GetXmlTagContentByGivenLangId("BlackListValidationWeb", langId);
                        }
                        initializer.PluginContext.ThrowException<InvalidPluginExecutionException>(message);
                    }
                    #endregion

                    #region Additional Product Validations
                    initializer.TraceMe("additionalProducts retrieve start");
                    AdditionalProductsBL additionalProductsBL = new AdditionalProductsBL(initializer.Service, initializer.TracingService);

                    // check individual customer age, driving license date and get additional products
                    var additionalProducts = additionalProductsBL.GetAdditionalProducts(groupCodeData, individualCustomer, selectedDateandBranchObject, TotalDuration);
                    if (!additionalProducts.ResponseResult.Result)
                    {
                        initializer.PluginContext.ThrowException<InvalidPluginExecutionException>(additionalProducts.ResponseResult.ExceptionDetail);
                    }
                    initializer.TraceMe("additionalProducts retrieve end");

                    //check mandatory additional products
                    AdditionalProductValidation additionalProductValidation = new AdditionalProductValidation(initializer.Service);
                    var currentProducts = selectedAdditionalProducts.ConvertAll(item => new AdditionalProductData
                    {
                        productCode = item.productCode,
                        productId = item.productId,
                        value = item.value,
                        maxPieces = item.maxPieces
                    });
                    //var result = additionalProductValidation.compareAdditionalProductForMandatoryProducts(currentProducts, additionalProducts.AdditionalProducts);
                    //if (!result)
                    //{
                    //    XrmHelper xrmHelper = new XrmHelper(initializer.Service);
                    //    var message = xrmHelper.GetXmlTagContentByGivenLangId("MandatoryAdditionalProducts", langId);
                    //    initializer.PluginContext.ThrowException<InvalidPluginExecutionException>(message);
                    //}
                    var result = additionalProductValidation.checkAdditionalProductsMaxPieces(currentProducts);
                    if (!result)
                    {
                        XrmHelper xrmHelper = new XrmHelper(initializer.Service);
                        var message = xrmHelper.GetXmlTagContentByGivenLangId("AdditionalProductMaxPiecesValidation", langId);
                        initializer.PluginContext.ThrowException<InvalidPluginExecutionException>(message);
                    }
                    result = additionalProductValidation.checkAdditionalProductsRules(currentProducts);
                    if (!result)
                    {
                        XrmHelper xrmHelper = new XrmHelper(initializer.Service);
                        var message = xrmHelper.GetXmlTagContentByGivenLangId("AdditionalProductRules", langId);
                        initializer.PluginContext.ThrowException<InvalidPluginExecutionException>(message);
                    }
                    result = additionalProductValidation.checkAdditionalProductsLogoId(currentProducts);
                    if (!result)
                    {
                        XrmHelper xrmHelper = new XrmHelper(initializer.Service);
                        var message = xrmHelper.GetXmlTagContentByGivenLangId("AdditionalProductMissingLogoId", langId);
                        initializer.PluginContext.ThrowException<InvalidPluginExecutionException>(message);
                    }
                    #endregion

                    if (((selectedPriceObject.campaignId.HasValue && selectedPriceObject.campaignId != Guid.Empty) || !string.IsNullOrWhiteSpace(CouponCode)) && (individualCustomerSegment == rnt_CustomerSegment.PersonnelOfTunalar || individualCustomerSegment == rnt_CustomerSegment.Staff))
                    {
                        XrmHelper xrmHelper = new XrmHelper(initializer.Service);
                        var message = xrmHelper.GetXmlTagContentByGivenLangId("PersonnelNotUseExtraDiscount", langId, this.reservationXmlPath);
                        initializer.PluginContext.ThrowException<InvalidPluginExecutionException>(message);
                    }
                }

                #region Corporate Customer Control
                if (selectedCustomerObject.corporateCustomerId.HasValue)
                {
                    string creditLimitSystem = configurationBL.GetConfigurationByName("CreditLimitSystem");
                    if (!string.IsNullOrWhiteSpace(creditLimitSystem) && Convert.ToBoolean(creditLimitSystem))
                    {
                        initializer.TracingService.Trace("checkCreditLimit control start");
                        bool checkCreditLimit = reservationBL.checkCreditLimit(selectedCustomerObject, selectedPriceObject, selectedAdditionalProducts, TotalDuration);
                        if (!checkCreditLimit)
                        {
                            string message = "Credit Limit of Customer Insufficiented";
                            initializer.PluginContext.ThrowException<InvalidPluginExecutionException>(message);
                        }
                    }

                }
                initializer.TracingService.Trace("checkCreditLimit control end");
                #endregion Corporate Customer Control
                #endregion Customer Control

                #region Campaign And Coupon Code Control
                if (selectedPriceObject.campaignId.HasValue && selectedPriceObject.campaignId.Value != Guid.Empty && !string.IsNullOrWhiteSpace(CouponCode))
                {
                    XrmHelper xrmHelper = new XrmHelper(initializer.Service);
                    var message = xrmHelper.GetXmlTagContentByGivenLangId("CouponCodeNotUseWithCampaign", langId, this.couponCodeXmlPath);
                    initializer.PluginContext.ThrowException<InvalidPluginExecutionException>(message);
                }

                #endregion

                #endregion Validations
                Currency = reservationBL.decideReservationCurrency(selectedCustomerObject, selectedDateandBranchObject.pickupDate.AddMinutes(StaticHelper.offset), new Guid(Currency)).ToString();
                initializer.TracingService.Trace("Currency : " + Currency);
                initializer.TracingService.Trace("CouponCode : " + CouponCode);
                var reservationCreateResponse = reservationBL.createReservation(selectedCustomerObject,
                                                                                selectedDateandBranchObject,
                                                                                selectedEquipmentObject,
                                                                                selectedAdditionalProducts,
                                                                                selectedPriceObject,
                                                                                reservationRelatedParameter,
                                                                                groupCodeInformationForDocument,
                                                                                new Guid(Currency),
                                                                                TotalDuration,
                                                                                CouponCode);

                initializer.TraceMe("Reservation created : " + reservationCreateResponse.reservationId);
                ReservationItemBL reservationItemBL = new ReservationItemBL(initializer.Service, initializer.TracingService);
                var reservation = reservationItemBL.createReservationItemForEquipment(selectedCustomerObject,
                                                                                      selectedDateandBranchObject,
                                                                                      selectedEquipmentObject,
                                                                                      selectedPriceObject,
                                                                                      reservationCreateResponse.reservationId,
                                                                                      new Guid(Currency),
                                                                                      TotalDuration,
                                                                                      trackingNumber,
                                                                                      reservationCreateResponse.dummyContactId,
                                                                                      null,
                                                                                      10,
                                                                                      reservationRelatedParameter.reservationChannel);

                initializer.TraceMe("Reservation equipment created : " + reservation);
                initializer.TraceMe("additional products created start  : ");
                var additionalProductResponse = reservationItemBL.createReservationItemForAdditionalProducts(selectedCustomerObject,
                                                                                                             selectedDateandBranchObject,
                                                                                                             selectedEquipmentObject,
                                                                                                             selectedPriceObject,
                                                                                                             selectedAdditionalProducts,
                                                                                                             (int)rnt_reservationitem_rnt_itemtypecode.AdditionalProduct,
                                                                                                             reservationCreateResponse.reservationId,
                                                                                                             new Guid(Currency),
                                                                                                             TotalDuration,
                                                                                                             trackingNumber,
                                                                                                             reservationCreateResponse.dummyContactId,
                                                                                                             reservationRelatedParameter.reservationChannel);
                reservationCreateResponse.reservationItemList = new List<Guid>();
                //for equipment
                reservationCreateResponse.reservationItemList.Add(reservation);
                //for additional prodcuts
                reservationCreateResponse.reservationItemList.AddRange(additionalProductResponse);
                initializer.TraceMe("additional products created end  : ");


                var reservationTotalAmount = reservationBL.getReservationTotalAmount(reservationCreateResponse.reservationId);



                #region check discount amount and add additional product
                if (selectedPriceObject.discountAmount.HasValue && selectedPriceObject.discountAmount.Value > 0)
                {
                    var productCode = configurationBL.GetConfigurationByName("additionalProducts_manuelDiscount");
                    AdditionalProductRepository additionalProductRepository = new AdditionalProductRepository(initializer.Service);
                    var additionalProduct = additionalProductRepository.getAdditionalProductByProductCode(productCode, new string[] { "rnt_pricecalculationtypecode",
                                                                                                                                                          "rnt_name"});

                    var manuelDiscountAdditionalProduct = new List<ReservationAdditionalProductParameters>();
                    manuelDiscountAdditionalProduct.Add(new ReservationAdditionalProductParameters().buildReservationAdditionalProductParameters(additionalProduct, selectedPriceObject.discountAmount.Value * -1));

                    reservationItemBL.createReservationItemForAdditionalProducts(selectedCustomerObject,
                                                                                 selectedDateandBranchObject,
                                                                                 selectedEquipmentObject,
                                                                                 selectedPriceObject,
                                                                                 manuelDiscountAdditionalProduct,
                                                                                 (int)rnt_reservationitem_rnt_itemtypecode.AdditionalProduct,
                                                                                 reservationCreateResponse.reservationId,
                                                                                 new Guid(Currency),
                                                                                 TotalDuration,
                                                                                 trackingNumber,
                                                                                 reservationCreateResponse.dummyContactId,
                                                                                 reservationRelatedParameter.reservationChannel);

                    reservationTotalAmount = reservationTotalAmount - selectedPriceObject.discountAmount;

                }
                #endregion check discount amount


                CouponCodeOperationsResponse couponCodeOperationsResponse = new CouponCodeOperationsResponse();

                #region Update Coupon Code
                if (!string.IsNullOrEmpty(CouponCode))
                {
                    CouponCodeDefinitionBL couponCodeDefinitionBL = new CouponCodeDefinitionBL(initializer.Service, initializer.TracingService);
                    CouponCodeDefinitionResponse couponCodeDefinitionResponse = couponCodeDefinitionBL.getCouponCodeDefinitionByCouponCode(CouponCode);

                    CouponCodeBL couponCodeBL = new CouponCodeBL(initializer.Service, initializer.TracingService);
                    couponCodeOperationsResponse = couponCodeBL.executeCouponCodeOperations(new CouponCodeOperationsParameter
                    {
                        couponCode = CouponCode,
                        reservationId = reservationCreateResponse.reservationId.ToString(),
                        reservationChannelCode = ReservationChannel,
                        reservationTypeCode = ReservationTypeCode,
                        pickupBranchId = selectedDateandBranchObject.pickupBranchId,
                        groupCodeInformationId = selectedEquipmentObject.groupCodeInformationId,
                        contactId = Convert.ToString(selectedCustomerObject.contactId),
                        accountId = selectedCustomerObject.contactId != Guid.Empty || selectedCustomerObject.contactId != null ? string.Empty : Convert.ToString(selectedCustomerObject.corporateCustomerId),
                        statusCode = (int)GlobalEnums.CouponCodeStatusCode.Used,
                    }, true);

                    if (!couponCodeOperationsResponse.ResponseResult.Result)
                    {
                        initializer.PluginContext.ThrowException<InvalidPluginExecutionException>(couponCodeOperationsResponse.ResponseResult.ExceptionDetail);
                    }

                    //if (!string.IsNullOrEmpty(CouponCode) && selectedPriceObject.discountAmount.HasValue && selectedPriceObject.discountAmount.Value > 0)
                    //{
                    //    initializer.TraceMe("coupon code process start : " + CouponCode);
                    //    initializer.TraceMe("coupon code object " + JsonConvert.SerializeObject(couponCodeOperationsResponse));
                    //    couponCodeBL.updateCouponCodeInMongoDB(couponCodeOperationsResponse.couponCodeData);
                    //    initializer.TraceMe("coupon code process end");
                    //}
                }


                #endregion Update Coupon Code
                CreatePaymentResponse createPaymentResponse = new CreatePaymentResponse();
                //now we neeed to make payment if it is pay now
                if (selectedPriceObject.paymentType == (int)PaymentEnums.PaymentType.PayNow)
                {
                    #region Comission
                    //var comissionAmount = selectedPriceObject.installmentTotalAmount - selectedPriceObject.price;
                    //if (comissionAmount != decimal.Zero)
                    //{
                    //    ConfigurationBL configurationBL = new ConfigurationBL(initializer.Service);
                    //    var productCode = configurationBL.GetConfigurationByName("additionalProducts_comission");
                    //    AdditionalProductRepository additionalProductRepository = new AdditionalProductRepository(initializer.Service);
                    //    var additionalProduct = additionalProductRepository.getAdditionalProductByProductCodeWithGivenColumns(productCode, new string[] { "rnt_pricecalculationtypecode",
                    //                                                                                                                                      "rnt_name"});

                    //    reservationItemBL.createReservationItemForAdditionalProduct(selectedCustomerObject,
                    //                                                                selectedDateandBranchObject,
                    //                                                                selectedEquipmentObject,
                    //                                                                new ReservationAdditionalProductParameters().buildReservationAdditionalProductParameters(additionalProduct, comissionAmount),
                    //                                                                reservationCreateResponse.reservationId,
                    //                                                                new Guid(Currency),
                    //                                                                TotalDuration,
                    //                                                                selectedPriceObject.paymentType);
                    //}
                    #endregion

                    #region invoice address data null check
                    var invoiceAddressResponse = reservationCreationValidation.checkReservationInvoiceAddressNullCheck(selectedCustomerObject.invoiceAddress.invoiceType);
                    if (!invoiceAddressResponse)
                    {
                        XrmHelper xrmHelper = new XrmHelper(initializer.Service);
                        var message = xrmHelper.GetXmlTagContentByGivenLangId("InvoiceAddressNullCheck", langId);
                        initializer.PluginContext.ThrowException<InvalidPluginExecutionException>(message);
                    }

                    #endregion invoice address data null check

                    #region Create Invoice
                    InvoiceBL invoiceBL = new InvoiceBL(initializer.Service, initializer.TracingService);
                    var invoiceId = invoiceBL.createInvoice(selectedCustomerObject.invoiceAddress,
                                                            reservationCreateResponse.reservationId,
                                                            null,
                                                            new Guid(Currency));
                    #endregion

                    // pass reservation paid amount to decimal zero for getting total amount
                    initializer.TraceMe("reservationTotalAmount : " + reservationTotalAmount);

                    var individualData = reservationBL.getIndividualCustomerInfo(selectedCustomerObject.contactId);

                    #region Credit Card Validation
                    CreditCardValidation creditCardValidation = new CreditCardValidation(initializer.Service, initializer.TracingService);
                    initializer.TraceMe("credit card validation start");
                    var res = creditCardValidation.checkCreditCard(selectedPriceObject.creditCardData.FirstOrDefault(), selectedCustomerObject.contactId, langId);
                    if (!res.ResponseResult.Result)
                    {
                        initializer.PluginContext.ThrowException<InvalidPluginExecutionException>(res.ResponseResult.ExceptionDetail);
                    }
                    #endregion

                    initializer.TraceMe("make payment start");
                    initializer.TraceMe("virtual_posId" + selectedPriceObject.virtualPosId);

                    var iyzicoCallBackUrl = configurationBL.GetConfigurationByName("3DPaymentCallBackUrl");
                    selectedPriceObject.callBackUrl = iyzicoCallBackUrl.Replace("resid", reservationCreateResponse.reservationId.ToString());

                    //todo response will handle
                    createPaymentResponse = paymentBL.callMakePaymentAction(new CreatePaymentParameters
                    {
                        reservationId = reservationCreateResponse.reservationId,
                        transactionCurrencyId = new Guid(Currency),
                        individualCustomerId = selectedCustomerObject.contactId,
                        conversationId = reservationCreateResponse.pnrNumber,
                        langId = langId,
                        paymentTransactionType = (int)PaymentEnums.PaymentTransactionType.SALE,
                        creditCardData = selectedPriceObject.creditCardData.FirstOrDefault(),
                        installment = selectedPriceObject.installment,
                        paidAmount = reservationTotalAmount.Value,
                        installmentAmount = selectedPriceObject.installmentTotalAmount,
                        invoiceAddressData = selectedCustomerObject.invoiceAddress,
                        virtualPosId = selectedPriceObject.virtualPosId,
                        paymentChannelCode = PaymentMapper.mapDocumentChanneltoPaymentChannel(reservationRelatedParameter.reservationChannel),
                        use3DSecure = selectedPriceObject.use3DSecure,
                        callBackUrl = selectedPriceObject.callBackUrl
                    });


                    #region Update Coupon Code
                    if (!string.IsNullOrEmpty(CouponCode) && selectedPriceObject.discountAmount.HasValue && selectedPriceObject.discountAmount.Value > 0)
                    {
                        initializer.TraceMe("coupon code process start : " + CouponCode);
                        CouponCodeBL couponCodeBL = new CouponCodeBL(initializer.Service, initializer.TracingService);
                        couponCodeBL.updateCouponCodeInMongoDB(couponCodeOperationsResponse.couponCodeData);
                        initializer.TraceMe("coupon code process end");
                    }
                    #endregion Update Coupon Code
                }

                initializer.TraceMe("create reservation end");
                initializer.TraceMe("make payment end");
                reservationCreateResponse.ResponseResult = ResponseResult.ReturnSuccess();
                // todo nettahsilat
                if (createPaymentResponse != null && createPaymentResponse.use3DSecure)
                {
                    reservationCreateResponse.use3DSecure = createPaymentResponse.use3DSecure;
                    reservationCreateResponse.htmlContent = createPaymentResponse.htmlContent;
                }

                if(reservationCreateResponse.ResponseResult.Result && createPaymentResponse.use3DSecure)
                {
                    reservationBL.setReservationState(reservationCreateResponse.reservationId, (int)ReservationEnums.StatusCode.Waitingfor3D);
                    paymentBL.setPaymentTransactionResultState(new Guid(createPaymentResponse.paymentId), (int)rnt_payment_rnt_transactionresult.waitingFor3D);
                }
                initializer.TraceMe(JsonConvert.SerializeObject(reservationCreateResponse));

                initializer.PluginContext.OutputParameters["ReservationResponse"] = JsonConvert.SerializeObject(reservationCreateResponse);
                initializer.TraceMe(JsonConvert.SerializeObject(initializer.PluginContext.OutputParameters["ReservationResponse"]));
            }
            catch (Exception ex)
            {
                initializer.TracingService.Trace("rez create error " + ex.Message);

                initializer.PluginContext.ThrowException<InvalidPluginExecutionException>(ex.Message);
            }
        }
    }
}

