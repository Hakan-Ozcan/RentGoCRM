using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.SDK.Common
{
    public class CacheHelper
    {
        private static readonly System.Runtime.Caching.MemoryCache _cache = System.Runtime.Caching.MemoryCache.Default;

        public static T Get<T>(string key)
        {
            System.Diagnostics.Contracts.Contract.Assert(!string.IsNullOrEmpty(key));

            return (T)_cache.Get(key); 
        }

        public static bool IsExist(string key)
        {
            System.Diagnostics.Contracts.Contract.Assert(!string.IsNullOrEmpty(key));

            return _cache.Any(i => i.Key == key);
        }

        public static void Remove(string key)
        {
            System.Diagnostics.Contracts.Contract.Assert(!string.IsNullOrEmpty(key));

            _cache.Remove(key);
        }

        public static void RemoveByContainsKey(string key)
        {
            System.Diagnostics.Contracts.Contract.Assert(!string.IsNullOrEmpty(key));

            var _cacheList = _cache.ToList().Where(z => z.Key.Contains(key));
            if (_cacheList != null && _cacheList.Any())
            {
                foreach (var item in _cacheList)
                {
                    _cache.Remove(item.Key);
                }
            }
            _cache.Remove(key);
        }

        public static void RemoveAll()
        {
            var _cacheList = _cache.ToList();
            if (_cacheList != null && _cacheList.Any())
            {
                foreach (var item in _cacheList)
                {
                    _cache.Remove(item.Key);
                }
            }
        }

        public static void Set(string key, object value, int expireAsMinute)
        {
            System.Diagnostics.Contracts.Contract.Assert(!string.IsNullOrEmpty(key));
            System.Diagnostics.Contracts.Contract.Assert(value != null);
            System.Diagnostics.Contracts.Contract.Assert(expireAsMinute > 0);

            if (IsExist(key))
            {
                Remove((key));
            }

            _cache.Add(key, value, DateTimeOffset.Now.AddMinutes(expireAsMinute));
        }
    }
}
