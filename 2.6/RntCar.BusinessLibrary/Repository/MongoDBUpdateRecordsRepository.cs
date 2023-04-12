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
    public class MongoDBUpdateRecordsRepository : RepositoryHandler
    {
        public MongoDBUpdateRecordsRepository(IOrganizationService Service) : base(Service)
        {
        }

        public MongoDBUpdateRecordsRepository(CrmServiceClient _crmServiceClient) : base(_crmServiceClient)
        {
        }

        public MongoDBUpdateRecordsRepository(IOrganizationService Service, CrmServiceClient _crmServiceClient) : base(Service, _crmServiceClient)
        {
        }

        public Entity getActiveRecordByGroupCodeInformationId(Guid groupCodeInformationId)
        {
            QueryExpression query = new QueryExpression("rnt_updatedrecordsmongodb");
            query.ColumnSet = new ColumnSet(true);
            query.Criteria.AddCondition("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active);
            query.Criteria.AddCondition("rnt_groupcodeinformationid", ConditionOperator.Equal, groupCodeInformationId);
            return this.retrieveMultiple(query).Entities.FirstOrDefault();
        }
        public Entity getActiveRecordByProductId(Guid productId)
        {
            QueryExpression query = new QueryExpression("rnt_updatedrecordsmongodb");
            query.ColumnSet = new ColumnSet(true);
            query.Criteria.AddCondition("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active);
            query.Criteria.AddCondition("rnt_productid", ConditionOperator.Equal, productId);
            return this.retrieveMultiple(query).Entities.FirstOrDefault();
        }

        public List<Entity> getAllActiveRecords()
        {
            QueryExpression query = new QueryExpression("rnt_updatedrecordsmongodb");
            query.ColumnSet = new ColumnSet(true);
            query.Criteria.AddCondition("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active);
            return this.retrieveMultiple(query).Entities.ToList();
        }
    }
}
