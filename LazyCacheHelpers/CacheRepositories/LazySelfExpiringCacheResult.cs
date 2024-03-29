﻿using System;
using System.Runtime.Caching;

namespace LazyCacheHelpers
{
    public class LazySelfExpiringCacheResult<TValue> : ILazySelfExpiringCacheResult<TValue>
    {
        public LazySelfExpiringCacheResult(TValue cacheItem, ILazyCachePolicy cachePolicyFactory)
            : this(cacheItem, cachePolicyFactory?.GeneratePolicy() ?? throw new ArgumentNullException(nameof(cachePolicyFactory)))
        { }

        public LazySelfExpiringCacheResult(TValue cacheItem, CacheItemPolicy cachePolicy)
        {
            CacheItem = cacheItem;
            CachePolicy = cachePolicy ?? throw new ArgumentNullException(nameof(cachePolicy));
        }

        public CacheItemPolicy CachePolicy { get; protected set; }

        public TValue CacheItem { get; protected set; }

        public static ILazySelfExpiringCacheResult<TValue> From(TValue cacheItem, int absoluteExpirationMillis)
            => From(cacheItem, TimeSpan.FromMilliseconds(absoluteExpirationMillis));

        public static ILazySelfExpiringCacheResult<TValue> From(TValue cacheItem, TimeSpan absoluteExpirationTimeSpan)
            => new LazySelfExpiringCacheResult<TValue>(cacheItem, LazyCachePolicy.NewAbsoluteExpirationPolicy(absoluteExpirationTimeSpan));
    }

    /// <summary>
    /// Static convenience class to simplify instantiation of Generic type directly from Cache Result Type...
    /// </summary>
    public static class LazySelfExpiringCacheResult
    {
        public static ILazySelfExpiringCacheResult<TValue> From<TValue>(TValue cacheItem, int secondsTTL)
            => From(cacheItem, TimeSpan.FromSeconds(secondsTTL));

        public static ILazySelfExpiringCacheResult<TValue> From<TValue>(TValue cacheItem, TimeSpan absoluteExpirationTimeSpan)
            => new LazySelfExpiringCacheResult<TValue>(cacheItem, LazyCachePolicy.NewAbsoluteExpirationPolicy(absoluteExpirationTimeSpan));

        public static ILazySelfExpiringCacheResult<TValue> From<TValue>(TValue cacheItem, CacheItemPolicy cacheItemPolicy)
            => new LazySelfExpiringCacheResult<TValue>(cacheItem, cacheItemPolicy);

    }
}
