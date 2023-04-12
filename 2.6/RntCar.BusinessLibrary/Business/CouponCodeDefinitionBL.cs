using Microsoft.Xrm.Sdk;
using Newtonsoft.Json;
using RestSharp;
using RntCar.BusinessLibrary.Handlers;
using RntCar.BusinessLibrary.Repository;
using RntCar.ClassLibrary;
using RntCar.ClassLibrary.MongoDB;
using RntCar.SDK.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.BusinessLibrary.Business
{
    public class CouponCodeDefinitionBL : BusinessHandler
    {
        public CouponCodeDefinitionBL(IOrganizationService orgService) : base(orgService)
        {
        }

        public CouponCodeDefinitionBL(IOrganizationService orgService, ITracingService tracingService) : base(orgService, tracingService)
        {
        }

        public CouponCodeDefinitionBL(PluginInitializer initializer, IOrganizationService orgService, ITracingService tracingService) : base(initializer, orgService, tracingService)
        {
        }

        public MongoDBResponse createCouponCodeDefinitionInMongoDB(Entity couponCodeDefinition)
        {
            ConfigurationBL configurationBL = new ConfigurationBL(this.OrgService);

            var responseUrl = configurationBL.GetConfigurationByName("MongoDBWepAPIURL");

            var data = this.buildcouponCodeDefinitionData(couponCodeDefinition);

            this.TracingService.Trace("after build class");

            RestSharpHelper restSharpHelper = new RestSharpHelper(responseUrl, "createCouponCodeDefinitionInMongoDB", Method.POST);

            restSharpHelper.AddJsonParameter<CouponCodeDefinitionData>(data);

            var response = restSharpHelper.Execute<MongoDBResponse>();

            if (!response.Result)
            {
                return MongoDBResponse.ReturnError(response.ExceptionDetail);
            }

            return response;
        }

        public MongoDBResponse updateCouponCodeDefinitionInMongoDB(Entity couponCodeDefinition)
        {
            ConfigurationBL configurationBL = new ConfigurationBL(this.OrgService);

            var responseUrl = configurationBL.GetConfigurationByName("MongoDBWepAPIURL");

            var data = this.buildcouponCodeDefinitionData(couponCodeDefinition);

            this.TracingService.Trace("after build class");

            RestSharpHelper restSharpHelper = new RestSharpHelper(responseUrl, "updateCouponCodeDefinitionInMongoDB", Method.POST);

            restSharpHelper.AddJsonParameter<CouponCodeDefinitionData>(data);
            restSharpHelper.AddQueryParameter("id", couponCodeDefinition.GetAttributeValue<string>("rnt_mongodbid"));

            var response = restSharpHelper.Execute<MongoDBResponse>();

            if (!response.Result)
            {
                return MongoDBResponse.ReturnError(response.ExceptionDetail);
            }

            return response;
        }

        public MongoDBResponse createCouponCodeListInMongoDB(Entity couponCodeDefinition)
        {
            ConfigurationBL configurationBL = new ConfigurationBL(this.OrgService);

            var responseUrl = configurationBL.GetConfigurationByName("MongoDBWepAPIURL");

            var data = new CouponCodeParameter
            {
                couponCodeDefinitionId = couponCodeDefinition.Id.ToString(),
                couponCodes = new List<string>()
            };
            this.TracingService.Trace("after build class");

            var numberOfCodes = couponCodeDefinition.GetAttributeValue<int>("rnt_numberofcoupons");

            var isUnique = couponCodeDefinition.GetAttributeValue<bool>("rnt_isunique");

            var isManuelCouponCode = couponCodeDefinition.GetAttributeValue<bool>("rnt_entermanuelcouponcode");

            var couponCode = string.Empty;
            if (isManuelCouponCode)
                couponCode = couponCodeDefinition.GetAttributeValue<string>("rnt_couponcode");
            else
                couponCode = CommonHelper.couponCodeGenerator(8);

            for (int i = 0; i < numberOfCodes; i++)
            {
                if (!isUnique)
                {
                    data.couponCodes.Add(couponCode);
                }
                else
                {
                    data.couponCodes.Add(CommonHelper.couponCodeGenerator(8));
                }
            }
            this.TracingService.Trace("after generate coupon codes");

            RestSharpHelper restSharpHelper = new RestSharpHelper(responseUrl, "bulkCreateCouponCodeInMongoDB", Method.POST);

            restSharpHelper.AddJsonParameter<CouponCodeParameter>(data);

            var response = restSharpHelper.Execute<MongoDBResponse>();

            if (!response.Result)
            {
                return MongoDBResponse.ReturnError(response.ExceptionDetail);
            }

            return response;
        }

        public MongoDBResponse addCouponCodeListInMongoDB(Entity couponCodeDefinition)
        {
            ConfigurationBL configurationBL = new ConfigurationBL(this.OrgService);

            var responseUrl = configurationBL.GetConfigurationByName("MongoDBWepAPIURL");

            var data = new CouponCodeParameter
            {
                couponCodeDefinitionId = couponCodeDefinition.Id.ToString(),
                couponCodes = new List<string>()
            };
            this.TracingService.Trace("after build class");

            var numberOfCodes = couponCodeDefinition.GetAttributeValue<int>("rnt_numberofcoupons");
            var additionalNumberOfCodes = couponCodeDefinition.GetAttributeValue<int>("rnt_additionalnumberofcoupons");

            var isUnique = couponCodeDefinition.GetAttributeValue<bool>("rnt_isunique");

            var isManuelCouponCode = couponCodeDefinition.GetAttributeValue<bool>("rnt_entermanuelcouponcode");

            var couponCode = string.Empty;
            if (isManuelCouponCode)
                couponCode = couponCodeDefinition.GetAttributeValue<string>("rnt_couponcode");
            else
                couponCode = CommonHelper.couponCodeGenerator(8);

            for (int i = 0; i < additionalNumberOfCodes; i++)
            {
                if (!isUnique)
                {
                    data.couponCodes.Add(couponCode);
                }
                else
                {
                    data.couponCodes.Add(CommonHelper.couponCodeGenerator(8));
                }
            }
            this.TracingService.Trace("after generate coupon codes");

            RestSharpHelper restSharpHelper = new RestSharpHelper(responseUrl, "bulkCreateCouponCodeInMongoDB", Method.POST);

            restSharpHelper.AddJsonParameter<CouponCodeParameter>(data);

            var response = restSharpHelper.Execute<MongoDBResponse>();

            if (!response.Result)
            {
                return MongoDBResponse.ReturnError(response.ExceptionDetail);
            }

            Entity updateCouponCodeDef = new Entity(couponCodeDefinition.LogicalName, couponCodeDefinition.Id);
            updateCouponCodeDef["rnt_additionalnumberofcoupons"] = null;
            updateCouponCodeDef["rnt_numberofcoupons"] = numberOfCodes + additionalNumberOfCodes;
            OrgService.Update(updateCouponCodeDef);

            return response;
        }

        public CouponCodeDefinitionResponse getCouponCodeDefinitionByCouponCode(string couponCode)
        {
            CouponCodeBL couponCodeBL = new CouponCodeBL(this.OrgService, this.TracingService);

            var couponCodeDetailsResponse = couponCodeBL.getCouponCodeDetailsByCouponCode(couponCode);

            if (couponCodeDetailsResponse.ResponseResult.Result)
            {
                var couponCodeDetails = couponCodeDetailsResponse.couponCodeList.FirstOrDefault();

                CouponCodeDefinitionRepository couponCodeDefinitionRepository = new CouponCodeDefinitionRepository(this.OrgService);
                var couponCodeDefinition = couponCodeDefinitionRepository.getCouponCodeDefinitionById(Guid.Parse(couponCodeDetails.couponCodeDefinitionId));

                return new CouponCodeDefinitionResponse
                {
                    ResponseResult = ResponseResult.ReturnSuccess(),
                    CouponCodeDefinition = this.buildcouponCodeDefinitionData(couponCodeDefinition)
                };
            }
            return new CouponCodeDefinitionResponse
            {
                ResponseResult = ResponseResult.ReturnError(couponCodeDetailsResponse.ResponseResult.ExceptionDetail)
            };
        }

        public CouponCodeDefinitionData buildcouponCodeDefinitionData(Entity couponCodeDefinition)
        {
            MultiSelectMappingRepository multiSelectRepository = new MultiSelectMappingRepository(this.OrgService);

            var branchCode = multiSelectRepository.getSelectedItemsGuidListByOptionValueandGivenColumn("rnt_branch",
                                couponCodeDefinition.Attributes.Contains("rnt_branchcodes") ? couponCodeDefinition.GetAttributeValue<OptionSetValueCollection>("rnt_branchcodes") : null);

            var groupCodes = multiSelectRepository.getSelectedItemsGuidListByOptionValueandGivenColumn("rnt_groupcode",
                                couponCodeDefinition.Attributes.Contains("rnt_groupcodeinformations") ? couponCodeDefinition.GetAttributeValue<OptionSetValueCollection>("rnt_groupcodeinformations") : null);

            var channels = couponCodeDefinition.GetAttributeValue<OptionSetValueCollection>("rnt_channelcodes");

            CouponCodeDefinitionData couponCodeDefinitionData = new CouponCodeDefinitionData
            {
                couponCodeDefinitionId = Convert.ToString(couponCodeDefinition.Id),
                branchCodes = JsonConvert.SerializeObject(branchCode),
                groupCodeInformations = JsonConvert.SerializeObject(groupCodes),
                channelCodes = JsonConvert.SerializeObject(channels),
                stateCode = couponCodeDefinition.GetAttributeValue<OptionSetValue>("statecode").Value,
                statusCode = couponCodeDefinition.GetAttributeValue<OptionSetValue>("statuscode").Value,
                startDate = couponCodeDefinition.GetAttributeValue<DateTime>("rnt_startdate"),
                endDate = couponCodeDefinition.GetAttributeValue<DateTime>("rnt_enddate"),
                validityStartDate = couponCodeDefinition.GetAttributeValue<DateTime>("rnt_validitystartdate"),
                validityEndDate = couponCodeDefinition.GetAttributeValue<DateTime>("rnt_validityenddate"),
                isUnique = couponCodeDefinition.GetAttributeValue<bool>("rnt_isunique"),
                name = couponCodeDefinition.GetAttributeValue<string>("rnt_name"),
                numberOfCoupons = couponCodeDefinition.GetAttributeValue<int>("rnt_numberofcoupons"),
                paylaterdiscountvalue = couponCodeDefinition.GetAttributeValue<decimal>("rnt_paylaterdiscountvalue"),
                paynowdiscountvalue = couponCodeDefinition.GetAttributeValue<decimal>("rnt_paynowdiscountvalue"),
                createdon = couponCodeDefinition.GetAttributeValue<DateTime>("createdon"),
                modifiedon = couponCodeDefinition.GetAttributeValue<DateTime>("modifiedon"),
                couponCode = couponCodeDefinition.GetAttributeValue<string>("rnt_couponcode"),
                enterManuelCouponCode = couponCodeDefinition.Attributes.Contains("rnt_entermanuelcouponcode") ? couponCodeDefinition.GetAttributeValue<bool>("rnt_entermanuelcouponcode") : false,
            };

            return couponCodeDefinitionData;
        }
    }
}
