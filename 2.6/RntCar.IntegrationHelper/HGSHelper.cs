using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Newtonsoft.Json;
using RazorEngine.Configuration;
using RazorEngine.Templating;
using RntCar.BusinessLibrary;
using RntCar.BusinessLibrary.Business;
using RntCar.BusinessLibrary.Helpers;
using RntCar.BusinessLibrary.Repository;
using RntCar.ClassLibrary;
using RntCar.ClassLibrary._Enums_1033;
using RntCar.Logger;
using RntCar.SDK.Common;
using RntCar.SDK.Mappers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Xml;

namespace RntCar.IntegrationHelper
{
    public class HGSHelper : IDisposable
    {
        private static EndpointAddress myEndpointAddress { get; set; }
        private static BasicHttpBinding myBasicHttpBinding { get; set; }
        public IOrganizationService orgService { get; set; }
        public ITracingService tracingService { get; set; }
        private HGSService.HgsWebUtilServicesClient hgsWebUtilServicesClient { get; set; }
        private OperationContextScope scope { get; set; }
        private string[] loginInfo { get; set; }
        private string endpointUrl { get; set; }
        public HGSHelper(IOrganizationService _service)
        {
            orgService = _service;
            this.prepareServiceConfiguration();
            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls11 | System.Net.SecurityProtocolType.Tls12;

            hgsWebUtilServicesClient = new HGSService.HgsWebUtilServicesClient(myBasicHttpBinding, myEndpointAddress);
            scope = new OperationContextScope(hgsWebUtilServicesClient.InnerChannel);
            var sec = new SecurityHeader(loginInfo[0], loginInfo[1]);
            OperationContext.Current.OutgoingMessageHeaders.Add(sec);

        }

        public HGSHelper(IOrganizationService _service, ITracingService _tracingService)
        {
            orgService = _service;
            tracingService = _tracingService;
            this.prepareServiceConfiguration();
            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls11 | System.Net.SecurityProtocolType.Tls12;
            if (hgsWebUtilServicesClient.State != CommunicationState.Opened || hgsWebUtilServicesClient.State != CommunicationState.Opening)
            {
                hgsWebUtilServicesClient = new HGSService.HgsWebUtilServicesClient(myBasicHttpBinding, myEndpointAddress);
                var sec = new SecurityHeader(loginInfo[0], loginInfo[1]);
                OperationContext.Current.OutgoingMessageHeaders.Add(sec);
            }
        }

        public SaleProductResponse saleProduct(SaleProductParameter saleProductParameter)
        {
            LoggerHelper loggerHelper = new LoggerHelper();
            var convertedParameter = new HGSService.requestSaleProductWEB
            {
                licenseNo = saleProductParameter.licenseNo,
                plateNo = saleProductParameter.plateNo,
                productId = saleProductParameter.productId,
                productType = saleProductParameter.productType,
                vehicleClass = saleProductParameter.vehicleClass,
                vehicleClassSpecified = true,
            };
            loggerHelper.traceInfo("Parameter " + JsonConvert.SerializeObject(saleProductParameter));

            var response = hgsWebUtilServicesClient.saleProduct(convertedParameter);

            loggerHelper.traceInfo("Response " + JsonConvert.SerializeObject(response));

            if (!response.errorCode.Equals("000")) // 000 means there is no error
            {
                return new SaleProductResponse
                {
                    ResponseResult = ClassLibrary.ResponseResult.ReturnError(response.errorInfo + " Parameter " + JsonConvert.SerializeObject(saleProductParameter))
                };
            }

            return new SaleProductResponse
            {
                ResponseResult = ResponseResult.ReturnSuccess()
            };
        }

        public CancelProductResponse cancelProduct(CancelProductParameter cancelProductParameter)
        {
            LoggerHelper loggerHelper = new LoggerHelper();
            var convertedParameter = new HGSService.requestCancelProductWEB
            {
                productId = cancelProductParameter.productId,
                cancelReason = cancelProductParameter.cancelReason,
                cancelReasonSpecified = true
            };

            loggerHelper.traceInfo("Parameter " + JsonConvert.SerializeObject(cancelProductParameter));

            var response = hgsWebUtilServicesClient.cancelProduct(convertedParameter);

            loggerHelper.traceInfo("Response " + JsonConvert.SerializeObject(response));
            if (!response.errorCode.Equals("000")) // 000 means there is no error
            {
                return new CancelProductResponse
                {
                    ResponseResult = ClassLibrary.ResponseResult.ReturnError(response.errorInfo + " Parameter " + JsonConvert.SerializeObject(cancelProductParameter))
                };
            }

            return new CancelProductResponse
            {
                ResponseResult = ClassLibrary.ResponseResult.ReturnSuccess()
            };
        }

        public UpdateDirectiveAmountsResponse updateDirectiveAmounts(UpdateDirectiveAmountsParameter updateDirectiveAmountsParameter)
        {
            var convertedParameter = new HGSService.requestUpdateDirectiveAmounts
            {
                productId = updateDirectiveAmountsParameter.productId,
                plateNo = updateDirectiveAmountsParameter.plateNo,
                accountNumber = updateDirectiveAmountsParameter.accountNumber,
                creditCardNumber = updateDirectiveAmountsParameter.creditCardNumber,
                loadingLowerLimit = updateDirectiveAmountsParameter.loadingLowerLimit,
                loadingLowerLimitSpecified = true,
                loadingAmount = updateDirectiveAmountsParameter.loadingAmount,
                loadingAmountSpecified = true
            };
            var response = hgsWebUtilServicesClient.updateDirectiveAmounts(convertedParameter);
            if (!response.errorCode.Equals("000")) // 000 means there is no error
            {
                return new UpdateDirectiveAmountsResponse
                {
                    ResponseResult = ClassLibrary.ResponseResult.ReturnError(response.errorInfo)
                };
            }

            return new UpdateDirectiveAmountsResponse
            {
                ResponseResult = ClassLibrary.ResponseResult.ReturnSuccess()
            };
        }

        public UpdateVehicleInfoResponse updateVehicleInfo(UpdateVehicleInfoParameter updateVehicleInfoParameter)
        {
            var convertedParameter = new HGSService.requestUpdateVehicleInfo
            {
                productId = updateVehicleInfoParameter.productId,
                plateNo = updateVehicleInfoParameter.plateNo,
                licenseNo = updateVehicleInfoParameter.licenseNo
            };
            var response = hgsWebUtilServicesClient.updateVehicleInfo(convertedParameter);
            if (!response.errorCode.Equals("000")) // 000 means there is no error
            {
                return new UpdateVehicleInfoResponse
                {
                    ResponseResult = ClassLibrary.ResponseResult.ReturnError(response.errorInfo)
                };
            }

            return new UpdateVehicleInfoResponse
            {
                ResponseResult = ClassLibrary.ResponseResult.ReturnSuccess()
            };
        }

        public GetHGSTransitListResponse getHgsTransitList(GetHGSTransitListParameter getHGSTransitListParameter)
        {

            var convertedParameter = new HGSService.requestHgsTransitListWEB
            {
                startDate = getHGSTransitListParameter.startDateTimeStamp.converttoDateTime().AddMinutes(StaticHelper.offset),
                finishDate = getHGSTransitListParameter.finishDateTimeStamp.converttoDateTime().AddMinutes(StaticHelper.offset),
                productId = getHGSTransitListParameter.productId,
                finishDateSpecified = true,
                startDateSpecified = true
            };
            List<HGSTransitData> convertedResponse = new List<HGSTransitData>();


            var response = hgsWebUtilServicesClient.getHgsTransitList(convertedParameter);
            if (response.errorCode != null && response.errorCode.Equals("006"))
            {
                return new GetHGSTransitListResponse
                {
                    showErrorMessage = true,
                    responseResult = ClassLibrary._Tablet.ResponseResult.ReturnError(response.errorInfo)
                };

            }

            else if (response.errorCode != null && !response.errorCode.Equals("000"))
            {
                return new GetHGSTransitListResponse
                {
                    responseResult = ClassLibrary._Tablet.ResponseResult.ReturnError(response.errorInfo)
                };
            }

            convertedResponse = response.transits.ToList().ConvertAll(item => new HGSTransitData
            {
                amount = decimal.Parse(item.Amount, new NumberFormatInfo() { NumberDecimalSeparator = "," }),
                description = item.Description,
                entryDateTime = item.EntryDateTime.converttoTimeStamp(),
                _entryDateTime = item.EntryDateTime,
                _exitDateTime = item.ExitDateTime,
                exitDateTime = item.ExitDateTime.converttoTimeStamp(),
                entryLocation = item.EntryLocation,
                exitLocation = item.ExitLocation
            });

            return new GetHGSTransitListResponse
            {
                transits = convertedResponse,
                responseResult = ClassLibrary._Tablet.ResponseResult.ReturnSuccess()
            };
        }

