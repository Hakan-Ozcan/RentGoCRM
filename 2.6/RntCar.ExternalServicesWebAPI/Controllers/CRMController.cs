using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Newtonsoft.Json;
using RazorEngine.Configuration;
using RazorEngine.Templating;
using RntCar.BusinessLibrary.Business;
using RntCar.BusinessLibrary.Helpers;
using RntCar.BusinessLibrary.Repository;
using RntCar.ClassLibrary;
using RntCar.ClassLibrary.Campaign.Pegasus;
using RntCar.Logger;
using RntCar.SDK.Common;
using System;
using System.IO;
using System.Web;
using System.Web.Http;

namespace RntCar.ExternalServicesWebAPI.Controllers
{
    public class CRMController : ApiController
    {
        [HttpPost]
        [HttpGet]
        [Route("api/crm/testme")]
        public string testme()
        {
            return "i am ok";
        }
        [HttpPost]
        [Route("api/crm/createPaymentRecord")]
        public CreatePaymentWithServiceResponse createPaymentRecord([FromBody] CreatePaymentWithServiceParameters createPaymentWithServiceParameters)
        {
            LoggerHelper loggerHelper = new LoggerHelper();
            loggerHelper.traceInfo("params " + JsonConvert.SerializeObject(createPaymentWithServiceParameters));
            try
            {
                CrmServiceHelper crmServiceHelper = new CrmServiceHelper();
                PaymentBL paymentBL = new PaymentBL(crmServiceHelper.IOrganizationService);
                // create new payment record
                var provider = paymentBL.getProviderCode(createPaymentWithServiceParameters.createPaymentParameters.reservationId, createPaymentWithServiceParameters.createPaymentParameters.contractId);
                var paymentId = paymentBL.createPayment(createPaymentWithServiceParameters.createPaymentParameters, provider);
                // update payment record with error message
                paymentBL.updatePaymentResult(paymentId, createPaymentWithServiceParameters.paymentResponse);
                loggerHelper.traceInfo("operation finished in peace");

                return new CreatePaymentWithServiceResponse
                {
                    paymentId = paymentId,
                    ResponseResult = ResponseResult.ReturnSuccess()
                };
            }
            catch (Exception ex)
            {
                loggerHelper.traceInfo("error : " + ex.Message);
                return new CreatePaymentWithServiceResponse
                {
                    ResponseResult = ResponseResult.ReturnError(ex.Message)
                };
            }
            finally
            {
                //
            }
        }

