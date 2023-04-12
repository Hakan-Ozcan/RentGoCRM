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
    public class LogoBankCodeMappingRepository : RepositoryHandler
    {
        public LogoBankCodeMappingRepository(IOrganizationService Service) : base(Service)
        {
        }

        public LogoBankCodeMappingRepository(CrmServiceClient _crmServiceClient) : base(_crmServiceClient)
        {
        }

        public LogoBankCodeMappingRepository(IOrganizationService Service, CrmServiceClient _crmServiceClient) : base(Service, _crmServiceClient)
        {
        }

        public List<Entity> getAllBankCode()
        {
            QueryExpression query = new QueryExpression("rnt_logobankcodemapping");
            query.ColumnSet = new ColumnSet(true);
            query.Criteria.AddCondition("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active);
            return this.retrieveMultiple(query).Entities.ToList();
        }
        public Entity getBankCodeByVirtualPosId(string virtualPosId)
        {
            QueryExpression query = new QueryExpression("rnt_logobankcodemapping");
            query.ColumnSet = new ColumnSet(true);
            query.Criteria.AddCondition("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active);
            query.Criteria.AddCondition("rnt_name", ConditionOperator.Equal, virtualPosId);
            return this.retrieveMultiple(query).Entities.FirstOrDefault();
        }

        public Entity getNetTahsilatVirtualPosId()
        {
            QueryExpression query = new QueryExpression("rnt_logobankcodemapping");
            query.ColumnSet = new ColumnSet(true);
            query.Criteria.AddCondition("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active);
            query.Criteria.AddCondition("rnt_nettahsilatposname", ConditionOperator.NotEqual, "Iyzico");
            return this.retrieveMultiple(query).Entities.FirstOrDefault();
        }
        public Entity getIyzicoVirtualPosId()
        {
            QueryExpression query = new QueryExpression("rnt_logobankcodemapping");
            query.ColumnSet = new ColumnSet(true);
            query.Criteria.AddCondition("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active);
            query.Criteria.AddCondition("rnt_nettahsilatposname", ConditionOperator.Equal, "Iyzico");
            return this.retrieveMultiple(query).Entities.FirstOrDefault();
        }
    }
}
