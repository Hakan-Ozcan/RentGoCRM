using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Tooling.Connector;
using RntCar.BusinessLibrary.Business;
using RntCar.ClassLibrary.Translation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.RedisCacheHelper.CachedItems
{
    public class TranslationCacheClient
    {
        public CrmServiceClient CrmServiceClient { get; set; }
        public string cacheKey { get; set; }
        public IOrganizationService OrganizationService { get; set; }
        public TranslationCacheClient(CrmServiceClient _crmServiceClient, string _cacheKey)
        {
            cacheKey = _cacheKey;
            CrmServiceClient = _crmServiceClient;
        }
        public TranslationCacheClient(IOrganizationService organizationService, string _cacheKey)
        {
            cacheKey = _cacheKey;
            OrganizationService = organizationService;
        }

        public TranslationCacheClient(IOrganizationService organizationService, string mongoDBHostName, string mongoDBDatabaseName, string cacheKey)
        {
            this.OrganizationService = organizationService;
            this.cacheKey = cacheKey;
        }

        public List<TranslationData> getTranslationCache(string entityName, int langId)
        {
            RedisClient<List<TranslationData>> redisClient = new RedisClient<List<TranslationData>>();
            var cachedItem = redisClient.getCacheValue(entityName + "_" + this.cacheKey);
            if (cachedItem != null)
            {
                return cachedItem;
            }

            var translationDatas = new List<TranslationData>();
            TranslationBL translationBL = new TranslationBL(this.OrganizationService);
            var data = translationBL.getTranslationEntity(entityName, langId);

            redisClient.setCacheValue(entityName + "_" + this.cacheKey, translationDatas);
            return translationDatas;
        }

        public List<TranslationData> getTranslationOneRecordCache(string entityName, Guid recordId, int langId)
        {
            RedisClient<List<TranslationData>> redisClient = new RedisClient<List<TranslationData>>();
            var cachedItem = redisClient.getCacheValue(entityName + "_" + this.cacheKey);
            if (cachedItem != null)
            {
                return cachedItem;
            }

            var translationDatas = new List<TranslationData>();
            TranslationBL translationBL = new TranslationBL(this.OrganizationService);
            var data = translationBL.getTranslationRecordByEntityName(entityName,recordId,langId);

            redisClient.setCacheValue(entityName + "_" + this.cacheKey, translationDatas);
            return translationDatas;
        }
    }
}
