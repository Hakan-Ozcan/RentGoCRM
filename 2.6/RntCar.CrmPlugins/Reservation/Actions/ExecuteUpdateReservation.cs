using Microsoft.Xrm.Sdk;
using Newtonsoft.Json;
using RntCar.BusinessLibrary;
using RntCar.BusinessLibrary.Business;
using RntCar.BusinessLibrary.Helpers;
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
    public class ExecuteUpdateReservation : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            PluginInitializer initializer = new PluginInitializer(serviceProvider);
            #region Parameters
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

            string BirdId;
            initializer.PluginContext.GetContextParameter<string>("BirdId", out BirdId);

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

            string TrackingNumber;
            initializer.PluginContext.GetContextParameter<string>("TrackingNumber", out TrackingNumber);

            string ReservationId;
            initializer.PluginContext.GetContextParameter<string>("ReservationId", out ReservationId);

            int ChangeType;
            initializer.PluginContext.GetContextParameter<int>("ChangeType", out ChangeType);

            bool isContract;
            initializer.PluginContext.GetContextParameter("isContract", out isContract);

            string additionalDrivers;
            initializer.PluginContext.GetContextParameter("AdditionalDrivers", out additionalDrivers);

            string PaymentStatus;
            initializer.PluginContext.GetContextParameter<string>("PaymentStatus", out PaymentStatus);

            int langId;
            initializer.PluginContext.GetContextParameter<int>("LangId", out langId);

            var selectedCustomerObject = JsonConvert.DeserializeObject<ReservationCustomerParameters>(SelectedCustomer);
            var selectedDateandBranchObject = JsonConvert.DeserializeObject<ReservationDateandBranchParameters>(SelectedDateAndBranch);
            var selectedEquipmentObject = JsonConvert.DeserializeObject<ReservationEquipmentParameters>(SelectedEquipment);
            var selectedPriceObject = JsonConvert.DeserializeObject<ReservationPriceParameters>(PriceParameters);
            var selectedAdditionalProducts = JsonConvert.DeserializeObject<List<ReservationAdditionalProductParameters>>(SelectedAdditionalProducts);

            ReservationRelatedParameters reservationRelatedParameter = new ReservationRelatedParameters
            {
                reservationChannel = ReservationChannel,
                reservationTypeCode = ReservationTypeCode,
                reservationId = new Guid(ReservationId),
                statusCode = 100000006
            };

            initializer.TraceMe("res ID : " + reservationRelatedParameter.reservationId);
            initializer.TraceMe("changeType : " + ChangeType);
            initializer.TraceMe("SelectedCustomer : " + SelectedCustomer);
            initializer.TraceMe("SelectedDateAndBranch : " + SelectedDateAndBranch);
            initializer.TraceMe("SelectedEquipment : " + SelectedEquipment);
            initializer.TraceMe("PriceObject : " + PriceParameters);
            initializer.TraceMe("SelectedAdditionalProducts : " + SelectedAdditionalProducts);
            initializer.TraceMe("AdditionalDrivers : " + additionalDrivers);
            initializer.TraceMe("PaymentStatus : " + PaymentStatus);
            initializer.TraceMe("isContract : " + isContract);
            initializer.TraceMe("CouponCode : " + CouponCode);
            initializer.TraceMe("BirdId : " + BirdId);
            //initializer.TraceMe("paymentChannelCode" + PaymentMapper.mapDocumentChanneltoPaymentChannel(reservationRelatedParameter.reservationChannel));
            #endregion
            try
            {
                #region Validations

                #region Reservation Date and Branch Inputs Validation
                //initializer.TraceMe("Reservation Date and Branch Inpputs Validation start");
                ReservationCreationValidation reservationCreationValidation = new ReservationCreationValidation(initializer.Service);
                var dateandBranchValidation = reservationCreationValidation.checkReservationDateandBranch(selectedDateandBranchObject.pickupDate, selectedDateandBranchObject.dropoffDate,
                                                                            selectedDateandBranchObject.pickupBranchId, selectedDateandBranchObject.dropoffBranchId);
                if (!dateandBranchValidation)
                {
                    XrmHelper xrmHelper = new XrmHelper(initializer.Service);
                    var message = xrmHelper.GetXmlTagContentByGivenLangId("DateandBranchNullCheck", langId);
                    initializer.PluginContext.ThrowException<InvalidPluginExecutionException>(message);
                }
                initializer.TraceMe("Reservation Date and Branch Inpputs Validation end");
                #endregion

                #region Existing Reservations and Contracts Validation
                initializer.TraceMe("Existing Reservations and Contracts Validation start");
                SystemParameterBL systemParameterBL = new SystemParameterBL(initializer.Service);
                var reservationRelatedSystemParameters = systemParameterBL.GetSystemParameters();
                if (reservationRelatedSystemParameters.isReservationandContractCheckEnabled)
                {
                    var validationResponse = reservationCreationValidation.checkExistingReservationsDateTime(selectedCustomerObject.contactId,
                                                                                                             reservationRelatedParameter.reservationId,
                                                                                                             selectedDateandBranchObject.pickupDate.AddMinutes(StaticHelper.offset),
                                                                                                             selectedDateandBranchObject.dropoffDate.AddMinutes(StaticHelper.offset),
                                                                                                             langId);
                    if (!validationResponse.ResponseResult.Result)
                    {
                        initializer.PluginContext.ThrowException<InvalidPluginExecutionException>(validationResponse.ResponseResult.ExceptionDetail);
                    }
                    ContractCreationValidation contractCreationValidation = new ContractCreationValidation(initializer.Service);
                    validationResponse = contractCreationValidation.checkExistingContractsDateTime(selectedCustomerObject.contactId,
                                                                                                   selectedDateandBranchObject.pickupDate.AddMinutes(StaticHelper.offset),
                                                                                                   selectedDateandBranchObject.dropoffDate.AddMinutes(StaticHelper.offset),
                                                                                                   langId);
                    if (!validationResponse.ResponseResult.Result)
                    {
                        initializer.PluginContext.ThrowException<InvalidPluginExecutionException>(validationResponse.ResponseResult.ExceptionDetail);
                    }
                }
                initializer.TraceMe("Existing Reservations and Contracts Validation edn");
                #endregion

                #region Branchs Working Hour Validations
                initializer.TraceMe("Branchs Working Hour Validations start");
                WorkingHourValidation workingHourValidation = new WorkingHourValidation(initializer.Service);
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
                initializer.TraceMe("Branchs Working Hour Validations end");
                #endregion

                #region Retrieve Individual Customer Detail Data
                initializer.TraceMe("individualCustomer retrieve start");
                IndividualCustomerBL individualCustomerBL = new IndividualCustomerBL(initializer.Service);
                var individualCustomer = individualCustomerBL.getIndividualCustomerInformationForValidation(selectedCustomerObject.contactId);
                initializer.TraceMe("individualCustomer retrieve end");
                #endregion

                #region Retrieve and Convert Group Code Information
                initializer.TraceMe("groupcodeInfo retrieve start");
                var groupCodeData = new GroupCodeInformationCRMData();
                var isDoubleCreditCard = false;
                ReservationRepository reservationRepository = new ReservationRepository(initializer.Service);
                var reservation = reservationRepository.getReservationById(Guid.Parse(ReservationId), new string[] { "rnt_groupcodeid",
                                                                                                                     "rnt_doublecreditcard",
                                                                                                                     "rnt_minimumage",
                                                                                                                     "rnt_minimumdriverlicience",
                                                                                                                     "rnt_youngdriverage",
                                                                                                                     "rnt_youngdriverlicence",
                                                                                                                     "rnt_paymentchoicecode",
                                                                                                                     "transactioncurrencyid",
                                                                                                                     "rnt_pricinggroupcodeid",
                                                                                                                     "rnt_reservationtypecode",
                                                                                                                     "rnt_paymentmethodcode"});
                var reservationGroupCode = reservation.GetAttributeValue<EntityReference>("rnt_groupcodeid").Id;

                initializer.TraceMe("reservationGroupCode : " + reservationGroupCode);
                initializer.TraceMe("selectedEquipmentObject.groupCodeInformationId : " + selectedEquipmentObject.groupCodeInformationId);
                if (selectedEquipmentObject.groupCodeInformationId == reservationGroupCode)
                {
                    groupCodeData.rnt_youngdriverlicence = reservation.GetAttributeValue<int>("rnt_youngdriverlicence");
                    groupCodeData.rnt_youngdriverage = reservation.GetAttributeValue<int>("rnt_youngdriverage");
                    groupCodeData.rnt_minimumage = reservation.GetAttributeValue<int>("rnt_minimumage");
                    isDoubleCreditCard = reservation.GetAttributeValue<bool>("rnt_doublecreditcard");

                    initializer.TraceMe("selectedEquipmentObject.groupCodeInformationId == reservationGroupCode");
                }
                else
                {
                    GroupCodeInformationBL groupCodeInformationBL = new GroupCodeInformationBL(initializer.Service);
                    var groupCodeInformationForDocument = new GroupCodeInformationDetailDataForDocument();
                    if ((ChangeType == (int)GlobalEnums.GroupCodeChangeType.Upgrade) || (ChangeType == (int)GlobalEnums.GroupCodeChangeType.Downgrade))
                    {
                        groupCodeInformationForDocument = groupCodeInformationBL.getGroupCodeInformationDetailForDocument(reservation.GetAttributeValue<EntityReference>("rnt_pricinggroupcodeid").Id);
                    }
                    else
                    {
                        groupCodeInformationForDocument = groupCodeInformationBL.getGroupCodeInformationDetailForDocument(selectedEquipmentObject.groupCodeInformationId);
                    }
                    groupCodeData = new GroupCodeInformationCRMData
                    {
                        rnt_youngdriverlicence = groupCodeInformationForDocument.youngDriverLicense,
                        rnt_youngdriverage = groupCodeInformationForDocument.youngDriverAge,
                        rnt_minimumage = groupCodeInformationForDocument.minimumAge,
                        rnt_findeks = groupCodeInformationForDocument.findeks
                    };
                    isDoubleCreditCard = groupCodeInformationForDocument.doubleCreditCard;
                    initializer.TraceMe("selectedEquipmentObject.groupCodeInformationId != reservationGroupCode");
                }

                initializer.TraceMe("groupcodeInfo retrieve end");
                #endregion

                #region Additional Product Validations
                initializer.TraceMe("additionalProducts retrieve start");
                AdditionalProductsBL additionalProductsBL = new AdditionalProductsBL(initializer.Service);

                // check individual customer age, driving license date and get additional products
                var additionalProducts = additionalProductsBL.GetAdditionalProducts(groupCodeData, individualCustomer, selectedDateandBranchObject, TotalDuration);
                if (!additionalProducts.ResponseResult.Result)
                {
                    initializer.PluginContext.ThrowException<InvalidPluginExecutionException>(additionalProducts.ResponseResult.ExceptionDetail);
                }
                initializer.TraceMe("additionalProducts retrieve end");

                initializer.TraceMe("additionalProducts validations convert start ");
                //check mandatory additional products
                AdditionalProductValidation additionalProductValidation = new AdditionalProductValidation(initializer.Service);
                var currentProducts = selectedAdditionalProducts.ConvertAll(item => new AdditionalProductData
                {
                    productCode = item.productCode,
                    productId = item.productId,
                    value = item.value,
                    maxPieces = item.maxPieces
                });
                initializer.TraceMe("additionalProducts validations convert end ");
                initializer.TraceMe("additionalProducts compareAdditionalProductForMandatoryProducts convert start");
                //initializer.TraceMe("currentProducts : " + JsonConvert.SerializeObject(currentProducts));
                //initializer.TraceMe("additionalProducts.AdditionalProducts : " + JsonConvert.SerializeObject(additionalProducts.AdditionalProducts));
                //var result = additionalProductValidation.compareAdditionalProductForMandatoryProducts(currentProducts, additionalProducts.AdditionalProducts);
                //if (!result)
                //{
                //    XrmHelper xrmHelper = new XrmHelper(initializer.Service);
                //    var message = xrmHelper.GetXmlTagContentByGivenLangId("MandatoryAdditionalProducts", langId);
                //    initializer.PluginContext.ThrowException<InvalidPluginExecutionException>(message);
                //}
                initializer.TraceMe("additionalProducts compareAdditionalProductForMandatoryProducts convert end");

                initializer.TraceMe("additionalProducts checkAdditionalProductsMaxPieces convert start");
                var result = additionalProductValidation.checkAdditionalProductsMaxPieces(currentProducts);
                if (!result)
                {
                    XrmHelper xrmHelper = new XrmHelper(initializer.Service);
                    var message = xrmHelper.GetXmlTagContentByGivenLangId("AdditionalProductMaxPiecesValidation", langId);
                    initializer.PluginContext.ThrowException<InvalidPluginExecutionException>(message);
                }
                initializer.TraceMe("additionalProducts checkAdditionalProductsMaxPieces convert end");

                initializer.TraceMe("additionalProducts checkAdditionalProductsRules convert start");
                result = additionalProductValidation.checkAdditionalProductsRules(currentProducts);
                if (!result)
                {
                    XrmHelper xrmHelper = new XrmHelper(initializer.Service);
                    var message = xrmHelper.GetXmlTagContentByGivenLangId("AdditionalProductRules", langId);
                    initializer.PluginContext.ThrowException<InvalidPluginExecutionException>(message);
                }
                initializer.TraceMe("additionalProducts checkAdditionalProductsRules convert end");

                initializer.TraceMe("additionalProducts checkAdditionalProductsLogoId convert start");
                result = additionalProductValidation.checkAdditionalProductsLogoId(currentProducts);
                if (!result)
                {
                    XrmHelper xrmHelper = new XrmHelper(initializer.Service);
                    var message = xrmHelper.GetXmlTagContentByGivenLangId("AdditionalProductMissingLogoId", langId);
                    initializer.PluginContext.ThrowException<InvalidPluginExecutionException>(message);
                }
                initializer.TraceMe("additionalProducts checkAdditionalProductsLogoId convert end");

                #endregion

                #endregion

                #region Update Reservation
                //initializer.TraceMe("updateReservation start");
                initializer.TraceMe("TrackingNumber : " + TrackingNumber);


                ReservationBL reservationBL = new ReservationBL(initializer.Service, initializer.TracingService);
                var reservationUpdateResponse = reservationBL.updateReservation(selectedCustomerObject,
                                                                                selectedDateandBranchObject,
                                                                                selectedEquipmentObject,
                                                                                selectedPriceObject,
                                                                                reservationRelatedParameter,
                                                                                selectedAdditionalProducts,
                                                                                new Guid(Currency),
                                                                                TotalDuration,
                                                                                TrackingNumber,
                                                                                ChangeType,
                                                                                isContract);

                initializer.TraceMe("reservationUpdateResponse : " + JsonConvert.SerializeObject(reservationUpdateResponse));


                //initializer.TraceMe("updateReservation end");
                #endregion

                #region Update Invoice start
                // initializer.TraceMe("invoice start");

                InvoiceRepository invoiceRepository = new InvoiceRepository(initializer.Service);
                var invoice = invoiceRepository.getInvoiceByReservationId(Guid.Parse(ReservationId));

                InvoiceBL invoiceBL = new InvoiceBL(initializer.Service);
                if (invoice != null)
                {
                    invoiceBL.updateInvoice(selectedCustomerObject.invoiceAddress,
                                            invoice.Id,
                                            Guid.Parse(ReservationId),
                                            null,
                                            Guid.Parse(Currency));
                }
                //initializer.TraceMe("invoice end");
                #endregion

                if (isContract)
                {
                    var paymentStatus = JsonConvert.DeserializeObject<PaymentStatus>(PaymentStatus);

                    #region Get Active reservationitems Guids
                    ReservationItemBL reservationItemBL = new ReservationItemBL(initializer.Service);
                    var reservationItems = reservationItemBL.getActiveReservationItemsGuidsByReservationId(Guid.Parse(ReservationId));

                    #endregion

                    #region create contract header
                    ContractBL contractBL = new ContractBL(initializer.Service, initializer.TracingService);
                    var createContractResponse = contractBL.createContractWithInitializeFromRequest(Guid.Parse(ReservationId), reservationUpdateResponse.pnrNumber, ReservationChannel, selectedPriceObject.campaignId.HasValue ? selectedPriceObject.campaignId.Value : Guid.Empty, CouponCode, BirdId);
                    #endregion

                    #region create invoice
                    var invoiceId = invoiceBL.createInvoice(selectedCustomerObject.invoiceAddress,
                                                       null,
                                                       createContractResponse.contractId,
                                                       new Guid(Currency));
                    #endregion

                    #region Create Contract items                    

                    ContractItemBL contractItemBL = new ContractItemBL(initializer.Service);
                    var createdContractItems = contractItemBL.createContractItemsWithInitializeFromRequest(reservationItems, createContractResponse.contractId, invoiceId, ReservationChannel, null);

                    var res = reservationRepository.getReservationById(reservationRelatedParameter.reservationId, new string[] { "rnt_paymentmethodcode", "createdon" });
                    if (res.GetAttributeValue<OptionSetValue>("rnt_paymentmethodcode").Value == (int)rnt_PaymentMethodCode.PayBroker)
                    {
                        initializer.TraceMe("broker res");
                        ReservationItemRepository reservationItemRepository = new ReservationItemRepository(initializer.Service);
                        var resItems = reservationItemRepository.getCompletedEquipmentandPriceDifferenceReservationItemsWithGivenColumns(res.Id, new string[] { });
                        if (resItems.Count > 0)
                        {
                            var l = new List<Guid>();
                            foreach (var item in resItems)
                            {
                                l.Add(item.Id);
                            }
                            ContractItemBL contractItemBL1 = new ContractItemBL(initializer.Service, initializer.TracingService);
                            var resContractItems = contractItemBL1.createContractItemsWithInitializeFromRequest(l, createContractResponse.contractId, invoiceId, ReservationChannel, null);

                            foreach (var cItems in resContractItems)
                            {
                                //items.FirstOrDefault().contractItemId
                                Entity e = new Entity("rnt_contractitem");
                                e["statecode"] = new OptionSetValue((int)GlobalEnums.StateCode.Active);
                                e["statuscode"] = new OptionSetValue((int)rnt_contractitem_StatusCode.Completed);
                                e.Id = cItems.contractItemId;
                                initializer.Service.Update(e);
                                if (cItems.itemTypeCode == (int)rnt_contractitem_rnt_itemtypecode.Equipment)
                                {
                                    createdContractItems.Add(cItems);
                                }
                            }

                        }
                        initializer.TraceMe("broker res end");

                    }
                    #endregion

                    #region Coupon Code Operations

                    CouponCodeOperationsResponse couponCodeOperationsResponse = new CouponCodeOperationsResponse();

                    if (selectedPriceObject.discountAmount.HasValue && selectedPriceObject.discountAmount.Value > 0)
                    {
                        initializer.TraceMe("discountAmount has value : " + selectedPriceObject.discountAmount);
                        ConfigurationBL configurationBL = new ConfigurationBL(initializer.Service);
                        var productCode = configurationBL.GetConfigurationByName("additionalProducts_manuelDiscount");
                        AdditionalProductRepository additionalProductRepository = new AdditionalProductRepository(initializer.Service);
                        var additionalProduct = additionalProductRepository.getAdditionalProductByProductCode(productCode, new string[] { "rnt_pricecalculationtypecode", "rnt_name" });

                        initializer.TraceMe("manuelDiscountAdditionalProductID " + additionalProduct.Id);
                        var manuelDiscountAdditionalProduct = new ContractAdditionalProductParameters
                        {
                            actualTotalAmount = selectedPriceObject.discountAmount.Value * -1,
                            actualAmount = selectedPriceObject.discountAmount.Value * -1,
                            maxPieces = 1,
                            value = 1,
                            productId = additionalProduct.Id,
                            priceCalculationType = additionalProduct.GetAttributeValue<OptionSetValue>("rnt_pricecalculationtypecode").Value,
                            monthlyPackagePrice = decimal.Zero,
                            productName = additionalProduct.GetAttributeValue<string>("rnt_name")
                        };

                        initializer.TraceMe("after build manuelDiscountAdditionalProduct");
                        var contractSelectedDateandBranchObject = new ContractDateandBranchParameters
                        {
                            dropoffBranchId = selectedDateandBranchObject.dropoffBranchId,
                            dropoffDate = selectedDateandBranchObject.dropoffDate,
                            pickupBranchId = selectedDateandBranchObject.pickupBranchId,
                            pickupDate = selectedDateandBranchObject.pickupDate
                        };
                        contractItemBL.createContractItemForAdditionalProduct(contractSelectedDateandBranchObject,
                                                                              manuelDiscountAdditionalProduct,
                                                                              selectedCustomerObject.contactId,
                                                                              selectedEquipmentObject.groupCodeInformationId,
                                                                              createContractResponse.contractId,
                                                                              Guid.Parse(Currency),
                                                                              invoiceId,
                                                                              TotalDuration,
                                                                              (int)rnt_contractitem_StatusCode.Completed,
                                                                              TrackingNumber,
                                                                              ReservationChannel,
                                                                              null,
                                                                              null);
                        initializer.TraceMe("created manuelDiscountAdditionalProduct");

                        initializer.TraceMe("CouponCode : " + CouponCode);
                        if (!string.IsNullOrEmpty(CouponCode))
                        {
                            CouponCodeBL couponCodeBL = new CouponCodeBL(initializer.Service, initializer.TracingService);
                            couponCodeOperationsResponse = couponCodeBL.executeCouponCodeOperations(new CouponCodeOperationsParameter
                            {
                                couponCode = CouponCode,
                                statusCode = (int)GlobalEnums.CouponCodeStatusCode.Used,
                                contractId = createContractResponse.contractId.ToString(),
                                reservationId = ReservationId,
                                reservationChannelCode = ReservationChannel,
                                reservationTypeCode = ReservationTypeCode,
                                pickupBranchId = selectedDateandBranchObject.pickupBranchId,
                                groupCodeInformationId = selectedEquipmentObject.groupCodeInformationId,
                                contactId = Convert.ToString(selectedCustomerObject.contactId),
                                accountId = selectedCustomerObject.contactId != Guid.Empty || selectedCustomerObject.contactId != null ? string.Empty : Convert.ToString(selectedCustomerObject.corporateCustomerId)
                            });
                        }

                        if (!couponCodeOperationsResponse.ResponseResult.Result)
                        {
                            initializer.PluginContext.ThrowException<InvalidPluginExecutionException>(couponCodeOperationsResponse.ResponseResult.ExceptionDetail);
                        }
                    }

                    #endregion Coupon Code Operations

                    #region Update ReservationPayments With Contract
                    initializer.TraceMe("updateReservationPaymentsWithContract start");
                    contractBL.updateReservationPaymentsWithContract(createContractResponse.contractId, Guid.Parse(ReservationId));
                    initializer.TraceMe("updateReservationPaymentsWithContract end");
                    #endregion

                    #region Update Reservation Credit Card Slip With contract
                    initializer.TraceMe("Update Reservation Credit Card Slip With contract start");
                    contractBL.updateReservationCreditCardSlipsWithContract(createContractResponse.contractId, Guid.Parse(ReservationId));
                    initializer.TraceMe("Update Reservation Credit Card Slip With contract end");
                    #endregion

                    #region complete reservation header
                    initializer.TraceMe("completeReservationHeaderandItemsForContract start");
                    reservationBL.completeReservationHeaderandItemsForContract(Guid.Parse(ReservationId));
                    initializer.TraceMe("completeReservationHeaderandItemsForContract end");
                    #endregion

                    #region additional drivers
                    initializer.TraceMe("createAdditionalDrivers start");
                    if (!string.IsNullOrEmpty(additionalDrivers))
                    {
                        AdditionalDriversBL additionalDriversBL = new AdditionalDriversBL(initializer.Service);
                        additionalDriversBL.createAdditionalDrivers(JsonConvert.DeserializeObject<List<Guid>>(additionalDrivers), createContractResponse.contractId);
                    }
                    initializer.TraceMe("createAdditionalDrivers end");
                    #endregion

                    #region Update reservation contract header
                    reservationBL.updateReservationHeaderForContract(Guid.Parse(ReservationId), createContractResponse.contractId);
                    #endregion                  

                    #region Update reservation is walk-in
                    reservationBL.updateReservationIsWalkin(res);
                    #endregion

                    #region Update Document Plans

                    PaymentPlanRepository paymentPlanRepository = new PaymentPlanRepository(initializer.Service);
                    var plans = paymentPlanRepository.getPaymentPlansByReservationId(reservationUpdateResponse.reservationId);

                    foreach (var item in plans)
                    {
                        Entity document = new Entity("rnt_documentpaymentplan");
                        document.Id = item.Id;
                        document["rnt_contractid"] = new EntityReference("rnt_contract", createContractResponse.contractId);
                        initializer.Service.Update(document);
                    }
                    #endregion

                    try
                    {
                        ContractHelper contractHelper = new ContractHelper(initializer.Service);
                        var corpContracts = contractHelper.checkMakePayment_CorporateContracts(reservation.GetAttributeValue<OptionSetValue>("rnt_reservationtypecode").Value,
                                                                                               reservation.GetAttributeValue<OptionSetValue>("rnt_paymentmethodcode").Value);
                        initializer.TraceMe("corpContracts : " + corpContracts);

                        if (!corpContracts)
                        {
                            initializer.TraceMe("not corp contracts");
                            #region collect cards from parameters
                            var paymentCard = selectedPriceObject.creditCardData.Where(x => x.cardType == (int)PaymentEnums.PaymentTransactionType.SALE &&
                                                                 (x.creditCardId != null || !string.IsNullOrEmpty(x.creditCardNumber))).FirstOrDefault();
                            var depositCard = selectedPriceObject.creditCardData.Where(x => x.cardType == (int)PaymentEnums.PaymentTransactionType.DEPOSIT &&
                                                                    (x.creditCardId != null || !string.IsNullOrEmpty(x.creditCardNumber))).FirstOrDefault();
                            #endregion

                            #region get reservation(contract) amount difference

                            if (reservation.GetAttributeValue<OptionSetValue>("rnt_reservationtypecode").Value == (int)rnt_ReservationTypeCode.Broker ||
                                reservation.GetAttributeValue<OptionSetValue>("rnt_reservationtypecode").Value == (int)rnt_ReservationTypeCode.Acente)
                            {
                                ConfigurationRepository configurationRepository = new ConfigurationRepository(initializer.Service);
                                var currency = configurationRepository.GetConfigurationByKey("currency_TRY");
                                if (new Guid(currency) != reservation.GetAttributeValue<EntityReference>("transactioncurrencyid").Id)
                                {
                                    initializer.TraceMe("different currency");
                                    ContractBL contractBL1 = new ContractBL(initializer.Service, initializer.TracingService);
                                    contractBL1.updateContractItemsandContractTurkishCurrency(createContractResponse.contractId, new Guid(currency));

                                    ReservationItemRepository reservationItemRepository = new ReservationItemRepository(initializer.Service);
                                    var items = reservationItemRepository.getCompletedReservationItemsWithGivenColumns(reservationRelatedParameter.reservationId, new string[] { });
                                    foreach (var item in items)
                                    {
                                        var currencyItem = new Entity("rnt_reservationitem");
                                        currencyItem.Id = item.Id;
                                        currencyItem["transactioncurrencyid"] = new EntityReference("transactioncurrency", reservation.GetAttributeValue<EntityReference>("transactioncurrencyid").Id);
                                        initializer.Service.Update(currencyItem);
                                    }

                                    ContractRepository contractRepository = new ContractRepository(initializer.Service);
                                    var c = contractRepository.getContractById(createContractResponse.contractId, new string[] { "rnt_corporatetotalamount" });
                                    reservationUpdateResponse.corporateAmount = c.GetAttributeValue<Money>("rnt_corporatetotalamount").Value * -1;
                                }

                            }

                            if (reservation.GetAttributeValue<OptionSetValue>("rnt_reservationtypecode").Value != (int)rnt_ReservationTypeCode.Broker)
                            {
                                reservationUpdateResponse.corporateAmount = 0;
                            }

                            var diff = contractBL.getContractDiffAmount(createContractResponse.contractId);
                            var depositAmount = contractBL.getContractDepositAmount(createContractResponse.contractId);

                            initializer.TraceMe("depositAmount end" + depositAmount);
                            initializer.TraceMe("diff" + diff);

                            // For upsell or extend date
                            diff = diff - reservationUpdateResponse.corporateAmount;
                            if (diff < 1 && string.IsNullOrEmpty(CouponCode))
                            {
                                diff = decimal.Zero;
                            }
                            initializer.TraceMe("latest diff : " + diff);


                            #endregion

                            #region Broker ve Acentada ödeme kartı zorunludur. Acenta full credit hariç

                            if (reservation.GetAttributeValue<OptionSetValue>("rnt_reservationtypecode").Value == (int)rnt_ReservationTypeCode.Broker ||
                                reservation.GetAttributeValue<OptionSetValue>("rnt_reservationtypecode").Value == (int)rnt_ReservationTypeCode.Acente)
                            {
                                if (paymentCard == null && diff > 0)
                                {
                                    throw new Exception("Broker ve Acentada ödeme kartı bilgisi zorunludur.");
                                }
                            }
                            #endregion

                            #region check double credit card
                            initializer.TraceMe("rnt_doublecreditcard : " + isDoubleCreditCard);
                            if (isDoubleCreditCard)
                            {

                                var creditCardResponse = contractHelper.checkDoubleCreditCard(diff.Value,
                                                                                              depositAmount.Value,
                                                                                              paymentCard,
                                                                                              depositCard,
                                                                                              isDoubleCreditCard,
                                                                                              selectedCustomerObject.contactId,
                                                                                              langId,
                                                                                              ChangeType);

                                initializer.TraceMe("creditCardResponse : " + JsonConvert.SerializeObject(creditCardResponse));
                                if (!string.IsNullOrEmpty(creditCardResponse))
                                {
                                    initializer.PluginContext.ThrowException<InvalidPluginExecutionException>(creditCardResponse);
                                }
                                if (depositAmount > 0)
                                {
                                    var resCard = contractHelper.checkPaymentCardandDepositCardIsSame(isDoubleCreditCard,
                                                                                                      reservationRelatedParameter.reservationId,
                                                                                                      paymentCard,
                                                                                                      depositCard,
                                                                                                      langId);

                                    if (!string.IsNullOrEmpty(resCard))
                                    {
                                        initializer.PluginContext.ThrowException<InvalidPluginExecutionException>(resCard);
                                    }
                                }
                            }
                            else if (paymentCard != null && depositCard == null)
                            {
                                depositCard = paymentCard;
                            }
                            else if (paymentCard == null && depositCard != null)
                            {
                                paymentCard = depositCard;
                            }


                            #endregion

                            #region make payment                         
                            initializer.TraceMe("reservationUpdateResponse : " + JsonConvert.SerializeObject(reservationUpdateResponse));

                            var contractPaymentResponse = contractBL.makeContractPayment(diff,
                                                                                         depositAmount,
                                                                                         paymentCard,
                                                                                         depositCard,
                                                                                         selectedCustomerObject.invoiceAddress,
                                                                                         Guid.Parse(Currency),
                                                                                         selectedCustomerObject.contactId,
                                                                                         createContractResponse.contractId,
                                                                                         Guid.Parse(ReservationId),
                                                                                         reservationUpdateResponse.pnrNumber,
                                                                                         paymentStatus,
                                                                                         langId,
                                                                                         selectedPriceObject.virtualPosId,
                                                                                         selectedPriceObject.installment,
                                                                                         PaymentMapper.mapDocumentChanneltoPaymentChannel(reservationRelatedParameter.reservationChannel),
                                                                                         selectedPriceObject.use3DSecure,
                                                                                         selectedPriceObject.callBackUrl);
                            initializer.TraceMe("contractPaymentResponse : " + JsonConvert.SerializeObject(contractPaymentResponse));
                            if (!contractPaymentResponse.ResponseResult.Result)
                            {
                                initializer.PluginContext.ThrowException<InvalidPluginExecutionException>(contractPaymentResponse.ResponseResult.ExceptionDetail);
                            }
                            #endregion
                        }

                        else
                        {
                            //fullcredit senaryosu için
                            if (reservation.GetAttributeValue<OptionSetValue>("rnt_reservationtypecode").Value == (int)rnt_ReservationTypeCode.Broker ||
                                reservation.GetAttributeValue<OptionSetValue>("rnt_reservationtypecode").Value == (int)rnt_ReservationTypeCode.Acente)
                            {
                                initializer.TraceMe("corp contracts for full credit");

                                ConfigurationRepository configurationRepository = new ConfigurationRepository(initializer.Service);
                                var currency = configurationRepository.GetConfigurationByKey("currency_TRY");
                                if (new Guid(currency) != reservation.GetAttributeValue<EntityReference>("transactioncurrencyid").Id)
                                {
                                    initializer.TraceMe("different currency");
                                    contractBL.updateContractItemsandContractTurkishCurrency(createContractResponse.contractId, new Guid(currency));

                                    ContractRepository contractRepository = new ContractRepository(initializer.Service);
                                    var c = contractRepository.getContractById(createContractResponse.contractId, new string[] { "rnt_corporatetotalamount" });
                                    reservationUpdateResponse.corporateAmount = c.GetAttributeValue<Money>("rnt_corporatetotalamount").Value * -1;
                                }

                            }
                        }

                        #region Update Coupon Code 

                        initializer.TraceMe("CouponCode" + CouponCode);
                        if (!string.IsNullOrEmpty(CouponCode) && selectedPriceObject.discountAmount.HasValue && selectedPriceObject.discountAmount.Value > 0)
                        {
                            initializer.TraceMe("Coupon code process start : " + CouponCode);
                            CouponCodeBL couponCodeBL = new CouponCodeBL(initializer.Service, initializer.TracingService);
                            couponCodeBL.updateCouponCodeInMongoDB(couponCodeOperationsResponse.couponCodeData);
                            initializer.TraceMe("Coupon code process end");
                        }
                        #endregion

                        #region sending equipment type item to mongodb

                        initializer.TraceMe("mongodb started");
                        try
                        {
                            var equipmentItems = createdContractItems.Where(p => p.itemTypeCode == (int)rnt_contractitem_rnt_itemtypecode.Equipment).ToList();
                            foreach (var item in equipmentItems)
                            {
                                initializer.RetryMethod<MongoDBResponse>(() => contractHelper.createContractItemInMongoDB(item), StaticHelper.retryCount, StaticHelper.retrySleep);
                            }

                        }
                        catch (Exception ex)
                        {
                            //will think a logic
                            initializer.TraceMe("mongodb integration error : " + ex.Message);
                        }
                        initializer.TraceMe("mongodb end");
                        #endregion

                    }
                    catch (Exception ex)
                    {
                        initializer.PluginContext.ThrowException<InvalidPluginExecutionException>(ex.Message);
                    }

                    createContractResponse.ResponseResult = ResponseResult.ReturnSuccess();
                    initializer.PluginContext.OutputParameters["CreateContractResponse"] = JsonConvert.SerializeObject(createContractResponse);
                }
                else
                {
                    if (reservation.GetAttributeValue<OptionSetValue>("rnt_paymentchoicecode").Value == (int)rnt_reservation_rnt_paymentchoicecode.PayNow)
                    {
                        //initializer.TraceMe("pay now reservation");
                        #region Reservation payment operations
                        var diff = reservationBL.getReservationDiffAmount(reservationRelatedParameter.reservationId);

                        if (diff < 0)
                        {
                            initializer.TraceMe("diff < 0 : " + diff);
                            //now make the payment
                            PaymentBL paymentBL = new PaymentBL(initializer.Service, initializer.TracingService);

                            paymentBL.createRefund(new CreateRefundParameters
                            {
                                refundAmount = ((decimal)-1 * diff).Value,
                                reservationId = reservationRelatedParameter.reservationId
                            });
                        }
                        //pay later reservations charge will be in contract creation
                        else if (diff > 0)
                        {
                            //need to check neccessary credit card info is filled
                            CreditCardValidation creditCardValidation = new CreditCardValidation(initializer.Service);
                            //initializer.TraceMe("credit card validation start");
                            var res = creditCardValidation.checkCreditCard(selectedPriceObject.creditCardData.FirstOrDefault(), selectedCustomerObject.contactId, langId);
                            if (!res.ResponseResult.Result)
                            {
                                initializer.PluginContext.ThrowException<InvalidPluginExecutionException>(res.ResponseResult.ExceptionDetail);
                            }
                            //initializer.TraceMe("credit card validation end");
                            //initializer.TraceMe("selectedPriceObject.virtualPosId" + selectedPriceObject.virtualPosId);

                            var individualData = reservationBL.getIndividualCustomerInfo(selectedCustomerObject.contactId);
                            PaymentBL paymentBL = new PaymentBL(initializer.Service);
                            paymentBL.callMakePaymentAction(new CreatePaymentParameters
                            {
                                reservationId = reservationUpdateResponse.reservationId,
                                transactionCurrencyId = new Guid(Currency),
                                individualCustomerId = selectedCustomerObject.contactId,
                                conversationId = reservationUpdateResponse.pnrNumber,
                                langId = langId,
                                paymentTransactionType = (int)PaymentEnums.PaymentTransactionType.SALE,
                                creditCardData = selectedPriceObject.creditCardData.FirstOrDefault(),
                                installment = selectedPriceObject.installment,
                                paidAmount = diff.Value,
                                paymentChannelCode = PaymentMapper.mapDocumentChanneltoPaymentChannel(reservationRelatedParameter.reservationChannel),
                                invoiceAddressData = selectedCustomerObject.invoiceAddress,
                                virtualPosId = selectedPriceObject.virtualPosId
                            });
                            //initializer.TraceMe("payment end for res update");

                        }
                        #endregion
                    }

                }
                initializer.TraceMe("res ID : " + reservationRelatedParameter.reservationId);
                initializer.TraceMe("changeType : " + ChangeType);
                initializer.TraceMe("SelectedCustomer : " + SelectedCustomer);
                initializer.TraceMe("SelectedDateAndBranch : " + SelectedDateAndBranch);
                initializer.TraceMe("SelectedEquipment : " + SelectedEquipment);
                initializer.TraceMe("PriceObject : " + PriceParameters);
                initializer.TraceMe("SelectedAdditionalProducts : " + SelectedAdditionalProducts);
                initializer.TraceMe("AdditionalDrivers : " + additionalDrivers);
                initializer.TraceMe("PaymentStatus : " + PaymentStatus);
                initializer.TraceMe("isContract : " + isContract);
                initializer.TraceMe("CouponCode : " + CouponCode);
                initializer.TraceMe("BirdId : " + BirdId);
                initializer.PluginContext.OutputParameters["ReservationResponse"] = JsonConvert.SerializeObject(reservationUpdateResponse);
                //initializer.TraceMe("reservation update finished");
            }
            catch (Exception ex)
            {
                initializer.TraceMe("res ID : " + reservationRelatedParameter.reservationId);
                initializer.TraceMe("changeType : " + ChangeType);
                initializer.TraceMe("SelectedCustomer : " + SelectedCustomer);
                initializer.TraceMe("SelectedDateAndBranch : " + SelectedDateAndBranch);
                initializer.TraceMe("SelectedEquipment : " + SelectedEquipment);
                initializer.TraceMe("PriceObject : " + PriceParameters);
                initializer.TraceMe("SelectedAdditionalProducts : " + SelectedAdditionalProducts);
                initializer.TraceMe("AdditionalDrivers : " + additionalDrivers);
                initializer.TraceMe("PaymentStatus : " + PaymentStatus);
                initializer.TraceMe("isContract : " + isContract);
                initializer.TraceMe("CouponCode : " + CouponCode);
                initializer.TraceMe("BirdId : " + BirdId);
                initializer.PluginContext.ThrowException<InvalidPluginExecutionException>(ex.Message);
            }
        }
    }
}
