using Microsoft.Xrm.Sdk;
using Newtonsoft.Json;
using RntCar.BusinessLibrary.Business;
using RntCar.BusinessLibrary.Validations;
using RntCar.ClassLibrary;
using RntCar.SDK.Common;
using System;
using System.Linq;
using System.ServiceModel;

namespace RntCar.CrmPlugins.Contact.Actions
{
    public class ExecuteCreateOrUpdateIndividualCustomerForAdditionalDrivers : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            PluginInitializer initializer = new PluginInitializer(serviceProvider);

            try
            {
                string additionalDriverParameter;
                initializer.PluginContext.GetContextParameter<string>("AdditionalDriverParameter", out additionalDriverParameter);

                int langId;
                initializer.PluginContext.GetContextParameter<int>("LangId", out langId);

                var parameters = JsonConvert.DeserializeObject<IndividualCustomerAdditionalDriverParameters>(additionalDriverParameter); //, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });

                initializer.TraceMe("additionalDriverParameter " + additionalDriverParameter);
                #region Validations
                IndividualCustomerBL individualCustomerBL = new IndividualCustomerBL(initializer.Service, initializer.TracingService);

                IndividualCustomerValidation individualCustomerValidation = new IndividualCustomerValidation(initializer.Service, initializer.TracingService);

                var result = true;
                var identityKey = parameters.isTurkishCitizen ? parameters.governmentId : parameters.passportNumber;
                var existingCustomerId = individualCustomerBL.getExistingCustomerIdByGovernmentIdOrPassportNumber(identityKey);
                if (existingCustomerId.HasValue)
                {
                    if (existingCustomerId.Value == parameters.documentContactId)
                    {
                        XrmHelper xrmHelper = new XrmHelper(initializer.Service);
                        var message = xrmHelper.GetXmlTagContentByGivenLangId("AdditionalDriverDocumentContactValidation", langId);
                        initializer.PluginContext.ThrowException<InvalidPluginExecutionException>(message);
                    }
                    else
                    {
                        parameters.contactId = existingCustomerId.Value;
                    }
                }

                BlackListBL blackListBL = new BlackListBL(initializer.Service, initializer.TracingService);
                var blackListResponse = blackListBL.BlackListValidation(identityKey);
                if (blackListResponse.BlackList.IsInBlackList)
                {
                    XrmHelper xrmHelper = new XrmHelper(initializer.Service);
                    var message = xrmHelper.GetXmlTagContentByGivenLangId("BlackListValidation", langId);
                    initializer.PluginContext.ThrowException<InvalidPluginExecutionException>(message);
                }

                if (parameters.isTurkishCitizen)
                {
                    result = individualCustomerValidation.checkBirthDateForTurkishCitizen(parameters.birthDate);
                    if (!result)
                    {
                        XrmHelper xrmHelper = new XrmHelper(initializer.Service);

                        var message = xrmHelper.GetXmlTagContentByGivenLangId("IndividualCustomerBirthDateValidation", langId);
                        initializer.PluginContext.ThrowException<InvalidPluginExecutionException>(message);
                    }
                }
                else
                {
                    result = individualCustomerValidation.checkBirthDateForAliens(parameters.birthDate);
                    if (!result)
                    {
                        XrmHelper xrmHelper = new XrmHelper(initializer.Service);

                        var message = xrmHelper.GetXmlTagContentByGivenLangId("IndividualCustomerBirthDateValidation", langId);
                        initializer.PluginContext.ThrowException<InvalidPluginExecutionException>(message);
                    }
                }

                result = individualCustomerValidation.CheckDrivingLicenseDate(parameters.drivingLicenseDate);
                if (!result)
                {
                    XrmHelper xrmHelper = new XrmHelper(initializer.Service);

                    var message = xrmHelper.GetXmlTagContentByGivenLangId("LicenseDateValidation", langId);
                    initializer.PluginContext.ThrowException<InvalidPluginExecutionException>(message);
                }



                var totalDuration = Convert.ToInt32(CommonHelper.calculateTotalDurationInDays(parameters.pickupDateTime.converttoTimeStamp(), parameters.dropoffDateTime.converttoTimeStamp()));

                AdditionalProductsBL additionalProductsBL = new AdditionalProductsBL(initializer.Service);
                var additionalProductResponse = additionalProductsBL.getYoungDriverProductByValidationsForAdditionalDrivers(parameters.reservationId,
                                                                                                                            parameters.contractId,
                                                                                                                            parameters.pickupDateTime,
                                                                                                                            parameters.drivingLicenseDate,
                                                                                                                            parameters.birthDate,
                                                                                                                            totalDuration);

                if (!additionalProductResponse.ResponseResult.Result)
                {
                    initializer.PluginContext.ThrowException<InvalidPluginExecutionException>(additionalProductResponse.ResponseResult.ExceptionDetail);
                }
                #endregion

                var response = new IndividualCustomerAdditionalDriverResponse
                {
                    additionalProduct = additionalProductResponse.AdditionalProducts?.FirstOrDefault(),
                };

                if (!parameters.contactId.HasValue)
                {
                    var contactId = individualCustomerBL.createIndividualCustomerForAdditionalDrivers(parameters);
                    response.contactId = contactId;
                    response.ResponseResult = ResponseResult.ReturnSuccess();
                    initializer.PluginContext.OutputParameters["AdditionalDriverResponse"] = JsonConvert.SerializeObject(response);
                }
                else
                {
                    individualCustomerBL.updateIndividualCustomerForAdditionalDrivers(parameters);
                    response.contactId = parameters.contactId.Value;
                    response.ResponseResult = ResponseResult.ReturnSuccess();
                    initializer.PluginContext.OutputParameters["AdditionalDriverResponse"] = JsonConvert.SerializeObject(response);
                }
            }
            catch (FaultException<OrganizationServiceFault> ex)
            {
                initializer.TraceMe("exception detail : " + ex.Message);
                initializer.TraceMe("exception error code : " + ex.Detail.ErrorCode);

                switch (ex.Detail.ErrorCode)
                {
                    // Duplicate primary key exception
                    //todo will replace by helper
                    case -2147088238:
                        throw new Exception("CustomErrorMessagefinder:Customer is already exists in CRM.");
                    default:
                        throw new Exception(ex.Message);

                }
            }
        }
    }
}
