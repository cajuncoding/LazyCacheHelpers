using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace LazyCacheHelpers
{
    /// <summary>
    /// BBernard
    /// Original Source (MIT License): https://github.com/cajuncoding/LazyCacheHelpers
    /// 
    /// This class provides a simple wrapper facade to much more easily manage in-memory data cache for storing and caching Lazy<T> loaded results that never change.
    /// This facade eliminates the redundancy of managing the ConcurrentDictionary and Lazy wrappers and uses generics to provide a simple GetOrAdd() method
    /// that takes in  Key and returns teh cached result from in-memory or generated from teh value factory via very efficient blocking-cache pattern!
    ///
    /// This supports both Sync and Async processing, using the same backing cache concurrent dictionary!
    /// </summary>
    public class LazyStaticInMemoryCache<TKey, TCacheItem>
    {
        private readonly ConcurrentDictionary<TKey, Lazy<TCacheItem>> _lazySyncCache = new ConcurrentDictionary<TKey, Lazy<TCacheItem>>();
        private readonly ConcurrentDictionary<TKey, Lazy<Task<TCacheItem>>> _lazyAsyncCache = new ConcurrentDictionary<TKey, Lazy<Task<TCacheItem>>>();

        public TCacheItem GetOrAdd(TKey key, Func<TKey, TCacheItem> cacheValueFactory)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            if (cacheValueFactory == null) throw new ArgumentNullException(nameof(cacheValueFactory));

            var localKeyRef = key;
            try
            {
                var cachedLazy = _lazySyncCache.GetOrAdd(localKeyRef,
                    new Lazy<TCacheItem>(() =>
                    {
                        var result = cacheValueFactory.Invoke(localKeyRef);
                        return result;
                    })
                );

                var lazyResult = cachedLazy.Value;
                return lazyResult;
            }
            catch (Exception)
            {
                //BBernard - Always remove from the cache if any exception occurs so that we do NOT allow negative caching (e.g. caching of failed results)
                _lazySyncCache.TryRemove(localKeyRef, out _);
                throw;
            }
        }

        public async Task<TCacheItem> GetOrAddAsync(TKey key, Func<TKey, Task<TCacheItem>> cacheValueFactoryAsync)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            if (cacheValueFactoryAsync == null) throw new ArgumentNullException(nameof(cacheValueFactoryAsync));

            var localKeyRef = key;
            try
            {
                var cachedAsyncLazy = _lazyAsyncCache.GetOrAdd(localKeyRef,
                    new Lazy<Task<TCacheItem>>(async () =>
                    {
                        var result = await cacheValueFactoryAsync.Invoke(localKeyRef);
                        return result;
                    })
                );

                var asyncLazyResult = await cachedAsyncLazy.Value;
                return asyncLazyResult;
            }
            catch (Exception)
            {
                //BBernard - Always remove from the cache if it exists and any exception occurs so that we do NOT allow negative caching (e.g. caching of failed results)
                //NOTE: We do a check to prevent redundant/duplicated calls to TryRemove...
                _lazyAsyncCache.TryRemove(localKeyRef, out _);

                throw;
            }
        }
    }
}