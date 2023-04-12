using Microsoft.Xrm.Tooling.Connector;
using RntCar.ClassLibrary;
using RntCar.BusinessLibrary.Repository;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RntCar.SDK.Mappers;
using Microsoft.Xrm.Sdk;

namespace RntCar.RedisCacheHelper.CachedItems
{
    public class TaxOfficeCacheClient
    {
        public CrmServiceClient CrmServiceClient { get; set; }
        public string cacheKey { get; set; }
        public IOrganizationService OrganizationService { get; set; }

        public TaxOfficeCacheClient(CrmServiceClient _crmServiceClient, string _cacheKey)
        {
            cacheKey = _cacheKey;
            CrmServiceClient = _crmServiceClient;
        }
        public TaxOfficeCacheClient(IOrganizationService organizationService, string _cacheKey)
        {
            cacheKey = _cacheKey;
            OrganizationService = organizationService;
        }
        public List<TaxOfficeData> getTaxOfficeCache()
        {
            RedisClient<List<TaxOfficeData>> redisClient = new RedisClient<List<TaxOfficeData>>();
            var cachedItem = redisClient.getCacheValue(this.cacheKey);
            if (cachedItem != null)
            {
                return cachedItem;
            }
            TaxOfficeRepository taxOfficeRepository = new TaxOfficeRepository(this.OrganizationService);
            var taxOffices = taxOfficeRepository.getActiveTaxOffices();

            var converted = new TaxOfficeMapper().createTaxOfficeList(taxOffices);
            //renew the cache
            redisClient.setCacheValue(this.cacheKey, converted);
            return converted;
        }
    }
}
