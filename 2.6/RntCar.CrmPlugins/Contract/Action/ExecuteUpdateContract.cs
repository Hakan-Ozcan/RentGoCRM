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
using RntCar.SDK.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RntCar.CrmPlugins.Contract.Action
{
    public class ExecuteUpdateContract : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            PluginInitializer initializer = new PluginInitializer(serviceProvider);
            List<ContractItemResponse> contractItemResponses = new List<ContractItemResponse>();
            try
            {
                #region try
                string contractParameters;
                initializer.PluginContext.GetContextParameter<string>("ContractData", out contractParameters);

                int langId;
                initializer.PluginContext.GetContextParameter<int>("LangId", out langId);

                var deserializedContractParameters = JsonConvert.DeserializeObject<ContractUpdateParameters>(contractParameters);
                //initializer.TraceMe("contract price parameters: " + deserializedContractParameters.priceParameters.price);
                #region Get contract details
                ContractRepository contractRepository = new ContractRepository(initializer.Service);
                var contract = contractRepository.getContractById(deserializedContractParameters.contractId,
                                                                  new string[] { "rnt_totalamount",
                                                                                 "rnt_reservationid",
                                                                                 "rnt_pickupdatetime",
                                                                                 "rnt_dropoffdatetime",
                                                                                 "rnt_contracttypecode",
                                                                                 "rnt_paymentmethodcode",
                                                                                 "rnt_cancontinuewithmonthly",
                                                                                 "rnt_generaltotalamount",
                                                                                 "rnt_dropoffdatetime",
                                                                                 "statuscode",
                                                                                 "rnt_ismonthly"});

                if(contract.GetAttributeValue<OptionSetValue>("statuscode").Value == (int)rnt_contract_StatusCode.EmniyetSuistimal)
                {
                    Entity e = new Entity("rnt_contract");
                    e.Id = contract.Id;
                    e["statuscode"] = new OptionSetValue((int)rnt_contract_StatusCode.Rental);
                    initializer.Service.Update(e);
                }
                #endregion
                initializer.TraceMe("contractParameters : " + JsonConvert.SerializeObject(deserializedContractParameters));

                ContractUpdateValidation contractUpdateValidation = new ContractUpdateValidation(initializer.Service, initializer.TracingService);
                var updateRes = contractUpdateValidation.checkMonthlyValidations(contract, deserializedContractParameters.dateAndBranch.dropoffDate.AddMinutes(StaticHelper.offset), false, deserializedContractParameters.isCarChanged);

                if (!updateRes.ResponseResult.Result)
                {
                    initializer.PluginContext.ThrowException<InvalidPluginExecutionException>(updateRes.ResponseResult.ExceptionDetail);
                }
                #region Validations

                AvailabilityValidation availabilityValidation = new AvailabilityValidation(initializer.Service, initializer.TracingService);
                var r = availabilityValidation.checkBrokerReservation_Contract(new AvailabilityParameters
                {
                    contractId = contract.Id.ToString(),
                    pickupDateTime = contract.GetAttributeValue<DateTime>("rnt_pickupdatetime"),
                    dropoffDateTime = deserializedContractParameters.dateAndBranch.dropoffDate.AddMinutes(StaticHelper.offset)
                }, 1055);

                if (!r.ResponseResult.Result)
                {
                    initializer.PluginContext.ThrowException<InvalidPluginExecutionException>(r.ResponseResult.ExceptionDetail);
                }

                var c = contractRepository.getContractById(deserializedContractParameters.contractId,
                                                            new string[] { "rnt_dropoffdatetime", "rnt_ismonthly" });

                if (deserializedContractParameters.isCarChanged &&
                   DateTime.UtcNow.AddMinutes(StaticHelper.offset) > c.GetAttributeValue<DateTime>("rnt_dropoffdatetime"))
                {
                    var str = string.Format(@"Araç değişikliği yapılmak istenen tarih , sözleşme iade tarihinden büyüktür.
                                            Lütfen uzama işlemini gerçekleştirdikten sonra araç değişikliğini yapınız.
                                            Araç değişiklik tarihi : {0}, Sözleşme iade tarihi : {1}", DateTime.UtcNow.AddMinutes(StaticHelper.offset), c.GetAttributeValue<DateTime>("rnt_dropoffdatetime"));
                    initializer.PluginContext.ThrowException<InvalidPluginExecutionException>(str);
                }

                if (deserializedContractParameters.dateAndBranch.dropoffDate.AddMinutes(StaticHelper.offset) > c.GetAttributeValue<DateTime>("rnt_dropoffdatetime")
                    && deserializedContractParameters.isCarChanged &&
                    !c.GetAttributeValue<bool>("rnt_ismonthly"))
                {
                    initializer.PluginContext.ThrowException<InvalidPluginExecutionException>("Araç değişikliği ve uzama aynı anda yapılamaz.Uzama işlemini tamamladıktan sonra , araç değişikliği girebilirsiniz.");
                }

                
                ContractHelper contractHelper = new ContractHelper(initializer.Service);
                var corpContracts = contractHelper.checkMakePayment_CorporateContracts(contract.GetAttributeValue<OptionSetValue>("rnt_contracttypecode").Value,
                                                                                       contract.GetAttributeValue<OptionSetValue>("rnt_paymentmethodcode").Value);
                //initializer.TraceMe("corpContracts : " + corpContracts);
                var paymentCard = new CreditCardData();
                if (!corpContracts)
                {
                    paymentCard = deserializedContractParameters.priceParameters.creditCardData.Where(x => (x.creditCardId != null || !string.IsNullOrEmpty(x.creditCardNumber))).FirstOrDefault();
                    if (deserializedContractParameters.priceParameters.amounttobePaid > decimal.Zero)
                    {
                        CreditCardValidation creditCardValidation = new CreditCardValidation(initializer.Service);
                        var creditCardResponse = creditCardValidation.checkCreditCard(paymentCard, deserializedContractParameters.contactId, langId);
                        if (!creditCardResponse.ResponseResult.Result)
                        {
                            initializer.PluginContext.ThrowException<InvalidPluginExecutionException>(creditCardResponse.ResponseResult.ExceptionDetail);
                        }
                    }
                }
                //initializer.TraceMe("contract amounttobepaid parameters: " + deserializedContractParameters.priceParameters.amounttobePaid);
                if (deserializedContractParameters.isCarChanged)
                {
                    var validationResponse = contractUpdateValidation.checkContractStatusForCarChangeRequest(deserializedContractParameters.contractId, langId);
                    if (!validationResponse.ResponseResult.Result)
                    {
                        initializer.PluginContext.ThrowException<InvalidPluginExecutionException>(validationResponse.ResponseResult.ExceptionDetail);
                    }
                }
                #endregion

                #region Get ContractItems
                ContractItemRepository contractItemRepository = new ContractItemRepository(initializer.Service);
                var beforeContractItems = contractItemRepository.getRentalContractItemsByContractId(deserializedContractParameters.contractId);
                #endregion

                var oneWayFeeResponse = contractHelper.calculateNewOneWayFeeAmount(deserializedContractParameters.contractId,
                                                                                   deserializedContractParameters.dateAndBranch.dropoffBranchId);


                #region Update Contract
                ContractBL contractBL = new ContractBL(initializer.Service, initializer.TracingService);

                initializer.TraceMe("contract update start");
                var response = contractBL.updateContract(deserializedContractParameters, contractItemResponses);
                initializer.TraceMe("contractItemResponses : " + contractItemResponses.Count);
                initializer.TraceMe("contract update end");
                #endregion

                #region update one way if needed

                if (oneWayFeeResponse.amount != decimal.Zero)
                {
                    Entity e = new Entity("rnt_contractitem");
                    e.Id = oneWayFeeResponse.contractItemId.Value;
                    e["rnt_baseprice"] = new Money(oneWayFeeResponse.amount);
                    e["rnt_totalamount"] = new Money(oneWayFeeResponse.amount);
                    initializer.Service.Update(e);
                }
                #endregion

                #region Update Additional Drivers
                initializer.TraceMe("createAdditionalDrivers start");
                if (deserializedContractParameters.additionalDrivers.Count > 0 && deserializedContractParameters.additionalDrivers != null)
                {

                    AdditionalDriversBL additionalDriversBL = new AdditionalDriversBL(initializer.Service, initializer.TracingService);
                    additionalDriversBL.createAdditionalDrivers(deserializedContractParameters.additionalDrivers, response.contractId);
                }
                initializer.TraceMe("createAdditionalDrivers end");
                #endregion

                #region Update Invoice Address
                InvoiceRepository invoiceRepository = new InvoiceRepository(initializer.Service);
                var invoice = invoiceRepository.getFirstInvoiceByContractId(deserializedContractParameters.contractId);

                InvoiceBL invoiceBL = new InvoiceBL(initializer.Service);
                if (invoice != null)
                {
                    invoiceBL.updateInvoice(deserializedContractParameters.addressData,
                                            invoice.Id,
                                            null,
                                            null,
                                            deserializedContractParameters.currency);
                }
                #endregion

                #region month operations
                if (contract.GetAttributeValue<bool>("rnt_ismonthly") &&
                    deserializedContractParameters.isDateorBranchChanged &&
                   !deserializedContractParameters.isCarChanged)
                {
                    if (deserializedContractParameters.canContinueMonthly)
                    {
                        initializer.TraceMe("aylık");
                        PaymentPlanRepository paymentPlanRepository = new PaymentPlanRepository(initializer.Service);
                        var plans = paymentPlanRepository.getPaymentPlansByContractId(deserializedContractParameters.contractId);

                        if (plans.Where(p => p.GetAttributeValue<OptionSetValue>("rnt_month").Value == deserializedContractParameters.dateAndBranch.dropoffDate.AddDays(-30).Month).FirstOrDefault() == null)
                        {
                            PaymentPlanBL paymentPlanBL = new PaymentPlanBL(initializer.Service);
                            paymentPlanBL.createPaymentPlan(deserializedContractParameters.priceParameters.paymentPlanData.FirstOrDefault(), null, deserializedContractParameters.contractId);
                        }


                        createNewInvoiceTemplate(initializer.Service,
                                                    deserializedContractParameters.contractId,
                                                    beforeContractItems,
                                                    contract,
                                                    1);
                    }
                    //bireysel dönme senaryosu
                    else
                    {
                        initializer.TraceMe("bireysel");
                        #region Bireysel
                        //ileri tarihli ödeme planındaki kayıtları silelim
                        PaymentPlanRepository paymentPlanRepository = new PaymentPlanRepository(initializer.Service);
                        var plans = paymentPlanRepository.getPaymentPlansByContractId(deserializedContractParameters.contractId);
                        var lastMonth = 0;
                        foreach (var item in plans)
                        {

                            if (item.GetAttributeValue<OptionSetValue>("rnt_month").Value > lastMonth)
                            {
                                lastMonth = item.GetAttributeValue<OptionSetValue>("rnt_month").Value;
                            }
                            else
                            {
                                lastMonth = 12 + item.GetAttributeValue<OptionSetValue>("rnt_month").Value;
                            }

                            if (lastMonth >= deserializedContractParameters.dateAndBranch.dropoffDate.Month)
                            {
                                PaymentPlanBL paymentPlanBL = new PaymentPlanBL(initializer.Service);
                                paymentPlanBL.deletePaymentPlanById(item.Id);
                                initializer.TraceMe("payment plans deleted");
                            }

                        }
                       

                        var latestContract = contractRepository.getContractById(deserializedContractParameters.contractId,
                                                                                new string[] { "rnt_generaltotalamount", "rnt_dropoffdatetime" });

                        //bireysel uzatılmısmı diye kontrol et
                        ContractInvoiceDateRepository contractInvoiceDateRepository = new ContractInvoiceDateRepository(initializer.Service);
                        var invoiceTemplate = contractInvoiceDateRepository.getLastInvoiceofMonth_notMonthly(deserializedContractParameters.contractId.ToString(),
                                                                                                             deserializedContractParameters.dateAndBranch.dropoffDate);

                        //o ay için ilk bireysel uzatma!
                        if (invoiceTemplate == null)
                        {
                            initializer.TraceMe("invoiceTemplate is null");

                            createNewInvoiceTemplate(initializer.Service,
                                                    deserializedContractParameters.contractId,
                                                    beforeContractItems,
                                                    contract,
                                                    2);
                        }
                        else
                        {
                            initializer.TraceMe("invoiceTemplate is not null");

                            #region Existing
                            var existingProducts = JsonConvert.DeserializeObject<List<InvoiceItemTemplate>>(invoiceTemplate.GetAttributeValue<string>("rnt_invoicetemplate"));
                            var lastContractItems = contractItemRepository.getRentalContractItemsByContractId(deserializedContractParameters.contractId);

                            var all = contractInvoiceDateRepository.getAllContractInvoicesExceptItSelf(deserializedContractParameters.contractId.ToString(), invoiceTemplate.Id.ToString());
                            foreach (var item in lastContractItems)
                            {
                                var existing = existingProducts.Where(p => p.contractItemId == item.Id).FirstOrDefault();
                                if (existing != null)
                                {
                                    decimal amount = 0;
                                    foreach (var i in all)
                                    {
                                        var list = JsonConvert.DeserializeObject<List<InvoiceItemTemplate>>(i.GetAttributeValue<string>("rnt_invoicetemplate"));
                                        amount += list.Where(p => p.contractItemId == item.Id).Sum(p => p.amount);
                                    }

                                    existing.amount = item.GetAttributeValue<Money>("rnt_totalamount").Value - amount;
                                }
                                else
                                {
                                    existingProducts.Add(new InvoiceItemTemplate
                                    {
                                        additionalProductId = item.Contains("rnt_additionalproductid") ? item.GetAttributeValue<EntityReference>("rnt_additionalproductid").Id : Guid.Empty,
                                        amount = item.GetAttributeValue<Money>("rnt_totalamount").Value,
                                        contractItemId = item.Id,
                                        equipmentId = item.Contains("rnt_equipment") ? item.GetAttributeValue<EntityReference>("rnt_equipment").Id : Guid.Empty,
                                        itemType = item.Contains("rnt_itemtypecode") ? item.GetAttributeValue<OptionSetValue>("rnt_itemtypecode").Value : 0,
                                    });

                                }
                            }
                            invoiceTemplate["rnt_extensiontypecode"] = new OptionSetValue(2);
                            var name = "Bireysel / Kurumsal";
                            name += " " + invoiceTemplate.GetAttributeValue<DateTime>("rnt_pickupdatetime").ToString("dd/MM/yyyy") + " " + latestContract.GetAttributeValue<DateTime>("rnt_dropoffdatetime").ToString("dd/MM/yyyy");
                            invoiceTemplate["rnt_dropoffdatetime"] = latestContract.GetAttributeValue<DateTime>("rnt_dropoffdatetime");
                            invoiceTemplate["rnt_amount"] = new Money(existingProducts.Sum(p => p.amount));
                            invoiceTemplate["rnt_invoicetemplate"] = JsonConvert.SerializeObject(existingProducts);
                            invoiceTemplate["rnt_name"] = name;
                            initializer.Service.Delete("rnt_contractinvoicedate", invoiceTemplate.Id);
                            initializer.Service.Create(invoiceTemplate);
                            #endregion
                        }
                        #endregion
                    }

                }
                #endregion

                #region payment
                if (!corpContracts)
                {
                    var latestContract = contractRepository.getContractById(deserializedContractParameters.contractId,
                                                                        new string[] { "rnt_totalamount" });

                    var latestAmount = latestContract.GetAttributeValue<Money>("rnt_totalamount").Value;
                    ContractHelper contractHelper1 = new ContractHelper(initializer.Service, initializer.TracingService);
                    var differenceAmount = contractHelper1.calculatePaymentAmount(deserializedContractParameters.contractId, null, null, true);
                    //var differenceAmount = latestAmount - initialAmount;

                    initializer.TraceMe("payment start");
                    initializer.TraceMe("differenceAmount : " + JsonConvert.SerializeObject(differenceAmount));

                    if (differenceAmount.contractAmountDifference > StaticHelper._one_ ||
                        differenceAmount.contractAmountDifference < (-1 * StaticHelper._one_))
                    {
                        //initializer.TraceMe("deserializedContractParameters.priceParameters.virtualPosId" + deserializedContractParameters.priceParameters.virtualPosId);
                        //update contract is always from branch for now
                        var paymentResponse = contractBL.makeContractPayment(differenceAmount.contractAmountDifference,
                                                                         decimal.Zero,
                                                                         paymentCard,
                                                                         null,
                                                                         deserializedContractParameters.addressData,
                                                                         deserializedContractParameters.currency,
                                                                         deserializedContractParameters.contactId,
                                                                         deserializedContractParameters.contractId,
                                                                         Guid.Empty,
                                                                         response.pnrNumber,
                                                                         new PaymentStatus
                                                                         {
                                                                             isDepositPaid = true,
                                                                             isReservationPaid = differenceAmount.contractAmountDifference == decimal.Zero ||
                                                                                                 differenceAmount.contractAmountDifference < 1 ? true : false,
                                                                         },
                                                                         langId,
                                                                         deserializedContractParameters.priceParameters.virtualPosId,
                                                                         deserializedContractParameters.priceParameters.installment,
                                                                         rnt_PaymentChannelCode.BRANCH,
                                                                         deserializedContractParameters.priceParameters.use3DSecure,
                                                                         deserializedContractParameters.priceParameters.callBackUrl);
                        if (!paymentResponse.ResponseResult.Result)
                        {
                            initializer.PluginContext.ThrowException<InvalidPluginExecutionException>(paymentResponse.ResponseResult.ExceptionDetail);
                        }
                    }
                }
                #endregion

                #region sending equipment type item to mongodb

                initializer.TraceMe("mongodb started");
                try
                {
                    foreach (var item in contractItemResponses)
                    {
                        //deactivate old item
                        if (item.status == (int)rnt_contractitem_StatusCode.Rental)
                        {
                            initializer.TraceMe("mongodb Rental start");
                            initializer.RetryMethod<MongoDBResponse>(() => contractHelper.updateContractItemInMongoDB(item), StaticHelper.retryCount, StaticHelper.retrySleep);
                            initializer.TraceMe("mongodb Rental end");
                        }
                        else if (item.status == (int)rnt_contractitem_StatusCode.WaitingForDelivery)
                        {
                            initializer.TraceMe("mongodb WaitingForDelivery create start");
                            initializer.RetryMethod<MongoDBResponse>(() => contractHelper.createContractItemInMongoDB(item), StaticHelper.retryCount, StaticHelper.retrySleep);
                            initializer.TraceMe("mongodb WaitingForDelivery create end");
                        }
                    }
                }
                catch (Exception ex)
                {
                    //will think a logic
                    initializer.TraceMe("mongodb integration error : " + ex.Message);
                }
                //initializer.TraceMe("mongodb end");

                #endregion
                //initializer.TraceMe("update contract user : " + initializer.PluginContext.InitiatingUserId);
                //initializer.TraceMe("contractParameters : " + JsonConvert.SerializeObject(deserializedContractParameters));
                initializer.PluginContext.OutputParameters["ContractUpdateResponse"] = JsonConvert.SerializeObject(response);
                #endregion

            }
            catch (Exception ex)
            {
                initializer.PluginContext.ThrowException<InvalidPluginExecutionException>(ex.Message);
            }
        }
        private class tempplans
        {
            public int month { get; set; }
            public Guid planId { get; set; }
        }
        private void createNewInvoiceTemplate(IOrganizationService organizationService,
                                             Guid contractId,
                                             List<Entity> beforeContractItems,
                                             Entity contract,
                                             int type)
        {
            List<InvoiceItemTemplate> invoiceItemTemplates = new List<InvoiceItemTemplate>();
            ContractItemRepository contractItemRepository = new ContractItemRepository(organizationService);
            ContractRepository contractRepository = new ContractRepository(organizationService);

            var afterContractItems = contractItemRepository.getRentalContractItemsByContractId(contractId);

            var latestContract = contractRepository.getContractById(contractId,
                                                                   new string[] { "rnt_generaltotalamount", "rnt_dropoffdatetime" });

            foreach (var item in afterContractItems)
            {
                var b = beforeContractItems.Where(p => p.Id == item.Id).FirstOrDefault();
                //new added
                if (b == null)
                {
                    invoiceItemTemplates.Add(new InvoiceItemTemplate
                    {
                        additionalProductId = item.Contains("rnt_additionalproductid") ? item.GetAttributeValue<EntityReference>("rnt_additionalproductid").Id : Guid.Empty,
                        amount = item.GetAttributeValue<Money>("rnt_totalamount").Value,
                        contractItemId = item.Id,
                        equipmentId = item.Contains("rnt_equipment") ? item.GetAttributeValue<EntityReference>("rnt_equipment").Id : Guid.Empty,
                        itemType = item.Contains("rnt_itemtypecode") ? item.GetAttributeValue<OptionSetValue>("rnt_itemtypecode").Value : 0,
                    });
                }
                else
                {
                    invoiceItemTemplates.Add(new InvoiceItemTemplate
                    {
                        additionalProductId = item.Contains("rnt_additionalproductid") ? item.GetAttributeValue<EntityReference>("rnt_additionalproductid").Id : Guid.Empty,
                        amount = item.GetAttributeValue<Money>("rnt_totalamount").Value - b.GetAttributeValue<Money>("rnt_totalamount").Value,
                        contractItemId = item.Id,
                        equipmentId = item.Contains("rnt_equipment") ? item.GetAttributeValue<EntityReference>("rnt_equipment").Id : Guid.Empty,
                        itemType = item.Contains("rnt_itemtypecode") ? item.GetAttributeValue<OptionSetValue>("rnt_itemtypecode").Value : 0,
                    });
                }
            }
            CreateContractInvoiceDateParameters createContractInvoiceDate = new CreateContractInvoiceDateParameters
            {
                amount = latestContract.GetAttributeValue<Money>("rnt_generaltotalamount").Value - contract.GetAttributeValue<Money>("rnt_generaltotalamount").Value,
                pickupDatime = contract.GetAttributeValue<DateTime>("rnt_dropoffdatetime"),
                dropoffDateTime = latestContract.GetAttributeValue<DateTime>("rnt_dropoffdatetime"),
                contractId = contractId,
                invoiceDate = type == 1 ? latestContract.GetAttributeValue<DateTime>("rnt_dropoffdatetime") : StaticHelper.GetLastDayOfMonth(latestContract.GetAttributeValue<DateTime>("rnt_dropoffdatetime")),
                type = type,

            };
            createContractInvoiceDate.templates = JsonConvert.SerializeObject(invoiceItemTemplates);
            ContractBL contractBL = new ContractBL(organizationService);
            contractBL.createContractInvoiceDate(createContractInvoiceDate);
        }
    }
}
