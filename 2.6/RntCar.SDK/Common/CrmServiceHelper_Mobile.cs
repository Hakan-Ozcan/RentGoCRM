using System;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using System.ServiceModel.Description;
using System.Net;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Tooling.Connector;

namespace RntCar.SDK.Common
{
    public class CrmServiceHelper_Mobile
    {
        private IOrganizationService _service;
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

        private OrganizationServiceProxy _serviceproxy;

        public OrganizationServiceProxy OrganizationServiceProxy
        {
            get { return _serviceproxy; }
        }

        public CrmServiceHelper_Mobile()
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            var crmConn = new CrmServiceClient(string.Format(_crmConnectionString, _username, _password, _organizationalURL, _appId, _redirectUri));
            WhoAmIRequest request = new WhoAmIRequest();
            WhoAmIResponse response = (WhoAmIResponse)crmConn.Execute(request);
            _service = (IOrganizationService)crmConn;
        }


    }
}
