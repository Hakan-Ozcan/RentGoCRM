using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Tooling.Connector;
using RntCar.BusinessLibrary.Handlers;
using System;
using System.Linq;

namespace RntCar.BusinessLibrary.Repository
{
    public class EquipmentInventoryRepository : RepositoryHandler
    {
        public EquipmentInventoryRepository(IOrganizationService Service) : base(Service)
        {
        }

        public EquipmentInventoryRepository(CrmServiceClient _crmServiceClient) : base(_crmServiceClient)
        {
        }

        public EquipmentInventoryRepository(IOrganizationService Service, Guid UserId) : base(Service, UserId)
        {
        }

        public EquipmentInventoryRepository(IOrganizationService Service, CrmServiceClient _crmServiceClient) : base(Service, _crmServiceClient)
        {
        }

        public EquipmentInventoryRepository(IOrganizationService Service, Guid UserId, string OrganizationName) : base(Service, UserId, OrganizationName)
        {
        }

        public Entity getEquipmentInventoryById(Guid equipmentId)
        {
            QueryExpression queryExpression = new QueryExpression("rnt_equipmentinventoryhistory");
            queryExpression.ColumnSet = new ColumnSet(true);
            queryExpression.Criteria.AddCondition("rnt_equipmentinventoryhistoryid", ConditionOperator.Equal, equipmentId);
            return this.retrieveMultiple(queryExpression).Entities.FirstOrDefault();            
        }

       
    }
}
