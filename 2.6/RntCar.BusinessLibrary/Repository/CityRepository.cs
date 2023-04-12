using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Tooling.Connector;
using RntCar.BusinessLibrary.Handlers;
using RntCar.ClassLibrary;
using System;
using System.Collections.Generic;

namespace RntCar.BusinessLibrary.Repository
{
    public class CityRepository : RepositoryHandler
    {
        public CityRepository(IOrganizationService Service) : base(Service)
        {
        }

        public CityRepository(CrmServiceClient _crmServiceClient) : base(_crmServiceClient)
        {
        }

        public CityRepository(IOrganizationService Service, Guid UserId) : base(Service, UserId)
        {
        }

        public CityRepository(IOrganizationService Service, Guid UserId, string OrganizationName) : base(Service, UserId, OrganizationName)
        {
        }

        public List<CityData> GetActiveCities()
        {
            QueryExpression expression = new QueryExpression("rnt_city");
            expression.ColumnSet = new ColumnSet("rnt_name", "rnt_countryid");
            expression.Criteria.AddCondition(new ConditionExpression("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active));

            var result = this.retrieveMultiple(expression);

            List<CityData> data = new List<CityData>();
            foreach (var item in result.Entities)
            {
                CityData b = new CityData
                {
                    cityId = Convert.ToString(item.Id),
                    cityName = item.GetAttributeValue<String>("rnt_name"),
                    countryId = item.Attributes.Contains("rnt_countryid") ? 
                                Convert.ToString(item.GetAttributeValue<EntityReference>("rnt_countryid").Id ) : 
                                null

                };
                data.Add(b);
            }

            return data;
        }
    }
}
