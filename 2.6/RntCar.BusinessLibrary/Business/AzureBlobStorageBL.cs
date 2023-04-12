using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Tooling.Connector;
using Newtonsoft.Json;
using RestSharp;
using RntCar.BusinessLibrary.Handlers;
using RntCar.ClassLibrary;
using RntCar.SDK.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.BusinessLibrary.Business
{
    public class AzureBlobStorageBL : BusinessHandler
    {
        public AzureBlobStorageBL(IOrganizationService orgService) : base(orgService)
        {
        }

        public AzureBlobStorageBL(CrmServiceClient crmServiceClient) : base(crmServiceClient)
        {
        }

        public AzureBlobStorageBL(IOrganizationService orgService, CrmServiceClient crmServiceClient) : base(orgService, crmServiceClient)
        {
        }

        public AzureBlobStorageBL(IOrganizationService orgService, ITracingService tracingService) : base(orgService, tracingService)
        {
        }

        public GetBlobUrlsByDirectoryResponse getBlobUrlsByDirectory(string blobParameters)
        {
            ConfigurationBL configurationBL = new ConfigurationBL(this.OrgService);
            var responseUrl = configurationBL.GetConfigurationByName("AzureBlobStorageWebApiUrl");

            RestSharpHelper restSharpHelper = new RestSharpHelper(responseUrl, "getBlobUrlsByDirectory", Method.POST);
            var param = JsonConvert.DeserializeObject<GetBlobUrlsByDirectoryParameter>(blobParameters);
            restSharpHelper.AddJsonParameter<GetBlobUrlsByDirectoryParameter>(param);

            return restSharpHelper.Execute<GetBlobUrlsByDirectoryResponse>();
        }

        public ImageGeneratorResponse generateImage(ImageGeneratorParameter parameter)
        {
            ConfigurationBL configurationBL = new ConfigurationBL(this.OrgService);
            var responseUrl = configurationBL.GetConfigurationByName("AzureBlobStorageWebApiUrl");

            RestSharpHelper restSharpHelper = new RestSharpHelper(responseUrl, "generateImage", Method.POST);
            restSharpHelper.AddJsonParameter<ImageGeneratorParameter>(parameter);

            var r =  restSharpHelper.Execute<ImageGeneratorResponse>();

            this.Trace(JsonConvert.SerializeObject(r));
            return r;
        }
    }
}
