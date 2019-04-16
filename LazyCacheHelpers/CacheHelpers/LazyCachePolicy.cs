using System;
using System.Threading;
using System.Runtime.Caching;

namespace LazyCacheHelpers
{
    /// <summary>
    /// BBernard
    /// Class to support creating cache expiration Policies with helpers to simplify the Expiration policy creation and even advanced elements
    /// such as providing random distribution of cache expirations for environments that have high load scenarios and many/multiple cache items
    /// that may expire at the same time.
    /// </summary>
    public class LazyCachePolicy
    {
        /// <summary>
        /// Helper method to more easily create an Absolute Expiration CacheItemPolicy directly from a Configuration
        /// Parameter name that has the TTL Seconds; with Default fallback if it does not exist.
        /// </summary>
        /// <param name="cacheTimeSpanToLive"></param>
        /// <param name="callbackWhenCacheEntryRemoved"></param>
        /// <returns></returns>
        public static CacheItemPolicy NewAbsoluteExpirationPolicy(string ttlSecondsConfigName, Action<CacheEntryRemovedArguments> callbackWhenCacheEntryRemoved = null)
        {
            var timeSpanToLive = LazyCacheConfig.GetCacheTTLFromConfig(ttlSecondsConfigName);
            return NewAbsoluteExpirationPolicy(timeSpanToLive, callbackWhenCacheEntryRemoved);
        }

        /// <summary>
        /// Helper method to more easily create an Absolute Expiration CacheItemPolicy
        /// </summary>
        /// <param name="cacheTimeSpanToLive"></param>
        /// <param name="callbackWhenCacheEntryRemoved"></param>
        /// <returns></returns>
        public static CacheItemPolicy NewAbsoluteExpirationPolicy(TimeSpan cacheTimeSpanToLive, Action<CacheEntryRemovedArguments> callbackWhenCacheEntryRemoved = null)
        {
            //NOTE: ALWAYS compute TTL via Utc Time to mitigate risks associated with TimeZones.
            var absoluteExpirationDateTimeOffset = DateTimeOffset.UtcNow.Add(cacheTimeSpanToLive);

            return new CacheItemPolicy()
            {
                Priority = CacheItemPriority.Default,
                AbsoluteExpiration = absoluteExpirationDateTimeOffset,
                //SlidingExpiration = MemoryCache.NoSlidingExpiration,
                RemovedCallback = callbackWhenCacheEntryRemoved != null
                        ? new CacheEntryRemovedCallback(callbackWhenCacheEntryRemoved)
                        : null
            };
        }

        //BBernard
        //Create a random number generator to help us salt our Cache intervals and better
        //  stimulate offset/distributed caching of items.  This is a simplistic way to help level out the 
        //  cache hit/miss distribution so that fewer requests take the same large hit to refresh
        //  when they expire by absolute time.
        //For Example: If in a high load environment, multiple requests could all result in cache misses
        //  and initialization of cached items with nearly identical expiration times, etc. thereby causing
        //  greater spikes when those items expire; this is a simple attempt to normalize those spikes into
        //  a more average level -- but will only benefit high load environments.
        //NOTE: Random is not thread-safe so we keep a ThreadLocal instance reference to eliminate any Thread-safe risks.
        private static readonly ThreadLocal<Random> _threadLocalRandomGenerator = new ThreadLocal<Random>(() => new Random(Guid.NewGuid().GetHashCode()));

        /// <summary>
        /// Provides logic to randomly distribute Cache TTL values within the specified threshhold to help ensure that
        ///     cached values are not all exactly equal.  This is helpful to provide dynamic caching that is much less likely
        ///     to result in all elements being expired from cache at the exact same time, but balances this with the fact that
        ///     they should expire relatively close in time.  So the Threshold value should be small; e.g. with 30 seconds, or
        ///     90 seconds of each other would be good thresholds to better distribute cache TTL values.
        ///     
        /// NOTE: Randomly distribute the Cache intervals and better stimulate offset/distribution of cache expiration of items.
        ///         This is a simplistic way to help level out the  cache hit/miss distribution so that fewer requests
        ///         take the same large hit to refresh when they expire at the same time.
        /// </summary>
        /// <param name="timeSpanTTL"></param>
        /// <param name="maxSecondsDistributionRange"></param>
        /// <returns></returns>
        public static TimeSpan RandomizeCacheTTLDistribution(TimeSpan timeSpanTTL, int maxSecondsDistributionRange = 31)
        {
            //BBernard
            //Salt the timespan with a small randomly generated offset value (e.g. spice it up a little with some salt)!
            //NOTE: We use a random number generator to help us salt our Cache intervals and better
            //  stimilate offset caching of items.  This is a simplistic way to help level out the 
            //  cache hit/miss distribution so that fewer requests take the same large hit to refresh
            //  when they expire by absolute time.
            //For Example: If in a high load environment multiple requests could all result in cache misses
            //  and initialization of cached items with nearly identical expiration times, etc. therefore cause
            //  greater spikes when those items expire; this is a simple attempt to normalize those spikes into
            //  a more average level.
            //NOTE: We salt with a value representing at least 1, and up to the defined range value or 30 seconds by default
            //      (e.g. >= 1, < 31).
            var randomGenerator = _threadLocalRandomGenerator.Value;
            var ttlSaltFromThreshold = randomGenerator.Next(1, Math.Max(maxSecondsDistributionRange, 31));
            var distributedTimeSpan = timeSpanTTL.Add(TimeSpan.FromSeconds(ttlSaltFromThreshold));

            //Return the final salted timespan for caching to use...
            return distributedTimeSpan;
        }
    }
}
