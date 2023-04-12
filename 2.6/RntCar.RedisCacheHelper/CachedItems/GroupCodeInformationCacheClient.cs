using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Tooling.Connector;
using RntCar.BusinessLibrary.Business;
using RntCar.ClassLibrary;
using RntCar.SDK.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RntCar.RedisCacheHelper.CachedItems
{
    public class GroupCodeInformationCacheClient
    {
        public CrmServiceClient CrmServiceClient { get; set; }
        public IOrganizationService OrganizationService { get; set; }
        public string cacheKey { get; set; }

        public GroupCodeInformationCacheClient(IOrganizationService organizationService)
        {
            this.OrganizationService = organizationService;
        }

        public GroupCodeInformationCacheClient(CrmServiceClient _crmServiceClient, string _cacheKey)
        {
            cacheKey = _cacheKey;
            CrmServiceClient = _crmServiceClient;
        }
        public GroupCodeInformationCacheClient(IOrganizationService organizationService, string _cacheKey)
        {
            cacheKey = _cacheKey;
            OrganizationService = organizationService;
        }
        public GroupCodeInformationCacheClient(IOrganizationService organizationService, CrmServiceClient _crmServiceClient, string _cacheKey)
        {
            cacheKey = _cacheKey;
            OrganizationService = organizationService;
            CrmServiceClient = _crmServiceClient;
        }
        public GroupCodeInformationDetailData getGroupCodeInformationDetailCache(Guid groupCodeInformationId)
        {

            RedisClient<GroupCodeInformationDetailData> redisClient = new RedisClient<GroupCodeInformationDetailData>();
            var cachedItem = redisClient.getCacheValue(this.cacheKey + "_" + groupCodeInformationId);
            if (cachedItem != null)
            {
                return cachedItem;
            }
            GroupCodeInformationBL groupCodeInformationBL = new GroupCodeInformationBL(this.OrganizationService);
            var data = groupCodeInformationBL.getGroupCodesInformationDetailById(groupCodeInformationId);
            //renew the cache
            redisClient.setCacheValue(this.cacheKey + "_" + groupCodeInformationId, data);

            return data;
        }
        public List<GroupCodeInformationDetailData> getAllGroupCodeInformationDetailCache(int langId)
        {

            RedisClient<List<GroupCodeInformationDetailData>> redisClient = new RedisClient<List<GroupCodeInformationDetailData>>();
            var cachedItem = redisClient.getCacheValue(this.cacheKey);
            if (cachedItem != null)
            {
                return cachedItem;
            }
            GroupCodeInformationBL groupCodeInformationBL = new GroupCodeInformationBL(this.OrganizationService);
            var data = groupCodeInformationBL.getAllGroupCodesInformationDetailData(langId);
            //renew the cache
            redisClient.setCacheValue(this.cacheKey, data);

            return data;
        }

        public List<OptionSetModel> getSegmentNameCache(string cacheKey, int langId)
        {
            RedisClient<List<OptionSetModel>> redisClient = new RedisClient<List<OptionSetModel>>();
            var cachedItem = redisClient.getCacheValue(cacheKey);

            if (cachedItem != null)
            {
                return cachedItem;
            }

            List<OptionSetModel> optionLabelList = new List<OptionSetModel>();
            XrmHelper xrmHelper = new XrmHelper(this.OrganizationService);
            var optionSets = xrmHelper.GetoptionsetTextOnValue("rnt_groupcodeinformations", "rnt_segment");

            optionSets.ForEach(option =>
            {
                optionLabelList.Add(new OptionSetModel
                {
                    label = option.Label.LocalizedLabels.Where(p => p.LanguageCode == langId).FirstOrDefault().Label,
                    value = option.Value.Value
                });
            });

            redisClient.setCacheValue(cacheKey, optionLabelList);

            return optionLabelList;
        }

        public List<GroupCodeImageData> GetGroupCodeImageDatas(string cacheKey)
        {
            RedisClient<List<GroupCodeImageData>> redisClient = new RedisClient<List<GroupCodeImageData>>();
            var cachedItem = redisClient.getCacheValue(cacheKey);

            if (cachedItem != null)
            {
                return cachedItem;
            }

            GroupCodeInformationBL groupCodeInformationBL = new GroupCodeInformationBL(this.OrganizationService);
            var data = groupCodeInformationBL.GetAllGroupCodeImages();
            //renew the cache
            redisClient.setCacheValue(cacheKey, data);

            return data;
        }
    }
}
