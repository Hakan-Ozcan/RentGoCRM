using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Tooling.Connector;
using RntCar.BusinessLibrary.Handlers;
using RntCar.ClassLibrary;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RntCar.BusinessLibrary.Repository
{
    public class GroupCodeInformationRepository : RepositoryHandler
    {
        public GroupCodeInformationRepository(IOrganizationService Service) : base(Service)
        {
        }

        public GroupCodeInformationRepository(IOrganizationService Service, Guid UserId) : base(Service, UserId)
        {
        }

        public GroupCodeInformationRepository(IOrganizationService Service, Guid UserId, string OrganizationName) : base(Service, UserId, OrganizationName)
        {
        }

        public GroupCodeInformationRepository(CrmServiceClient _crmServiceClient) : base(_crmServiceClient)
        {
        }

        public GroupCodeInformationRepository(IOrganizationService Service, CrmServiceClient _crmServiceClient) : base(Service, _crmServiceClient)
        {
        }

        public Entity getGroupCodeInformationById(Guid id)
        {
            return this.retrieveById("rnt_groupcodeinformations", id);
            
        }
        public Entity getGroupCodeInformationById(Guid id, string[] columns)
        {
            return this.retrieveById("rnt_groupcodeinformations", id, columns);
        }
        public List<Entity> getAllGroupCodeInformations()
        {
            QueryExpression expression = new QueryExpression("rnt_groupcodeinformations");
            expression.ColumnSet = new ColumnSet(true);
            expression.Criteria.AddCondition("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active);
            return this.retrieveMultiple(expression).Entities.ToList();
        }
        public List<Entity> getGroupCodeInformationByGivenIds(List<string> ids)
        {
            QueryExpression expression = new QueryExpression("rnt_groupcodeinformations");
            string[] columns = new string[] { "rnt_deposit",
                                              "rnt_findeks",
                                              "rnt_image",
                                              "rnt_minimumage",
                                              "rnt_minimumdriverlicence",
                                              "rnt_segment",
                                              "rnt_showroombrandid",
                                              "rnt_showroommodelid",
                                              "rnt_youngdriverage",
                                              "rnt_youngdriverlicence",
                                              "rnt_name",
                                              "rnt_color",
                                              "rnt_enginevolumeid"

                                            };
            expression.ColumnSet = new ColumnSet(columns);
            expression.Criteria.AddCondition("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active);
            expression.Criteria.AddCondition("rnt_groupcodeinformationsid", ConditionOperator.In, ids.ToArray());
            return this.retrieveMultiple(expression).Entities.ToList();
        }
        public List<Entity> getGroupCodeInformationByGivenIds(List<string> ids,string[] columns)
        {
            QueryExpression expression = new QueryExpression("rnt_groupcodeinformations");           
            expression.ColumnSet = new ColumnSet(columns);
            expression.Criteria.AddCondition("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active);
            expression.Criteria.AddCondition("rnt_groupcodeinformationsid", ConditionOperator.In, ids.ToArray());
            return this.retrieveMultiple(expression).Entities.ToList();
        }
        public Entity getGroupCodeInformationForDocument(string groupCodeInformationId)
        {
            QueryExpression expression = new QueryExpression("rnt_groupcodeinformations");
            string[] columns = new string[] { "rnt_doublecreditcard",
                                               "rnt_findeks",
                                               "rnt_minimumage",
                                               "rnt_minimumdriverlicence",
                                               "rnt_youngdriverage",
                                               "rnt_youngdriverlicence",
                                               "rnt_overkilometerprice",
                                               "rnt_deposit",
                                               "rnt_segment",
                                               "rnt_upgradegroupcodes"
                                            };
            expression.ColumnSet = new ColumnSet(columns);
            expression.Criteria.AddCondition("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active);
            expression.Criteria.AddCondition("rnt_groupcodeinformationsid", ConditionOperator.Equal, groupCodeInformationId);
            return this.retrieveMultiple(expression).Entities.FirstOrDefault();
        }

        public List<Entity> getAllGroupCodeImages()
        {
            QueryExpression expression = new QueryExpression("rnt_groupcodeimage");
            expression.ColumnSet = new ColumnSet(true);
            expression.Criteria.AddCondition("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active);
            return this.retrieveMultiple(expression).Entities.ToList();
        }

    }
}
