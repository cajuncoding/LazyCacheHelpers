using System;
using System.Runtime.Caching;

namespace LazyCacheHelpers
{
    public class LazySelfExpiringCacheResult<TValue> : ILazySelfExpiringCacheResult<TValue>
    {
        public LazySelfExpiringCacheResult(TValue cacheItem, ILazyCachePolicy cachePolicy)
            : this(cacheItem, cachePolicy?.GeneratePolicy())
        { }

        public LazySelfExpiringCacheResult(TValue cacheItem, CacheItemPolicy cachePolicy)
        {
            CacheItem = cacheItem;
            CachePolicy = cachePolicy ?? throw new ArgumentNullException(nameof(cachePolicy));
        }

        public CacheItemPolicy CachePolicy { get; }

        public TValue CacheItem { get; }

        public static LazySelfExpiringCacheResult<TValue> NewAbsoluteExpirationResult(TValue cacheItem, int absoluteExpirationMillis)
            => NewAbsoluteExpirationResult(cacheItem, TimeSpan.FromMilliseconds(absoluteExpirationMillis));

        public static LazySelfExpiringCacheResult<TValue> NewAbsoluteExpirationResult(TValue cacheItem, TimeSpan absoluteExpirationTimeSpan)
            => new LazySelfExpiringCacheResult<TValue>(cacheItem, LazyCachePolicy.NewAbsoluteExpirationPolicy(absoluteExpirationTimeSpan));
    }
}