        public GetHGSTransactionListResponse getHgsTransactionList(GetHGSTransactionListParameter getHGSTransactionListParameter)
        {

            var convertedParameter = new HGSService.requestHgsTransactionListWEB
            {
                startDate = getHGSTransactionListParameter.startDateTimeStamp.converttoDateTime().AddMinutes(StaticHelper.offset),
                finishDate = getHGSTransactionListParameter.finishDateTimeStamp.converttoDateTime().AddMinutes(StaticHelper.offset),
                plateNo = getHGSTransactionListParameter.plateNo,
                productId = getHGSTransactionListParameter.productId,
                finishDateSpecified = true,
                startDateSpecified = true
            };
            List<HGSTransactionData> convertedResponse = new List<HGSTransactionData>();


            var response = hgsWebUtilServicesClient.getHgsTransactionList(convertedParameter);
            if (response.errorCode != null && response.errorCode.Equals("006"))
            {
                return new GetHGSTransactionListResponse
                {
                    showErrorMessage = true,
                    ResponseResult = ResponseResult.ReturnError(response.errorInfo)
                };

            }

            else if (response.errorCode != null && !response.errorCode.Equals("000"))
            {
                return new GetHGSTransactionListResponse
                {
                    ResponseResult = ResponseResult.ReturnError(response.errorInfo)
                };
            }

            convertedResponse = response.transactions.ToList().ConvertAll(item => new HGSTransactionData
            {
                tranAmount = decimal.Parse(item.TranAmount, new NumberFormatInfo() { NumberDecimalSeparator = "," }),
                tranDescription = item.TranDescription,
                tranDateTime = item.TranDateTime.converttoTimeStamp(),
                _tranDateTime = item.TranDateTime,
                user = new UserInfo()
                {
                    BankCode = item.User.BankCode,
                    BoxOffice = item.User.BoxOffice,
                    BranchCityCode = item.User.BranchCityCode,
                    BranchName = item.User.BranchName,
                    BranchNo = item.User.BranchNo,
                    BranchTownName = item.User.BranchTownName,
                    HeadOfficeNo = item.User.HeadOfficeNo,
                    SubOrganisation = item.User.SubOrganisation,
                    UserCode = item.User.UserCode,
                    UserName = item.User.UserName,
                    UserSurname = item.User.UserSurname
                }
            });

            return new GetHGSTransactionListResponse
            {
                transactions = convertedResponse,
                ResponseResult = ResponseResult.ReturnSuccess()
            };
        }

