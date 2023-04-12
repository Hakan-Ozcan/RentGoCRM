using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Tooling.Connector;
using RntCar.BusinessLibrary.Handlers;
using RntCar.ClassLibrary._Enums_1033;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.BusinessLibrary.Repository
{
    public class EquipmentTransactionHistoryRepository : RepositoryHandler
    {
        public EquipmentTransactionHistoryRepository(IOrganizationService Service) : base(Service)
        {
        }

        public EquipmentTransactionHistoryRepository(CrmServiceClient _crmServiceClient) : base(_crmServiceClient)
        {
        }

        public EquipmentTransactionHistoryRepository(IOrganizationService Service, CrmServiceClient _crmServiceClient) : base(Service, _crmServiceClient)
        {
        }

        public EquipmentTransactionHistoryRepository(IOrganizationService Service, Guid UserId) : base(Service, UserId)
        {
        }

        public EquipmentTransactionHistoryRepository(IOrganizationService Service, Guid UserId, string OrganizationName) : base(Service, UserId, OrganizationName)
        {
        }

        public Entity getEquipmentTransactionHistoryByIdByGivenColumns(Guid id, string[] columns)
        {
            return this.retrieveById("rnt_equipmenttransactionhistory", id, columns);
        }
        public List<Entity> getEquipmentTransactionHistoryByContractIdByGivenColumns(Guid contractId, string[] columns)
        {
            QueryExpression queryExpression = new QueryExpression("rnt_equipmenttransactionhistory");
            //100000000 --> completed
            queryExpression.ColumnSet = new ColumnSet(columns);
            queryExpression.Criteria.AddCondition("statuscode", ConditionOperator.Equal, (int)rnt_equipmenttransactionhistory_StatusCode.Completed);
            queryExpression.Criteria.AddCondition("rnt_contractid", ConditionOperator.Equal, contractId);
            return this.retrieveMultiple(queryExpression).Entities.ToList();
        }
    }
}
