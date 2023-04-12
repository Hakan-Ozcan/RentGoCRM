using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Tooling.Connector;
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
    public class PluginTraceBL : BusinessHandler
    {
        #region Constructors
        public PluginTraceBL(IOrganizationService orgService) : base(orgService)
        {
        }

        public PluginTraceBL(CrmServiceClient crmServiceClient) : base(crmServiceClient)
        {
        }

        public PluginTraceBL(GlobalEnums.ConnectionType enums = GlobalEnums.ConnectionType.default_Service, string UserId = "") : base(enums, UserId)
        {
        }

        public PluginTraceBL(IOrganizationService orgService, CrmServiceClient crmServiceClient) : base(orgService, crmServiceClient)
        {
        }

        public PluginTraceBL(IOrganizationService orgService, ITracingService tracingService) : base(orgService, tracingService)
        {
        }

        public PluginTraceBL(IOrganizationService orgService, Guid userId) : base(orgService, userId)
        {
        }

        public PluginTraceBL(PluginInitializer initializer, IOrganizationService orgService, ITracingService tracingService) : base(initializer, orgService, tracingService)
        {
        }

        public PluginTraceBL(IOrganizationService orgService, ITracingService tracingService, CrmServiceClient crmServiceClient) : base(orgService, tracingService, crmServiceClient)
        {
        }

        public PluginTraceBL(IOrganizationService orgService, Guid userId, string orgName) : base(orgService, userId, orgName)
        {
        }
        #endregion

        public void deletePluginTraceLogById(Guid id)
        {
            this.OrgService.Delete("plugintracelog", id);
        }
        public List<LogDetail> convertEntityToLogDetail(List<Entity> entities)
        {
            return entities.ConvertAll(p => new LogDetail
            {
                correlationId = p.GetAttributeValue<Guid>("correlationid").ToString(),
                createdBy = p.GetAttributeValue<EntityReference>("createdby").Id.ToString(),
                createdOn = p.GetAttributeValue<DateTime>("createdon"),
                depth = p.GetAttributeValue<int>("depth"),
                duration = p.GetAttributeValue<int>("performanceexecutionduration"),
                exceptionDetail = p.GetAttributeValue<string>("exceptiondetails"),
                isSystemCreated = p.GetAttributeValue<bool>("isystemcreated"),
                messageBlock = p.GetAttributeValue<string>("messageblock"),
                messageName = p.GetAttributeValue<string>("messagename"),
                mode = p.GetAttributeValue<OptionSetValue>("mode").Value,
                operationType = p.GetAttributeValue<OptionSetValue>("operationtype").Value,
                organizationId = p.GetAttributeValue<Guid>("organizationid").ToString(),
                performanceStartTime = p.GetAttributeValue<DateTime>("performanceexecutionstarttime"),
                pluginStepId = p.GetAttributeValue<Guid>("pluginstepid").ToString(),
                pluginTraceLogId = p.Id.ToString(),
                primaryEntity = p.GetAttributeValue<string>("primaryentity"),
                requestId = p.GetAttributeValue<Guid>("requestid").ToString(),
                typeName = p.GetAttributeValue<string>("typename")
            });
        }

    }
}
