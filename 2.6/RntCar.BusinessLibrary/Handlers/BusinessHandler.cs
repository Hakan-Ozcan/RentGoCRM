using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Tooling.Connector;
using RntCar.ClassLibrary;
using RntCar.SDK.Common;
using System;
using System.Threading;

namespace RntCar.BusinessLibrary.Handlers
{
    public abstract class BusinessHandler : HandlerBase, IDisposable
    {
        public CrmServiceClient CrmServiceClient { get; set; } = null;
        public Guid? UserId { get; set; }
        public IOrganizationService OrgService { get; set; }

        public ITracingService TracingService { get; set; }

        public IOrganizationService AdminOrgService { get; set; }
        public string OrgName { get; set; }

        public PluginInitializer PluginInitializer { get; set; }

        public BusinessHandler()
        {

        }

        public BusinessHandler(GlobalEnums.ConnectionType enums = GlobalEnums.ConnectionType.default_Service, String UserId = "")
        {
            if (enums == GlobalEnums.ConnectionType.default_Service)
            {
                if (String.IsNullOrEmpty(UserId))
                    this.Initialize();
                else
                    this.Initialize(UserId);

            }
            //else if (enums == GlobalEnums.ConnectionType.WebAPI)
            //{
            //    this.InitializeRestClient();
            //}
            else if (enums == GlobalEnums.ConnectionType.Both)
            {
                if (String.IsNullOrEmpty(UserId))
                {
                    this.Initialize();

                }
                else
                {
                    this.Initialize(UserId);
                }
                //this.InitializeRestClient();
            }

        }

        public BusinessHandler(PluginInitializer initializer, IOrganizationService orgService, ITracingService tracingService)
        {
            this.PluginInitializer = initializer;
            this.OrgService = orgService;
            this.TracingService = tracingService;
        }

        public BusinessHandler(IOrganizationService orgService)
        {
            this.OrgService = orgService;
        }
        public BusinessHandler(CrmServiceClient crmServiceClient)
        {
            this.CrmServiceClient = crmServiceClient;
        }
        public BusinessHandler(IOrganizationService orgService, CrmServiceClient crmServiceClient)
        {
            this.OrgService = orgService;
            this.CrmServiceClient = crmServiceClient;
        }
        public BusinessHandler(IOrganizationService orgService, ITracingService tracingService, CrmServiceClient crmServiceClient)
        {
            this.OrgService = orgService;
            this.TracingService = tracingService;
            this.CrmServiceClient = crmServiceClient;

        }
        public BusinessHandler(IOrganizationService orgService, ITracingService tracingService)
        {
            this.OrgService = orgService;
            this.TracingService = tracingService;
        }
        public BusinessHandler(IOrganizationService orgService, Guid userId)
        {
            this.UserId = userId;
            this.OrgService = orgService;

        }

        public BusinessHandler(IOrganizationService orgService, Guid userId, String orgName)
        {
            this.UserId = userId;
            this.OrgService = orgService;
        }
      
        public void Trace(string traceText, bool forceTrace = true)
        {
            if (this.TracingService != null && forceTrace)
            {
                this.TracingService.Trace(DateTime.Now + " - " + traceText);
            }
        }
        ~BusinessHandler()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                PluginInitializer = null;
                UserId = null;
                OrgService = null;
                OrgName = null;
                TracingService = null;
                CrmServiceClient = null;
            }

        }
        private void Initialize()
        {
            CrmServiceHelper helpers = new CrmServiceHelper();
            this.OrgService = helpers.IOrganizationService;
        }
        private void Initialize(String CallerId)
        {
            CrmServiceHelper helpers = new CrmServiceHelper(new Guid(CallerId));
            this.OrgService = helpers.IOrganizationService;
        }

        public virtual void updateMongoDBCreateRelatedFields(Entity entity, string mongodbId)
        {
            try
            {
                Entity e = new Entity(entity.LogicalName);
                e.Attributes["rnt_mongodbid"] = mongodbId;
                e.Attributes["rnt_mongodbsyncdate"] = entity.GetAttributeValue<DateTime>("createdon");
                e.Id = entity.Id;
                this.OrgService.Update(e);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public virtual void UpdateMongoDBUpdateRelatedFields(Entity entity)
        {
            try
            {
                Entity e = new Entity(entity.LogicalName);
                //e["rnt_mongodbexceptiondetail"] = null;
                e["rnt_mongodbupdatesyncdate"] = entity.GetAttributeValue<DateTime>("modifiedon");
                e.Id = entity.Id;
                this.OrgService.Update(e);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public virtual void UpdateMongoDBUpdateRelatedFieldsWithError(Entity entity, string exceptionDetail)
        {
            try
            {
                Entity e = new Entity(entity.LogicalName);
                e["rnt_mongodbexceptiondetail"] = exceptionDetail;
                e.Id = entity.Id;
                this.OrgService.Update(e);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public virtual void updateEntity(Entity entity)
        {
            this.OrgService.Update(entity);
        }
        public virtual Guid createEntity(Entity entity)
        {
            return this.OrgService.Create(entity);
        }
    }
}
