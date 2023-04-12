using Microsoft.Xrm.Sdk;
using RntCar.BusinessLibrary.Business;
using RntCar.ClassLibrary;
using RntCar.ClassLibrary._Mobile;
using RntCar.ClassLibrary._Web;
using System.Collections.Generic;

namespace RntCar.RedisCacheHelper.CachedItems
{
    public class KilometerLimitCacheClient
    {
        public IOrganizationService OrganizationService { get; set; }
        public string cacheKey { get; set; }
        public KilometerLimitCacheClient(IOrganizationService organizationService, string _cacheKey)
        {
            cacheKey = _cacheKey;
            OrganizationService = organizationService;
        }

        public List<KmLimitData_Web> getKilometerLimitCache(List<GroupCodeInformationDetailData> groupCodeInformationDetailDatas,int duration)
        {

            RedisClient<List<KmLimitData_Web>> redisClient = new RedisClient<List<KmLimitData_Web>>();
            var cachedItem = redisClient.getCacheValue(this.cacheKey + "_" + duration);
            if (cachedItem != null)
            {
                return cachedItem;
            }
            List<KmLimitData_Web> kmLimitData_Webs = new List<KmLimitData_Web>();
            foreach (var item in groupCodeInformationDetailDatas)
            {
                KilometerLimitBL kilometerLimitBL = new KilometerLimitBL(this.OrganizationService);
                var limit = kilometerLimitBL.getKilometerLimitForGivenDurationandGroupCode(duration, item.groupCodeInformationId);

                KmLimitData_Web kmLimitData_Web = new KmLimitData_Web
                {
                    groupCodeInformationId = item.groupCodeInformationId,
                    kmLimit = limit
                };
                kmLimitData_Webs.Add(kmLimitData_Web);
            }
            //renew the cache
            redisClient.setCacheValue(this.cacheKey + "_" + duration, kmLimitData_Webs);

            return kmLimitData_Webs;
        }

        public List<KmLimitData_Mobile> getKilometerLimitCache_Mobile(List<GroupCodeInformationDetailData> groupCodeInformationDetailDatas, int duration)
        {

            RedisClient<List<KmLimitData_Mobile>> redisClient = new RedisClient<List<KmLimitData_Mobile>>();
            var cachedItem = redisClient.getCacheValue(this.cacheKey + "_" + duration);
            if (cachedItem != null)
            {
                return cachedItem;
            }
            List<KmLimitData_Mobile> kmLimitData_Mobiles = new List<KmLimitData_Mobile>();
            foreach (var item in groupCodeInformationDetailDatas)
            {
                KilometerLimitBL kilometerLimitBL = new KilometerLimitBL(this.OrganizationService);
                var limit = kilometerLimitBL.getKilometerLimitForGivenDurationandGroupCode(duration, item.groupCodeInformationId);

                KmLimitData_Mobile kmLimitData_Mobile = new KmLimitData_Mobile
                {
                    groupCodeInformationId = item.groupCodeInformationId,
                    kmLimit = limit
                };
                kmLimitData_Mobiles.Add(kmLimitData_Mobile);
            }
            //renew the cache
            redisClient.setCacheValue(this.cacheKey + "_" + duration, kmLimitData_Mobiles);

            return kmLimitData_Mobiles;
        }
    }
}
