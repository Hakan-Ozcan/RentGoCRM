using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Tooling.Connector;
using RntCar.BusinessLibrary.Repository;
using RntCar.ClassLibrary;
using RntCar.MongoDBHelper.Helper;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RntCar.RedisCacheHelper.CachedItems
{
    public class WorkingHourCacheClient
    {
        private string mongoDBHostName;
        private string mongoDBDatabaseName;

        public CrmServiceClient CrmServiceClient { get; set; }
        public string cacheKey { get; set; }
        public IOrganizationService OrganizationService { get; set; }
        public WorkingHourCacheClient(CrmServiceClient _crmServiceClient, string _cacheKey)
        {
            cacheKey = _cacheKey;
            CrmServiceClient = _crmServiceClient;
        }
        public WorkingHourCacheClient(IOrganizationService organizationService, string _cacheKey)
        {
            cacheKey = _cacheKey;
            OrganizationService = organizationService;
        }

        public WorkingHourCacheClient(IOrganizationService organizationService, string mongoDBHostName, string mongoDBDatabaseName, string cacheKey)
        {
            this.OrganizationService = organizationService;
            this.mongoDBHostName = mongoDBHostName;
            this.mongoDBDatabaseName = mongoDBDatabaseName;
            this.cacheKey = cacheKey;
        }

        public List<WorkingHourData> getWorkingHourCache(string brokerCode, int channelCode, bool useVirtualBranchId = false)
        {
            RedisClient<List<WorkingHourData>> redisClient = new RedisClient<List<WorkingHourData>>();
            var cachedItem = redisClient.getCacheValue(brokerCode + "_"+ this.cacheKey);
            if (cachedItem != null)
            {
                return cachedItem;
            }
            WorkingHourRepository workingHourRepository = new WorkingHourRepository(this.OrganizationService);
            var activeWorkingHours = workingHourRepository.getWorkingHours();

            var workingHours = new List<WorkingHourData>();
            VirtualBranchHelper virtualBranchHelper = new VirtualBranchHelper(mongoDBHostName, mongoDBDatabaseName);
            var activeVirtualBranches = virtualBranchHelper.getActiveBranchesByBrokerCodeandByChannelCode(brokerCode, channelCode);

            activeWorkingHours.ForEach(item =>
            {
                var virtualBranches = activeVirtualBranches.Where(v => v.branch == item.BranchId && v.brokerCode == brokerCode).ToList();

                virtualBranches.ForEach(virtualBranch =>
                {
                    workingHours.Add(new WorkingHourData
                    {
                        BranchId = useVirtualBranchId ? new Guid(virtualBranch.virtualBranchId) : item.BranchId,
                        BranchName = virtualBranch.accountBranch,
                        BeginingTime = item.BeginingTime,
                        BeginingTimeStr = item.BeginingTimeStr,
                        DayCode = item.DayCode,
                        EndTime = item.EndTime,
                        EndTimeStr = item.EndTimeStr,
                        WorkingHourId = item.WorkingHourId
                    });
                });
            });

            //renew the cache
            redisClient.setCacheValue(brokerCode + "_" + this.cacheKey, workingHours);
            return workingHours;
        }
    }
}
