using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Tooling.Connector;
using RntCar.BusinessLibrary.Business;
using RntCar.ClassLibrary;
using System.Collections.Generic;

namespace RntCar.RedisCacheHelper.CachedItems
{
    public class DistrictCacheClient
    {
        public CrmServiceClient CrmServiceClient { get; set; }
        public IOrganizationService OrganizationService { get; set; }

        public string cacheKey { get; set; }
        public DistrictCacheClient(CrmServiceClient _crmServiceClient, string _cacheKey)
        {
            cacheKey = _cacheKey;
            CrmServiceClient = _crmServiceClient;
        }
        public DistrictCacheClient(IOrganizationService organizationService, string _cacheKey)
        {
            cacheKey = _cacheKey;
            OrganizationService = organizationService;
        }
        public DistrictCacheClient(IOrganizationService organizationService, CrmServiceClient _crmServiceClient, string _cacheKey)
        {
            cacheKey = _cacheKey;
            OrganizationService = organizationService;
            CrmServiceClient = _crmServiceClient;
        }

        public List<DistrictData> getDistricts()
        {

            RedisClient<List<DistrictData>> redisClient = new RedisClient<List<DistrictData>>();
            var cachedItem = redisClient.getCacheValue(this.cacheKey);
            if (cachedItem != null)
            {
                return cachedItem;
            }
            DistrictBL DistrictBL = new DistrictBL(this.OrganizationService);
            var branchs = DistrictBL.getDistricts();

            //renew the cache
            redisClient.setCacheValue(this.cacheKey, branchs);
            return branchs;
        }
    }
}
