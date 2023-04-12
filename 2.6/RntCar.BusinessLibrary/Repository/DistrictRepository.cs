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
    public class DistrictRepository : RepositoryHandler
    {
        public DistrictRepository(IOrganizationService Service) : base(Service)
        {
        }

        public DistrictRepository(IOrganizationService Service, Guid UserId) : base(Service, UserId)
        {
        }

        public DistrictRepository(IOrganizationService Service, CrmServiceClient _crmServiceClient) : base(Service, _crmServiceClient)
        {
        }

        public DistrictRepository(IOrganizationService Service, Guid UserId, string OrganizationName) : base(Service, UserId, OrganizationName)
        {
        }

        public EntityCollection GetActiveDistricts()
        {
            QueryExpression expression = new QueryExpression("rnt_district");
            expression.ColumnSet = new ColumnSet("rnt_name", "rnt_districtid" ,"rnt_cityid");
            expression.Criteria.AddCondition(new ConditionExpression("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active));
            return this.retrieveMultiple(expression);
        }
    }
}
