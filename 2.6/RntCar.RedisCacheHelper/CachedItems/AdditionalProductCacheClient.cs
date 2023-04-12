using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Tooling.Connector;
using RntCar.BusinessLibrary.Business;
using RntCar.BusinessLibrary.Repository;
using RntCar.ClassLibrary;
using RntCar.SDK.Mappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.RedisCacheHelper.CachedItems
{
    public class AdditionalProductCacheClient
    {
        public CrmServiceClient CrmServiceClient { get; set; }
        public string cacheKey { get; set; }
        public IOrganizationService OrganizationService { get; set; }
        public AdditionalProductCacheClient(CrmServiceClient _crmServiceClient, string _cacheKey)
        {
            cacheKey = _cacheKey;
            CrmServiceClient = _crmServiceClient;
        }
        public AdditionalProductCacheClient(IOrganizationService organizationService, string _cacheKey)
        {
            cacheKey = _cacheKey;
            OrganizationService = organizationService;
        }
        public AdditionalProductData getAdditionalProductCache(string additionalProductCode)
        {

            RedisClient<AdditionalProductData> redisClient = new RedisClient<AdditionalProductData>();
            var cachedItem = redisClient.getCacheValue(this.cacheKey);
            if (cachedItem != null)
            {
                return cachedItem;
            }
            AdditionalProductRepository additionalProductRepository = new AdditionalProductRepository(this.OrganizationService, this.CrmServiceClient);
            var otherAdditonalProduct = additionalProductRepository.getAdditionalProductByProductCode(additionalProductCode);
            var data = new AdditionalProductsBL().buildAdditionalProductData(otherAdditonalProduct, decimal.Zero);

            //renew the cache
            redisClient.setCacheValue(this.cacheKey, data);

            return data;
        }
        public AdditionalProductData getAdditionalProductCacheWithFixedPrice(string additionalProductCode)
        {

            RedisClient<AdditionalProductData> redisClient = new RedisClient<AdditionalProductData>();
            var cachedItem = redisClient.getCacheValue(this.cacheKey);
            if (cachedItem != null)
            {
                return cachedItem;
            }
            AdditionalProductRepository additionalProductRepository = new AdditionalProductRepository(this.OrganizationService, this.CrmServiceClient);
            var otherAdditonalProduct = additionalProductRepository.getAdditionalProductByProductCode(additionalProductCode);
            var data = new AdditionalProductMapper().buildAdditionalProductDataWithFixedPrice(otherAdditonalProduct);

            //renew the cache
            redisClient.setCacheValue(this.cacheKey, data);

            return data;
        }

        public List<AdditionalProductData> getAdditionalProductsCache(int totalDuration, Guid brachId, Guid? currencyId = null)
        {
            RedisClient<List<AdditionalProductData>> redisClient = new RedisClient<List<AdditionalProductData>>();
            var cachedItem = redisClient.getCacheValue(this.cacheKey);
            if (cachedItem != null)
            {
                return cachedItem;
            }
            AdditionalProductRepository additionalProductRepository = new AdditionalProductRepository(this.OrganizationService, this.CrmServiceClient);
            AdditionalProductsBL additionalProductsBL = new AdditionalProductsBL(this.OrganizationService, this.CrmServiceClient);

            var additionalProducts = additionalProductRepository.GetAdditionalProductsWithCalculatedPrice(totalDuration, brachId);
            if(currencyId != null)
            {
                additionalProductsBL.calculateMultiCurrency(currencyId, additionalProducts, totalDuration);
            }           
            //renew the cache
            redisClient.setCacheValue(this.cacheKey, additionalProducts);

            return additionalProducts;
        }
    }
}
