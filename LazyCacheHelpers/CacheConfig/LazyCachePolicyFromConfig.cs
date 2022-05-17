using System;
using System.Runtime.Caching;
using System.Threading;
using LazyNS = LazyCacheHelpers;

namespace LazyCacheHelpers
{
    /// <summary>
    /// BBernard
    /// Original Source (MIT License): https://github.com/cajuncoding/LazyCacheHelpers
    /// 
    /// Class to support creating cache expiration Policies with helpers to simplify the Expiration policy creation and even advanced elements
    /// such as providing random distribution of cache expiration times for environments that have high load scenarios and many/multiple cache items
    /// that may expire at the same time.
    /// </summary>
    public class LazyCachePolicyFromConfig
    {
        /// <summary>
        /// Helper method to more easily create an Absolute Expiration CacheItemPolicy directly from a Configuration
        /// Parameter names that has the TTL Seconds; this will return the first identified valid configuration key.
        /// </summary>
        /// <param name="ttlConfigKeysToSearch"></param>
        /// <param name="callbackWhenCacheEntryRemoved"></param>
        /// <returns></returns>
        public static CacheItemPolicy NewAbsoluteExpirationPolicy(string[] ttlConfigKeysToSearch, Action<CacheEntryRemovedArguments> callbackWhenCacheEntryRemoved = null)
        {
            var timeSpanToLive = LazyCacheConfig.GetCacheTTLFromConfig(ttlConfigKeysToSearch, LazyCacheConfig.NeverCacheTTL);
            if (timeSpanToLive.TotalMilliseconds > 0)
            {
                return LazyCachePolicy.NewAbsoluteExpirationPolicy(timeSpanToLive, callbackWhenCacheEntryRemoved);
            }

            //Finally return the cache policy for the first identified valid configuration key.
            return null;
        }

        /// <summary>
        /// Helper method to more easily create an Absolute Expiration CacheItemPolicy directly from a Configuration
        /// Parameter name that has the TTL Seconds; with Default fallback if it does not exist.
        /// </summary>
        /// <param name="ttlSecondsConfigKey"></param>
        /// <param name="callbackWhenCacheEntryRemoved"></param>
        /// <returns></returns>
        public static CacheItemPolicy NewAbsoluteExpirationPolicy(string ttlSecondsConfigKey, Action<CacheEntryRemovedArguments> callbackWhenCacheEntryRemoved = null)
        {
            var timeSpanToLive = LazyCacheConfig.GetCacheTTLFromConfig(ttlSecondsConfigKey, LazyCacheConfig.NeverCacheTTL);
            if (timeSpanToLive.TotalMilliseconds > 0)
            {
                return LazyCachePolicy.NewAbsoluteExpirationPolicy(timeSpanToLive, callbackWhenCacheEntryRemoved);
            }

            return null;
        }
    }
}
