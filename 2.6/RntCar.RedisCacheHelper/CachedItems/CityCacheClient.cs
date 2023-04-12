using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Tooling.Connector;
using RntCar.BusinessLibrary.Repository;
using RntCar.ClassLibrary;
using System.Collections.Generic;

namespace RntCar.RedisCacheHelper.CachedItems
{
    public class CityCacheClient
    {
        public CrmServiceClient CrmServiceClient { get; set; }
        public string cacheKey { get; set; }
        public IOrganizationService OrganizationService { get; set; }
        public CityCacheClient(CrmServiceClient _crmServiceClient, string _cacheKey)
        {
            cacheKey = _cacheKey;
            CrmServiceClient = _crmServiceClient;
        }
        public CityCacheClient(IOrganizationService organizationService, string _cacheKey)
        {
            cacheKey = _cacheKey;
            OrganizationService = organizationService;
        }
        public List<CityData> getCityCache()
        {
            RedisClient<List<CityData>> redisClient = new RedisClient<List<CityData>>();
            var cachedItem = redisClient.getCacheValue(this.cacheKey);
            if (cachedItem != null)
            {
                return cachedItem;
            }
            CityRepository cityRepository = new CityRepository(this.OrganizationService);
            var cities = cityRepository.GetActiveCities();
            //renew the cache
            redisClient.setCacheValue(this.cacheKey, cities);
            return cities;
        }
    }
}
