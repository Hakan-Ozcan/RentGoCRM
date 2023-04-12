using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using RntCar.BusinessLibrary.Handlers;
using RntCar.ClassLibrary._Enums_1033;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.BusinessLibrary.Repository
{
    public class MultiSelectMappingRepository : RepositoryHandler
    {
        public MultiSelectMappingRepository(IOrganizationService Service) : base(Service)
        {
        }

        public MultiSelectMappingRepository(IOrganizationService Service, Guid UserId, string OrganizationName) : base(Service, UserId, OrganizationName)
        {
        }

        public List<Guid> getSelectedItemsGuidListByOptionValueandGivenColumn(string columnName, OptionSetValueCollection optionSetValues)
        {
            List<object> values = new List<object>();
            List<Guid> guids = new List<Guid>();
            if (optionSetValues != null)
            {
                foreach (var item in optionSetValues)
                {
                    //create a object list and get option collection values
                    values.Add(item.Value);
                }
                QueryExpression query = new QueryExpression("rnt_multiselectmapping");
                query.ColumnSet = new ColumnSet(columnName);

                //Condition operator 'in' needs to object array 
                query.Criteria.AddCondition("rnt_optionvalue", ConditionOperator.In, values.ToArray());
                query.Criteria.AddCondition(columnName, ConditionOperator.NotNull);
                var collection = this.Service.RetrieveMultiple(query);

                foreach (var item in collection.Entities)
                {
                    if (item.Attributes.Contains(columnName))
                        guids.Add(item.GetAttributeValue<EntityReference>(columnName).Id);
                }
            }

            return guids;
        }

        public List<Entity> getBrokerMappings(string accountId)
        {
            QueryExpression query = new QueryExpression("rnt_multiselectmapping");
            query.ColumnSet = new ColumnSet(new string[] { "rnt_segmentcode", "rnt_segmentdefinition", "rnt_optionvalue" });
            query.Criteria.AddCondition("rnt_account", ConditionOperator.Equal, accountId);

            return this.Service.RetrieveMultiple(query).Entities.ToList();
        }
        public int getOptionValueByGuid(string columnName, Guid selectedId)
        {
            int optionValue = 0;
            QueryExpression query = new QueryExpression("rnt_multiselectmapping");
            query.ColumnSet = new ColumnSet("rnt_optionvalue");
             
            query.Criteria.AddCondition(columnName, ConditionOperator.Equal, selectedId);
            var entity = this.Service.RetrieveMultiple(query).Entities.FirstOrDefault();

            if (entity != null && entity.Id != Guid.Empty)
                optionValue = entity.GetAttributeValue<int>("rnt_optionvalue");
            return optionValue;
        }
    }
}
