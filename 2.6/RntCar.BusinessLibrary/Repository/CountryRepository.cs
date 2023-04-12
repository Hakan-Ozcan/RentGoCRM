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
    public class CountryRepository : RepositoryHandler
    {
        public CountryRepository(IOrganizationService Service) : base(Service)
        {
        }

        public CountryRepository(CrmServiceClient _crmServiceClient) : base(_crmServiceClient)
        {
        }

        public CountryRepository(IOrganizationService Service, Guid UserId) : base(Service, UserId)
        {
        }

        public CountryRepository(IOrganizationService Service, Guid UserId, string OrganizationName) : base(Service, UserId, OrganizationName)
        {
        }

        public List<CountryData> GetActiveCountries()
        {
            QueryExpression expression = new QueryExpression("rnt_country");
            expression.ColumnSet = new ColumnSet("rnt_name", "rnt_countrycode", "rnt_telephonecode");
            expression.Criteria.AddCondition(new ConditionExpression("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active));

            var result = this.retrieveMultiple(expression);

            List<CountryData> data = new List<CountryData>();
            foreach (var item in result.Entities)
            {
                CountryData b = new CountryData
                {
                    countryId = Convert.ToString(item.Id),
                    countryName = item.GetAttributeValue<string>("rnt_name"),
                    countryCode = item.GetAttributeValue<string>("rnt_countrycode"),
                    countryDialCode = item.GetAttributeValue<string>("rnt_telephonecode")
                };
                data.Add(b);
            }

            return data;
        }

        public CountryData GetActiveCountriesWithId(Guid countryId)
        {
            QueryExpression expression = new QueryExpression("rnt_country");
            expression.ColumnSet = new ColumnSet("rnt_name", "rnt_countrycode", "rnt_telephonecode");
            expression.Criteria.AddCondition(new ConditionExpression("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active));
            expression.Criteria.AddCondition(new ConditionExpression("rnt_countryid", ConditionOperator.Equal, countryId));

            var result = this.retrieveMultiple(expression);

            CountryData data = new CountryData();
            foreach (var item in result.Entities)
            {
                data = new CountryData
                {
                    countryId = Convert.ToString(item.Id),
                    countryName = item.GetAttributeValue<string>("rnt_name"),
                    countryCode = item.GetAttributeValue<string>("rnt_countrycode"),
                    countryDialCode = item.GetAttributeValue<string>("rnt_telephonecode")
                };
            }

            return data;
        }
    }
}
