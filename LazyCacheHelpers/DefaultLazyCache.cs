using System;
using System.Runtime.Caching;
using System.Threading.Tasks;

namespace LazyCacheHelpers
{
    /// <summary>
    /// BBernard/CajunCoding
    /// Original Source (MIT License): https://github.com/cajuncoding/LazyCacheHelpers
    /// 
    /// This class provides simple way to get started with teh LazyCacheHandler by implementing an easy to use 
    /// Default instance of LazyCacheHandler that supports generic caching of any data with any CacheKey.
    /// 
    /// Static implementation follows similar patterns for applications to quickly consume the default cache implementation which
    /// uses the .Net Memory Cache (via LazyDotNetMemoryCacheRepository) for underlying cache storage with all of the benefits
    /// of self-populating cache and lazy initialization as implemented by the LazyCacheHandler class.
    /// 
    /// </summary>
    public static class DefaultLazyCache
    {
        //Added methods to CacheHelper to work with MemoryCache more easily.
        //NOTE: .Net MemoryCache supports this does NOT support Garbage Collection and Resource Reclaiming so it should
        //      be used whenever caching dynamic runtime data.
        private static readonly LazyCacheHandler<object> _lazyCache = new LazyCacheHandler<object>();

        /// <summary>
        /// Add or update the cache with the specified cache key and item that will be Lazy Initialized from Lambda function/logic.
        /// This method ensures that the item is initialized with full thread safety and that only one thread ever executes the work
        /// to initialize the item to be cached -- significantly improving server utilization and performance.
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="key"></param>
        /// <param name="fnValueFactory"></param>
        /// <param name="cachePolicyFactory"></param>
        /// <returns></returns>
        public static TValue GetOrAddFromCache<TKey, TValue>(TKey key, Func<TValue> fnValueFactory, ILazyCachePolicy cachePolicyFactory) 
            where TValue: class
        {
            TValue result = GetOrAddFromCache(key, fnValueFactory, cachePolicyFactory.GeneratePolicy());
            return result;
        }

        /// <summary>
        /// Add or update the cache with the specified cache key and item that will be Lazy Initialized from Lambda function/logic.
        /// This method ensures that the item is initialized with full thread safety and that only one thread ever executes the work
        /// to initialize the item to be cached -- significantly improving server utilization and performance.
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="key"></param>
        /// <param name="fnValueFactory"></param>
        /// <param name="cacheItemPolicy"></param>
        /// <returns></returns>
        public static TValue GetOrAddFromCache<TKey, TValue>(TKey key, Func<TValue> fnValueFactory, CacheItemPolicy cacheItemPolicy)
            where TValue : class
        {
            TValue result = LazyCachePolicy.IsPolicyEnabled(cacheItemPolicy)
                ? (TValue) _lazyCache.GetOrAddFromCache(key, fnValueFactory, cacheItemPolicy)
                : fnValueFactory();
            return result;
        }

        /// <summary>
        /// Add or update the cache with the specified cache key and item that will be Lazy Initialized from Lambda function/logic.
        /// In this overload the logic must also construct and return the result as well as the cache expiration policy together
        /// as any implementation of ILazySelfExpiringCacheResult&lt;TValue&gt; of which a default implementation can be easily
        /// created from LazySelfExpiringCacheResult&lt;TValue&gt;.NewAbsoluteExpirationResult(...).
        /// 
        /// This method ensures that the item is initialized with full thread safety and that only one thread ever executes the work
        /// to initialize the item to be cached (Self-populated Cache) -- significantly improving server utilization and performance.
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="key"></param>
        /// <param name="fnValueFactory"></param>
        /// <returns></returns>
        public static TValue GetOrAddFromCache<TKey, TValue>(TKey key, Func<ILazySelfExpiringCacheResult<TValue>> fnValueFactory)
            where TValue : class
        {
            var result = (TValue)_lazyCache.GetOrAddFromCache(key, fnValueFactory);
            return result;
        }

