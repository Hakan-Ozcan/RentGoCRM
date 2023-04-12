using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Tooling.Connector;
using RestSharp;
using RntCar.BusinessLibrary.Handlers;
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
    public class PriceHourEffectBL : BusinessHandler
    {
        public PriceHourEffectBL(IOrganizationService orgService) : base(orgService)
        {
        }

        public PriceHourEffectBL(CrmServiceClient crmServiceClient) : base(crmServiceClient)
        {
        }

        public PriceHourEffectBL(IOrganizationService orgService, CrmServiceClient crmServiceClient) : base(orgService, crmServiceClient)
        {
        }

        public PriceHourEffectBL(IOrganizationService orgService, ITracingService tracingService) : base(orgService, tracingService)
        {
        }

        public PriceHourEffectBL(IOrganizationService orgService, Guid userId) : base(orgService, userId)
        {
        }

        public PriceHourEffectBL(PluginInitializer initializer, IOrganizationService orgService, ITracingService tracingService) : base(initializer, orgService, tracingService)
        {
        }

        public PriceHourEffectBL(IOrganizationService orgService, ITracingService tracingService, CrmServiceClient crmServiceClient) : base(orgService, tracingService, crmServiceClient)
        {
        }

        public PriceHourEffectBL(IOrganizationService orgService, Guid userId, string orgName) : base(orgService, userId, orgName)
        {
        }
        public MongoDBResponse createPriceHourEffectInMongoDB(Entity priceHourEffect)
        {
            ConfigurationBL configurationBL = new ConfigurationBL(this.OrgService);

            var responseUrl = configurationBL.GetConfigurationByName("MongoDBWepAPIURL");

            RestSharpHelper restSharpHelper = new RestSharpHelper(responseUrl, "CreatePriceHourEffectInMongoDB", Method.POST);
            this.Trace("before buildPriceHourEffectData");
            this.Trace("priceHourEffect.GetAttributeValue<DateTime>(createdon)" + priceHourEffect.GetAttributeValue<DateTime>("createdon"));
            var priceHourEffectData = this.buildPriceHourEffectData(priceHourEffect);
            this.Trace("after buildPriceHourEffectData");
            restSharpHelper.AddJsonParameter<PriceHourEffectData>(priceHourEffectData);

            var response = restSharpHelper.Execute<MongoDBResponse>();

            if (!response.Result)
            {
                return MongoDBResponse.ReturnError(response.ExceptionDetail);
            }

            return response;
        }
        public MongoDBResponse updatePriceHourEffectInMongoDB(Entity priceHourEffect)
        {
            ConfigurationBL configurationBL = new ConfigurationBL(this.OrgService);

            var responseUrl = configurationBL.GetConfigurationByName("MongoDBWepAPIURL");

            RestSharpHelper restSharpHelper = new RestSharpHelper(responseUrl, "UpdatePriceHourEffectInMongoDB", Method.POST);
            this.Trace("before buildPriceHourEffectData");
            
            var priceHourEffectData = this.buildPriceHourEffectData(priceHourEffect);
            this.Trace("after buildPriceHourEffectData");
            restSharpHelper.AddJsonParameter<PriceHourEffectData>(priceHourEffectData);
            restSharpHelper.AddQueryParameter("id", priceHourEffect.GetAttributeValue<string>("rnt_mongodbid"));

            var response = restSharpHelper.Execute<MongoDBResponse>();

            if (!response.Result)
            {
                return MongoDBResponse.ReturnError(response.ExceptionDetail);
            }

            return response;
        }
        public PriceHourEffectData buildPriceHourEffectData(Entity priceHourEffect)
        {
            PriceHourEffectData priceHourEffectData = new PriceHourEffectData
            {                
                createdon = priceHourEffect.GetAttributeValue<DateTime>("createdon"),
                modifiedon = priceHourEffect.GetAttributeValue<DateTime>("modifiedon"),
                statuscode = priceHourEffect.GetAttributeValue<OptionSetValue>("statuscode").Value,
                statecode = priceHourEffect.GetAttributeValue<OptionSetValue>("statecode").Value,
                effectRate = priceHourEffect.GetAttributeValue<int>("rnt_effectrate"),
                minimumMinute = priceHourEffect.GetAttributeValue<int>("rnt_minimumminute"),
                maximumMinute = priceHourEffect.GetAttributeValue<int>("rnt_maximumminute"),
                priceHourEffectId = priceHourEffect.Id.ToString()
            };
            return priceHourEffectData;
        }
    }
}
