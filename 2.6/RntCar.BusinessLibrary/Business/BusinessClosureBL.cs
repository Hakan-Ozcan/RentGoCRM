using Microsoft.Xrm.Sdk;
using Newtonsoft.Json;
using RestSharp;
using RntCar.BusinessLibrary.Handlers;
using RntCar.BusinessLibrary.Repository;
using RntCar.ClassLibrary.MongoDB;
using RntCar.SDK.Common;
using System;

namespace RntCar.BusinessLibrary.Business
{
    public class BusinessClosureBL : BusinessHandler
    {
        public BusinessClosureBL(IOrganizationService orgService) : base(orgService)
        {

        }

        public BusinessClosureBL(IOrganizationService orgService, ITracingService tracingService) : base(orgService, tracingService)
        {
        }

        public MongoDBResponse createBusinessClosureInMongoDB(Entity businessClosure)
        {
            ConfigurationBL configurationBL = new ConfigurationBL(this.OrgService);

            var responseUrl = configurationBL.GetConfigurationByName("MongoDBWepAPIURL");
            RestSharpHelper restSharpHelper = new RestSharpHelper(responseUrl, "CreateBusinessClosureInMongoDB", Method.POST);

            var businessClosureData = this.buildBusinessClosureData(businessClosure);
            restSharpHelper.AddJsonParameter<RntCar.ClassLibrary.MongoDB.BusinessClosure.BusinessClosureData>(businessClosureData);

            var response = restSharpHelper.Execute<MongoDBResponse>();

            if (!response.Result)
            {
                return MongoDBResponse.ReturnError(response.ExceptionDetail);
            }

            return response;
        }
        public MongoDBResponse updateBusinessClosureInMongoDB(Entity businessClosure)
        {
            ConfigurationBL configurationBL = new ConfigurationBL(this.OrgService);

            var responseUrl = configurationBL.GetConfigurationByName("MongoDBWepAPIURL");
            RestSharpHelper restSharpHelper = new RestSharpHelper(responseUrl, "UpdateBusinessClosureInMongoDB", Method.POST);

            var businessClosureData = this.buildBusinessClosureData(businessClosure);
            restSharpHelper.AddJsonParameter<RntCar.ClassLibrary.MongoDB.BusinessClosure.BusinessClosureData>(businessClosureData);
            restSharpHelper.AddQueryParameter("id", Convert.ToString(businessClosure.Id));

            var response = restSharpHelper.Execute<MongoDBResponse>();

            if (!response.Result)
            {
                return MongoDBResponse.ReturnError(response.ExceptionDetail);
            }

            return response;
        }
        private RntCar.ClassLibrary.MongoDB.BusinessClosure.BusinessClosureData buildBusinessClosureData(Entity businessClosure)
        {
            MultiSelectMappingRepository multiSelectRepository = new MultiSelectMappingRepository(this.OrgService);
            var branchCode = multiSelectRepository.getSelectedItemsGuidListByOptionValueandGivenColumn("rnt_branch",
               businessClosure.Attributes.Contains("rnt_branchcode") ? businessClosure.GetAttributeValue<OptionSetValueCollection>("rnt_branchcode") : null);

            RntCar.ClassLibrary.MongoDB.BusinessClosure.BusinessClosureData businessClosureData = new RntCar.ClassLibrary.MongoDB.BusinessClosure.BusinessClosureData
            {
                
                businessClosureId = Convert.ToString(businessClosure.Id),
                beginDate = businessClosure.GetAttributeValue<DateTime>("rnt_begindate"),
                beginDateTimestamp = businessClosure.GetAttributeValue<DateTime>("rnt_begindate").converttoTimeStamp(),
                endDate = businessClosure.GetAttributeValue<DateTime>("rnt_endate"),
                endDateTimestamp = businessClosure.GetAttributeValue<DateTime>("rnt_endate").converttoTimeStamp(),
                name = businessClosure.Attributes.Contains("rnt_name") ? businessClosure.GetAttributeValue<string>("rnt_name") : string.Empty,                
                statuscode = businessClosure.GetAttributeValue<OptionSetValue>("statuscode").Value,
                statecode = businessClosure.GetAttributeValue<OptionSetValue>("statecode").Value,
                branchValues = JsonConvert.SerializeObject(branchCode)
            };

            return businessClosureData;
        }
    }

}
