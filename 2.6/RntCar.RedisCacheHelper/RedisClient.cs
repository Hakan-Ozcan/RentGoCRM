using Newtonsoft.Json;
using RntCar.SDK.Common;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;

namespace RntCar.RedisCacheHelper
{
    public class RedisClient<T>
    {
        static RedisClient()
        {
            RedisClient<T>.lazyConnection = new Lazy<ConnectionMultiplexer>(() =>
            {
                return ConnectionMultiplexer.Connect(StaticHelper.GetConfiguration("RedisCacheConnection"));
            });
        }

        private static Lazy<ConnectionMultiplexer> lazyConnection;

        public static ConnectionMultiplexer Connection
        {
            get
            {
                return lazyConnection.Value;
            }
        }
        public bool setCacheValue(string key, T value)
        {
            return RedisClient<T>
                .Connection
                .GetDatabase()
                .StringSet(key, JsonConvert.SerializeObject(value), TimeSpan.FromMinutes(Convert.ToInt32(StaticHelper.GetConfiguration("RedisCacheExpiration"))));
        }
        public T getCacheValue(string key)
        {
            var val = RedisClient<T>.Connection.GetDatabase().StringGet(key);
            if(string.IsNullOrEmpty(Convert.ToString(val)))
            {
                return default(T);
            }
            try
            {
                return JsonConvert.DeserializeObject<T>(Convert.ToString(val));
            }
            catch
            {
                return (T)TypeDescriptor.GetConverter(typeof(T)).ConvertFromInvariantString(val);
            }
        }
    }
}

