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
    public class EquipmentPartRepository : RepositoryHandler
    {
        public EquipmentPartRepository(IOrganizationService Service) : base(Service)
        {
        }

        public EquipmentPartRepository(CrmServiceClient _crmServiceClient) : base(_crmServiceClient)
        {
        }

        public EquipmentPartRepository(IOrganizationService Service, CrmServiceClient _crmServiceClient) : base(Service, _crmServiceClient)
        {
        }

        public List<Entity> getAllEquipmentParts()
        {
            QueryExpression query = new QueryExpression("rnt_carpart");
            query.ColumnSet = new ColumnSet(true);
            query.Criteria.AddCondition("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active);
            return this.retrieveMultiple(query).Entities.ToList();
        }
        public Entity getEquipmentPartById(Guid equipmentPartId)
        {
            QueryExpression query = new QueryExpression("rnt_carpart");
            query.ColumnSet = new ColumnSet(true);
            query.Criteria.AddCondition("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active);
            query.Criteria.AddCondition("rnt_carpartid", ConditionOperator.Equal, equipmentPartId);
            return this.retrieveMultiple(query).Entities.FirstOrDefault();
        }
    }
}
