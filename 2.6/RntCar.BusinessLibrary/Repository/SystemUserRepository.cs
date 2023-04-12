using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Tooling.Connector;
using RntCar.BusinessLibrary.Business;
using RntCar.BusinessLibrary.Handlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.BusinessLibrary.Repository
{
    public class SystemUserRepository : RepositoryHandler
    {
        public SystemUserRepository(IOrganizationService Service) : base(Service)
        {
        }

        public SystemUserRepository(CrmServiceClient _crmServiceClient) : base(_crmServiceClient)
        {
        }

        public SystemUserRepository(IOrganizationService Service, CrmServiceClient _crmServiceClient) : base(Service, _crmServiceClient)
        {
        }

        public Entity getSystemUserById(Guid userId)
        {
            QueryExpression queryExpression = new QueryExpression("systemuser");
            queryExpression.ColumnSet = new ColumnSet(true);
            queryExpression.Criteria.AddCondition("isdisabled", ConditionOperator.Equal, false);
            queryExpression.Criteria.AddCondition("systemuserid", ConditionOperator.Equal, userId);
            return this.retrieveMultiple(queryExpression).Entities.FirstOrDefault();
        }
        public Entity getCurrentUser(string[] columns)
        {
            QueryExpression queryExpression = new QueryExpression("systemuser");
            queryExpression.ColumnSet = new ColumnSet(columns);
            queryExpression.Criteria.AddCondition("isdisabled", ConditionOperator.Equal, false);
            queryExpression.Criteria.AddCondition("systemuserid", ConditionOperator.EqualUserId);
            return this.retrieveMultiple(queryExpression).Entities.FirstOrDefault();
        }
        public List<Entity> getAlluserExceptWebUser()
        {
            ConfigurationBL configurationBL = new ConfigurationBL(this.Service);
            var value = configurationBL.GetConfigurationByName("WebUserId");
            var valueAdmin = configurationBL.GetConfigurationByName("CrmAdminGuid");

            QueryExpression queryExpression = new QueryExpression("systemuser");
            queryExpression.ColumnSet = new ColumnSet(true);
            queryExpression.Criteria.AddCondition("isdisabled", ConditionOperator.Equal, false);
            queryExpression.Criteria.AddCondition("systemuserid", ConditionOperator.NotEqual, value);
            queryExpression.Criteria.AddCondition("systemuserid", ConditionOperator.NotEqual, valueAdmin);
            return this.retrieveMultiple(queryExpression).Entities.ToList();
        }
        public Entity getSystemUserByIdWithGivenColumns(Guid userId, string[] columns)
        {
            QueryExpression queryExpression = new QueryExpression("systemuser");
            queryExpression.ColumnSet = new ColumnSet(columns);
            queryExpression.Criteria.AddCondition("isdisabled", ConditionOperator.Equal, false);
            queryExpression.Criteria.AddCondition("systemuserid", ConditionOperator.Equal, userId);
            return this.retrieveMultiple(queryExpression).Entities.FirstOrDefault();
        }
        public List<Entity> getSystemUsersByBusinessRoleCodeandBranchWithGivenColumns(int businessRoleCode, string branchId, string[] columns)
        {
            var fetchString = $@"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='true'>
                                  <entity name='systemuser'>
                                    <attribute name='systemuserid' />
                                    <filter type='and'>
                                        <condition attribute='rnt_businessrolecode' operator='eq' value='{businessRoleCode}' />
                                    </filter>
                                    <link-entity name='businessunit' from='businessunitid' to='businessunitid' link-type='inner' alias='as'>
                                      <link-entity name='rnt_branch' from='rnt_businessunitid' to='businessunitid' link-type='inner' alias='at'>
                                        <filter type='and'>
                                          <condition attribute='rnt_branchid' operator='eq' value='{branchId}' />
                                        </filter>
                                      </link-entity>
                                    </link-entity>
                                  </entity>
                                </fetch>";
            return this.retrieveMultiple(new FetchExpression(fetchString)).Entities.ToList();
        }
        public List<Entity> getSystemUsersByBranch(Guid branchId)
        {
            var fetchString = $@"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='true'>
                                  <entity name='systemuser'>
                                    <attribute name='systemuserid' />    
                                    <attribute name='fullname' /> 
                                    <attribute name='rnt_businessrolecode' /> 
                                 <filter type='and'>
                                      <condition attribute='isdisabled' operator='eq' value='0' />
                                    </filter>
                                    <link-entity name='businessunit' from='businessunitid' to='businessunitid' link-type='inner' alias='as'>
                                      <link-entity name='rnt_branch' from='rnt_businessunitid' to='businessunitid' link-type='inner' alias='at'>
                                        <filter type='and'>
                                          <condition attribute='rnt_branchid' operator='eq' value='{branchId}' />
                                        </filter>
                                      </link-entity>
                                    </link-entity>
                                  </entity>
                                </fetch>";
            return this.retrieveMultiple(new FetchExpression(fetchString)).Entities.ToList();
        }
    }
}
