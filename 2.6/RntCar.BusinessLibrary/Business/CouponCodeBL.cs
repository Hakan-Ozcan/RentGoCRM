using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Newtonsoft.Json;
using RestSharp;
using RntCar.BusinessLibrary.Handlers;
using RntCar.BusinessLibrary.Repository;
using RntCar.BusinessLibrary.Validations;
using RntCar.ClassLibrary;
using RntCar.ClassLibrary.MongoDB;
using RntCar.ClassLibrary.odata;
using RntCar.SDK.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.BusinessLibrary.Business
{
    public class CouponCodeBL : BusinessHandler
    {
        public CouponCodeBL(IOrganizationService orgService) : base(orgService)
        {
        }

        public CouponCodeBL(IOrganizationService orgService, ITracingService tracingService) : base(orgService, tracingService)
        {
        }

        public CouponCodeBL(PluginInitializer initializer, IOrganizationService orgService, ITracingService tracingService) : base(initializer, orgService, tracingService)
        {
        }

        public MongoDBResponse updateCouponCodeInMongoDB(CouponCodeData couponCodeData)
        {
            this.Trace("updateCouponCodeInMongoDB");
            ConfigurationBL configurationBL = new ConfigurationBL(this.OrgService);

            var responseUrl = configurationBL.GetConfigurationByName("MongoDBWepAPIURL");

            this.Trace("responseUrl" + responseUrl);

            RestSharpHelper restSharpHelper = new RestSharpHelper(responseUrl, "updateCouponCodeInMongoDB", Method.POST);

            this.Trace("after build class");
            restSharpHelper.AddJsonParameter<CouponCodeData>(couponCodeData);
            restSharpHelper.AddQueryParameter("id", couponCodeData.mongoId);

            var response = restSharpHelper.Execute<MongoDBResponse>();

            if (!response.Result)
            {
                return MongoDBResponse.ReturnError(response.ExceptionDetail);
            }

            return response;
        }

        public EntityCollection getCouponCodes(QueryExpression queryExpression)
        {
            this.Trace("lets start");
            var conditions = queryExpression.Criteria.Conditions;

            EntityCollection results = new EntityCollection();
            ConfigurationBL configurationBL = new ConfigurationBL(this.OrgService);
            //todo place in business classes
            var responseUrl = configurationBL.GetConfigurationByName("MongoDBodataURL");
            RestSharpHelper helper = new RestSharpHelper(responseUrl, "getCouponCodes", Method.GET);

            if (conditions.Count > 0 &&
                conditions.FirstOrDefault()?.AttributeName == "rnt_reservationid" &&
                conditions.FirstOrDefault()?.Operator == ConditionOperator.Equal)
            {
                this.Trace("condition reservation " + conditions.FirstOrDefault()?.Values.FirstOrDefault());
                helper = new RestSharpHelper(responseUrl, "getCouponCodesByReservationId", Method.GET);
                helper.AddQueryParameter("reservationId", Convert.ToString(conditions.FirstOrDefault()?.Values.FirstOrDefault()));
            }
            else if (conditions.Count > 0 &&
                conditions.FirstOrDefault()?.AttributeName == "rnt_contractid" &&
                conditions.FirstOrDefault()?.Operator == ConditionOperator.Equal)
            {
                this.Trace("condition contract " + conditions.FirstOrDefault()?.Values.FirstOrDefault());
                helper = new RestSharpHelper(responseUrl, "getCouponCodesByContractId", Method.GET);
                helper.AddQueryParameter("contractId", Convert.ToString(conditions.FirstOrDefault()?.Values.FirstOrDefault()));
            }
            else if (conditions.Count > 0 &&
                conditions.FirstOrDefault()?.AttributeName == "rnt_couponcodedefinitionid" &&
                conditions.FirstOrDefault()?.Operator == ConditionOperator.Equal)
            {
                this.Trace("condition definition " + conditions.FirstOrDefault()?.Values.FirstOrDefault());
                helper = new RestSharpHelper(responseUrl, "getCouponCodesByDefinitionId", Method.GET);
                helper.AddQueryParameter("couponCodeDefinitionId", Convert.ToString(conditions.FirstOrDefault()?.Values.FirstOrDefault()));
            }
            else if (conditions.Count > 0 &&
                conditions.FirstOrDefault()?.AttributeName == "rnt_accountid" &&
                conditions.FirstOrDefault()?.Operator == ConditionOperator.Equal)
            {
                this.Trace("condition account " + conditions.FirstOrDefault()?.Values.FirstOrDefault());
                helper = new RestSharpHelper(responseUrl, "getCouponCodesByAccountId", Method.GET);
                helper.AddQueryParameter("accountId", Convert.ToString(conditions.FirstOrDefault()?.Values.FirstOrDefault()));
            }
            else if (conditions.Count > 0 &&
                conditions.FirstOrDefault()?.AttributeName == "rnt_contactid" &&
                conditions.FirstOrDefault()?.Operator == ConditionOperator.Equal)
            {
                this.Trace("condition contact " + conditions.FirstOrDefault()?.Values.FirstOrDefault());
                helper = new RestSharpHelper(responseUrl, "getCouponCodesByContactId", Method.GET);
                helper.AddQueryParameter("contactId", Convert.ToString(conditions.FirstOrDefault()?.Values.FirstOrDefault()));
            }

            var response = helper.Execute();
            var v = JsonConvert.DeserializeObject<CouponCodeWrapper>(response.Content);
            results.EntityName = "rnt_couponcode";

            foreach (var item in v.value)
            {
                Entity e = new Entity("rnt_couponcode");
                e["rnt_couponcodeid"] = item.couponCodeId;
                //e["rnt_name"] = item.trackingNumber;
                if (!string.IsNullOrEmpty(item.accountId))
                    e["rnt_accountid"] = new EntityReference("account", Guid.Parse(item.accountId));
                if (!string.IsNullOrEmpty(item.contactId))
                    e["rnt_contactid"] = new EntityReference("contact", Guid.Parse(item.contactId));
                if (!string.IsNullOrEmpty(item.contractId))
                    e["rnt_contractid"] = new EntityReference("rnt_contract", Guid.Parse(item.contractId));
                if (!string.IsNullOrEmpty(item.reservationId))
                    e["rnt_reservationid"] = new EntityReference("rnt_reservation", Guid.Parse(item.reservationId));

                e["rnt_couponcodedefinitionid"] = new EntityReference("rnt_couponcodedefinition", Guid.Parse(item.couponCodeDefinitionId));
                e["rnt_statuscode"] = new OptionSetValue(item.statusCode);
                e["rnt_couponcode"] = item.couponCode;

                results.Entities.Add(e);
            }
            this.Trace(JsonConvert.SerializeObject(v.value));

            return results;
        }

        public CouponCodeResponse getAvailableCouponCodeDetailsByCouponCode(string couponCode)
        {
            ConfigurationBL configurationBL = new ConfigurationBL(this.OrgService);

            var responseUrl = configurationBL.GetConfigurationByName("MongoDBWepAPIURL");

            this.Trace("responseUrl:" + responseUrl);

            this.Trace("after build class");

            RestSharpHelper restSharpHelper = new RestSharpHelper(responseUrl, "getAvailableCouponCodeDetailsByCouponCode", Method.POST);

            restSharpHelper.AddQueryParameter("couponCode", couponCode);

            var response = restSharpHelper.Execute<CouponCodeResponse>();

            return response;
        }

        public CouponCodeResponse getCouponCodeDetailsByCouponCode(string couponCode)
        {
            ConfigurationBL configurationBL = new ConfigurationBL(this.OrgService);

            var responseUrl = configurationBL.GetConfigurationByName("MongoDBWepAPIURL");

            this.Trace("after build class");

            RestSharpHelper restSharpHelper = new RestSharpHelper(responseUrl, "getCouponCodeDetailsByCouponCode", Method.POST);

            restSharpHelper.AddQueryParameter("couponCode", couponCode);

            var response = restSharpHelper.Execute<CouponCodeResponse>();

            return response;
        }

        public CouponCodeOperationsResponse executeCouponCodeOperations(CouponCodeOperationsParameter parameter, bool updateCouponCode = false, int? langId = 1033)
        {
            try
            {
                if (langId.HasValue && langId.Value == 0)
                {
                    langId = 1033;
                }
                // set definition values as zero 
                var definitionPayLaterDiscountValue = decimal.Zero;
                var definitionPayNowDiscountValue = decimal.Zero;
                var definitionType = 0;
                var definitionName = string.Empty;

                var couponCodeResponse = this.getCouponCodeDetailsByCouponCode(parameter.couponCode);

                if (couponCodeResponse == null)
                {
                    XrmHelper xrmHelper = new XrmHelper(this.OrgService);
                    var message = xrmHelper.GetXmlTagContentByGivenLangId("CouponCodeIsNotFound", langId, this.couponCodeXmlPath);
                    return new CouponCodeOperationsResponse { ResponseResult = ResponseResult.ReturnError(message) };
                }
                if (!couponCodeResponse.ResponseResult.Result)
                {
                    return new CouponCodeOperationsResponse { ResponseResult = ResponseResult.ReturnError(couponCodeResponse.ResponseResult.ExceptionDetail) };
                }

                if (!(couponCodeResponse.couponCodeList.Count > 0))
                {
                    XrmHelper xrmHelper = new XrmHelper(this.OrgService);
                    var message = xrmHelper.GetXmlTagContentByGivenLangId("CouponCodeIsNotFound", langId, this.couponCodeXmlPath);
                    return new CouponCodeOperationsResponse { ResponseResult = ResponseResult.ReturnError(message) };
                }

                var couponCode = couponCodeResponse.couponCodeList.Where(x => x.statusCode == (int)GlobalEnums.CouponCodeStatusCode.Generated).FirstOrDefault();
                if (couponCode == null || string.IsNullOrWhiteSpace(couponCode.couponCode))
                {
                    couponCode = couponCodeResponse.couponCodeList.FirstOrDefault();
                }
                this.Trace("couponCodeResponse : " + JsonConvert.SerializeObject(couponCodeResponse));

                this.Trace("couponCode.statusCode : " + couponCode.statusCode);

                if (couponCode.statusCode == (int)GlobalEnums.CouponCodeStatusCode.Used)
                {
                    // check list for not unique coupon codes
                    couponCode = couponCodeResponse.couponCodeList.Where(item => item.statusCode != (int)GlobalEnums.CouponCodeStatusCode.Used).FirstOrDefault();
                    if (couponCode == null)
                    {
                        XrmHelper xrmHelper = new XrmHelper(this.OrgService);
                        var message = xrmHelper.GetXmlTagContentByGivenLangId("CouponCodeUsed", langId, this.couponCodeXmlPath);
                        return new CouponCodeOperationsResponse
                        {
                            ResponseResult = ResponseResult.ReturnError(message)
                        };
                    }
                }
                else if (couponCode.statusCode == (int)GlobalEnums.CouponCodeStatusCode.Burned)
                {
                    XrmHelper xrmHelper = new XrmHelper(this.OrgService);
                    var message = xrmHelper.GetXmlTagContentByGivenLangId("CouponInvalidDate", langId, this.couponCodeXmlPath);
                    return new CouponCodeOperationsResponse
                    {
                        ResponseResult = ResponseResult.ReturnError(message)
                    };
                }

                #region Validations
                CouponCodeDefinitionValidations couponCodeDefinitionValidations = new CouponCodeDefinitionValidations(this.OrgService, this.TracingService);
                CouponCodeDefinitionRepository couponCodeDefinitionRepository = new CouponCodeDefinitionRepository(this.OrgService);
                var definition = couponCodeDefinitionRepository.getCouponCodeDefinitionById(Guid.Parse(couponCode.couponCodeDefinitionId));
                this.Trace("definition : " + JsonConvert.SerializeObject(definition));

                int validStatus = 0;
                var validationResponse = couponCodeDefinitionValidations.checkCuponCodeValidationDate(definition, out validStatus);
                this.Trace("checkCuponCodeValidationDate : " + validationResponse);
                if (!validationResponse)
                {
                    var message = string.Empty;
                    XrmHelper xrmHelper = new XrmHelper(this.OrgService);

                    if (validStatus == 1)
                    {
                        CouponCodeData couponCodeData = new CouponCodeData()
                        {
                            couponCode = couponCode.couponCode,
                            mongoId = couponCode.mongoId,
                            couponCodeDefinitionId = couponCode.couponCodeDefinitionId,
                            statusCode = (int)GlobalEnums.CouponCodeStatusCode.Burned
                        };

                        this.updateCouponCodeInMongoDB(couponCodeData);

                        message = xrmHelper.GetXmlTagContentByGivenLangId("CouponInvalidDate", langId, this.couponCodeXmlPath);
                    }
                    else if (validStatus == 2)
                    {
                        message = xrmHelper.GetXmlTagContentByGivenLangId("CouponEarlyDate", langId, this.couponCodeXmlPath);
                        var validityStartDate = definition.GetAttributeValue<DateTime>("rnt_validitystartdate").AddHours(3);
                        message = string.Format(message, validityStartDate.ToString("dd-MM-yyyy"));
                    }
                    else
                    {
                        message = xrmHelper.GetXmlTagContentByGivenLangId("CouponInvalidDate", langId, this.couponCodeXmlPath);
                    }
                    return new CouponCodeOperationsResponse
                    {
                        ResponseResult = ResponseResult.ReturnError(message)
                    };
                }

                validationResponse = couponCodeDefinitionValidations.checkCuponCodeDefinitionChannel(definition, parameter.reservationChannelCode);
                this.Trace("checkCuponCodeDefinitionChannel : " + validationResponse);
                if (!validationResponse)
                {
                    XrmHelper xrmHelper = new XrmHelper(this.OrgService);
                    var message = xrmHelper.GetXmlTagContentByGivenLangId("CouponCodeChannel", langId, this.couponCodeXmlPath);
                    return new CouponCodeOperationsResponse
                    {
                        ResponseResult = ResponseResult.ReturnError(message)
                    };
                }

                validationResponse = couponCodeDefinitionValidations.checkCuponCodeDefinitionReservationType(definition, parameter.reservationTypeCode);
                this.Trace("checkCuponCodeDefinitionReservationType : " + validationResponse);
                if (!validationResponse)
                {
                    XrmHelper xrmHelper = new XrmHelper(this.OrgService);
                    var message = xrmHelper.GetXmlTagContentByGivenLangId("CouponCodeReservationType", langId, this.couponCodeXmlPath);
                    return new CouponCodeOperationsResponse
                    {
                        ResponseResult = ResponseResult.ReturnError(message)
                    };
                }

                validationResponse = couponCodeDefinitionValidations.checkCuponCodeDefinitionGroupCodes(definition, parameter.groupCodeInformationId);
                this.Trace("checkCuponCodeDefinitionGroupCodes : " + validationResponse);
                if (!validationResponse)
                {
                    XrmHelper xrmHelper = new XrmHelper(this.OrgService);
                    var message = xrmHelper.GetXmlTagContentByGivenLangId("CouponCodeGroupCode", langId, this.couponCodeXmlPath);
                    return new CouponCodeOperationsResponse
                    {
                        ResponseResult = ResponseResult.ReturnError(message)
                    };
                }

                validationResponse = couponCodeDefinitionValidations.checkCuponCodeDefinitionBranchs(definition, parameter.pickupBranchId);
                this.Trace("checkCuponCodeDefinitionBranchs : " + validationResponse);
                if (!validationResponse)
                {
                    XrmHelper xrmHelper = new XrmHelper(this.OrgService);
                    var message = xrmHelper.GetXmlTagContentByGivenLangId("CouponCodePickupBranch", langId, this.couponCodeXmlPath);
                    return new CouponCodeOperationsResponse
                    {
                        ResponseResult = ResponseResult.ReturnError(message)
                    };
                }

                #endregion Validations

                this.Trace("retrieve coupon code details");


                definitionPayLaterDiscountValue = definition.GetAttributeValue<decimal>("rnt_paylaterdiscountvalue");
                definitionPayNowDiscountValue = definition.GetAttributeValue<decimal>("rnt_paynowdiscountvalue");
                definitionType = definition.GetAttributeValue<OptionSetValue>("rnt_type").Value;
                definitionName = definition.GetAttributeValue<string>("rnt_name");

                this.Trace("updateCouponCode : " + updateCouponCode);
                if (updateCouponCode)
                {
                    if (parameter.statusCode.HasValue)
                    {
                        if (parameter.statusCode == (int)GlobalEnums.CouponCodeStatusCode.Used)
                        {
                            this.Trace("retrieve coupon code definition : " + definition.Id);

                            if (!string.IsNullOrEmpty(parameter.contactId))
                                couponCode.contactId = parameter.contactId;
                            if (!string.IsNullOrEmpty(parameter.accountId))
                                couponCode.accountId = parameter.accountId;
                        }
                        else if (parameter.statusCode == (int)GlobalEnums.CouponCodeStatusCode.Burned)
                        {
                            // if coupon code released remove account and contact from coupon code
                            couponCode.accountId = string.Empty;
                            couponCode.contactId = string.Empty;
                        }

                        couponCode.statusCode = parameter.statusCode.Value;
                    }
                    if (!string.IsNullOrEmpty(parameter.reservationId))
                        couponCode.reservationId = parameter.reservationId;
                    if (!string.IsNullOrEmpty(parameter.contractId))
                        couponCode.contractId = parameter.contractId;

                    this.updateCouponCodeInMongoDB(couponCode);
                }

                return new CouponCodeOperationsResponse
                {
                    couponCodeData = couponCode,
                    couponCode = parameter.couponCode,
                    definitionType = definitionType,
                    definitionPayLaterDiscountValue = definitionPayLaterDiscountValue,
                    definitionPayNowDiscountValue = definitionPayNowDiscountValue,
                    definitionName = definitionName,
                    ResponseResult = ResponseResult.ReturnSuccess()
                };
            }
            catch (Exception ex)
            {
                return new CouponCodeOperationsResponse { ResponseResult = ResponseResult.ReturnError(ex.Message) };
            }
        }
    }
}
