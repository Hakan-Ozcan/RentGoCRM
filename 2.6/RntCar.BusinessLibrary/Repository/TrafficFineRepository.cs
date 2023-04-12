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
    public class TrafficFineRepository : RepositoryHandler
    {
        public TrafficFineRepository(IOrganizationService Service) : base(Service)
        {
        }

        public TrafficFineRepository(CrmServiceClient _crmServiceClient) : base(_crmServiceClient)
        {
        }

        public TrafficFineRepository(IOrganizationService Service, CrmServiceClient _crmServiceClient) : base(Service, _crmServiceClient)
        {
        }

        public TrafficFineRepository(IOrganizationService Service, Guid UserId) : base(Service, UserId)
        {
        }

        public TrafficFineRepository(IOrganizationService Service, Guid UserId, string OrganizationName) : base(Service, UserId, OrganizationName)
        {
        }

        public Entity getTrafficFineBytutanak_sira_no(string tutanakserino,string contractItemId)
        {
            QueryExpression queryExpression = new QueryExpression("rnt_trafficfine");
            queryExpression.ColumnSet = new ColumnSet(true);
            queryExpression.Criteria.AddCondition("rnt_tutanaksirano", ConditionOperator.Equal, tutanakserino);
            queryExpression.Criteria.AddCondition("rnt_contractitemid", ConditionOperator.Equal, contractItemId);

            return this.retrieveMultiple(queryExpression).Entities.FirstOrDefault();
        }

        public Entity getTrafficFineById(Guid Id)
        {
            try
            {
                return this.Service.Retrieve("rnt_trafficfine", Id, new ColumnSet(true));
            }
            catch
            {
                return null;
            }
        }
    }
}
