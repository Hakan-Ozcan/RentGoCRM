using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using RntCar.BusinessLibrary.Handlers;
using RntCar.ClassLibrary;
using System;

namespace RntCar.BusinessLibrary.Repository
{
    // Tolga AYKURT - 04.03.2019
    public class BlackListRepository: RepositoryHandler
    {
        #region CONSTRUCTORS
        public BlackListRepository(IOrganizationService Service) : base(Service)
        {
        }

        public BlackListRepository(IOrganizationService Service, Guid UserId) : base(Service, UserId)
        {

        }
        #endregion

        #region METHODS
        // Tolga AYKURT - 04.03.2019
        public Entity GetBlackList(string identityKey)
        {
            Entity response = null;

            var query = new QueryExpression("rnt_blacklist");
            query.Criteria.AddCondition(new ConditionExpression("rnt_identitykey", ConditionOperator.Equal, identityKey));
            query.Criteria.AddCondition(new ConditionExpression("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active));
            query.ColumnSet = new ColumnSet(true);

            var blackListRecords = this.Service.RetrieveMultiple(query);

            if(blackListRecords != null && blackListRecords.Entities != null && blackListRecords.Entities.Count > 0)
            {
                response = blackListRecords.Entities[0];
            }

            return response;
        }
        #endregion
    }
}
