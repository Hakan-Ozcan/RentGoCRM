using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Tooling.Connector;
using RntCar.BusinessLibrary.Handlers;
using RntCar.BusinessLibrary.Repository;
using RntCar.ClassLibrary;
using RntCar.ClassLibrary.MongoDB;
using RntCar.SDK.Common;

namespace RntCar.BusinessLibrary.Business
{
    public class ConfigurationBL : BusinessHandler
    {
        public ConfigurationBL() : base()
        {

        }
        public ConfigurationBL(IOrganizationService orgService) : base(orgService)
        {
        }

        public ConfigurationBL(CrmServiceClient crmServiceClient) : base(crmServiceClient)
        {
        }

        public ConfigurationBL(GlobalEnums.ConnectionType enums = GlobalEnums.ConnectionType.default_Service, string UserId = "") : base(enums, UserId)
        {
        }

        public ConfigurationBL(IOrganizationService orgService, CrmServiceClient crmServiceClient) : base(orgService, crmServiceClient)
        {
        }

        public ConfigurationBL(IOrganizationService orgService, ITracingService tracingService) : base(orgService, tracingService)
        {
        }

        public string GetConfigurationByName(string key)
        {
            ConfigurationRepository repository = new ConfigurationRepository(this.OrgService,this.CrmServiceClient);
            var value =  repository.GetConfigurationByKey(key);            
            return value;
        }

        // Tolga AYKURT - 12.03.2019
        public void UpdateConfiguration(string key, string value)
        {
            var repository = new ConfigurationRepository(this.OrgService);
            var configurationEntity = repository.GetConfigurationEntity(key);
            var updateEntity = new Entity(configurationEntity.LogicalName, configurationEntity.Id);

            updateEntity.Attributes["rnt_value"] = value;
            OrgService.Update(updateEntity);
        }

        public MongoDBResponse createConfigurationtoMongoDB(Entity entity)
        {
            var responseUrl = this.GetConfigurationByName("MongoDBWepAPIURL");
            RestSharpHelper restSharpHelper = new RestSharpHelper(responseUrl, "CreateCrmConfigurationInMongoDB", RestSharp.Method.POST);

            this.Trace("mongodb contract build params start");
            var configurationData = this.buildConfigurationData(entity);
            this.Trace("mongodb contract build params end");

            restSharpHelper.AddJsonParameter<CrmConfigurationData>(configurationData);
            restSharpHelper.AddQueryParameter("id", entity.Id.ToString());
            var response = restSharpHelper.Execute<MongoDBResponse>();

            if (!response.Result)
            {
                return MongoDBResponse.ReturnError(response.ExceptionDetail);
            }

            return response;
        }
        public MongoDBResponse updateConfigurationtoMongoDB(Entity entity)
        {
            var responseUrl = this.GetConfigurationByName("MongoDBWepAPIURL");
            RestSharpHelper restSharpHelper = new RestSharpHelper(responseUrl, "UpdateCrmConfigurationInMongoDB", RestSharp.Method.POST);

            this.Trace("mongodb contract build params start");
            var configurationData = this.buildConfigurationData(entity);                 
            this.Trace("mongodb contract build params end");

            restSharpHelper.AddJsonParameter<CrmConfigurationData>(configurationData);
            restSharpHelper.AddQueryParameter("id", entity.Id.ToString());
            var response = restSharpHelper.Execute<MongoDBResponse>();

            if (!response.Result)
            {
                return MongoDBResponse.ReturnError(response.ExceptionDetail);
            }

            return response;
        }
        private CrmConfigurationData buildConfigurationData(Entity entity)
        {
            return new CrmConfigurationData
            {
                crmConfigurationId = entity.Id.ToString(),
                name = entity.GetAttributeValue<string>("rnt_name"),
                value = entity.GetAttributeValue<string>("rnt_value"),
                statecode = entity.GetAttributeValue<OptionSetValue>("statecode").Value
            };
        }
    }
}
