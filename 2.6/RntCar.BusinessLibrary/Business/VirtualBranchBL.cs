using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Tooling.Connector;
using Newtonsoft.Json;
using RestSharp;
using RntCar.BusinessLibrary.Handlers;
using RntCar.BusinessLibrary.Repository;
using RntCar.ClassLibrary._Enums_1033;
using RntCar.ClassLibrary.MongoDB;
using RntCar.SDK.Common;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.BusinessLibrary.Business
{
    public class VirtualBranchBL : BusinessHandler
    {
        public VirtualBranchBL(IOrganizationService orgService) : base(orgService)
        {
        }

        public VirtualBranchBL(CrmServiceClient crmServiceClient) : base(crmServiceClient)
        {
        }

        public VirtualBranchBL(IOrganizationService orgService, ITracingService tracingService) : base(orgService, tracingService)
        {
        }

        public MongoDBResponse createVirtualBranchInMongoDB(Entity virtualBranch)
        {
            ConfigurationBL configurationBL = new ConfigurationBL(this.OrgService);

            var responseUrl = configurationBL.GetConfigurationByName("MongoDBWepAPIURL");
            RestSharpHelper restSharpHelper = new RestSharpHelper(responseUrl, "CreateVirtualBranchInMongoDB", Method.POST);
            var virtualBranchData = this.buildVirtualBranchData(virtualBranch);

            restSharpHelper.AddJsonParameter<VirtualBranchData>(virtualBranchData);

            var response = restSharpHelper.Execute<MongoDBResponse>();

            if (!response.Result)
            {
                return MongoDBResponse.ReturnError(response.ExceptionDetail);
            }

            return response;
        }
        public MongoDBResponse updateVirtualBranchInMongoDB(Entity virtualBranch)
        {
            ConfigurationBL configurationBL = new ConfigurationBL(this.OrgService);

            var responseUrl = configurationBL.GetConfigurationByName("MongoDBWepAPIURL");
            RestSharpHelper restSharpHelper = new RestSharpHelper(responseUrl, "UpdateVirtualBranchInMongoDB", Method.POST);

            var virtualBranchData = this.buildVirtualBranchData(virtualBranch);
            restSharpHelper.AddJsonParameter<VirtualBranchData>(virtualBranchData);
            restSharpHelper.AddQueryParameter("id", Convert.ToString(virtualBranch.Id));

            var response = restSharpHelper.Execute<MongoDBResponse>();

            if (!response.Result)
            {
                return MongoDBResponse.ReturnError(response.ExceptionDetail);
            }

            return response;
        }

        private VirtualBranchData buildVirtualBranchData(Entity virtualBranch)
        {
            Entity account = null;
            if (virtualBranch.Contains("rnt_accountid"))
            {
                CorporateCustomerRepository corporateCustomerRepository = new CorporateCustomerRepository(this.OrgService);
                account = corporateCustomerRepository.getCorporateCustomerById(virtualBranch.GetAttributeValue<EntityReference>("rnt_accountid").Id, new string[] { "rnt_brokercode" });
            }

            this.Trace("build start");
            
            var channelCode = virtualBranch.GetAttributeValue<OptionSetValue>("rnt_virtualchannelcode").Value;

            VirtualBranchData virtualBranchData = new VirtualBranchData
            {
                latitude = virtualBranch.Contains("rnt_latitude") ? Convert.ToDouble(virtualBranch["rnt_latitude"], new CultureInfo("en-US")) : Convert.ToDouble(0),
                longitude = virtualBranch.Contains("rnt_latitude") ? Convert.ToDouble(virtualBranch["rnt_longitude"], new CultureInfo("en-US")) : Convert.ToDouble(0),
                telephone = virtualBranch.GetAttributeValue<string>("rnt_telephone"),
                addressDetail = virtualBranch.GetAttributeValue<string>("rnt_addressdetail"),
                email = virtualBranch.GetAttributeValue<string>("rnt_email"),
                cityId = virtualBranch.Contains("rnt_cityid") ? (Guid?)virtualBranch.GetAttributeValue<EntityReference>("rnt_cityid").Id : null,
                cityName = virtualBranch.Contains("rnt_cityid") ? virtualBranch.GetAttributeValue<EntityReference>("rnt_cityid").Name : null,
                useBranchInformation = virtualBranch.Contains("rnt_usebranchinformation") ? virtualBranch.GetAttributeValue<bool>("rnt_usebranchinformation") : true,
                name = virtualBranch.Attributes.Contains("rnt_name") ? virtualBranch.GetAttributeValue<string>("rnt_name") : string.Empty,
                channelCode = channelCode.ToString(),
                virtualBranchId = Convert.ToString(virtualBranch.Id),
                brokerCode = account != null ?
                             account.Attributes.Contains("rnt_brokercode") ? account.GetAttributeValue<string>("rnt_brokercode") : string.Empty :
                             channelCode == (int)rnt_virtualbranch_rnt_virtualchannelcode.Web ? "web" :
                             channelCode == (int)rnt_virtualbranch_rnt_virtualchannelcode.Mobile ? "mobile" : string.Empty,
                accountBranch = virtualBranch.GetAttributeValue<string>("rnt_accountbranch"),
                branch = virtualBranch.GetAttributeValue<EntityReference>("rnt_branchid").Id,
                statuscode = virtualBranch.GetAttributeValue<OptionSetValue>("statuscode").Value,
                statecode = virtualBranch.GetAttributeValue<OptionSetValue>("statecode").Value,
                webRank = virtualBranch.Attributes.Contains("rnt_webrank") ? virtualBranch.GetAttributeValue<int>("rnt_webrank") : 0
            };
            this.Trace(JsonConvert.SerializeObject(virtualBranchData));
            return virtualBranchData;
        }
    }
}
