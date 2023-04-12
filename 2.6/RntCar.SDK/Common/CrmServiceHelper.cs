using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Tooling.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.ServiceModel.Description;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.SDK.Common
{
    public class CrmServiceHelper
    {
        private CrmServiceClient _crmServiceClient;
        private IOrganizationService _service;
        private Dictionary<string, string> RegionList = new Dictionary<string, string>();
        private static readonly string _crmConnectionString = StaticHelper.GetConfiguration("connString");
        private static readonly string _username = StaticHelper.GetConfiguration("username");
        private static readonly string _password = StaticHelper.GetConfiguration("password");
        private static readonly string _organizationalURL = StaticHelper.GetConfiguration("organizationalURL");
        private static readonly string _appId = StaticHelper.GetConfiguration("appId");
        private static readonly string _redirectUri = StaticHelper.GetConfiguration("redirectUri");

        public IOrganizationService IOrganizationService
        {
            get { return _service; }
        }
        public CrmServiceClient CrmServiceClient
        {
            get { return _crmServiceClient; }
        }

        public CrmServiceHelper()
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            var crmConn = new CrmServiceClient(string.Format(_crmConnectionString, _username, _password, _organizationalURL, _appId, _redirectUri));
            WhoAmIRequest request = new WhoAmIRequest();
            WhoAmIResponse response = (WhoAmIResponse)crmConn.Execute(request);
            _service = (IOrganizationService)crmConn;
        }
        private string GetRegion(string region)
        {
            if (String.IsNullOrEmpty(region))
            {
                return string.Empty;
            }

            RegionList.Add("1", "North America");
            RegionList.Add("2", "South America");
            RegionList.Add("3", "Canada");
            RegionList.Add("4", "EMEA");
            RegionList.Add("5", "APAC");
            RegionList.Add("6", "Australia");
            RegionList.Add("7", "Japan");
            RegionList.Add("8", "India");
            RegionList.Add("9", "North America");

            return RegionList[region];
        }
        public CrmServiceHelper(Guid callerId)
        {

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            using (var crmConn = new CrmServiceClient(_crmConnectionString))
            {
                WhoAmIRequest request = new WhoAmIRequest();
                WhoAmIResponse response = (WhoAmIResponse)crmConn.Execute(request);

                crmConn.CallerId = callerId;
                _service = (IOrganizationService)crmConn;
            }

            //ClientCredentials c = new ClientCredentials();

            //c.UserName.UserName = StaticHelper.GetConfiguration("username");
            //c.UserName.Password = StaticHelper.GetConfiguration("password");
            //using (var _serviceproxy = new OrganizationServiceProxy(new Uri(StaticHelper.GetConfiguration("organizationalURL")),
            //                                                    null, c, null))
            //{
            //    _serviceproxy.CallerId = callerId;
            //    _service = (IOrganizationService)_serviceproxy;



            //    _serviceproxy.ServiceConfiguration.CurrentServiceEndpoint.Behaviors.Add(new ProxyTypesBehavior());
            //}

        }
    }
}
