using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Tooling.Connector;
using System;

namespace RntCar.BusinessLibrary.Handlers
{
    public class RepositoryHandler
    {
        public CrmServiceClient CrmServiceClient;
        public IOrganizationService Service { get; set; }
        public Guid UserId { get; set; }
        public String OrganizationName { get; set; }

        public RepositoryHandler(IOrganizationService Service)
        {
            this.Service = Service;
        }
        public RepositoryHandler(CrmServiceClient _crmServiceClient)
        {
            this.CrmServiceClient = _crmServiceClient;
        }
        public RepositoryHandler(IOrganizationService Service, CrmServiceClient _crmServiceClient)
        {
            this.Service = Service;
            this.CrmServiceClient = _crmServiceClient;
        }
        public RepositoryHandler(IOrganizationService Service, Guid UserId)
        {
            this.Service = Service;
            this.UserId = UserId;
        }
        public Entity retrieveById(string entityName, Guid id)
        {
            if (this.CrmServiceClient != null)
            {
                return this.CrmServiceClient.Retrieve(entityName, id, new ColumnSet(true));
            }
            else
            {
                return this.Service.Retrieve(entityName, id, new ColumnSet(true));
            }
        }
        public Entity retrieveById(string entityName, Guid id, string[] columns)
        {
            if (this.CrmServiceClient != null)
            {
                return this.CrmServiceClient.Retrieve(entityName, id, new ColumnSet(columns));
            }
            else
            {
                return this.Service.Retrieve(entityName, id, new ColumnSet(columns));
            }
        }
        public Entity retrieveById(string entityName, Guid id, bool allColumns)
        {
            if (this.CrmServiceClient != null)
            {
                return this.CrmServiceClient.Retrieve(entityName, id, new ColumnSet(allColumns));
            }
            else
            {
                return this.Service.Retrieve(entityName, id, new ColumnSet(allColumns));
            }
        }
        public EntityCollection retrieveMultiple(QueryExpression queryExpression)
        {
            if (this.CrmServiceClient != null)
            {
                return this.CrmServiceClient.RetrieveMultiple(queryExpression);
            }
            else
            {
                return this.Service.RetrieveMultiple(queryExpression);
            }
        }
        public EntityCollection retrieveMultiple(QueryByAttribute queryExpression)
        {
            if (this.CrmServiceClient != null)
            {
                return this.CrmServiceClient.RetrieveMultiple(queryExpression);
            }
            else
            {
                return this.Service.RetrieveMultiple(queryExpression);
            }
        }
        public EntityCollection retrieveMultiple(FetchExpression fetchExpression)
        {
            if (this.CrmServiceClient != null)
            {
                return this.CrmServiceClient.RetrieveMultiple(fetchExpression);
            }
            else
            {
                return this.Service.RetrieveMultiple(fetchExpression);
            }
        }
        public RepositoryHandler(IOrganizationService Service, Guid UserId, String OrganizationName)
        {
            this.Service = Service;
            this.UserId = UserId;
            this.OrganizationName = OrganizationName;
        }
    }
}
