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
    public class CouponCodeDefinitionRepository : RepositoryHandler
    {
        public CouponCodeDefinitionRepository(IOrganizationService Service) : base(Service)
        {
        }

        public CouponCodeDefinitionRepository(CrmServiceClient _crmServiceClient) : base(_crmServiceClient)
        {
        }

        public CouponCodeDefinitionRepository(IOrganizationService Service, CrmServiceClient _crmServiceClient) : base(Service, _crmServiceClient)
        {
        }

        public CouponCodeDefinitionRepository(IOrganizationService Service, Guid UserId) : base(Service, UserId)
        {
        }

        public CouponCodeDefinitionRepository(IOrganizationService Service, Guid UserId, string OrganizationName) : base(Service, UserId, OrganizationName)
        {
        }

        public Entity getCouponCodeDefinitionById(Guid id)
        {
            return this.retrieveById("rnt_couponcodedefinition", id, true);
        }
    }
}