        public void processHGSBatch(LoggerHelper loggerHelper = null, string contractItemId = null)
        {
            CrmServiceHelper crmServiceHelper = new CrmServiceHelper();
            if (loggerHelper == null)
                loggerHelper = new LoggerHelper();

            var contractItems = new List<Entity>();
            loggerHelper.traceInfo("retrieve started");
            var duration1 = Convert.ToInt32(StaticHelper.GetConfiguration("duration"));
            if (string.IsNullOrEmpty(contractItemId))
            {
                //ContractItemRepository contractItemRepository = new ContractItemRepository(crmServiceHelper.IOrganizationService);
                //contractItems = contractItemRepository.getCompletedContractItemEquipmentsByXlastDays(90);

                ////for brokers 
                //var priceDifference = contractItemRepository.getBrokerCompletedContractItemsByXlastDays(90);
                //contractItems.AddRange(priceDifference);

                ContractItemRepository contractItemRepository = new ContractItemRepository(crmServiceHelper.IOrganizationService);
                contractItems = contractItemRepository.getCompletedContractItemsByGivenDays(DateTime.Now.AddDays(-duration1).ToString("yyyy-MM-dd"), DateTime.Now.ToString("yyyy-MM-dd"));
                //contractItems = contractItemRepository.getCompletedContractItemsByGivenDays("2021-12-01", "2021-12-31");
            }
            else
            {
                ContractItemRepository contractItemRepository = new ContractItemRepository(crmServiceHelper.IOrganizationService);
                contractItems.Add(contractItemRepository.getContractItemId(new Guid(contractItemId)));
            }

            var i = 1;

            decimal totalAmount = 0;

            loggerHelper.traceInfo("started hgs job");
            contractItems = contractItems.DistinctBy(p => p.Id).ToList();
            //contractItems =  contractItems.Skip(2394).ToList();

            loggerHelper.traceInfo("count " + contractItems.Count);
            foreach (var item in contractItems)
            {
                List<HGSTransitData> hgsTransitDatas = new List<HGSTransitData>();
                var totalPayment = decimal.Zero;

                loggerHelper.traceInfo("contract item Id : " + item.Id);


                var contractId = item.GetAttributeValue<EntityReference>("rnt_contractid").Id;
                if (!item.Contains("rnt_equipment"))
                {
                    i++;
                    loggerHelper.traceInfo("equipment not contains data: " + item.Id);
                    continue;
                }
                ContractRepository contractRepository = new ContractRepository(crmServiceHelper.IOrganizationService);
                var contract = contractRepository.getContractById(contractId, new string[] { "statuscode", "rnt_ismonthly", "rnt_contractnumber" });
                if (contract.GetAttributeValue<OptionSetValue>("statuscode").Value == (int)rnt_contract_StatusCode.Rental &&
                   contract.GetAttributeValue<bool>("rnt_ismonthly") &&
                   string.IsNullOrEmpty(contractItemId))
                {
                    i++;
                    loggerHelper.traceInfo("aylık devam eden sözleşmerin hgs'i fatura uygulamasında kesilir: " + item.Id);
                    continue;
                }
                using (StreamReader r = new StreamReader(@"C:\Creatif\BatchApplications\RntCar.ProcessHGS\Resources\exclude.json"))
                {
                    string json = r.ReadToEnd();
                    details exclude_item = JsonConvert.DeserializeObject<details>(json);
                    loggerHelper.traceInfo("manuel çekim yapılan sözleşmeler kontrol ediliyor.");
                    loggerHelper.traceInfo("pnr number : " + contract.GetAttributeValue<string>("rnt_contractnumber"));
                    if (exclude_item.detailist.Where(p => p.contract == contract.GetAttributeValue<string>("rnt_contractnumber")).FirstOrDefault() != null)
                    {
                        i++;
                        loggerHelper.traceInfo(contract.GetAttributeValue<string>("rnt_contractnumber") + " nolu sözleşmenin hgs'i manuel işlenmiştir");
                        continue;
                    }

                }

                var equipmentId = item.GetAttributeValue<EntityReference>("rnt_equipment").Id;
                try
                {
                    EquipmentRepository equipmentRepository = new EquipmentRepository(crmServiceHelper.IOrganizationService);
                    var hgsNumber = equipmentRepository.getEquipmentHGSNumber(equipmentId);
                    var equipment = equipmentRepository.getEquipmentByIdByGivenColumns(equipmentId, new string[] { "rnt_platenumber", "rnt_hgslabelid" });
                    EntityReference hgsLabelRef = equipment.GetAttributeValue<EntityReference>("rnt_hgslabelid");
                    Guid hgsLabelId = hgsLabelRef != null && hgsLabelRef.Id != Guid.Empty ? hgsLabelRef.Id : Guid.Empty;

                    loggerHelper.traceInfo("hgsNumber" + hgsNumber);
                    if (!string.IsNullOrEmpty(hgsNumber))
                    {
                        #region GetHgsRecords from Integration

                        var duration = Convert.ToInt32(StaticHelper.GetConfiguration("extendDuration"));
                        loggerHelper.traceInfo("getting hgs list started");

                        loggerHelper.traceInfo("getting hgs list end");

                        #endregion

                    }
                    //now make the payment
                    if (totalPayment > decimal.Zero)
                    {
                        loggerHelper.traceInfo("totalPayment > 0");
                        PaymentRepository paymentRepository = new PaymentRepository(crmServiceHelper.IOrganizationService);
                        var payment = paymentRepository.getLastPayment_Contract(contractId);
                        EntityReference creditCardReference = null;
                        if (payment != null)
                        {
                            creditCardReference = payment.GetAttributeValue<EntityReference>("rnt_customercreditcardid");
                        }
                        else
                        {
                            payment = paymentRepository.getDeposit_Contract(contractId);
                            if (payment != null)
                            {
                                creditCardReference = payment.GetAttributeValue<EntityReference>("rnt_customercreditcardid");
                            }

                        }

                        var c = contractRepository.getContractById(contractId, new string[] { "rnt_paymentmethodcode" });

                        var passCreditCard = c.GetAttributeValue<OptionSetValue>("rnt_paymentmethodcode").Value == (int)rnt_PaymentMethodCode.FullCredit ||
                                             c.GetAttributeValue<OptionSetValue>("rnt_paymentmethodcode").Value == (int)rnt_PaymentMethodCode.Current;

                        ConfigurationRepository configurationRepository = new ConfigurationRepository(crmServiceHelper.IOrganizationService);
                        var hgsCode = configurationRepository.GetConfigurationByKey("additionalProduct_HGS");

                        AdditionalProductRepository additionalProductRepository = new AdditionalProductRepository(crmServiceHelper.IOrganizationService);
                        var hgsAdditionalProduct = additionalProductRepository.getAdditionalProductByProductCode(hgsCode);

                        InvoiceRepository invoiceRepository = new InvoiceRepository(crmServiceHelper.IOrganizationService);
                        //get default invoice , naming will be consider
                        var invoice = invoiceRepository.getFirstActiveInvoiceByContractId(contractId);
                        loggerHelper.traceInfo("manual payment parameters start");
                        loggerHelper.traceInfo("invoice : " + invoice.FirstOrDefault()?.Id);


                        var manualPaymentParameters = new ManualPaymentParameters
                        {
                            creditCardId = creditCardReference?.Id,
                            additionalProductId = hgsAdditionalProduct.Id,
                            amount = totalPayment,
                            contractId = contractId,
                            invoiceId = invoice.FirstOrDefault()?.Id,
                            manualPaymentType = (int)rnt_ManualPaymentTypeCode.AddAdditionalProductWithPayment,
                            isDebt = true,
                            langId = 1055,
                            channelCode = (int)rnt_ReservationChannel.Job
                        };
                        loggerHelper.traceInfo("manual payment parameters end");
                        loggerHelper.traceInfo("request start");
                        ManualPaymentBL manualPaymentBL = new ManualPaymentBL(this.orgService);
                        var manualPaymentResponse = manualPaymentBL.makeManualPayment(manualPaymentParameters);

                        // Send email

                        loggerHelper.traceInfo("sending mail start");
                        IndividualCustomerRepository individualCustomerRepository = new IndividualCustomerRepository(crmServiceHelper.IOrganizationService);
                        var customer = individualCustomerRepository.getIndividualCustomerById(item.GetAttributeValue<EntityReference>("rnt_customerid").Id);
                        var pickupDateTime = item.GetAttributeValue<DateTime>("rnt_pickupdatetime");

                        var config = new TemplateServiceConfiguration();
                        config.DisableTempFileLocking = true;
                        var service = RazorEngineService.Create(config);
                        string template = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, @"C:\Creatif\BatchApplications\RntCar.ProcessHGS\Resources\HGSMail.html"));
                        var result = service.RunCompile(template, "templateKey", null, new
                        {
                            ContactName = customer.GetAttributeValue<string>("fullname"),
                            TotalPayment = totalPayment,
                            PickupDateTime = pickupDateTime,
                            HGSTransits = hgsTransitDatas
                        });

                        Entity fromActivityParty = new Entity("activityparty");
                        Entity toActivityParty = new Entity("activityparty");

                        var contactId = item.GetAttributeValue<EntityReference>("rnt_customerid").Id;
                        fromActivityParty["partyid"] = new EntityReference("systemuser", new Guid(configurationRepository.GetConfigurationByKey("CrmAdminGuid")));
                        toActivityParty["partyid"] = new EntityReference("contact", contactId);

                        Entity email = new Entity("email");
                        email["from"] = new Entity[] { fromActivityParty };
                        email["to"] = new Entity[] { toActivityParty };
                        email["regardingobjectid"] = new EntityReference("rnt_contract", contractId);
                        email["subject"] = "HGS Ücret Tahsili";
                        email["description"] = result;
                        email["directioncode"] = true;
                        Guid emailId = crmServiceHelper.IOrganizationService.Create(email);

                        SendEmailRequest sendEmailRequest = new SendEmailRequest
                        {
                            EmailId = emailId,
                            TrackingToken = "",
                            IssueSend = true
                        };

                        SendEmailResponse sendEmailresp = (SendEmailResponse)crmServiceHelper.IOrganizationService.Execute(sendEmailRequest);
                        loggerHelper.traceInfo("sending mail end");



                        loggerHelper.traceInfo("hgs charged succesfully");
                        totalAmount += totalPayment;
                    }

                    loggerHelper.traceInfo("all operations finished in peace for this item: " + item.Id);
                    loggerHelper.traceInfo(StaticHelper.endLineStar);
                }
                catch (Exception ex)
                {
                    i++;
                    loggerHelper.traceInfo("error : " + ex.Message);
                    loggerHelper.traceInfo("error stack trace: " + ex.StackTrace);
                    continue;
                }
                loggerHelper.traceInfo("contract count : " + i);
                i++;
                Console.WriteLine(i);
                Console.WriteLine(totalAmount);

            }
            loggerHelper.traceInfo("end hgs job");

        }

        public void processHGSBatch(Guid contractId, LoggerHelper loggerHelper = null)
        {
            CrmServiceHelper crmServiceHelper = new CrmServiceHelper();
            if (loggerHelper == null)
                loggerHelper = new LoggerHelper();

            loggerHelper.traceInfo("retrieve started");

            try
            {
                var totalPayment = decimal.Zero;
                HGSTransitListRepository hGSTransitListRepository = new HGSTransitListRepository(crmServiceHelper.IOrganizationService);
                EntityCollection hgsTransitList = hGSTransitListRepository.getHGSTransitListNotInvoicedWithContractId(contractId);
                List<HGSTransitData> hgsTransitDatas = new List<HGSTransitData>();
                foreach (var hgsTransit in hgsTransitList.Entities)
                {
                    decimal amount = Math.Round(hgsTransit.GetAttributeValue<Money>("rnt_amount").Value, 2);
                    HGSTransitData hGSTransitData = new HGSTransitData()
                    {
                        entryDateTime = hgsTransit.GetAttributeValue<DateTime>("rnt_entrydatetime").converttoTimeStamp(),
                        entryLocation = hgsTransit.GetAttributeValue<string>("rnt_entrylocation"),
                        exitDateTime = hgsTransit.GetAttributeValue<DateTime>("rnt_exitdatetime").converttoTimeStamp(),
                        exitLocation = hgsTransit.GetAttributeValue<string>("rnt_exitlocation"),
                        amount = amount,
                        _entryDateTime = hgsTransit.GetAttributeValue<DateTime>("rnt_entrydatetime"),
                        _exitDateTime = hgsTransit.GetAttributeValue<DateTime>("rnt_exitdatetime")
                    };
                    hgsTransitDatas.Add(hGSTransitData);
                    totalPayment += amount;
                }
                //now make the payment
                if (totalPayment > decimal.Zero)
                {
                    ContractRepository contractRepository = new ContractRepository(crmServiceHelper.IOrganizationService);
                    var contract = contractRepository.getContractById(contractId, new string[] { "rnt_paymentmethodcode", "rnt_pickupdatetime", "rnt_customerid", "statuscode" });
                    var contractStatusCode = contract.GetAttributeValue<OptionSetValue>("statuscode").Value;
                    EntityReference customerRef = contract.GetAttributeValue<EntityReference>("rnt_customerid");
                    var pickupDateTime = contract.GetAttributeValue<DateTime>("rnt_pickupdatetime");

                    loggerHelper.traceInfo("totalPayment > 0");
                    PaymentRepository paymentRepository = new PaymentRepository(crmServiceHelper.IOrganizationService);
                    var payment = paymentRepository.getLastPayment_Contract(contractId);
                    EntityReference creditCardReference = null;
                    if (payment != null)
                    {
                        creditCardReference = payment.GetAttributeValue<EntityReference>("rnt_customercreditcardid");
                    }
                    else
                    {
                        payment = paymentRepository.getDeposit_Contract(contractId);
                        if (payment != null)
                        {
                            creditCardReference = payment.GetAttributeValue<EntityReference>("rnt_customercreditcardid");
                        }

                    }

                    var passCreditCard = contract.GetAttributeValue<OptionSetValue>("rnt_paymentmethodcode").Value == (int)rnt_PaymentMethodCode.FullCredit ||
                                         contract.GetAttributeValue<OptionSetValue>("rnt_paymentmethodcode").Value == (int)rnt_PaymentMethodCode.Current;
                    if (passCreditCard || creditCardReference == null)
                    {
                        return;
                    }

                    ConfigurationRepository configurationRepository = new ConfigurationRepository(crmServiceHelper.IOrganizationService);
                    var hgsCode = configurationRepository.GetConfigurationByKey("additionalProduct_HGS");

                    AdditionalProductRepository additionalProductRepository = new AdditionalProductRepository(crmServiceHelper.IOrganizationService);
                    var hgsAdditionalProduct = additionalProductRepository.getAdditionalProductByProductCode(hgsCode);

                    AdditionalProductHelper additionalProductHelper = new AdditionalProductHelper(crmServiceHelper.IOrganizationService);
                    ContractItemBL contractItemBL = new ContractItemBL(crmServiceHelper.IOrganizationService);

                    InvoiceRepository invoiceRepository = new InvoiceRepository(crmServiceHelper.IOrganizationService);
                    //get default invoice , naming will be consider
                    var invoice = invoiceRepository.getFirstActiveInvoiceByContractId(contractId);
                    loggerHelper.traceInfo("manual payment parameters start");
                    loggerHelper.traceInfo("invoice : " + invoice.FirstOrDefault()?.Id);

                    var manualPaymentParameters = new ManualPaymentParameters
                    {
                        creditCardId = creditCardReference?.Id,
                        additionalProductId = hgsAdditionalProduct.Id,
                        amount = totalPayment,
                        contractId = contractId,
                        invoiceId = invoice.FirstOrDefault()?.Id,
                        manualPaymentType = (int)rnt_ManualPaymentTypeCode.AddAdditionalProductWithPayment,
                        isDebt = true,
                        langId = 1055,
                        channelCode = (int)rnt_ReservationChannel.Job
                    };
                    loggerHelper.traceInfo("manual payment parameters end");
                    loggerHelper.traceInfo("request start");
                    ManualPaymentBL manualPaymentBL = new ManualPaymentBL(this.orgService);
                    var manualPaymentResponse = manualPaymentBL.makeManualPayment(manualPaymentParameters);

                    // Send email

                    loggerHelper.traceInfo("sending mail start");
                    IndividualCustomerRepository individualCustomerRepository = new IndividualCustomerRepository(crmServiceHelper.IOrganizationService);
                    var customer = individualCustomerRepository.getIndividualCustomerById(customerRef.Id);

                    var config = new TemplateServiceConfiguration();
                    config.DisableTempFileLocking = true;
                    var service = RazorEngineService.Create(config);
                    string template = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, @"C:\Creatif\BatchApplications\RntCar.ProcessHGS\Resources\HGSMail.html"));
                    var result = service.RunCompile(template, "templateKey", null, new
                    {
                        ContactName = customer.GetAttributeValue<string>("fullname"),
                        TotalPayment = totalPayment,
                        PickupDateTime = pickupDateTime,
                        HGSTransits = hgsTransitDatas
                    });

                    Entity fromActivityParty = new Entity("activityparty");
                    Entity toActivityParty = new Entity("activityparty");

                    fromActivityParty["partyid"] = new EntityReference("systemuser", new Guid(configurationRepository.GetConfigurationByKey("CrmAdminGuid")));
                    toActivityParty["partyid"] = new EntityReference(customerRef.LogicalName, customerRef.Id);

                    Entity email = new Entity("email");
                    email["from"] = new Entity[] { fromActivityParty };
                    email["to"] = new Entity[] { toActivityParty };
                    email["regardingobjectid"] = new EntityReference("rnt_contract", contractId);
                    email["subject"] = "HGS Ücret Tahsili";
                    email["description"] = result;
                    email["directioncode"] = true;
                    Guid emailId = crmServiceHelper.IOrganizationService.Create(email);

                    SendEmailRequest sendEmailRequest = new SendEmailRequest
                    {
                        EmailId = emailId,
                        TrackingToken = "",
                        IssueSend = true
                    };

                    SendEmailResponse sendEmailresp = (SendEmailResponse)crmServiceHelper.IOrganizationService.Execute(sendEmailRequest);
                    loggerHelper.traceInfo("sending mail end");



                    loggerHelper.traceInfo("hgs charged succesfully");
                }
            }
            catch (Exception ex)
            {
                loggerHelper.traceInfo("error : " + ex.Message);
                loggerHelper.traceInfo("error stack trace: " + ex.StackTrace);
            }

            loggerHelper.traceInfo("started hgs job");
        }
        public void processHGSBatch(Guid contractItemId)
        {
            CrmServiceHelper crmServiceHelper = new CrmServiceHelper();
            LoggerHelper loggerHelper = new LoggerHelper();

            ContractItemRepository contractItemRepository = new ContractItemRepository(crmServiceHelper.IOrganizationService);
            var item = contractItemRepository.getContractItemId(contractItemId);


            var i = 1;

            decimal totalAmount = 0;
            loggerHelper.traceInfo("started hgs job");

            List<Guid> hgsItems = new List<Guid>();
            List<HGSTransitData> hgsTransitDatas = new List<HGSTransitData>();
            var totalPayment = decimal.Zero;

            loggerHelper.traceInfo("contract item Id : " + item.Id);
            loggerHelper.traceInfo("pnr number : " + item.GetAttributeValue<string>("rnt_pnrnumber"));

            var contractId = item.GetAttributeValue<EntityReference>("rnt_contractid").Id;
            if (!item.Contains("rnt_equipment"))
            {
                loggerHelper.traceInfo("equipment not contains data: " + item.Id);
            }
            var equipmentId = item.GetAttributeValue<EntityReference>("rnt_equipment").Id;
            try
            {
                EquipmentRepository equipmentRepository = new EquipmentRepository(crmServiceHelper.IOrganizationService);
                var hgsNumber = equipmentRepository.getEquipmentHGSNumber(equipmentId);
                var equipment = equipmentRepository.getEquipmentByIdByGivenColumns(equipmentId, new string[] { "rnt_platenumber", "rnt_hgslabelid" });
                EntityReference hgsLabelRef = equipment.GetAttributeValue<EntityReference>("rnt_hgslabelid");
                Guid hgsLabelId = hgsLabelRef != null && hgsLabelRef.Id != Guid.Empty ? hgsLabelRef.Id : Guid.Empty;

                loggerHelper.traceInfo("hgsNumber" + hgsNumber);
                if (!string.IsNullOrEmpty(hgsNumber))
                {
                    #region GetHgsRecords from Integration

                    var duration = Convert.ToInt32(StaticHelper.GetConfiguration("extendDuration"));
                    loggerHelper.traceInfo("getting hgs list started");
                    GetHGSTransitListResponse getHGSTransitListResponse = new GetHGSTransitListResponse();
                    using (var hgsHelper = new HGSHelper(crmServiceHelper.IOrganizationService))
                    {
                        getHGSTransitListResponse = hgsHelper.getHgsTransitList(new GetHGSTransitListParameter
                        {
                            productId = hgsNumber,
                            finishDateTimeStamp = item.GetAttributeValue<DateTime>("rnt_dropoffdatetime").AddMinutes(-StaticHelper.offset).AddDays(duration).converttoTimeStamp(),
                            startDateTimeStamp = item.GetAttributeValue<DateTime>("rnt_pickupdatetime").AddMinutes(-StaticHelper.offset).AddDays(-duration).converttoTimeStamp()
                        });
                    }
                    loggerHelper.traceInfo("getting hgs list end");

                    #endregion

                    if (getHGSTransitListResponse.transits != null)
                    {
                        //getHGSTransitListResponse.transits = getHGSTransitListResponse.transits.Where(p => p.entryDateTime.isBetween(item.GetAttributeValue<DateTime>("rnt_pickupdatetime").converttoTimeStamp(), item.GetAttributeValue<DateTime>("rnt_dropoffdatetime").converttoTimeStamp())).ToList();

                        getHGSTransitListResponse.transits = getHGSTransitListResponse.transits.Where(p => p.exitDateTime.isBetween(item.GetAttributeValue<DateTime>("rnt_pickupdatetime").converttoTimeStamp(), item.GetAttributeValue<DateTime>("rnt_dropoffdatetime").converttoTimeStamp())).ToList();

                        #region Iterate new HGS list if it is missing in CRM create it
                        loggerHelper.traceInfo("getHGSTransitListResponse.transits?.Count" + getHGSTransitListResponse.transits?.Count);
                        if (getHGSTransitListResponse.transits?.Count > 0)
                        {
                            // Tüm transitlere aracın plaka bilgisini koy
                            getHGSTransitListResponse.transits.ForEach(t => t.plateNumber = equipment.GetAttributeValue<string>("rnt_platenumber"));
                            //getting existing hgstransitlists
                            HGSTransitListRepository hGSTransitListRepository = new HGSTransitListRepository(crmServiceHelper.IOrganizationService);
                            var hgsTransitList = hGSTransitListRepository.getHGSTransitListByContractId(contractId).Entities;
                            //count == 0 kontrolü normalde olmaz.
                            //Tabletten her zaman veri gelmesini bekliyoruz(eğer hgs'si varsa)
                            //tabletten kapatırken entegrasyon calısmadı yada baska bir sebepten dolayı datayı alamazsa batch uygulamanın eklemesi için == 0 kontrolü gerekiyor
                            if (hgsTransitList.Count == 0)
                            {
                                hgsTransitDatas = getHGSTransitListResponse.transits;

                                foreach (var hgsTransitItem in getHGSTransitListResponse.transits)
                                {
                                    try
                                    {
                                        loggerHelper.traceInfo("HGSTransitList CRM create start");
                                        HGSTransitListBL hGSTransitListBL = new HGSTransitListBL(crmServiceHelper.IOrganizationService);
                                        var id = hGSTransitListBL.createHGSList(hgsTransitItem, contractId, item.Id, equipmentId, hgsLabelId);
                                        hgsItems.Add(id);
                                        loggerHelper.traceInfo("HGSTransitList CRM create end");

                                        totalPayment += hgsTransitItem.amount;

                                        loggerHelper.traceInfo("totalPayment is now" + totalPayment);
                                    }
                                    catch (Exception ex)
                                    {
                                        loggerHelper.traceInfo("HGSTransitList CRM create error : " + ex.Message);
                                        continue;
                                    }

                                }

                            }
                            else
                            {
                                List<HGSTransitData> crmTransitData = new List<HGSTransitData>();
                                foreach (var hgsTransitItem in hgsTransitList)
                                {
                                    loggerHelper.traceInfo("HGSTransitList CRM Item : " + hgsTransitItem.Id);

                                    var _entrtyDate = hgsTransitItem.GetAttributeValue<DateTime>("rnt_entrydatetime").converttoTimeStamp();
                                    var _exitDate = hgsTransitItem.GetAttributeValue<DateTime>("rnt_exitdatetime").converttoTimeStamp();
                                    var _entryLocation = hgsTransitItem.GetAttributeValue<string>("rnt_entrylocation");
                                    var _exitLocation = hgsTransitItem.GetAttributeValue<string>("rnt_exitlocation");
                                    var _amount = hgsTransitItem.GetAttributeValue<Money>("rnt_amount").Value;
                                    crmTransitData.Add(new HGSTransitData
                                    {
                                        entryDateTime = _entrtyDate,
                                        entryLocation = _entryLocation,
                                        exitDateTime = _exitDate,
                                        exitLocation = _exitLocation,
                                        amount = _amount,
                                        _entryDateTime = hgsTransitItem.GetAttributeValue<DateTime>("rnt_entrydatetime"),
                                        _exitDateTime = hgsTransitItem.GetAttributeValue<DateTime>("rnt_exitdatetime")
                                    });
                                }
                                //getHGSTransitListResponse.transits.ForEach(p => p.entryDateTime = p.entryDateTime.converttoDateTime().AddSeconds(-p.entryDateTime.converttoDateTime().Second).converttoTimeStamp());
                                //getHGSTransitListResponse.transits.ForEach(p => p.exitDateTime = p.exitDateTime.converttoDateTime().AddSeconds(-p.exitDateTime.converttoDateTime().Second).converttoTimeStamp());
                                foreach (var x in getHGSTransitListResponse.transits)
                                {
                                    x.exitDateTime = x.exitDateTime.converttoDateTime().AddSeconds(x.exitDateTime.converttoDateTime().Second * -1).converttoTimeStamp();
                                    x.entryDateTime = x.entryDateTime.converttoDateTime().AddSeconds(x.entryDateTime.converttoDateTime().Second * -1).converttoTimeStamp();
                                    x._exitDateTime = x.exitDateTime.converttoDateTime();
                                    x._entryDateTime = x.entryDateTime.converttoDateTime();
                                }
                                foreach (var x in crmTransitData)
                                {
                                    x.exitDateTime = x.exitDateTime.converttoDateTime().AddSeconds(x.exitDateTime.converttoDateTime().Second * -1).converttoTimeStamp();
                                    x.entryDateTime = x.entryDateTime.converttoDateTime().AddSeconds(x.entryDateTime.converttoDateTime().Second * -1).converttoTimeStamp();
                                    x._exitDateTime = x.exitDateTime.converttoDateTime();
                                    x._entryDateTime = x.entryDateTime.converttoDateTime();
                                }
                                var newHGSgetHGSTransitList = getHGSTransitListResponse
                                                         .transits.Except(crmTransitData, new HGSTransitDataComparer()).ToList();

                                hgsTransitDatas = newHGSgetHGSTransitList;

                                foreach (var newHGS in newHGSgetHGSTransitList)
                                {
                                    try
                                    {
                                        loggerHelper.traceInfo("HGSTransitList CRM create start");
                                        HGSTransitListBL hGSTransitListBL = new HGSTransitListBL(crmServiceHelper.IOrganizationService);
                                        var id = hGSTransitListBL.createHGSList(newHGS, contractId, item.Id, equipmentId, hgsLabelId);
                                        hgsItems.Add(id);

                                        loggerHelper.traceInfo("HGSTransitList CRM create end");

                                        totalPayment += newHGS.amount;

                                        loggerHelper.traceInfo("totalPayment is now" + totalPayment);
                                    }
                                    catch (Exception ex)
                                    {
                                        loggerHelper.traceInfo("HGSTransitList CRM create error : " + ex.Message);
                                    }
                                    loggerHelper.traceInfo("totalPayment is now" + totalPayment);
                                }
                            }
                        }

                        #endregion
                    }
                }
                //now make the payment
                if (totalPayment > decimal.Zero)
                {
                    loggerHelper.traceInfo("totalPayment > 0");
                    PaymentRepository paymentRepository = new PaymentRepository(crmServiceHelper.IOrganizationService);
                    var payment = paymentRepository.getLastPayment_Contract(contractId);
                    EntityReference creditCardReference = null;
                    if (payment != null)
                    {
                        creditCardReference = payment.GetAttributeValue<EntityReference>("rnt_customercreditcardid");
                    }
                    else
                    {
                        payment = paymentRepository.getDeposit_Contract(contractId);
                        if (payment != null)
                        {
                            creditCardReference = payment.GetAttributeValue<EntityReference>("rnt_customercreditcardid");
                        }

                    }
                    ContractRepository contractRepository = new ContractRepository(crmServiceHelper.IOrganizationService);
                    var c = contractRepository.getContractById(contractId, new string[] { "rnt_paymentmethodcode" });

                    var passCreditCard = c.GetAttributeValue<OptionSetValue>("rnt_paymentmethodcode").Value == (int)rnt_PaymentMethodCode.FullCredit ||
                                         c.GetAttributeValue<OptionSetValue>("rnt_paymentmethodcode").Value == (int)rnt_PaymentMethodCode.Current;
                    if (!passCreditCard && creditCardReference == null)
                    {
                        foreach (var a in hgsItems)
                        {
                            try
                            {
                                crmServiceHelper.IOrganizationService.Delete("rnt_hgstransitlist", a);
                            }
                            catch
                            {
                            }

                        }
                    }

                    ConfigurationRepository configurationRepository = new ConfigurationRepository(crmServiceHelper.IOrganizationService);
                    var hgsCode = configurationRepository.GetConfigurationByKey("additionalProduct_HGS");

                    AdditionalProductRepository additionalProductRepository = new AdditionalProductRepository(crmServiceHelper.IOrganizationService);
                    var hgsAdditionalProduct = additionalProductRepository.getAdditionalProductByProductCode(hgsCode);

                    InvoiceRepository invoiceRepository = new InvoiceRepository(crmServiceHelper.IOrganizationService);
                    //get default invoice , naming will be consider
                    var invoice = invoiceRepository.getFirstActiveInvoiceByContractId(contractId);
                    loggerHelper.traceInfo("manual payment parameters start");
                    loggerHelper.traceInfo("invoice : " + invoice.FirstOrDefault()?.Id);
                    var manualPaymentParameters = new ManualPaymentParameters
                    {
                        creditCardId = creditCardReference?.Id,
                        additionalProductId = hgsAdditionalProduct.Id,
                        amount = totalPayment,
                        contractId = contractId,
                        invoiceId = invoice.FirstOrDefault()?.Id,
                        manualPaymentType = (int)rnt_ManualPaymentTypeCode.AddAdditionalProductWithPayment,
                        isDebt = true,
                        langId = 1055,
                        channelCode = (int)rnt_ReservationChannel.Job
                    };
                    loggerHelper.traceInfo("manual payment parameters end");
                    loggerHelper.traceInfo("request start");
                    ManualPaymentBL manualPaymentBL = new ManualPaymentBL(this.orgService);
                    var manualPaymentResponse = manualPaymentBL.makeManualPayment(manualPaymentParameters);

                    // Send email
                    if (manualPaymentResponse.ResponseResult.Result)
                    {
                        loggerHelper.traceInfo("sending mail start");
                        IndividualCustomerRepository individualCustomerRepository = new IndividualCustomerRepository(crmServiceHelper.IOrganizationService);
                        var customer = individualCustomerRepository.getIndividualCustomerById(item.GetAttributeValue<EntityReference>("rnt_customerid").Id);
                        var pickupDateTime = item.GetAttributeValue<DateTime>("rnt_pickupdatetime");

                        var config = new TemplateServiceConfiguration();
                        config.DisableTempFileLocking = true;
                        var service = RazorEngineService.Create(config);
                        string template = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, @"C:\Creatif\BatchApplications\RntCar.ProcessHGS\Resources\HGSMail.html"));
                        var result = service.RunCompile(template, "templateKey", null, new
                        {
                            ContactName = customer.GetAttributeValue<string>("fullname"),
                            TotalPayment = totalPayment,
                            PickupDateTime = pickupDateTime,
                            HGSTransits = hgsTransitDatas
                        });

                        Entity fromActivityParty = new Entity("activityparty");
                        Entity toActivityParty = new Entity("activityparty");

                        var contactId = item.GetAttributeValue<EntityReference>("rnt_customerid").Id;
                        fromActivityParty["partyid"] = new EntityReference("systemuser", new Guid(configurationRepository.GetConfigurationByKey("CrmAdminGuid")));
                        toActivityParty["partyid"] = new EntityReference("contact", contactId);

                        Entity email = new Entity("email");
                        email["from"] = new Entity[] { fromActivityParty };
                        email["to"] = new Entity[] { toActivityParty };
                        email["regardingobjectid"] = new EntityReference("rnt_contract", contractId);
                        email["subject"] = "HGS Ücret Tahsili";
                        email["description"] = result;
                        email["directioncode"] = true;
                        Guid emailId = crmServiceHelper.IOrganizationService.Create(email);

                        SendEmailRequest sendEmailRequest = new SendEmailRequest
                        {
                            EmailId = emailId,
                            TrackingToken = "",
                            IssueSend = true
                        };

                        SendEmailResponse sendEmailresp = (SendEmailResponse)crmServiceHelper.IOrganizationService.Execute(sendEmailRequest);
                        loggerHelper.traceInfo("sending mail end");

                    }

                    loggerHelper.traceInfo("hgs charged succesfully");
                    totalAmount += totalPayment;
                }

                loggerHelper.traceInfo("all operations finished in peace for this item: " + item.Id);
                loggerHelper.traceInfo(StaticHelper.endLineStar);
            }
            catch (Exception ex)
            {
                loggerHelper.traceInfo("error : " + ex.Message);
                loggerHelper.traceInfo("error stack trace: " + ex.StackTrace);
            }
            i++;
            Console.WriteLine(i);
            Console.WriteLine(totalAmount);


            loggerHelper.traceInfo("end hgs job");

        }

        public void MakePaymentHGS()
        {
            LoggerHelper loggerHelper = new LoggerHelper();
            AdditionalProductHelper additionalProductHelper = new AdditionalProductHelper(this.orgService);

            HGSTransitListRepository hGSTransitListRepository = new HGSTransitListRepository(this.orgService);
            ConfigurationRepository configurationRepository = new ConfigurationRepository(this.orgService);
            AdditionalProductRepository additionalProductRepository = new AdditionalProductRepository(this.orgService);
            ContractRepository contractRepository = new ContractRepository(this.orgService);
            IndividualCustomerRepository individualCustomerRepository = new IndividualCustomerRepository(this.orgService);

            ContractItemBL contractItemBL = new ContractItemBL(this.orgService);
            ContractItemRepository contractItemRepository = new ContractItemRepository(this.orgService);

            var hgsTransitList = hGSTransitListRepository.getHGSTransitListNotPaymentWithFtpConsensus();
            var distinctContractRefList = hgsTransitList.Entities.Select(x => x.GetAttributeValue<EntityReference>("rnt_contractid")).Distinct();
            var hgsCode = configurationRepository.GetConfigurationByKey("additionalProduct_HGS");

            var hgsAdditionalProduct = additionalProductRepository.getAdditionalProductByProductCode(hgsCode);


            int i = distinctContractRefList.Count();
            List<Guid> invoiceIdList = new List<Guid>();
            foreach (var contractRef in distinctContractRefList)
            {
                Console.WriteLine(i--);
                try
                {
                    #region Field Map

                    bool sendEmail = false;
                    int billingType = (int)rnt_BillingTypeCode.Individual;
                    Guid corporateId = Guid.Empty;

                    var contract = contractRepository.getContractById(contractRef.Id);
                    EntityReference equipmentRef = contract.GetAttributeValue<EntityReference>("rnt_equipmentid");
                    EntityReference pickupBranchRef = contract.GetAttributeValue<EntityReference>("rnt_pickupbranchid");
                    EntityReference dropoffBranchRef = contract.GetAttributeValue<EntityReference>("rnt_dropoffbranchid");
                    DateTime pickupDate = contract.GetAttributeValue<DateTime>("rnt_pickupdatetime");
                    DateTime dropoffDate = contract.GetAttributeValue<DateTime>("rnt_dropoffdatetime");
                    EntityReference customerRef = contract.GetAttributeValue<EntityReference>("rnt_customerid");
                    EntityReference groupCodeInformationRef = contract.GetAttributeValue<EntityReference>("rnt_pricinggroupcodeid");
                    EntityReference currencyRef = contract.GetAttributeValue<EntityReference>("transactioncurrencyid");
                    int paymentMethodType = contract.GetAttributeValue<OptionSetValue>("rnt_paymentmethodcode").Value;
                    int contractStatusCode = contract.GetAttributeValue<OptionSetValue>("statuscode").Value;
                    int contractItemStatusCode = ContractMapper.getContractItemStatusCodeByContractStatusCode(contractStatusCode);
                    if (paymentMethodType == (int)rnt_PaymentMethodCode.FullCredit || paymentMethodType == (int)rnt_PaymentMethodCode.Current)
                    {
                        billingType = (int)rnt_BillingTypeCode.Corporate;
                        corporateId = contract.GetAttributeValue<EntityReference>("rnt_corporateid") != null ? contract.GetAttributeValue<EntityReference>("rnt_corporateid").Id : Guid.Empty;
                    }

                    ContractDateandBranchParameters contractDateandBranchParameter = new ContractDateandBranchParameters()
                    {
                        pickupBranchId = pickupBranchRef.Id,
                        dropoffBranchId = dropoffBranchRef.Id,
                        pickupDate = pickupDate,
                        dropoffDate = dropoffDate
                    };
                    #endregion

                    #region HGS Transit List
                    var processHGSTransactionList = hgsTransitList.Entities.Where(x => x.GetAttributeValue<EntityReference>("rnt_contractid").Id == contractRef.Id).ToList();

                    decimal totalHGSPayment = 0;
                    var hgsTransitDatus = new List<HGSTransitData>();
                    foreach (var processHGSTransaction in processHGSTransactionList)
                    {
                        totalHGSPayment = totalHGSPayment + (processHGSTransaction.GetAttributeValue<Money>("rnt_amount")).Value;
                        var transitData = new HGSTransitData
                        {
                            entryDateTime = processHGSTransaction.GetAttributeValue<DateTime>("rnt_entrydatetime").converttoTimeStamp(),
                            entryLocation = processHGSTransaction.GetAttributeValue<string>("rnt_entrylocation"),
                            exitDateTime = processHGSTransaction.GetAttributeValue<DateTime>("rnt_exitdatetime").converttoTimeStamp(),
                            exitLocation = processHGSTransaction.GetAttributeValue<string>("rnt_exitlocation"),
                            amount = Math.Round(processHGSTransaction.GetAttributeValue<Money>("rnt_amount").Value, 2),
                            _entryDateTime = processHGSTransaction.GetAttributeValue<DateTime>("rnt_entrydatetime"),
                            _exitDateTime = processHGSTransaction.GetAttributeValue<DateTime>("rnt_exitdatetime")
                        };
                        hgsTransitDatus.Add(transitData);
                    }

                    #endregion

                    #region HGS Payment Control

                    decimal contractHGSProductAmount = 0;
                    var hgsAdditionalProductContractItemList = contractItemBL.getContractItemsByAdditionalProduct(hgsAdditionalProduct.Id, contractRef.Id);
                    foreach (var hgsAdditionalProductContractItem in hgsAdditionalProductContractItemList.Entities)
                    {
                        Money totalAmount = hgsAdditionalProductContractItem.GetAttributeValue<Money>("rnt_totalamount");
                        contractHGSProductAmount = contractHGSProductAmount + totalAmount.Value;
                    }

                    EntityCollection hgsAllProcessTransitList = hGSTransitListRepository.getHGSTransitListByContractId(contractRef.Id);

                    decimal hgsAllTotalPayment = 0;
                    foreach (var hgsAllProcessTransit in hgsAllProcessTransitList.Entities)
                    {
                        hgsAllTotalPayment = hgsAllTotalPayment + hgsAllProcessTransit.GetAttributeValue<Money>("rnt_amount").Value;
                    }
                    decimal checkHGSAmount = hgsAllTotalPayment - contractHGSProductAmount;
                    if (checkHGSAmount == 0 && hgsAllTotalPayment == 0 && contractHGSProductAmount == 0)
                    {
                        Entity equipmentContractItem = new Entity();
                        if (contractStatusCode == (int)rnt_contract_StatusCode.Rental)
                        {
                            equipmentContractItem = contractItemRepository.getRentalEquipment(equipmentRef.Id);
                        }
                        else
                        {
                            equipmentContractItem = contractItemRepository.getCompletedEquipmentContractItemByContractandEquipmentId(contractRef.Id, equipmentRef.Id, new string[] { });
                        }

                        foreach (var updateTransaction in processHGSTransactionList)
                        {
                            updateTransaction.Attributes["rnt_contractitemid"] = new EntityReference(equipmentContractItem.LogicalName, equipmentContractItem.Id);
                            this.orgService.Update(updateTransaction);
                        }
                    }
                    else if (checkHGSAmount <= 0)
                    {
                        totalHGSPayment = checkHGSAmount;
                        Entity tempHgs = hgsAdditionalProductContractItemList.Entities[0];
                        foreach (var updateTransaction in processHGSTransactionList)
                        {
                            updateTransaction.Attributes["rnt_contractitemid"] = new EntityReference(tempHgs.LogicalName, tempHgs.Id);
                            this.orgService.Update(updateTransaction);
                        }
                    }
                    else if (checkHGSAmount < totalHGSPayment)
                    {
                        totalHGSPayment = checkHGSAmount;
                    }
                    #endregion 

                    bool isMonthly = contract.GetAttributeValue<bool>("rnt_ismonthly");

                    List<Guid> contracItemsId = new List<Guid>();

                    #region Kredi Kartı İşlemleri
                    ContractAdditionalProductParameters contractItemAdditionalProductParameter = new ContractAdditionalProductParameters()
                    {
                        productId = hgsAdditionalProduct.Id,
                        productName = hgsAdditionalProduct.GetAttributeValue<string>("rnt_name"),
                        billingType = billingType,
                        actualAmount = totalHGSPayment
                    };
                    CreditCardBL creditCardBL = new CreditCardBL(this.orgService);
                    CreditCardData creditCard = new CreditCardData();

                    if (billingType == (int)rnt_BillingTypeCode.Individual)
                    {
                        var creditCardList = creditCardBL.getCustomerCreditCards(Convert.ToString(customerRef.Id), Guid.Empty, contract.Id, Guid.Empty);
                        if (string.IsNullOrWhiteSpace(creditCardList.selectedCreditCardId))
                        {
                            creditCardList = creditCardBL.getCustomerCreditCards(Convert.ToString(customerRef.Id), Guid.Empty, Guid.Empty, Guid.Empty);
                        }
                        if (creditCardList.creditCards.Count > 0)
                        {
                            if (string.IsNullOrWhiteSpace(creditCardList.selectedCreditCardId))
                            {
                                creditCard = creditCardList.creditCards[0];
                            }
                            else
                            {
                                creditCard = creditCardList.creditCards.Where(x => x.creditCardId.Value == new Guid(creditCardList.selectedCreditCardId)).FirstOrDefault();
                            }
                        }

                    }
                    #endregion


                    #region Contract Relationship

                    if (totalHGSPayment > 0)
                    {
                        Guid contractItemId = contractItemBL.createContractItemForAdditionalProduct(contractDateandBranchParameter,
                                                                                                               contractItemAdditionalProductParameter,
                                                                                                               customerRef.Id,
                                                                                                               groupCodeInformationRef.Id,
                                                                                                               contractRef.Id,
                                                                                                               currencyRef.Id,
                                                                                                               Guid.Empty,
                                                                                                               1,
                                                                                                               contractItemStatusCode,
                                                                                                               string.Empty,
                                                                                                               (int)rnt_ReservationChannel.Job,
                                                                                                               Guid.Empty,
                                                                                                               corporateId,
                                                                                                               (int)rnt_contractitem_rnt_itemtypecode.Fine, false);

                        foreach (var processHGSTransaction in processHGSTransactionList)
                        {
                            Entity hgsTransaction = new Entity(processHGSTransaction.LogicalName, processHGSTransaction.Id);
                            hgsTransaction["rnt_contractitemid"] = new EntityReference("rnt_contractitem", contractItemId);
                            this.orgService.Update(hgsTransaction);
                        }

                        contracItemsId.Add(contractItemId);
                        sendEmail = true;
                    }
                    #endregion

                    if (isMonthly)
                    {
                        //Aylık Sözleşme Kiralık Durumda Değilse HGS Kesilir. 
                        if (contractStatusCode == (int)rnt_contract_StatusCode.Rental)
                        {
                            continue;
                        }
                    }


                    #region Contract Relationship For Service Amount
                    decimal totalPayment = totalHGSPayment;
                    if (contractStatusCode == (int)rnt_contract_StatusCode.Completed)
                    {
                        var serviceContractItem = additionalProductHelper.getAdditionalProductService_Contract(hgsAdditionalProduct.Id, contract.Id);
                        if (serviceContractItem.subProduct != null && serviceContractItem.serviceItem == null && hgsAllTotalPayment != 0)
                        {
                            var subAmount = additionalProductHelper.calculateFineProductServicePrice(Guid.Empty, hgsAdditionalProduct.Id, hgsAllTotalPayment, serviceContractItem.subProduct.GetAttributeValue<Money>("rnt_price").Value);
                            Guid contractSubItemId = contractItemBL.CreateContractItem(contract.Id,
                                                                                 subAmount,
                                                                                 serviceContractItem.subProduct.Id,
                                                                                 (int)rnt_ReservationChannel.Job, (int)ClassLibrary._Enums_1033.rnt_contractitem_rnt_itemtypecode.Fine, new Guid(), corporateId, false);

                            totalPayment = totalPayment + subAmount;
                            contracItemsId.Add(contractSubItemId);
                        }
                    }

                    #endregion

                    loggerHelper.traceInfo("manual payment parameters start");

                    #region Invoice And Payment
                    if (contracItemsId.Count > 0)
                    {
                        InvoiceRepository invoiceRepository = new InvoiceRepository(this.orgService);

                        InvoiceAddressData invoiceAddressData = GetInvoiceAddress(contract, billingType);
                        if (invoiceAddressData != null && invoiceAddressData.invoiceAddressId.HasValue && invoiceAddressData.invoiceAddressId.Value != Guid.Empty)
                        {
                            if (creditCard != null && creditCard.creditCardId != Guid.Empty && billingType == (int)rnt_BillingTypeCode.Individual)
                            {
                                //Kart Bilgileri varsa ödeme alınır. Yoksa sadece fatura kesilir.
                                MakePayment(contract, creditCard, invoiceAddressData, totalPayment);
                            }
                            else
                            {
                                sendEmail = false;
                            }

                            if (sendEmail)
                            {
                                #region Send Email
                                loggerHelper.traceInfo("sending mail start");
                                var customer = individualCustomerRepository.getIndividualCustomerById(contract.GetAttributeValue<EntityReference>("rnt_customerid").Id);
                                var pickupDateTime = contract.GetAttributeValue<DateTime>("rnt_pickupdatetime");

                                var config = new TemplateServiceConfiguration();
                                config.DisableTempFileLocking = true;
                                var service = RazorEngineService.Create(config);
                                string template = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, @"C:\Creatif\BatchApplications\RntCar.HGSFTPIntegration\Resources\HGSMail.html"));
                                var result = service.RunCompile(template, "templateKey", null, new
                                {
                                    ContactName = customer.GetAttributeValue<string>("fullname"),
                                    TotalPayment = Math.Round(totalHGSPayment, 2),
                                    PickupDateTime = pickupDateTime,
                                    HGSTransits = hgsTransitDatus
                                });

                                Entity fromActivityParty = new Entity("activityparty");
                                Entity toActivityParty = new Entity("activityparty");

                                var contactId = contract.GetAttributeValue<EntityReference>("rnt_customerid").Id;
                                fromActivityParty["partyid"] = new EntityReference("systemuser", new Guid(configurationRepository.GetConfigurationByKey("CrmAdminGuid")));
                                toActivityParty["partyid"] = new EntityReference("contact", contactId);

                                Entity email = new Entity("email");
                                email["from"] = new Entity[] { fromActivityParty };
                                email["to"] = new Entity[] { toActivityParty };
                                email["regardingobjectid"] = new EntityReference("rnt_contract", contract.Id);
                                email["subject"] = "HGS Ücret Tahsili";
                                email["description"] = result;
                                email["directioncode"] = true;
                                Guid emailId = this.orgService.Create(email);

                                SendEmailRequest sendEmailRequest = new SendEmailRequest
                                {
                                    EmailId = emailId,
                                    TrackingToken = "",
                                    IssueSend = true
                                };

                                SendEmailResponse sendEmailresp = (SendEmailResponse)this.orgService.Execute(sendEmailRequest);
                                #endregion
                            }

                            loggerHelper.traceInfo("sending mail end");
                            loggerHelper.traceInfo("hgs charged succesfully");
                        }
                    }

                    #endregion

                }
                catch (Exception ex)
                {
                }
            }



        }

        public void CreateInvoiceForHGS()
        {
            HGSTransitListRepository hGSTransitListRepository = new HGSTransitListRepository(this.orgService);
            ContractRepository contractRepository = new ContractRepository(this.orgService);
            var hgsTransitList = hGSTransitListRepository.getHGSTransitListNotInvoicedWithFtpConsensus();
            var distinctContractRefList = hgsTransitList.Entities.Select(x => x.GetAttributeValue<EntityReference>("rnt_contractid")).Distinct();
            int i = distinctContractRefList.Count();

            List<Guid> invoiceIdList = new List<Guid>();

            foreach (var contractRef in distinctContractRefList)
            {
                try
                {
                    Guid invoiceId = CheckAndCreateNewInvoice(contractRepository, contractRef);
                    if (invoiceId != Guid.Empty)
                    {
                        //Workflow ile oluşturulan draft statüdeki faturaya, kalem eklenir ve sözleşme kalemi ile ilişki kurulur.
                        ExecuteWorkflowRequest contractInvoice = new ExecuteWorkflowRequest()
                        {
                            WorkflowId = Guid.Parse("AC45A087-6A39-41C3-B794-0E60537A164C"),
                            EntityId = contractRef.Id
                        };
                        this.orgService.Execute(contractInvoice);
                        //Workflow asekron olduğu için, değerler listede toplanır. Sonrasında tekrar çalıştırılacaktır.
                        invoiceIdList.Add(invoiceId);
                        Console.WriteLine(i--);
                    }
                }
                catch (Exception ex)
                {
                    if (ex.Message.Contains("timeout"))
                    {
                        CrmServiceHelper crmServiceHelper = new CrmServiceHelper();
                        this.orgService = crmServiceHelper.IOrganizationService;
                    }
                }
            }

            //foreach (var invoiceId in invoiceIdList)
            //{

            //    ExecuteWorkflowRequest executeSendToLogo = new ExecuteWorkflowRequest()
            //    {
            //        WorkflowId = Guid.Parse("1C8E21D1-DEA0-4C59-BF76-8D386EBEA9D6"),
            //        EntityId = invoiceId
            //    };
            //    this.orgService.Execute(executeSendToLogo);
            //}
        }

        private Guid CheckAndCreateNewInvoice(ContractRepository contractRepository, EntityReference contractRef)
        {
            Guid invoiceId = new Guid();
            LinkEntity contractLink = new LinkEntity();
            contractLink.EntityAlias = "contractAlias";
            contractLink.LinkToEntityName = "rnt_contract";
            contractLink.LinkToAttributeName = "rnt_contractid";
            contractLink.LinkFromEntityName = "rnt_invoice";
            contractLink.LinkFromAttributeName = "rnt_contractid";
            contractLink.LinkCriteria.AddCondition("statuscode", ConditionOperator.Equal, (int)ClassLibrary._Enums_1033.rnt_contractitem_StatusCode.Completed);

            QueryExpression getInvoiceQuery = new QueryExpression("rnt_invoice");
            getInvoiceQuery.Criteria.AddCondition("rnt_contractid", ConditionOperator.Equal, contractRef.Id);
            getInvoiceQuery.Criteria.AddCondition("statuscode", ConditionOperator.Equal, (int)ClassLibrary._Enums_1033.rnt_invoice_StatusCode.Draft);
            getInvoiceQuery.LinkEntities.Add(contractLink);

            EntityCollection invoiceList = this.orgService.RetrieveMultiple(getInvoiceQuery);
            if (invoiceList.Entities.Count == 0)
            {
                int billingType = (int)rnt_BillingTypeCode.Individual;
                var contract = contractRepository.getContractById(contractRef.Id);
                OptionSetValue paymentMethod = contract.GetAttributeValue<OptionSetValue>("rnt_paymentmethodcode");
                OptionSetValue contractType = contract.GetAttributeValue<OptionSetValue>("rnt_contracttypecode");

                if (paymentMethod.Value == (int)rnt_PaymentMethodCode.FullCredit || paymentMethod.Value == (int)rnt_PaymentMethodCode.Current)
                {
                    billingType = (int)rnt_BillingTypeCode.Corporate;
                }
                else if (contractType.Value == (int)rnt_ReservationTypeCode.Kurumsal)
                {
                    billingType = (int)rnt_BillingTypeCode.Corporate;
                }

                InvoiceAddressData invoiceAddressData = GetInvoiceAddress(contract, billingType);
                if (invoiceAddressData != null && invoiceAddressData.invoiceAddressId.HasValue && invoiceAddressData.invoiceAddressId.Value != Guid.Empty)
                {
                    ConfigurationRepository configurationRepository = new ConfigurationRepository(this.orgService);
                    var currency = configurationRepository.GetConfigurationByKey("currency_TRY");

                    InvoiceBL invoiceBL = new InvoiceBL(this.orgService);
                    invoiceId = invoiceBL.createInvoice(invoiceAddressData, null, contract.Id, new Guid(currency));
                }
            }
            else
            {
                invoiceId = invoiceList.Entities[0].Id;
            }
            return invoiceId;
        }

        public InvoiceAddressData GetInvoiceAddress(Entity contract, int billingType)
        {
            InvoiceAddressRepository invoiceAddressRepository = new InvoiceAddressRepository(this.orgService);

            InvoiceAddressData invoiceAdress = new InvoiceAddressData();
            OptionSetValue paymentMethod = contract.GetAttributeValue<OptionSetValue>("rnt_paymentmethodcode");
            if (paymentMethod.Value == (int)rnt_PaymentMethodCode.FullCredit || billingType == (int)rnt_BillingTypeCode.Corporate)
            {
                InvoiceAddressBL invoiceAddressBL = new InvoiceAddressBL(this.orgService);
                CorporateCustomerRepository corporateCustomerRepository = new CorporateCustomerRepository(this.orgService);
                EntityReference corporateRef = contract.GetAttributeValue<EntityReference>("rnt_corporateid");
                var corporateEntity = corporateCustomerRepository.getCorporateCustomerById(corporateRef.Id);
                var key = corporateEntity.GetAttributeValue<string>("rnt_taxnumber");
                invoiceAdress = invoiceAddressBL.getInvoiceAddressByGovermentIdOrByTaxNumber(key).FirstOrDefault();
            }
            else
            {
                EntityReference customerRef = contract.GetAttributeValue<EntityReference>("rnt_customerid");
                var address = invoiceAddressRepository.getIndividualInvoiceAddressByCustomerIdByGivenColumns(customerRef.Id).FirstOrDefault();
                if (address != null && address.Id != Guid.Empty)
                {
                    invoiceAdress = InvoiceHelper.buildInvoiceAddressDataFromInvoiceAddressEntity(address);
                }
            }
            if (invoiceAdress == null || invoiceAdress.invoiceAddressId == null || invoiceAdress.invoiceAddressId == Guid.Empty)
            {
                QueryExpression getInvoiceQuery = new QueryExpression("rnt_invoice");
                getInvoiceQuery.ColumnSet = new ColumnSet(true);
                getInvoiceQuery.Criteria.AddCondition("rnt_contractid", ConditionOperator.Equal, contract.Id);
                getInvoiceQuery.Criteria.AddCondition("rnt_invoicetypecode", ConditionOperator.Equal, billingType);
                getInvoiceQuery.Criteria.AddCondition("statuscode", ConditionOperator.Equal, (int)rnt_invoice_StatusCode.IntegratedWithLogo);
                EntityCollection invoiceList = this.orgService.RetrieveMultiple(getInvoiceQuery);
                if (invoiceList.Entities.Count > 0)
                {
                    Entity temp = invoiceList.Entities[0];
                    invoiceAdress = InvoiceHelper.buildInvoiceAddressDataFromInvoiceEntity(temp);
                }
                else
                {
                    QueryExpression getInvoiceContractQuery = new QueryExpression("rnt_invoice");
                    getInvoiceContractQuery.ColumnSet = new ColumnSet(true);
                    getInvoiceContractQuery.Criteria.AddCondition("rnt_contractid", ConditionOperator.Equal, contract.Id);
                    getInvoiceContractQuery.Criteria.AddCondition("statuscode", ConditionOperator.Equal, (int)rnt_invoice_StatusCode.IntegratedWithLogo);
                    EntityCollection invoiceContractList = this.orgService.RetrieveMultiple(getInvoiceContractQuery);
                    if (invoiceContractList.Entities.Count > 0)
                    {
                        Entity temp = invoiceContractList.Entities[0];
                        invoiceAdress = InvoiceHelper.buildInvoiceAddressDataFromInvoiceEntity(temp);
                    }
                }
            }

            return invoiceAdress;
        }

        private void MakePayment(Entity contract, CreditCardData creditCard, InvoiceAddressData invoiceAdress, decimal paidAmount)
        {
            CreditCardData newCreditCard = new CreditCardData()
            {
                cardToken = creditCard.cardToken,
                cardUserKey = creditCard.cardUserKey,
                creditCardId = creditCard.creditCardId,
            };

            var createPaymentParameters = new CreatePaymentParameters
            {
                contractId = contract.Id,
                transactionCurrencyId = new Guid("036024DD-E8A5-E911-A847-000D3A2BD64E"),
                individualCustomerId = contract.GetAttributeValue<EntityReference>("rnt_customerid").Id,
                conversationId = contract.GetAttributeValue<string>("rnt_pnrnumber"),
                langId = 1055,
                paymentTransactionType = (int)PaymentEnums.PaymentTransactionType.SALE,
                creditCardData = newCreditCard,
                installment = 1, // installment can not be selected during contract creation
                paidAmount = paidAmount,//remainingAmount == decimal.Zero ? d : remainingAmount,
                invoiceAddressData = invoiceAdress,
                virtualPosId = 0,
                paymentChannelCode = rnt_PaymentChannelCode.BATCH
            };

            var paymentBL = new PaymentBL(this.orgService);
            CreatePaymentResponse response = new CreatePaymentResponse();
            try
            {
                response = paymentBL.callMakePaymentAction(createPaymentParameters);
            }
            catch (Exception ex)
            {
            }
        }

        private void prepareServiceConfiguration()
        {
            ConfigurationRepository configurationRepository = new ConfigurationRepository(this.orgService);
            loginInfo = configurationRepository.GetConfigurationByKey("hgsServiceLoginInfo").Split(';');
            endpointUrl = configurationRepository.GetConfigurationByKey("hgsEndPointUrl");

            myBasicHttpBinding = new BasicHttpBinding();
            myBasicHttpBinding.Name = "HGSServiceSoap";
            myBasicHttpBinding.Security.Mode = BasicHttpSecurityMode.Transport;
            myBasicHttpBinding.Security.Message.ClientCredentialType = BasicHttpMessageCredentialType.UserName;
            myBasicHttpBinding.OpenTimeout = TimeSpan.FromMinutes(10);
            myBasicHttpBinding.CloseTimeout = TimeSpan.FromMinutes(10);
            myBasicHttpBinding.ReceiveTimeout = TimeSpan.FromMinutes(10);
            myBasicHttpBinding.SendTimeout = TimeSpan.FromMinutes(10);
            myBasicHttpBinding.MaxReceivedMessageSize = 2147483647;

            myEndpointAddress = new EndpointAddress(endpointUrl);//todo will be read from xrm helper
        }

        ~HGSHelper()
        {
            Dispose(false);
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.hgsWebUtilServicesClient = null;
                this.orgService = null;
                this.tracingService = null;
                this.scope = null;
            }
        }
    }
    public class HGSTransitDataComparer : IEqualityComparer<HGSTransitData>
    {

        public bool Equals(HGSTransitData x, HGSTransitData y)
        {
            return (string.Equals(x.exitDateTime, y.exitDateTime) &&
                    string.Equals(x.entryDateTime, y.entryDateTime) &&
                    //string.Equals(x.entryLocation == null ? string.Empty : x.entryLocation, y.entryLocation == null ? string.Empty : y.entryLocation) &&
                    string.Equals(x.exitLocation == null ? string.Empty : x.exitLocation, y.exitLocation == null ? string.Empty : y.exitLocation) &&
                    string.Equals(x.amount, y.amount));
        }

        public int GetHashCode(HGSTransitData obj)
        {
            return obj.entryDateTime.GetHashCode();
        }
    }
    internal class SecurityHeader : MessageHeader
    {
        private const string HeaderName = "Security";
        private const string HeaderNamespace = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd";
        public override string Name => HeaderName;
        public override string Namespace => HeaderNamespace;
        public string headerString;

        public SecurityHeader(string username, string password)
        {
            this.headerString = @"<UsernameToken xmlns=""http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd""> 
                <Username>" + username + @"</Username> 
                <Password>" + password + @"</Password> 
                <Nonce >" + Guid.NewGuid().ToString() + @"</Nonce> 
                <Created>" + DateTime.Now.ToUniversalTime().ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'") + @"</Created> </UsernameToken>";
        }



        protected override void OnWriteHeaderContents(XmlDictionaryWriter writer, MessageVersion messageVersion)
        {
            var r = XmlReader.Create(new StringReader(headerString));
            r.MoveToContent();
            writer.WriteNode(r, false);
        }
    }
    public class details
    {
        public List<exclude> detailist { get; set; }

    }
    public class exclude
    {
        public string contract { get; set; }
        public string Plaka { get; set; }
    }
    public class deneme
    {
        public DateTime entryTime { get; set; }
        public DateTime exitTime { get; set; }
    }
}