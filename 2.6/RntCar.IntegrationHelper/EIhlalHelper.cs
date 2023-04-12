using Microsoft.Xrm.Sdk;
using Newtonsoft.Json;
using RntCar.BusinessLibrary;
using RntCar.BusinessLibrary.Business;
using RntCar.BusinessLibrary.Repository;
using RntCar.ClassLibrary;
using RntCar.ClassLibrary._Enums_1033;
using RntCar.Logger;
using RntCar.SDK.Common;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace RntCar.IntegrationHelper
{
    public class EIhlalHelper
    {
        public IOrganizationService orgService { get; set; }
        public ITracingService tracingService { get; set; }
        public EIhlalHelper(IOrganizationService service)
        {
            this.orgService = service;
        }
        public EIhlalHelper(IOrganizationService service, ITracingService tracingService)
        {
            this.orgService = service;
            this.tracingService = tracingService;
        }
        public EIhlalTrafficFineResponse getTrafficFine(string plateNumber)
        {
            XrmHelper xrmHelper = new XrmHelper(this.orgService);
            var serviceBaseUrl = xrmHelper.getConfigurationValueByName("e-ihlalTrafficFineServiceUrl");
            var apiKey = xrmHelper.getConfigurationValueByName("e-ihlalApiKey");
            var serviceQuery = "?p=" + apiKey + "&plaka=" + plateNumber;
            RestSharpHelper restSharpHelper = new RestSharpHelper(serviceBaseUrl + serviceQuery, "", RestSharp.Method.POST);
            var response = restSharpHelper.Execute();
            XmlSerializerHelper serializer = new XmlSerializerHelper();
            return serializer.Deserialize<EIhlalTrafficFineResponse>(response.RawBytes, "RSP");
        }
        public EIhlalHighwayResponse getHighwayFine(string plateNumber)
        {
            XrmHelper xrmHelper = new XrmHelper(this.orgService);
            var serviceBaseUrl = xrmHelper.getConfigurationValueByName("e-ihlalHighwayFineServiceUrl");
            var apiKey = xrmHelper.getConfigurationValueByName("e-ihlalApiKey");
            var serviceQuery = "?p=" + apiKey + "&plaka=" + plateNumber;
            RestSharpHelper restSharpHelper = new RestSharpHelper(serviceBaseUrl + serviceQuery, "", RestSharp.Method.POST);
            var response = restSharpHelper.Execute();
            XmlSerializerHelper serializer = new XmlSerializerHelper();
            return serializer.Deserialize<EIhlalHighwayResponse>(response.RawBytes, "RSP");
        }

        public void processTrafficFineBatch()
        {
            LoggerHelper loggerHelper = new LoggerHelper();
            CrmServiceHelper crmServiceHelper = new CrmServiceHelper();
            CultureInfo culture = CultureInfo.InvariantCulture;
            ContractItemRepository contractItemRepository = new ContractItemRepository(crmServiceHelper.IOrganizationService);
            var contractItems = contractItemRepository.getCompletedContractItemEquipmentsByXlastDays(60);
            ConfigurationRepository configurationRepository = new ConfigurationRepository(crmServiceHelper.IOrganizationService);
            var trafficCode = configurationRepository.GetConfigurationByKey("additionalProduct_trafficFineProductCode");

            AdditionalProductRepository additionalProductRepository = new AdditionalProductRepository(crmServiceHelper.IOrganizationService);
            var trafficAdditionalProduct = additionalProductRepository.getAdditionalProductByProductCode(trafficCode);
            foreach (var contractItem in contractItems)
            {
                loggerHelper.traceInfo("contractId : " + contractItem.GetAttributeValue<EntityReference>("rnt_contractid").Id);
                loggerHelper.traceInfo("contractItem.Id: " + contractItem.Id);
                try
                {

                    var fines = this.getTrafficFine(contractItem.GetAttributeValue<EntityReference>("rnt_equipment").Name);
                    var fineAmount = decimal.Zero;
                    List<EIhlalTrafficFineData> fineData = new List<EIhlalTrafficFineData>();

                    if (fines.result)
                    {
                        var e = fines.trafficFineData.Where(item => DateTime.ParseExact(item.fineDate + " " + item.fineTime + ":00", "dd.MM.yyyy HH:mm:ss", culture) >= contractItem.GetAttributeValue<DateTime>("rnt_pickupdatetime") &&
                                                                    DateTime.ParseExact(item.fineDate + " " + item.fineTime + ":00", "dd.MM.yyyy HH:mm:ss", culture) <= contractItem.GetAttributeValue<DateTime>("rnt_dropoffdatetime")).ToList();

                        foreach (var fine in e)
                        {
                            TrafficFineRepository trafficFineRepository = new TrafficFineRepository(crmServiceHelper.IOrganizationService);
                            var crmFine = trafficFineRepository.getTrafficFineBytutanak_sira_no(fine.reportNo, contractItem.Id.ToString());

                            loggerHelper.traceInfo(contractItem.GetAttributeValue<EntityReference>("rnt_equipment").Name + " : " + fine.fineDate);
                            if (crmFine == null)
                            {
                                loggerHelper.traceInfo("couldnt found in crm : " + fine.reportNo);
                                fineAmount += decimal.Parse(fine.fineAmount, new NumberFormatInfo() { NumberDecimalSeparator = "," });
                                fineData.Add(fine);
                            }
                        }
                        if (fineAmount > decimal.Zero)
                        {
                            loggerHelper.traceInfo("fine amount > 0 " + fineAmount);
                            var contractId = contractItem.GetAttributeValue<EntityReference>("rnt_contractid").Id;
                            PaymentRepository paymentRepository = new PaymentRepository(crmServiceHelper.IOrganizationService);
                            var payment = paymentRepository.getLastPayment_Contract(contractId);
                            var creditCardReference = payment?.GetAttributeValue<EntityReference>("rnt_customercreditcardid");

                            InvoiceRepository invoiceRepository = new InvoiceRepository(crmServiceHelper.IOrganizationService);
                            //get default invoice , naming will be consider
                            var invoice = invoiceRepository.getFirstActiveInvoiceByContractId(contractId);

                            var manualPaymentParameters = new ManualPaymentParameters
                            {
                                creditCardId = creditCardReference?.Id,
                                additionalProductId = trafficAdditionalProduct.Id,
                                amount = fineAmount * 0.75M,
                                contractId = contractId,
                                invoiceId = invoice.FirstOrDefault()?.Id,
                                manualPaymentType = (int)rnt_ManualPaymentTypeCode.AddAdditionalProductWithPayment,
                                isDebt = true,
                                langId = 1055,
                                channelCode = (int)rnt_ReservationChannel.Job
                            };
                            loggerHelper.traceInfo("manual payment parameters end");
                            loggerHelper.traceInfo("request start");
                            ManualPaymentBL manualPaymentBL = new ManualPaymentBL(crmServiceHelper.IOrganizationService);
                            var manualPaymenResponse = manualPaymentBL.makeManualPayment(manualPaymentParameters);

                            loggerHelper.traceInfo("manualPaymenResponse " + JsonConvert.SerializeObject(manualPaymenResponse));
                            //create traffic fines
                            if (fineData.Count > 0)
                            {
                                foreach (var fdata in fineData)
                                {
                                    try
                                    {
                                        TrafficBL trafficBL = new TrafficBL(crmServiceHelper.IOrganizationService);
                                        trafficBL.createFineProduct(fdata,
                                                                    contractItem.GetAttributeValue<EntityReference>("rnt_contractid").Id,
                                                                    contractItem.Id,
                                                                    contractItem.GetAttributeValue<EntityReference>("rnt_equipment").Id);
                                    }
                                    catch (Exception ex)
                                    {
                                        loggerHelper.traceInfo("createFineProduct error :" + ex.Message);
                                        continue;                                       
                                    }
                                   
                                }
                            }

                        }
                    }
                }
                catch (Exception ex)
                {
                    loggerHelper.traceInfo("general error :" + ex.Message);
                    continue;
                }

            }
        }
    }
}
