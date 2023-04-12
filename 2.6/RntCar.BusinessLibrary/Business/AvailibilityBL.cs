using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Tooling.Connector;
using Newtonsoft.Json;
using RestSharp;
using RntCar.BusinessLibrary.Handlers;
using RntCar.BusinessLibrary.Repository;
using RntCar.BusinessLibrary.Validations;
using RntCar.ClassLibrary;
using RntCar.ClassLibrary.MongoDB;
using RntCar.SDK.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RntCar.BusinessLibrary.Business
{
    public class AvailibilityBL : BusinessHandler
    {
        public AvailibilityBL(IOrganizationService orgService) : base(orgService)
        {
        }
        public AvailibilityBL(IOrganizationService orgService, ITracingService tracingService) : base(orgService, tracingService)
        {
        }

        public AvailibilityBL(PluginInitializer initializer, IOrganizationService orgService, ITracingService tracingService) : base(initializer, orgService, tracingService)
        {
        }

        public AvailibilityBL(CrmServiceClient crmServiceClient) : base(crmServiceClient)
        {
        }

        public AvailabilityResponse calculateAvailibility(string availibilityParameters)
        {
            ConfigurationBL configurationBL = new ConfigurationBL(this.OrgService);
            var responseUrl = configurationBL.GetConfigurationByName("MongoDBWepAPIURL");

            RestSharpHelper restSharpHelper = new RestSharpHelper(responseUrl, "CalculateAvailability", Method.POST);
            var param = JsonConvert.DeserializeObject<AvailabilityParameters>(availibilityParameters);
            restSharpHelper.AddJsonParameter<AvailabilityParameters>(param);

            return restSharpHelper.Execute<AvailabilityResponse>();
        }

        public string calculateAvailibility(AvailabilityParameters param, int langId)
        {
            this.Trace("configuration retrieve start");
            this.Trace("param.customerType : " + param.customerType);
            ConfigurationBL configurationBL = new ConfigurationBL(this.OrgService, this.TracingService);
            var responseUrl = configurationBL.GetConfigurationByName("MongoDBWepAPIURL");
            this.Trace("configuration retrieve end : " + responseUrl);
            this.Trace("system parameter start");
            SystemParameterBL systemParameterBL = new SystemParameterBL(this.OrgService);
            var shiftDuration = systemParameterBL.getReservationShiftDuration();
            this.Trace("system parameter retrieve start");

            param.shiftDuration = shiftDuration;
            Guid corpId = Guid.Empty;
            if (param.customerType == (int)GlobalEnums.CustomerType.Individual &&
               !string.IsNullOrEmpty(param.individualCustomerId))
            {
                this.Trace("individual pricing setting segmentation");
                IndividualCustomerRepository individualCustomerRepository = new IndividualCustomerRepository(this.OrgService);
                var contact = individualCustomerRepository.getIndividualCustomerByIdWithGivenColumns(new Guid(param.individualCustomerId), new string[] { "rnt_segmentcode" });
                param.segment = contact.Attributes.Contains("rnt_segmentcode") ?
                                (int?)contact.GetAttributeValue<OptionSetValue>("rnt_segmentcode").Value :
                                null;

                this.Trace("IndividualCustomerRepository end");
            }

            if (param.customerType == (int)GlobalEnums.CustomerType.Individual &&
               !string.IsNullOrEmpty(param.corporateCustomerId) &&
               Guid.TryParse(param.corporateCustomerId, out corpId) &&
               corpId != Guid.Empty)
            {                
                this.Trace("individual but corporate pricing" + param.corporateCustomerId);
                CorporateCustomerRepository corporateCustomerRepository = new CorporateCustomerRepository(this.OrgService);
                if (param.isMonthly)
                {             
                    param.priceCodeId = corporateCustomerRepository.getCorporateCustomerMonthlyPriceCode(new Guid(param.corporateCustomerId));
                }
                else
                {
                    param.priceCodeId = corporateCustomerRepository.getCorporateCustomerPriceCode(new Guid(param.corporateCustomerId));
                }
                this.Trace("param.priceCodeId" + param.priceCodeId);

            }
            else if (param.customerType != (int)GlobalEnums.CustomerType.Individual &&
                    !string.IsNullOrEmpty(param.corporateCustomerId) &&
                    Guid.TryParse(param.corporateCustomerId, out corpId) &&
                    corpId != Guid.Empty)
            {
                this.Trace("corporate pricing");
                CorporateCustomerRepository corporateCustomerRepository = new CorporateCustomerRepository(this.OrgService);
                var acc = corporateCustomerRepository.getCorporateCustomerById(new Guid(param.corporateCustomerId), new string[] { "rnt_pricecodeid", 
                                                                                                                                   "rnt_accounttypecode",
                                                                                                                                   "rnt_processindividualprices" ,
                                                                                                                                   "rnt_pricefactorgroupcode"});
                if (param.isMonthly)
                {
                    param.priceCodeId = corporateCustomerRepository.getCorporateCustomerMonthlyPriceCode(new Guid(param.corporateCustomerId));
                }
                else
                {
                    param.priceCodeId = corporateCustomerRepository.getCorporateCustomerPriceCode(new Guid(param.corporateCustomerId));
                }
                this.Trace("param.priceCodeId" + param.priceCodeId);
                param.processIndividualPrices_broker = acc.Contains("rnt_processindividualprices") ? acc.GetAttributeValue<bool>("rnt_processindividualprices") : false;
                param.corporateType = acc.GetAttributeValue<OptionSetValue>("rnt_accounttypecode").Value;
                if (acc.Contains("rnt_pricefactorgroupcode"))
                {
                    param.accountGroup = acc.GetAttributeValue<OptionSetValue>("rnt_pricefactorgroupcode").Value.ToString();
                }                
                if ((param.customerType == (int)GlobalEnums.CustomerType.Broker  ||
                      param.customerType == (int)GlobalEnums.CustomerType.Agency)
                    && !string.IsNullOrEmpty(param.reservationId))
                {
                    this.Trace("exchange rate operations type");
                    ReservationRepository reservationRepository = new ReservationRepository(this.OrgService);
                    var res = reservationRepository.getReservationById(new Guid(param.reservationId), new string[] { "transactioncurrencyid" });

                    this.Trace("transactioncurrencyid " + res.GetAttributeValue<EntityReference>("transactioncurrencyid").Id);
                    CurrencyRepository currencyRepository = new CurrencyRepository(this.OrgService);
                    var currency = currencyRepository.getCurrencyById(res.GetAttributeValue<EntityReference>("transactioncurrencyid").Id, new string[] { "exchangerate" });
                    param.exchangeRate = currency.GetAttributeValue<decimal>("exchangerate");
                }
                this.Trace("corp type");
            }            
          
            this.Trace("availibility params " + JsonConvert.SerializeObject(param));

            RestSharpHelper restSharpHelper = new RestSharpHelper(responseUrl, "CalculateAvailability", Method.POST);
            restSharpHelper.AddJsonParameter<AvailabilityParameters>(param);
            this.Trace("before mongodb");
            var response = restSharpHelper.Execute<AvailabilityResponse>();            
            //this.Trace("restSharpHelper.RestResponse.Content :" + restSharpHelper.RestResponse.Content);
            //this.Trace("response" + JsonConvert.SerializeObject(response));

            if (!string.IsNullOrEmpty(param.reservationId))
            {
                this.Trace("process price started");
                this.processPrices(response, param.reservationId, string.Empty);
                this.Trace("process price end");

            }
            // if the selected group code is not in the availability data, the haserror parameter will return true
            this.processErrorMessages(response, langId);
            return JsonConvert.SerializeObject(response);
        }

        public AvailabilityResponse decideChangedGroupCodeStatus(AvailabilityData _currentGroupCode, List<AvailabilityData> _changedGroupCode)
        {
            try
            {

                var currentGroupCode = _currentGroupCode;
                var changedGroupCode = _changedGroupCode; 

                GroupCodeInformationBL groupCodeInformationBL = new GroupCodeInformationBL(this.OrgService);
                var groupCodeInformationDetail = groupCodeInformationBL.getGroupCodeInformationDetailForDocument(_currentGroupCode.groupCodeInformationId); 

                foreach (var item in changedGroupCode)
                {
                    // if reservation is pay now, the contract will create without any action
                    if (item.payNowTotalPrice > currentGroupCode.totalPrice)
                    {
                        item.isUpsell = true;
                        if (groupCodeInformationDetail.upgradeGroupCodes.Exists(e => e.Equals(item.groupCodeInformationId)))
                        {
                            item.isUpgrade = true;
                        }
                    }
                    else if (item.payNowTotalPrice <= currentGroupCode.totalPrice)
                    {
                        item.isDownsell = true;
                        item.isDowngrade = true;
                    }
                }

                return new AvailabilityResponse
                {
                    availabilityData = changedGroupCode,
                    ResponseResult = ResponseResult.ReturnSuccess()
                };

            }
            catch (Exception ex)
            {
                return new AvailabilityResponse
                {
                    ResponseResult = ResponseResult.ReturnError(ex.Message)
                };
            }

        }

        public AvailabilityResponse CheckAvailibility(string availibilityParameters, int langId, int channel)
        {

            var param = JsonConvert.DeserializeObject<AvailabilityParameters>(availibilityParameters);
            param.dropoffDateTime = param.dropoffDateTime.AddMinutes(StaticHelper.offset);
            param.pickupDateTime = param.pickupDateTime.AddMinutes(StaticHelper.offset);

            //this.Trace("after added offset" + JsonConvert.SerializeObject(param));

            var branchsAvailibility = this.CheckBranchsAvailibility(param);

            if (!branchsAvailibility.ResponseResult.Result)
            {
                return branchsAvailibility;
            }

            var dateTimesAvailibility = this.CheckDateTimesAvailibility(param);

            if (!dateTimesAvailibility.ResponseResult.Result)
            {
                return dateTimesAvailibility;
            }
            this.Trace("date and time checks are done");
            var workingHourAvailibility = this.CheckWorkingHourAvailability(param);
            this.Trace("CheckWorkingHourAvailability are done" + workingHourAvailibility.ResponseResult.ExceptionDetail);

            if (!workingHourAvailibility.ResponseResult.Result)
            {
                return workingHourAvailibility;
            }
            AvailabilityValidation availabilityValidation = new AvailabilityValidation(this.OrgService, this.TracingService);
            var responseMinimumday = availabilityValidation.checkMinimumReservationDuration(param, langId, channel);
            if (!responseMinimumday.ResponseResult.Result)
            {
                return responseMinimumday;
            }

            if (!string.IsNullOrEmpty(param.reservationId) &&
                new Guid(param.reservationId) != Guid.Empty)
            {
                this.Trace("checking broker reservation");
                var r = availabilityValidation.checkBrokerReservation(param, langId);
                if (!r.ResponseResult.Result)
                {
                    return r;
                }
            }

            //check minimum reservation day
            return new AvailabilityResponse
            {
                ResponseResult = ResponseResult.ReturnSuccess()
            };
        }

        public AvailabilityResponse CheckBranchsAvailibility(AvailabilityParameters param)
        {
            var message = string.Empty;

            if (param.pickupBranchId == null || param.pickupBranchId == Guid.Empty)
            {
                XrmHelper xrmHelper = new XrmHelper(this.OrgService, this.PluginInitializer.InitiatingUserId);
                message = xrmHelper.GetXmlTagContent(this.PluginInitializer.InitiatingUserId, "NullPickupBranchBranch");
                return new AvailabilityResponse
                {
                    ResponseResult = ResponseResult.ReturnError(message)
                };
            }
            else if (param.dropOffBranchId == null || param.dropOffBranchId == Guid.Empty)
            {
                XrmHelper xrmHelper = new XrmHelper(this.OrgService, this.PluginInitializer.InitiatingUserId);
                message = xrmHelper.GetXmlTagContent(this.PluginInitializer.InitiatingUserId, "NullDropoffBranchBranch");
                return new AvailabilityResponse
                {
                    ResponseResult = ResponseResult.ReturnError(message)
                };
            }

            return new AvailabilityResponse
            {
                ResponseResult = ResponseResult.ReturnSuccess()
            };
        }

        public AvailabilityResponse CheckDateTimesAvailibility(AvailabilityParameters param)
        {
            //var message = string.Empty;
            //BranchRepository branchRepository = new BranchRepository(this.OrgService);
            //var earlistPickupTime = branchRepository.getActiveBranchsByBranchIds(new object[] { param.pickupBranchId }).FirstOrDefault().earlistPickupTime;

            //if (earlistPickupTime.HasValue && param.pickupDateTime < DateTime.UtcNow.AddMinutes(StaticHelper.offset + earlistPickupTime.Value))
            //{
            //    XrmHelper xrmHelper = new XrmHelper(this.OrgService, this.PluginInitializer.InitiatingUserId);
            //    message = xrmHelper.GetXmlTagContent(this.PluginInitializer.InitiatingUserId, "InvalidPickupDateTime");
            //    return new AvailabilityResponse
            //    {
            //        ResponseResult = ResponseResult.ReturnError(message)
            //    };
            //}
            if (param.dropoffDateTime <= param.pickupDateTime)
            {
                XrmHelper xrmHelper = new XrmHelper(this.OrgService, this.PluginInitializer.InitiatingUserId);
                var message = xrmHelper.GetXmlTagContent(this.PluginInitializer.InitiatingUserId, "InvalidDropoffDateTime");
                return new AvailabilityResponse
                {
                    ResponseResult = ResponseResult.ReturnError(message)
                };
            }

            return new AvailabilityResponse
            {
                ResponseResult = ResponseResult.ReturnSuccess()
            };
        }

        public AvailabilityResponse CheckWorkingHourAvailability(AvailabilityParameters param)
        {

            WorkingHourValidation workingHourValidation = new WorkingHourValidation(this.OrgService, this.TracingService);
            //todo langId will read from param
            var message = workingHourValidation.checkBranchWorkingHour(param.pickupBranchId,
                                                                       param.dropOffBranchId,
                                                                       param.pickupDateTime,
                                                                       param.dropoffDateTime,
                                                                       1055,
                                                                       param.channel);

            if (!string.IsNullOrEmpty(message))
            {
                return new AvailabilityResponse
                {
                    ResponseResult = ResponseResult.ReturnError(message)
                };
            }
            return new AvailabilityResponse
            {
                ResponseResult = ResponseResult.ReturnSuccess()
            };
        }

        public CalculatePricesForUpdateContractResponse calculatePricesUpdateContract(AvailabilityResponse availabilityResponse, Guid relatedGroupCodeId, Guid contractId)
        {
            this.Trace("selected group code availability t");
            var calculatePricesForUpdateContractResponse = new CalculatePricesForUpdateContractResponse
            {
                amountobePaid = decimal.Zero,
                calculatedEquipmentPrice = decimal.Zero,
                documentEquipmentPrice = decimal.Zero,
                ratio = decimal.Zero,
                campaignInfo = null,
                documentHasCampaignBefore = false,
                canUserStillHasCampaignBenefit = false,
            };
            //todo will handle from business method
            var relatedGroupCodeAvailabilityInformation = availabilityResponse.availabilityData
                                                          .Where(p => p.groupCodeInformationId.Equals(relatedGroupCodeId))
                                                          .FirstOrDefault();

            this.Trace("selected group code availability" + JsonConvert.SerializeObject(relatedGroupCodeAvailabilityInformation));
            if (relatedGroupCodeAvailabilityInformation == null)
            {
                //todo will replace with config
                XrmHelper xrmHelper = new XrmHelper(this.OrgService);
                var message = xrmHelper.GetXmlTagContent(this.UserId, "RelatedGroupCodeAvailabilityInformationNull", this.availabilityXmlPath);
                calculatePricesForUpdateContractResponse.ResponseResult = ResponseResult.ReturnError(message);
            }
            else if (relatedGroupCodeAvailabilityInformation != null &&
                (relatedGroupCodeAvailabilityInformation.hasError || !relatedGroupCodeAvailabilityInformation.isPriceCalculatedSafely))
            {
                if (relatedGroupCodeAvailabilityInformation.hasError && !string.IsNullOrEmpty(relatedGroupCodeAvailabilityInformation.errorMessage))
                    calculatePricesForUpdateContractResponse.ResponseResult = ResponseResult.ReturnError(relatedGroupCodeAvailabilityInformation.errorMessage);
                else if (!relatedGroupCodeAvailabilityInformation.isPriceCalculatedSafely && !string.IsNullOrEmpty(relatedGroupCodeAvailabilityInformation.priceErrorMessage))
                    calculatePricesForUpdateContractResponse.ResponseResult = ResponseResult.ReturnError(relatedGroupCodeAvailabilityInformation.priceErrorMessage);
              
                else
                {
                    XrmHelper xrmHelper = new XrmHelper(this.OrgService);
                    var message = xrmHelper.GetXmlTagContent(this.UserId, "RelatedGroupCodeAvailabilityInformationNull", this.availabilityXmlPath);
                    calculatePricesForUpdateContractResponse.ResponseResult = ResponseResult.ReturnError(message);
                }
            }
            else
            {
                calculatePricesForUpdateContractResponse.paymentPlanData = relatedGroupCodeAvailabilityInformation.paymentPlanData;
                calculatePricesForUpdateContractResponse.operationType = relatedGroupCodeAvailabilityInformation.operationType;
                this.Trace("else part");
                this.Trace("relatedGroupCodeAvailabilityInformation.documentItemId.Value " + relatedGroupCodeAvailabilityInformation.documentItemId.Value);
                this.Trace("contractId " + contractId);

                ContractItemRepository contractItemRepository = new ContractItemRepository(this.OrgService);
                var sum = contractItemRepository.getSumEquipmentContractItems(relatedGroupCodeAvailabilityInformation.documentItemId.Value, contractId);
                this.Trace("sum : " + sum);

                calculatePricesForUpdateContractResponse.ResponseResult = ResponseResult.ReturnSuccess();
                //ödediğim tutar                    
                calculatePricesForUpdateContractResponse.documentEquipmentPrice = relatedGroupCodeAvailabilityInformation.documentEquipmentPrice + sum;
                //araç tutarı
                calculatePricesForUpdateContractResponse.calculatedEquipmentPrice = relatedGroupCodeAvailabilityInformation.totalPrice + sum;
                //ödenecek tutar
                calculatePricesForUpdateContractResponse.amountobePaid = calculatePricesForUpdateContractResponse.calculatedEquipmentPrice -
                                                                         calculatePricesForUpdateContractResponse.documentEquipmentPrice;
                calculatePricesForUpdateContractResponse.ratio = relatedGroupCodeAvailabilityInformation.ratio;
                calculatePricesForUpdateContractResponse.trackingNumber = availabilityResponse.trackingNumber;
                calculatePricesForUpdateContractResponse.campaignInfo = relatedGroupCodeAvailabilityInformation.CampaignInfo;
                calculatePricesForUpdateContractResponse.canUserStillHasCampaignBenefit = relatedGroupCodeAvailabilityInformation.canUserStillHasCampaignBenefit;
                calculatePricesForUpdateContractResponse.documentHasCampaignBefore = relatedGroupCodeAvailabilityInformation.documentHasCampaignBefore;

            }

            return calculatePricesForUpdateContractResponse;
        }

        public GetPriceCalculationSummariesResponse getPriceCalculationPriceSummaries(string trackingNumber,
                                                                                     string groupCodeInformationId,
                                                                                     string campaignId,
                                                                                     string pickupBranchId,
                                                                                     string documentId)
        {
            ConfigurationBL configurationBL = new ConfigurationBL(this.OrgService, this.TracingService);
            var responseUrl = configurationBL.GetConfigurationByName("MongoDBWepAPIURL");
            RestSharpHelper restSharpHelper = new RestSharpHelper(responseUrl, "getPriceCalculationSummaries", Method.POST);
            restSharpHelper.AddJsonParameter<GetPriceCalculationSummariesRequest>(new GetPriceCalculationSummariesRequest
            {
                campaignId = campaignId,
                groupCodeInformationId = groupCodeInformationId,
                trackingNumber = trackingNumber,
                pickupBranchId = pickupBranchId,
                documentId = documentId
            });
            return restSharpHelper.Execute<GetPriceCalculationSummariesResponse>();
        }

        public void processErrorMessages(AvailabilityResponse response, int langId)
        {
            this.handleAvailabilityDataErrorMessageForAvailabilityClosure(response, langId);
            this.handleAvailabilityDataErrorMessageForPriceCouldntCalculated(response, langId);
        }

        private void handleAvailabilityDataErrorMessageForAvailabilityClosure(AvailabilityResponse response, int langId)
        {
            try
            {

                var hasErrorData = response.availabilityData.Where(p => p.hasError == true).ToList();
                if (hasErrorData.Count > 0)
                {
                    XrmHelper xrmHelper = new XrmHelper(this.OrgService);
                    foreach (var item in hasErrorData)
                    {
                        this.Trace("handleAvailabilityDataErrorMessageForAvailabilityClosure before error found :" + item.errorMessage);
                        // get error message from xml
                        var message = xrmHelper.GetXmlTagContentByGivenLangId(item.errorMessage, langId);
                        this.Trace("handleAvailabilityDataErrorMessageForAvailabilityClosure after error found :" + item.errorMessage);
                        // change error message tag with real message
                        response.availabilityData.Where(x => x.groupCodeInformationId == item.groupCodeInformationId).FirstOrDefault().errorMessage = message;
                    }
                }
            }
            catch
            {
                //todo log will implement
            }
        }

        private void handleAvailabilityDataErrorMessageForPriceCouldntCalculated(AvailabilityResponse response, int langId)
        {
            try
            {
                var errorData = response.availabilityData.Where(p => p.isPriceCalculatedSafely == false).ToList();
                if (errorData.Count > 0)
                {
                    XrmHelper xrmHelper = new XrmHelper(this.OrgService);
                    foreach (var item in errorData)
                    {
                        this.Trace("handleAvailabilityDataErrorMessageForPriceCouldntCalculated before error found :" + item.errorMessage);
                        // get error message from xml
                        var message = xrmHelper.GetXmlTagContentByGivenLangId(item.priceErrorMessage, langId, this.availabilityXmlPath);
                        this.Trace("handleAvailabilityDataErrorMessageForPriceCouldntCalculated after error found :" + item.errorMessage);

                        // change error message tag with real message
                        response.availabilityData.Where(x => x.groupCodeInformationId == item.groupCodeInformationId).FirstOrDefault().priceErrorMessage = message;
                    }
                }
            }
            catch (Exception ex)
            {
            }
        }

        public void processPrices(AvailabilityResponse response, string reservationId, string contractId)
        {
            ReservationRepository reservationRepository = new ReservationRepository(this.OrgService);
            var res = reservationRepository.getReservationById(new Guid(reservationId), new string[] { "rnt_netpayment" });

            foreach (var item in response.availabilityData)
            {
                //this.Trace("process prices --> documentEquipmentPrice : " + item.documentEquipmentPrice);
                //var paidAmount = res.Attributes.Contains("rnt_paidamount") ? res.GetAttributeValue<Money>("rnt_paidamount").Value : decimal.Zero;
                //var refundAmount = res.Attributes.Contains("rnt_refundamount") ? res.GetAttributeValue<Money>("rnt_refundamount").Value : decimal.Zero;

                var netPayment = res.Attributes.Contains("rnt_netpayment") ? res.GetAttributeValue<Money>("rnt_netpayment").Value : decimal.Zero;
                if (netPayment >= decimal.Zero)
                {
                    item.paidPriceEquipment = item.documentEquipmentPrice;
                }
                item.totalPaidPrice = netPayment;
                item.amounttobePaid_Equipment = item.totalPrice - item.paidPriceEquipment;
                this.Trace("item groupCode " + item.groupCodeInformationName);
                this.Trace("item.totalPrice " + item.totalPrice);
                this.Trace("item.paidPriceEquipment " + item.paidPriceEquipment);
            }
        }
    }
}
