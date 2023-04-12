using Microsoft.Xrm.Sdk;
using RntCar.BusinessLibrary.Handlers;
using RntCar.BusinessLibrary.Repository;
using RntCar.ClassLibrary;
using RntCar.SDK.Common;
using System.Collections.Generic;

namespace RntCar.BusinessLibrary.Business
{
    public class CountryBL : BusinessHandler
    {
        public CountryBL(GlobalEnums.ConnectionType enums = GlobalEnums.ConnectionType.default_Service) : base(enums)
        {
        }
        public CountryBL(IOrganizationService orgService) : base(orgService)
        {
        }

        public List<CountryData> GetCountriesWithCache(string countryKey)
        {
            List<CountryData> data = new List<CountryData>();
            CountryRepository repository = new CountryRepository(this.OrgService);

            if (CacheHelper.IsExist(countryKey))
            {
                data = CacheHelper.Get<List<CountryData>>(countryKey);
            }
            else
            {
                data = repository.GetActiveCountries();
                //Todo time can be configurable
                CacheHelper.Set(countryKey, data, 2880);
            }

            return data;
        }
    }
}
