using Microsoft.Xrm.Sdk;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using RntCar.BusinessLibrary.Business;
using RntCar.BusinessLibrary.Validations;
using RntCar.ClassLibrary;
using RntCar.SDK.Common;
using System;
using System.ServiceModel;

namespace RntCar.CrmPlugins.Contact.Actions
{
    public class ExecuteCreateIndividualCustomer : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            PluginInitializer initializer = new PluginInitializer(serviceProvider);
            IndividualCustomerBL individualCustomerBL = new IndividualCustomerBL(initializer, initializer.Service, initializer.TracingService);

            string customerInformation;
            initializer.PluginContext.GetContextParameter<string>("CustomerInformation", out customerInformation);

            string LangId;
            initializer.PluginContext.GetContextParameter<string>("LangId", out LangId);
            initializer.TraceMe("LangId" + LangId);
            if (string.IsNullOrEmpty(LangId))
            {
                LangId = "1055";
            };
            var _langId = Convert.ToInt32(LangId);
            IndividualCustomerCreateParameter individualCustomerParameters = JsonConvert.DeserializeObject<IndividualCustomerCreateParameter>(customerInformation,
                                                                                                                 new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });
            try
            {

                IndividualCustomerValidation individualCustomerValidation = new IndividualCustomerValidation(initializer.Service, initializer.TracingService);
                var result = individualCustomerValidation.CheckIndividualCustomerInput(customerInformation);
                if (!result)
                {
                    XrmHelper xrmHelper = new XrmHelper(initializer.Service, initializer.InitiatingUserId);

                    var message = xrmHelper.GetXmlTagContentByGivenLangId("IndividualCustomerCreateInputCheck", _langId);
                    initializer.PluginContext.ThrowException<InvalidPluginExecutionException>(message);
                }
                initializer.TraceMe("create start");
                var response = individualCustomerBL.createCustomer(customerInformation, _langId);
                initializer.PluginContext.OutputParameters["ResponseResult"] = JsonConvert.SerializeObject(response.ResponseResult);
                initializer.PluginContext.OutputParameters["IndividualCustomerId"] = response.CustomerId;
                initializer.PluginContext.OutputParameters["IndividualAddressId"] = response.IndividualAddressId;
                initializer.PluginContext.OutputParameters["InvoiceAddressId"] = response.InvoiceAddressId;
            }
            catch (FaultException<OrganizationServiceFault> ex)
            {
                initializer.TraceMe("exception detail : " + ex.Message);
                initializer.TraceMe("exception error code : " + ex.Detail.ErrorCode);
                initializer.TraceMe("langID : " + _langId);

                switch (ex.Detail.ErrorCode)
                {
                    // Duplicate primary key exception
                    //todo will replace by helper
                    case -2147088238:

                        if (_langId == 1033)
                            throw new Exception("CustomErrorMessagefinder:Customer is already exists in CRM.");
                        else if (_langId == 1055)
                        {
                            throw new Exception(string.Format("CustomErrorMessagefinder: Girdiğiniz bilgilere ait müşteri crm'de mevcuttur.TCKN/Pasaport/Email :{0}{1}/{2}", individualCustomerParameters.governmentId, individualCustomerParameters.passportNumber, individualCustomerParameters.email));
                        }
                           
                        break;

                    case -2147220937:
                        if (_langId == 1033)
                            throw new Exception("CustomErrorMessagefinder:Customer is already exists in CRM.");
                        else if (_langId == 1055)
                            throw new Exception(string.Format("CustomErrorMessagefinder: Girdiğiniz bilgilere ait müşteri crm'de mevcuttur.TCKN/Pasaport/Email :{0}{1}/{2}", individualCustomerParameters.governmentId, individualCustomerParameters.passportNumber, individualCustomerParameters.email));
                        break;

                    case -2147220891:
                        if (_langId == 1033)
                        {
                            initializer.TraceMe(ex.Message);
                            if(ex.Message.Contains("Invalid GovernmentId") || ex.Message.Contains("TC Kimlik Numarası Geçersiz"))
                            {
                                throw new Exception("Your identity information is not correct");
                            }
                            else
                            {
                                throw new Exception(ex.Message);
                            }
                         
                        }
                            
                        else if (_langId == 1055)
                        {
                            if (ex.Message.Contains("Invalid GovernmentId"))
                            {
                                throw new Exception("CustomErrorMessagefinder: Kimlik Bilgilerinizi Hatalı Girdiniz.");
                            }
                            else
                            {
                                throw new Exception(ex.Message);
                            }
                        }                            
                        break;

                    default:
                        throw new Exception(ex.Message);

                }
            }
            catch (Exception ex)
            {
                initializer.TracingService.Trace("exception detail : " + ex.Message);
                throw new InvalidPluginExecutionException(ex.Message);
            }


        }
    }
}
