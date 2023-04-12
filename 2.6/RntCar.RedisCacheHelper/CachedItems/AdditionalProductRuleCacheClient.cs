using Microsoft.Xrm.Sdk;
using RntCar.BusinessLibrary.Repository;
using RntCar.ClassLibrary._Mobile.AdditionalProductRules;
using RntCar.ClassLibrary._Web;
using System.Collections.Generic;

namespace RntCar.RedisCacheHelper.CachedItems
{
    public class AdditionalProductRuleCacheClient
    {

        public string cacheKey { get; set; }
        public IOrganizationService OrganizationService { get; set; }

        public AdditionalProductRuleCacheClient(IOrganizationService organizationService, string _cacheKey)
        {
            cacheKey = _cacheKey;
            OrganizationService = organizationService;
        }
        public List<AdditionalProductRule_Web> getAdditionalProductRuleCache()
        {

            RedisClient<List<AdditionalProductRule_Web>> redisClient = new RedisClient<List<AdditionalProductRule_Web>>();
            var cachedItem = redisClient.getCacheValue(this.cacheKey);
            if (cachedItem != null)
            {
                return cachedItem;
            }
            AdditionalProductRuleRepository additionalProductRuleRepository = new AdditionalProductRuleRepository(this.OrganizationService);
            var additionalProductRule = additionalProductRuleRepository.getAdditonalProductRules();

            List<AdditionalProductRule_Web> additionalProductRule_Webs = new List<AdditionalProductRule_Web>();
            foreach (var item in additionalProductRule)
            {
                AdditionalProductRule_Web additionalProductRule_Web = new AdditionalProductRule_Web
                {
                    additionalProductId = item.GetAttributeValue<EntityReference>("rnt_product").Id,
                    parentAdditionalProductId = item.GetAttributeValue<EntityReference>("rnt_parentproduct").Id,
                };
                additionalProductRule_Webs.Add(additionalProductRule_Web);
            }

            //renew the cache
            redisClient.setCacheValue(this.cacheKey, additionalProductRule_Webs);

            return additionalProductRule_Webs;
        }

        public List<AdditionalProductRule_Mobile> getMobileAdditionalProductRuleCache()
        {
            RedisClient<List<AdditionalProductRule_Mobile>> redisClient = new RedisClient<List<AdditionalProductRule_Mobile>>();
            var cachedItem = redisClient.getCacheValue(this.cacheKey);
            if (cachedItem != null)
            {
                return cachedItem;
            }
            AdditionalProductRuleRepository additionalProductRuleRepository = new AdditionalProductRuleRepository(this.OrganizationService);
            var additionalProductRule = additionalProductRuleRepository.getAdditonalProductRules();

            List<AdditionalProductRule_Mobile> additionalProductRule_Mobiles = new List<AdditionalProductRule_Mobile>();
            foreach (var item in additionalProductRule)
            {
                AdditionalProductRule_Mobile additionalProductRule_Mobile = new AdditionalProductRule_Mobile
                {
                    additionalProductId = item.GetAttributeValue<EntityReference>("rnt_product").Id,
                    parentAdditionalProductId = item.GetAttributeValue<EntityReference>("rnt_parentproduct").Id,
                };
                additionalProductRule_Mobiles.Add(additionalProductRule_Mobile);
            }

            //renew the cache
            redisClient.setCacheValue(this.cacheKey, additionalProductRule_Mobiles);

            return additionalProductRule_Mobiles;
        }
    }
}
