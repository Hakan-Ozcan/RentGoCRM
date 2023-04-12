using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Tooling.Connector;
using RntCar.BusinessLibrary.Handlers;
using RntCar.ClassLibrary.Translation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.BusinessLibrary.Repository
{
    public class TranslationRepository : RepositoryHandler
    {
        public TranslationRepository(IOrganizationService Service) : base(Service)
        {
        }

        public TranslationRepository(IOrganizationService Service, Guid UserId) : base(Service, UserId)
        {
        }

        public TranslationRepository(IOrganizationService Service, Guid UserId, string OrganizationName) : base(Service, UserId, OrganizationName)
        {
        }

        public TranslationRepository(CrmServiceClient _crmServiceClient) : base(_crmServiceClient)
        {
        }

        public EntityCollection getTranslationAllEntity(string entityName, int langCode)
        {
            LinkEntity linkEntity = new LinkEntity();
            linkEntity.LinkFromEntityName = "rnt_translation";
            linkEntity.LinkFromAttributeName = "regardingobjectid";
            linkEntity.LinkToEntityName = $"{entityName}";
            linkEntity.LinkToAttributeName = $"{entityName}id";
            linkEntity.JoinOperator = JoinOperator.Inner;

            QueryExpression query = new QueryExpression("rnt_translation");
            query.ColumnSet = new ColumnSet(true);
            query.Criteria = new FilterExpression(LogicalOperator.And);
            query.Criteria.AddCondition("rnt_langcode", ConditionOperator.Equal, langCode);
            query.LinkEntities.Add(linkEntity);
            var result = this.retrieveMultiple(query);
            return result;
        }

        public EntityCollection getTranslationRecordByEntityName(string entityName, Guid relatedId, int langCode)
        {
            QueryExpression query = new QueryExpression("rnt_translation");
            query.ColumnSet = new ColumnSet(true);
            query.Criteria = new FilterExpression(LogicalOperator.And);
            query.Criteria.AddCondition("rnt_langcode", ConditionOperator.Equal, langCode);
            query.Criteria.AddCondition("regardingobjectid", ConditionOperator.Equal, relatedId);
            var result = this.retrieveMultiple(query);
            return result;
        }
    }
}
