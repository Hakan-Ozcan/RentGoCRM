using System;
using System.Globalization;
using System.Linq;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Tooling.Connector;
using Newtonsoft.Json;
using RntCar.BusinessLibrary.Handlers;
using RntCar.BusinessLibrary.Repository;
using RntCar.ClassLibrary;
using RntCar.ClassLibrary._Enums_1033;
using RntCar.SDK.Common;

namespace RntCar.BusinessLibrary.Business
{
    public class TrafficBL : BusinessHandler
    {
        public TrafficBL(IOrganizationService orgService) : base(orgService)
        {
        }

        public TrafficBL(CrmServiceClient crmServiceClient) : base(crmServiceClient)
        {
        }

        public TrafficBL(GlobalEnums.ConnectionType enums = GlobalEnums.ConnectionType.default_Service, string UserId = "") : base(enums, UserId)
        {
        }

        public TrafficBL(IOrganizationService orgService, CrmServiceClient crmServiceClient) : base(orgService, crmServiceClient)
        {
        }

        public TrafficBL(IOrganizationService orgService, ITracingService tracingService) : base(orgService, tracingService)
        {
        }

        public TrafficBL(IOrganizationService orgService, Guid userId) : base(orgService, userId)
        {
        }

        public TrafficBL(PluginInitializer initializer, IOrganizationService orgService, ITracingService tracingService) : base(initializer, orgService, tracingService)
        {
        }

        public TrafficBL(IOrganizationService orgService, ITracingService tracingService, CrmServiceClient crmServiceClient) : base(orgService, tracingService, crmServiceClient)
        {
        }

        public TrafficBL(IOrganizationService orgService, Guid userId, string orgName) : base(orgService, userId, orgName)
        {

        }
        public void createFineProduct(RntCar.ClassLibrary._Tablet.EIhlalFineData data, Guid contractId, Guid contractItemId, Guid equipmentId)
        {
            Entity e = new Entity("rnt_trafficfine");
            e["rnt_cezamaddesi"] = data.ceza_maddesi;
            e["rnt_cezatarihivesaati"] = data.fineDate.converttoDateTime();
            e["rnt_cezatutari"] = new Money(data.fineAmount);
            e["rnt_duzenleyenbirim"] = data.duzenleyen_birim;
            e["rnt_ililce"] = data.il_ilce;
            e["rnt_kesildigiyer"] = data.displayText;
            if (!string.IsNullOrEmpty(data.teblig_tarihi))
            {
                CultureInfo culture = CultureInfo.InvariantCulture;
                var parsedDate = DateTime.ParseExact(data.teblig_tarihi, "dd.MM.yyyy", culture);
                e["rnt_tebligtarihi"] = parsedDate;
            }
            e["rnt_name"] = data.tutanak_sira_no;
            e["rnt_tutanakseri"] = data.tutanak_seri;
            e["rnt_tutanaksirano"] = data.tutanak_sira_no;
            e["rnt_contractid"] = new EntityReference("rnt_contract", contractId);
            e["rnt_contractitemid"] = new EntityReference("rnt_contractitem", contractItemId);
            e["rnt_equipmentid"] = new EntityReference("rnt_equipment", equipmentId);
            this.OrgService.Create(e);
        }

        public void createFineProduct(EIhlalTrafficFineData data, Guid contractId, Guid contractItemId, Guid equipmentId)
        {
            CultureInfo culture = CultureInfo.InvariantCulture;
            Entity e = new Entity("rnt_trafficfine");

            var d = DateTime.ParseExact(data.fineDate + " " + data.fineTime + ":00", "dd.MM.yyyy HH:mm:ss", culture);
            e["rnt_cezamaddesi"] = data.fineClause;
            e["rnt_cezatarihivesaati"] = d;
            e["rnt_cezatutari"] = new Money(decimal.Parse(data.fineAmount, new NumberFormatInfo() { NumberDecimalSeparator = "," }));
            e["rnt_duzenleyenbirim"] = data.organizingUnit;
            e["rnt_ililce"] = data.addressInfo;
            e["rnt_kesildigiyer"] = data.finePlace;
            if (!string.IsNullOrEmpty(data.communiqueDate))
            {
                var parsedDate = DateTime.ParseExact(data.communiqueDate, "dd.MM.yyyy", culture);
                e["rnt_tebligtarihi"] = parsedDate;
            }
            e["rnt_name"] = data.reportNo;
            e["rnt_tutanakseri"] = data.reportInfo;
            e["rnt_tutanaksirano"] = data.reportNo;
            e["rnt_contractid"] = new EntityReference("rnt_contract", contractId);
            e["rnt_contractitemid"] = new EntityReference("rnt_contractitem", contractItemId);
            e["rnt_equipmentid"] = new EntityReference("rnt_equipment", equipmentId);
            this.OrgService.Create(e);
        }

