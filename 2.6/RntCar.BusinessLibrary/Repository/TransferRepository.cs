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
    public class TransferRepository : RepositoryHandler
    {
        public TransferRepository(IOrganizationService Service) : base(Service)
        {
        }

        public TransferRepository(CrmServiceClient _crmServiceClient) : base(_crmServiceClient)
        {
        }

        public TransferRepository(IOrganizationService Service, CrmServiceClient _crmServiceClient) : base(Service, _crmServiceClient)
        {
        }

        public TransferRepository(IOrganizationService Service, Guid UserId) : base(Service, UserId)
        {
        }

        public TransferRepository(IOrganizationService Service, Guid UserId, string OrganizationName) : base(Service, UserId, OrganizationName)
        {
        }
        public Entity getTransferById(Guid transferId)
        {
            QueryExpression queryExpression = new QueryExpression("rnt_transfer");
            queryExpression.ColumnSet = new ColumnSet(true);
            queryExpression.Criteria.AddCondition("rnt_transferid", ConditionOperator.Equal, transferId);
            return this.retrieveMultiple(queryExpression).Entities.FirstOrDefault();
        }

        public List<Entity> getMaintenanceTransfersByEquipmentId(Guid equipmentId, string[] columns)
        {
            QueryExpression queryExpression = new QueryExpression("rnt_transfer");
            queryExpression.ColumnSet = new ColumnSet(columns);
            queryExpression.Criteria.AddCondition("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active);
            queryExpression.Criteria.AddCondition("rnt_transfertype", ConditionOperator.Equal, (int)ClassLibrary._Enums_1033.rnt_TransferType.Bakim);
            queryExpression.Criteria.AddCondition("rnt_equipmentid", ConditionOperator.Equal, equipmentId);
            return this.retrieveMultiple(queryExpression).Entities.ToList();
        }

        public List<Entity> getTransfersByEquipmentId(Guid equipmentId, string[] columns)
        {
            QueryExpression queryExpression = new QueryExpression("rnt_transfer");
            queryExpression.ColumnSet = new ColumnSet(columns);
            queryExpression.Criteria.AddCondition("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active);
            queryExpression.Criteria.AddCondition("rnt_equipmentid", ConditionOperator.Equal, equipmentId);
            return this.retrieveMultiple(queryExpression).Entities.ToList();
        }

        public List<Entity> getAllTransfers(string[] columns)
        {
            QueryExpression queryExpression = new QueryExpression("rnt_transfer");
            queryExpression.ColumnSet = new ColumnSet(columns);
            queryExpression.Criteria.AddCondition("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active);

            return this.retrieveMultiple(queryExpression).Entities.ToList();
        }
    }
}
