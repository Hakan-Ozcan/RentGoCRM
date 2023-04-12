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
    public class EquipmentInventoryPriceRepository : RepositoryHandler
    {
        public EquipmentInventoryPriceRepository(IOrganizationService Service) : base(Service)
        {
        }

        public EquipmentInventoryPriceRepository(CrmServiceClient _crmServiceClient) : base(_crmServiceClient)
        {
        }

        public EquipmentInventoryPriceRepository(IOrganizationService Service, CrmServiceClient _crmServiceClient) : base(Service, _crmServiceClient)
        {
        }

        public EquipmentInventoryPriceRepository(IOrganizationService Service, Guid UserId) : base(Service, UserId)
        {
        }

        public EquipmentInventoryPriceRepository(IOrganizationService Service, Guid UserId, string OrganizationName) : base(Service, UserId, OrganizationName)
        {
        }

        public List<Entity> getEquipmentInventoryPriceByGroupCodeByGivenColumns(Guid groupCodeInformationId,string[] columns)
        {
            QueryExpression queryExpression = new QueryExpression("rnt_inventoryprice");
            queryExpression.ColumnSet = new ColumnSet(columns);
            
            queryExpression.Criteria.AddCondition("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active);
            return this.retrieveMultiple(queryExpression).Entities.ToList();
        }
    }
}
