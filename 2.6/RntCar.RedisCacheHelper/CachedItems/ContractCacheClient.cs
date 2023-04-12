namespace RntCar.RedisCacheHelper.CachedItems
{
    public class ContractCacheClient
    {
        public void setContractDocumentCache(string cacheKey,string cacheValue)
        {
            RedisClient<string> redisClient = new RedisClient<string>();
        
            redisClient.setCacheValue(cacheKey, cacheValue);
        }
        public string getDocumentCache(string cacheKey)
        {
            RedisClient<string> redisClient = new RedisClient<string>();

            return redisClient.getCacheValue(cacheKey);
        }
    }
}