        /// <summary>
        /// Add or update the cache with the specified cache key and item that will be Lazy Initialized Asynchronously from Lambda function/logic.
        /// This method ensures that the item is initialized with full thread safety and that only one thread ever executes the work
        /// to initialize the item to be cached -- significantly improving server utilization and performance.
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="key"></param>
        /// <param name="fnAsyncValueFactory"></param>
        /// <param name="cachePolicyFactory"></param>
        /// <returns></returns>
        public static async Task<TValue> GetOrAddFromCacheAsync<TKey, TValue>(TKey key, Func<Task<TValue>> fnAsyncValueFactory, ILazyCachePolicy cachePolicyFactory)
            where TValue : class
        {
            TValue result = await GetOrAddFromCacheAsync<TKey, TValue>(key, fnAsyncValueFactory, cachePolicyFactory.GeneratePolicy());
            return result;
        }

        /// <summary>
        /// Add or update the cache with the specified cache key and item that will be Lazy Initialized Asynchronously from Lambda function/logic.
        /// This method ensures that the item is initialized with full thread safety and that only one thread ever executes the work
        /// to initialize the item to be cached -- significantly improving server utilization and performance.
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="key"></param>
        /// <param name="fnAsyncValueFactory"></param>
        /// <param name="cacheItemPolicy"></param>
        /// <returns></returns>
        public static async Task<TValue> GetOrAddFromCacheAsync<TKey, TValue>(TKey key, Func<Task<TValue>> fnAsyncValueFactory, CacheItemPolicy cacheItemPolicy) 
            where TValue : class
        {
            //Because the underlying cache is set up to store any object and the async coercion isn't as easy as the synchronous,
            //  we must wrap the original generics typed async factory into a new Func<> that matches the required type.
            var wrappedFnValueFactory = new Func<Task<object>>(async () => await fnAsyncValueFactory());

            TValue result = LazyCachePolicy.IsPolicyEnabled(cacheItemPolicy)
                ? (TValue)await _lazyCache.GetOrAddFromCacheAsync(key, wrappedFnValueFactory, cacheItemPolicy)
                : await fnAsyncValueFactory();

            return result;
        }

        /// <summary>
        /// Add or update the cache with the specified cache key and item that will be Lazy Initialized from Lambda function/logic.
        /// In this overload the logic must also construct and return the result as well as the cache expiration policy together
        /// as any implementation of ILazySelfExpiringCacheResult&lt;TValue&gt; of which a default implementation can be easily
        /// created from LazySelfExpiringCacheResult&lt;TValue&gt;.NewAbsoluteExpirationResult(...).
        /// 
        /// This method ensures that the item is initialized with full thread safety and that only one thread ever executes the work
        /// to initialize the item to be cached (Self-populated Cache) -- significantly improving server utilization and performance.
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="key"></param>
        /// <param name="fnAsyncValueFactory"></param>
        /// <returns></returns>
        public static async Task<TValue> GetOrAddFromCacheAsync<TKey, TValue>(TKey key, Func<Task<ILazySelfExpiringCacheResult<TValue>>> fnAsyncValueFactory)
            where TValue : class
        {
            //Because the underlying cache is set up to store any object and the async coercion isn't as easy as the synchronous,
            //  we must wrap the original generics typed async factory into a new Func<> that matches the required type.
            var wrappedFnValueFactory = new Func<Task<ILazySelfExpiringCacheResult<object>>>(async () => await fnAsyncValueFactory());

            var result = (TValue)await _lazyCache.GetOrAddFromCacheAsync(key, wrappedFnValueFactory);
            return result;
        }

        /// <summary>
        /// Remove the item/data corresponding to the specified cache key from the Cache.
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="key"></param>
        public static void RemoveFromCache<TKey>(TKey key)
        {
            _lazyCache.RemoveFromCache(key);
        }

        /// <summary>
        /// Clear/Purge all entries from the underlying Cache Repository.
        /// </summary>
        public static void ClearEntireCache()
        {
            _lazyCache.ClearEntireCache();
        }

        /// <summary>
        /// Returns the total count of Cache Entries.
        /// </summary>
        /// <returns></returns>
        public static long CacheCount()
        {
            return _lazyCache.CacheEntryCount();
        }
    }
}
