﻿using LazyCacheHelpers;
using System;
using System.Threading.Tasks;

namespace LazyCacheHelpersTests
{
    /// <summary>
    /// BBernard
    /// Original Source (MIT License): https://github.com/raerae1616/LazyCacheHelpers
    /// 
    /// Sample Facade class to illustrate how a real applicaion might utilize LazyCacheHelpers classes in
    /// their own context with a CacheHelper that wraps it up and handles things like Cache Configurations,
    /// Cache Keys, etc. with a more simplified Facade interface to de-couple and simplify the rest of the
    /// consuming code within the application.
    /// </summary>
    public class TestCacheFacade
    {
        public static string GetCachedData(ILazyCacheParams cacheParams, Func<string> fnValueFactory)
        {
            var result = DefaultLazyCache.GetOrAddFromCache(cacheParams, fnValueFactory, cacheParams);
            return result;
        }

        public static string GetCachedSelfExpiringData(ILazyCacheParams cacheParams, Func<ILazySelfExpiringCacheResult<string>> fnSelfExpiringValueFactory)
        {
            var result = DefaultLazyCache.GetOrAddFromCache(cacheParams, fnSelfExpiringValueFactory);
            return result;
        }

        public static async Task<string> GetCachedDataAsync(ILazyCacheParams cacheParams, Func<Task<string>> fnValueFactory)
        {
            var result = await DefaultLazyCache.GetOrAddFromCacheAsync<ILazyCacheKey, string>(
                cacheParams,
                fnValueFactory,
                cacheParams
            );

            return result;
        }

        public static async Task<string> GetCachedSelfExpiringDataAsync(ILazyCacheParams cacheParams, Func<Task<ILazySelfExpiringCacheResult<string>>> fnSelfExpiringValueFactory)
        {
            var result = await DefaultLazyCache.GetOrAddFromCacheAsync(cacheParams, fnSelfExpiringValueFactory);
            return result;
        }

        public static void RemoveCachedData(ILazyCacheKey cacheKey)
        {
            DefaultLazyCache.RemoveFromCache(cacheKey);
        }

        public static void ClearCache()
        {
            DefaultLazyCache.ClearEntireCache();
        }

        public static long CacheCount()
        {
            return DefaultLazyCache.CacheCount();
        }
    }


}
