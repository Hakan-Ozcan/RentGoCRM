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
    public class DamageRepository : RepositoryHandler
    {
        public DamageRepository(IOrganizationService Service) : base(Service)
        {
        }

        public DamageRepository(CrmServiceClient _crmServiceClient) : base(_crmServiceClient)
        {
        }

        public DamageRepository(IOrganizationService Service, CrmServiceClient _crmServiceClient) : base(Service, _crmServiceClient)
        {
        }
        public List<Entity> getDamagesByEquipmentIdWithGivenColumns(Guid equipmentId, string[] columns)
        {
            QueryExpression query = new QueryExpression("rnt_damage");
            query.ColumnSet = new ColumnSet(columns);
            query.Criteria.AddCondition("statuscode", ConditionOperator.Equal, (int)DamageEnums.StatusCode.Open);
            query.Criteria.AddCondition("rnt_equipmentid", ConditionOperator.Equal, equipmentId);
            return this.retrieveMultiple(query).Entities.ToList();
        }
    }
}