        [HttpPost]
        [Route("api/crm/createRefundRecord")]
        public CreatePaymentWithServiceResponse createRefundRecord([FromBody] CreatePaymentWithServiceParameters createPaymentWithServiceParameters)
        {
            LoggerHelper loggerHelper = new LoggerHelper();
            loggerHelper.traceInfo("params " + JsonConvert.SerializeObject(createPaymentWithServiceParameters));
            try
            {
                CrmServiceHelper crmServiceHelper = new CrmServiceHelper();
                foreach (var item in createPaymentWithServiceParameters.refundEntities)
                {
                    PaymentBL paymentBL = new PaymentBL(crmServiceHelper.IOrganizationService);
                    var id = paymentBL.createRefundEntity(item);

                    if (item.transactionResult == 1)
                    {
                        Entity e = new Entity("rnt_payment");
                        e.Id = id;
                        e["rnt_transactionresult"] = new OptionSetValue((int)PaymentEnums.PaymentTransactionResult.Success);
                        crmServiceHelper.IOrganizationService.Update(e);
                    }
                    else
                    {
                        Entity e = new Entity("rnt_payment");
                        e.Id = id;
                        e["rnt_transactionresult"] = new OptionSetValue((int)PaymentEnums.PaymentTransactionResult.Error);
                        crmServiceHelper.IOrganizationService.Update(e);
                    }
                }

                return new CreatePaymentWithServiceResponse
                {
                    ResponseResult = ResponseResult.ReturnSuccess()
                };
            }
            catch (Exception ex)
            {
                loggerHelper.traceInfo("error : " + ex.Message);
                return new CreatePaymentWithServiceResponse
                {
                    ResponseResult = ResponseResult.ReturnError(ex.Message)
                };
            }
            finally
            {
                //
            }
        }
        [HttpPost]
        [Route("api/crm/sendtrafficmail")]
        public void sendtrafficmail([FromUri] Guid trafficFineId, [FromUri] Guid contractId)
        {
            LoggerHelper loggerHelper = new LoggerHelper();
            loggerHelper.traceInfo("params " + trafficFineId);
            loggerHelper.traceInfo("contractId " + contractId);
            try
            {
                CrmServiceHelper crmServiceHelper = new CrmServiceHelper();
                var config = new TemplateServiceConfiguration();
                config.DisableTempFileLocking = true;
                var service = RazorEngineService.Create(config);
                string template = File.ReadAllText(HttpContext.Current.Server.MapPath("~") + @"\Resources\Traffic.html");
                loggerHelper.traceInfo("getting template");
                TrafficFineRepository trafficFineRepository = new TrafficFineRepository(crmServiceHelper.IOrganizationService);
                var traffic = trafficFineRepository.getTrafficFineById(trafficFineId);
                loggerHelper.traceInfo("traffic retrieved");

                ContractRepository contractRepository = new ContractRepository(crmServiceHelper.IOrganizationService);
                var contract = contractRepository.getContractById(contractId);
                loggerHelper.traceInfo("contract retrieved");
                loggerHelper.traceInfo(JsonConvert.SerializeObject(traffic));

                var t = traffic.GetAttributeValue<Money>("rnt_cezatutari").Value * 0.75M;
                loggerHelper.traceInfo("t  : " + t);

                AdditionalProductHelper additionalProductHelper = new AdditionalProductHelper(null, crmServiceHelper.IOrganizationService);
                var fineProduct = additionalProductHelper.getFineAdditionalProduct();
                var subAmount = additionalProductHelper.calculateFineProductServicePrice(fineProduct.Id, fineProduct.Id, t, 0);
                var p = new
                {
                    ContactName = contract.GetAttributeValue<EntityReference>("rnt_customerid").Name,
                    TotalPayment = t + subAmount,
                    PickupDateTime = contract.GetAttributeValue<DateTime>("rnt_pickupdatetime"),
                    trafficFines = new traffic()
                    {
                        serino = traffic.GetAttributeValue<string>("rnt_tutanaksirano"),
                        plaka = traffic.GetAttributeValue<EntityReference>("rnt_equipmentid").Name,
                        //Sunucu Saati 3 saat geride olduğu için LocalTime kullanılmamıştır.
                        tarih = traffic.GetAttributeValue<DateTime>("rnt_cezatarihivesaati").AddHours(3).ToString("dd/MM/yyyy HH:mm"),
                        tutar = Convert.ToString(t),
                        yer = traffic.GetAttributeValue<string>("rnt_kesildigiyer"),
                        subtutar = Convert.ToString(subAmount),
                    }
                };
                loggerHelper.traceInfo(JsonConvert.SerializeObject(p));
                var result = service.RunCompile(template, "templateKey", null, p);

                loggerHelper.traceInfo("result parsed");

                ConfigurationRepository configurationRepository = new ConfigurationRepository(crmServiceHelper.IOrganizationService);

                Entity fromActivityParty = new Entity("activityparty");
                Entity toActivityParty = new Entity("activityparty");

                var contactId = contract.GetAttributeValue<EntityReference>("rnt_customerid").Id;
                fromActivityParty["partyid"] = new EntityReference("systemuser", new Guid(configurationRepository.GetConfigurationByKey("CrmAdminGuid")));
                toActivityParty["partyid"] = new EntityReference("contact", contactId);

                Entity email = new Entity("email");
                email["from"] = new Entity[] { fromActivityParty };
                email["to"] = new Entity[] { toActivityParty };
                email["regardingobjectid"] = new EntityReference("rnt_contract", contractId);
                email["subject"] = "Trafik Cezası Ücret Tahsili";
                email["description"] = result;
                email["directioncode"] = true;
                Guid emailId = crmServiceHelper.IOrganizationService.Create(email);
                loggerHelper.traceInfo("email created : " + emailId);

                SendEmailRequest sendEmailRequest = new SendEmailRequest
                {
                    EmailId = emailId,
                    TrackingToken = "",
                    IssueSend = true
                };

                SendEmailResponse sendEmailresp = (SendEmailResponse)crmServiceHelper.IOrganizationService.Execute(sendEmailRequest);
                loggerHelper.traceInfo("email send");
            }
            catch (Exception ex)
            {
                loggerHelper.traceInfo("error : " + ex.Message);

            }
            finally
            {
                //
            }
        }
        [HttpPost]
        [Route("api/crm/checkpegasusmembership")]
        public PegasusMemberResponse checkPegasusMembership([FromBody] CheckPegasusMembershipRequest checkPegasusMembershipRequest)
        {
            try
            {
                LoggerHelper loggerHelper = new LoggerHelper();
                CrmServiceHelper crmServiceHelper = new CrmServiceHelper();
                loggerHelper.traceInfo("checkPegasusMembershipRequest " + JsonConvert.SerializeObject(checkPegasusMembershipRequest));
                PegasusBL pegasusBL = new PegasusBL(crmServiceHelper.IOrganizationService);
                return pegasusBL.checkMemberShip(checkPegasusMembershipRequest);
            }
            catch (Exception)
            {
                throw;
            }
        }
        private class traffic
        {
            public string tarih { get; set; }
            public string yer { get; set; }
            public string plaka { get; set; }
            public string serino { get; set; }
            public string tutar { get; set; }
            public string subtutar { get; set; }
        }
    }
}
