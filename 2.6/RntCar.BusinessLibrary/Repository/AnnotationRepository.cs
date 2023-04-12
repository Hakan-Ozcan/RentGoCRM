using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Tooling.Connector;
using RntCar.BusinessLibrary.Handlers;
using RntCar.ClassLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.BusinessLibrary.Repository
{
    public class AnnotationRepository : RepositoryHandler
    {
        public AnnotationRepository(IOrganizationService Service) : base(Service)
        {
        }

        public AnnotationRepository(CrmServiceClient _crmServiceClient) : base(_crmServiceClient)
        {
        }

        public AnnotationRepository(IOrganizationService Service, CrmServiceClient _crmServiceClient) : base(Service, _crmServiceClient)
        {
        }

        public AnnotationRepository(IOrganizationService Service, Guid UserId) : base(Service, UserId)
        {
        }

        public AnnotationRepository(IOrganizationService Service, Guid UserId, string OrganizationName) : base(Service, UserId, OrganizationName)
        {
        }

        public Entity getLatestAnnotationByObjectIdByFileName(Guid objectId, string fileName, string[] columns)
        {
            QueryExpression queryExpression = new QueryExpression("annotation");
            queryExpression.ColumnSet = new ColumnSet(columns);
            queryExpression.Criteria.AddCondition("objectid", ConditionOperator.Equal, objectId.ToString());
            queryExpression.Criteria.AddCondition("filename", ConditionOperator.Equal, fileName);
            queryExpression.AddOrder("createdon", OrderType.Descending);
            return this.Service.RetrieveMultiple(queryExpression).Entities.FirstOrDefault();
        }
        public Entity getLatestAnnotationByObjectIdByFileName(Guid objectId, string fileName)
        {
            QueryExpression queryExpression = new QueryExpression("annotation");
            queryExpression.ColumnSet = new ColumnSet(true);
            queryExpression.Criteria.AddCondition("objectid", ConditionOperator.Equal, objectId.ToString());
            //queryExpression.Criteria.AddCondition("filename", ConditionOperator.Equal, fileName);
            queryExpression.AddOrder("createdon", OrderType.Descending);
            return this.Service.RetrieveMultiple(queryExpression).Entities.FirstOrDefault();
        }

       
    }
}
