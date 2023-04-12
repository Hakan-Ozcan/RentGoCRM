using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Tooling.Connector;
using RntCar.BusinessLibrary.Business;
using RntCar.ClassLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.RedisCacheHelper.CachedItems
{
    public class ShowRoomProductCacheClient
    {
        public CrmServiceClient CrmServiceClient { get; set; }
        public IOrganizationService OrganizationService { get; set; }

        public string cacheKey { get; set; }
        public ShowRoomProductCacheClient(CrmServiceClient _crmServiceClient, string _cacheKey)
        {
            cacheKey = _cacheKey;
            CrmServiceClient = _crmServiceClient;
        }
        public ShowRoomProductCacheClient(IOrganizationService organizationService, string _cacheKey)
        {
            cacheKey = _cacheKey;
            OrganizationService = organizationService;
        }
        public ShowRoomProductCacheClient(IOrganizationService organizationService, CrmServiceClient _crmServiceClient, string _cacheKey)
        {
            cacheKey = _cacheKey;
            OrganizationService = organizationService;
            CrmServiceClient = _crmServiceClient;
        }

        public List<ShowRoomProductData> getAllShowRoomDetailCache(int langId)
        {

            RedisClient<List<ShowRoomProductData>> redisClient = new RedisClient<List<ShowRoomProductData>>();
            var cachedItem = redisClient.getCacheValue(this.cacheKey);
            if (cachedItem != null)
            {
                return cachedItem;
            }
            ShowRoomProductBL showRoomProductBL = new ShowRoomProductBL(this.OrganizationService);
            var data = showRoomProductBL.getActiveShowRoomProducts(langId);
            //renew the cache
            redisClient.setCacheValue(this.cacheKey, data);

            return data;
        }
    }
}
