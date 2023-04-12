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
    public class AdditionalDriverRepository : RepositoryHandler
    {
        public AdditionalDriverRepository(IOrganizationService Service) : base(Service)
        {
        }

        public AdditionalDriverRepository(CrmServiceClient _crmServiceClient) : base(_crmServiceClient)
        {
        }

        public AdditionalDriverRepository(IOrganizationService Service, CrmServiceClient _crmServiceClient) : base(Service, _crmServiceClient)
        {
        }

        public AdditionalDriverRepository(IOrganizationService Service, Guid UserId) : base(Service, UserId)
        {
        }

        public AdditionalDriverRepository(IOrganizationService Service, Guid UserId, string OrganizationName) : base(Service, UserId, OrganizationName)
        {
        }

        public Entity getActiveAdditionalDriverByContactIdandContractId(Guid contactId, Guid contractId)
        {
            QueryExpression query = new QueryExpression("rnt_additionaldriver");
            query.ColumnSet = new ColumnSet(true);
            query.Criteria.AddCondition("rnt_contractid", ConditionOperator.Equal, contractId);
            query.Criteria.AddCondition("rnt_contactid", ConditionOperator.Equal, contactId);
            query.Criteria.AddCondition("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active);
            return this.Service.RetrieveMultiple(query).Entities.FirstOrDefault();
        }
        public List<Entity> getAdditionalDriversByContractId(Guid contractId)
        {
            QueryExpression query = new QueryExpression("rnt_additionaldriver");
            query.ColumnSet = new ColumnSet(true);
            query.Criteria.AddCondition("rnt_contractid", ConditionOperator.Equal, contractId);
            query.Criteria.AddCondition("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active);
            return this.Service.RetrieveMultiple(query).Entities.ToList();
        }
    }
}
