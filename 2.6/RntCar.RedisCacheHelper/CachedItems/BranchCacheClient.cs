using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Tooling.Connector;
using RntCar.BusinessLibrary.Repository;
using RntCar.ClassLibrary;
using RntCar.ClassLibrary.MongoDB;
using RntCar.MongoDBHelper.Helper;
using RntCar.MongoDBHelper.Model;
using RntCar.MongoDBHelper.Repository;
using RntCar.SDK.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RntCar.RedisCacheHelper.CachedItems
{
    public class BranchCacheClient
    {
        private string mongoDBHostName;
        private string mongoDBDatabaseName;

        public CrmServiceClient CrmServiceClient { get; set; }
        public string cacheKey { get; set; }
        public IOrganizationService OrganizationService { get; set; }
        public BranchCacheClient(CrmServiceClient _crmServiceClient, string _cacheKey)
        {
            cacheKey = _cacheKey;
            CrmServiceClient = _crmServiceClient;
        }
        public BranchCacheClient(IOrganizationService organizationService, string _cacheKey)
        {
            cacheKey = _cacheKey;
            OrganizationService = organizationService;
        }

        public BranchCacheClient(IOrganizationService organizationService, string _mongoDBHostName, string _mongoDBDatabaseName, string _cacheKey)
        {
            OrganizationService = organizationService;
            mongoDBHostName = _mongoDBHostName;
            mongoDBDatabaseName = _mongoDBDatabaseName;
            cacheKey = _cacheKey;
        }
        public BranchCacheClient(string _mongoDBHostName, string _mongoDBDatabaseName, string _cacheKey)
        {
            mongoDBHostName = _mongoDBHostName;
            mongoDBDatabaseName = _mongoDBDatabaseName;
            cacheKey = _cacheKey;
        }

        public List<BranchData> getBranchCache(string brokerCode, int channelCode, bool useVirtualBranchId = false)
        {
            RedisClient<List<BranchData>> redisClient = new RedisClient<List<BranchData>>();
            var cachedItem = redisClient.getCacheValue(this.cacheKey + "_" + brokerCode);
            if (cachedItem != null)
            {
                return cachedItem;
            }

            var branches = new List<BranchData>();
            VirtualBranchHelper virtualBranchHelper = new VirtualBranchHelper(mongoDBHostName, mongoDBDatabaseName);
            var activeVirtualBranches = virtualBranchHelper.getActiveBranchesByBrokerCodeandByChannelCode(brokerCode, channelCode);
            List<object> branchIds = new List<object>();
            activeVirtualBranches.ForEach(virtualBranch =>
            {
                branchIds.Add(virtualBranch.branch);
            });
            if (branchIds.Count == 0)
            {
                return new List<BranchData>();
            }
            BranchRepository branchRepository = new BranchRepository(this.OrganizationService == null ? new CrmServiceHelper().IOrganizationService : this.OrganizationService);
            var activeBranches = branchRepository.getActiveBranchsByBranchIds(branchIds.ToArray());


            activeBranches.ForEach(item =>
            {

                var virtualBranches = activeVirtualBranches.Where(v => v.branch == new Guid(item.BranchId)).ToList();

                virtualBranches.ForEach(virtualBranch =>
                {
                    branches.Add(new BranchData
                    {
                        BranchId = useVirtualBranchId ? virtualBranch.virtualBranchId : item.BranchId,
                        addressDetail = virtualBranch.useBranchInformation ? item.addressDetail : virtualBranch.addressDetail,
                        BranchName = virtualBranch.accountBranch,
                        CityId = virtualBranch.useBranchInformation ? item.CityId : Convert.ToString(virtualBranch.cityId),
                        CityName = item.CityName,
                        emailaddress = virtualBranch.useBranchInformation ? item.emailaddress : virtualBranch.email,
                        latitude = virtualBranch.useBranchInformation ? item.latitude : virtualBranch.latitude,
                        longitude = virtualBranch.useBranchInformation ?  item.longitude : virtualBranch.longitude,
                        telephone = virtualBranch.useBranchInformation ?  item.telephone : virtualBranch.telephone,
                        seoDescription = item.seoDescription,
                        seoKeyword = item.seoKeyword,
                        seoTitle = item.seoTitle,
                        earlistPickupTime = item.earlistPickupTime,
                        branchZone = item.branchZone,
                        webRank = virtualBranch.webRank,
                        postalCode = item.postalCode
                    });
                });
            });

            //renew the cache
            redisClient.setCacheValue(this.cacheKey + "_" + brokerCode, branches);
            return branches;

        }

    }
}