        public void updateFineProduct(Guid trafficFineId, Guid contractId, Guid contractItemId)
        {
            Entity entity = new Entity("rnt_trafficfine");
            entity.Id = trafficFineId;
            entity["rnt_contractid"] = new EntityReference("rnt_contract", contractId);
            entity["rnt_contractitemid"] = new EntityReference("rnt_contractitem", contractItemId);
            this.OrgService.Update(entity);
        }

        public void processTrafficFineWF(Entity trafficFine)
        {
            this.Trace("start process");
            ContractItemRepository contractItemRepository = new ContractItemRepository(this.OrgService);
            ConfigurationRepository configurationRepository = new ConfigurationRepository(this.OrgService);
            AdditionalProductRepository additionalProductRepository = new AdditionalProductRepository(this.OrgService);

            this.Trace("declared variables");
            var trafficCode = configurationRepository.GetConfigurationByKey("additionalProduct_trafficFineProductCode");
            var trafficAdditionalProduct = additionalProductRepository.getAdditionalProductByProductCode(trafficCode);
            var trafficFineId = trafficFine.Id;
            var trafficFineDate = trafficFine.GetAttributeValue<DateTime>("rnt_cezatarihivesaati");
            var equipment = trafficFine.GetAttributeValue<EntityReference>("rnt_equipmentid");
            var contractItems = contractItemRepository.getNotCancelledEquipmentContractItemByEquipmentId(equipment.Id, trafficFineDate.AddMinutes(StaticHelper.offset));

            try
            {
                Entity contractItem = null;

                var item = contractItems.Where(p => trafficFineDate.AddMinutes(StaticHelper.offset).isBetween(p.GetAttributeValue<DateTime>("rnt_pickupdatetime"),
                                                                                                              p.GetAttributeValue<DateTime>("rnt_dropoffdatetime"))).ToList();

                item = item.DistinctBy(p => p.GetAttributeValue<EntityReference>("rnt_equipment").Id).ToList();

                if (item.Count == 1)
                {
                    contractItem = item.FirstOrDefault();
                    var fineAmount = trafficFine.Attributes.Contains("rnt_cezatutari") ? trafficFine.GetAttributeValue<Money>("rnt_cezatutari").Value : decimal.Zero;
                    var contractId = contractItem.GetAttributeValue<EntityReference>("rnt_contractid").Id;
                    PaymentRepository paymentRepository = new PaymentRepository(this.OrgService);
                    var payment = paymentRepository.getLastPayment_Contract(contractId);

                    EntityReference creditCardReference = null;
                    if (payment != null)
                    {
                        this.Trace("getting card from payment");
                        creditCardReference = payment?.GetAttributeValue<EntityReference>("rnt_customercreditcardid");
                    }
                    else
                    {
                        payment = paymentRepository.getDeposit_Contract(contractId);
                        if(payment != null)
                        {
                            this.Trace("getting card from deposit");
                            creditCardReference = payment?.GetAttributeValue<EntityReference>("rnt_customercreditcardid");
                        }
                    }
                    

                    InvoiceRepository invoiceRepository = new InvoiceRepository(this.OrgService);
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

                    this.Trace("request start");
                    ManualPaymentBL manualPaymentBL = new ManualPaymentBL(this.OrgService);
                    var manualPaymenResponse = manualPaymentBL.makeManualPayment(manualPaymentParameters);
                    this.Trace("reqeust end");
                    this.updateFineProduct(trafficFineId,
                                           contractId,
                                           contractItem.Id);                   
                   
                    try
                    {
                        this.Trace("sending mail start");

                        //sending mail
                        var baseUrl = configurationRepository.GetConfigurationByKey("ExternalCrmWebApiUrl");
                        RestSharpHelper restSharpHelper = new RestSharpHelper(baseUrl, "sendtrafficmail", RestSharp.Method.POST);
                        restSharpHelper.AddQueryParameter("trafficFineId", trafficFineId.ToString());
                        restSharpHelper.AddQueryParameter("contractId", contractId.ToString());
                        restSharpHelper.Execute();
                        this.Trace("sending mail end");

                    }
                    catch (Exception ex)
                    {

                        this.Trace("email error : " + ex.Message);
                    }
                   
                    this.Trace("Update fine product end");
                }
                else
                {
                    this.Trace("problem occured during finding contract item : " + item.Count);
                }
               

               
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
