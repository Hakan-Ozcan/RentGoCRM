using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Tooling.Connector;
using RntCar.BusinessLibrary.Handlers;
using System;

namespace RntCar.BusinessLibrary.Helpers
{
    public abstract class HelperHandler : HandlerBase
    {
        public IOrganizationService IOrganizationService { get; set; }
        public ITracingService ITracingService { get; set; }

        public CrmServiceClient CrmServiceClient { get; set; }
        
        public HelperHandler(CrmServiceClient crmServiceClient)
        {
            CrmServiceClient = crmServiceClient;
        }
        public HelperHandler(IOrganizationService organizationService)
        {
            IOrganizationService = organizationService;
        }
        public HelperHandler(IOrganizationService organizationService, ITracingService tracingService)
        {
            IOrganizationService = organizationService;
            ITracingService = tracingService;
        }
        public HelperHandler(CrmServiceClient crmServiceClient, IOrganizationService organizationService, ITracingService tracingService)
        {
            CrmServiceClient = crmServiceClient;
            IOrganizationService = organizationService;
            ITracingService = tracingService;
        }
        public HelperHandler(CrmServiceClient crmServiceClient, IOrganizationService organizationService)
        {
            IOrganizationService = organizationService;
            CrmServiceClient = crmServiceClient;
        }
        public virtual void Trace(string text)
        {
            if (this.ITracingService != null)
            {
                this.ITracingService.Trace(text);
            }
        }
        public virtual Entity retrieveById(string entityName, Guid id, params string[] columns)
        {
            if (CrmServiceClient != null)
            {
                return this.CrmServiceClient.Retrieve(entityName, id, new ColumnSet(columns));
            }
            else if (IOrganizationService != null)
            {
                return this.IOrganizationService.Retrieve(entityName, id, new ColumnSet(columns));
            }
            return null;
        }
    }
}
