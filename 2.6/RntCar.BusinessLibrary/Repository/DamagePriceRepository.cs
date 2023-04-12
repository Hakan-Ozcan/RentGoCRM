using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Tooling.Connector;
using RntCar.BusinessLibrary.Handlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.BusinessLibrary.Repository
{
    public class DamagePriceRepository : RepositoryHandler
    {
        public DamagePriceRepository(IOrganizationService Service) : base(Service)
        {
        }

        public DamagePriceRepository(CrmServiceClient _crmServiceClient) : base(_crmServiceClient)
        {
        }

        public DamagePriceRepository(IOrganizationService Service, CrmServiceClient _crmServiceClient) : base(Service, _crmServiceClient)
        {
        }
        public Entity getDamagePriceWithParameters(Guid equipmentPartId, Guid damageSizeId, Guid damageTypeId)
        {
            QueryExpression query = new QueryExpression("rnt_damageprice");
            query.ColumnSet = new ColumnSet(true);
            query.Criteria.AddCondition("rnt_carpartid", ConditionOperator.Equal, equipmentPartId);
            query.Criteria.AddCondition("rnt_damagesizeid", ConditionOperator.Equal, damageSizeId);
            query.Criteria.AddCondition("rnt_demagetypeid", ConditionOperator.Equal, damageTypeId);
            return this.retrieveMultiple(query).Entities.FirstOrDefault();
        }

        public EntityCollection getAllDamagePrices()
        {
            QueryExpression query = new QueryExpression("rnt_damageprice");
            query.ColumnSet = new ColumnSet(true);
            query.Criteria.AddCondition("statecode", ConditionOperator.Equal, 0);
            return this.retrieveMultiple(query);
        }

    }
}
