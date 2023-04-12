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
    public class ManuelDiscountRateRepository : RepositoryHandler
    {
        public ManuelDiscountRateRepository(IOrganizationService Service) : base(Service)
        {
        }

        public ManuelDiscountRateRepository(CrmServiceClient _crmServiceClient) : base(_crmServiceClient)
        {
        }

        public ManuelDiscountRateRepository(IOrganizationService Service, CrmServiceClient _crmServiceClient) : base(Service, _crmServiceClient)
        {
        }

        public Entity getManuelDiscountRateByBusinessUnitIdByBusinessRoleCode(Guid businessUnitId, int businessRoleCode)
        {
            if( businessUnitId == Guid.Empty|| businessRoleCode == 0 )
            {
                return null;
            }

            QueryExpression queryExpression = new QueryExpression("rnt_manueldiscountrate");
            queryExpression.ColumnSet = new ColumnSet(true);
            queryExpression.Criteria.AddCondition("rnt_businessunitid", ConditionOperator.Equal, businessUnitId);
            queryExpression.Criteria.AddCondition("rnt_businessrolecode", ConditionOperator.Equal, businessRoleCode);
            queryExpression.Criteria.AddCondition("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active);
            return this.retrieveMultiple(queryExpression).Entities.FirstOrDefault();
        }
    }
}
