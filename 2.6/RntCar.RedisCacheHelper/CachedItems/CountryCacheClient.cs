using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Tooling.Connector;
using RntCar.BusinessLibrary.Repository;
using RntCar.ClassLibrary;
using System.Collections.Generic;

namespace RntCar.RedisCacheHelper.CachedItems
{
    public class CountryCacheClient
    {
        public CrmServiceClient CrmServiceClient { get; set; }
        public string cacheKey { get; set; }
        public IOrganizationService OrganizationService { get; set; }
        public CountryCacheClient(CrmServiceClient _crmServiceClient, string _cacheKey)
        {
            cacheKey = _cacheKey;
            CrmServiceClient = _crmServiceClient;
        }
        public CountryCacheClient(IOrganizationService organizationService, string _cacheKey)
        {
            cacheKey = _cacheKey;
            OrganizationService = organizationService;
        }
        public List<CountryData> getCountryCache()
        {

            RedisClient<List<CountryData>> redisClient = new RedisClient<List<CountryData>>();
            var cachedItem = redisClient.getCacheValue(this.cacheKey);
            if (cachedItem != null)
            {
                return cachedItem;
            }
            CountryRepository countryRepository = new CountryRepository(this.OrganizationService);
            var countries = countryRepository.GetActiveCountries();
           //renew the cache
            redisClient.setCacheValue(this.cacheKey, countries);

            return countries;
        }
    }
}
