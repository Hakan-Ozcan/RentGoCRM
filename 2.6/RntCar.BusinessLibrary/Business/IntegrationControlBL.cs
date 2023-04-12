using Microsoft.Xrm.Sdk;
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
    public class IntegrationControlBL : BusinessHandler
    {

        public IntegrationControlBL(GlobalEnums.ConnectionType enums = GlobalEnums.ConnectionType.default_Service) : base(enums)
        {
        }
        public IntegrationControlBL(IOrganizationService orgService) : base(orgService)
        {
        }
        public IntegrationControlBL(IOrganizationService orgService, ITracingService tracingService) : base(orgService, tracingService)
        {
        }

        public IntegrationControlBL(PluginInitializer initializer, IOrganizationService orgService, ITracingService tracingService) : base(initializer, orgService, tracingService)
        {
        }

        public RestResponseMembers checkIntegrationMethod(string responseUrl, string methodName, int methodType, string extraFields)
        {

            ExtraFields extrasFieldList = JsonConvert.DeserializeObject<ExtraFields>(extraFields);
            RestSharpHelper restSharpHelper = new RestSharpHelper(responseUrl, methodName, (Method)methodType);


            List<ExtraField> headerList = extrasFieldList.extraFields.Where(x => x.extraFieldType == "Header").ToList();
            if (headerList.Count > 0)
            {
                Dictionary<string, string> headerParameterList = new Dictionary<string, string>();
                foreach (var headerExtrasField in headerList)
                {
                    headerParameterList.Add(headerExtrasField.extraFieldKey, headerExtrasField.extraFieldValue);
                }
                restSharpHelper.PrepareRequest(headerParameterList);
            }

            List<ExtraField> bodyList = extrasFieldList.extraFields.Where(x => x.extraFieldType == "Body").ToList();
            if (bodyList.Count > 0)
            {
                Dictionary<string, string> bodyParameterList = new Dictionary<string, string>();
                foreach (var bodyExtrasField in bodyList)
                {
                    bodyParameterList.Add(bodyExtrasField.extraFieldKey, bodyExtrasField.extraFieldValue);
                }
                restSharpHelper.AddBody(bodyParameterList);
            }

            return restSharpHelper.Execute();
        }
    }
}
