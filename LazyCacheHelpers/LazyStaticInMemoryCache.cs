using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LazyCacheHelpers
{
    /// <summary>
    /// BBernard
    /// Original Source (MIT License): https://github.com/cajuncoding/LazyCacheHelpers
    /// 
    /// This class provides a simple wrapper facade to much more easily manage in-memory data cache for storing and caching Lazy&lt;T&gt; loaded results that rarely ever change
    /// from either synchronous or async value factories provided.
    ///
    /// The values generated & stored in this cache can be removed manually but will never automatically expire as there is no "cache eviction policy" support in this. It provides a greatly
    /// simplified wrapper for Lazy initialized values that are self-populating with thread safety; encapsulating the Lazy + ConcurrentDictionary pattern in a
    /// class that greatly simplifies it's use as well as handling of exceptions .
    ///
    /// In addition, this class provides support for both Synchronous and Async processes that are defined as value factory delegates (very easy with Lambda expressions).
    /// This facade eliminates the redundancy of managing the ConcurrentDictionary and Lazy wrappers and uses generics to provide a simple GetOrAdd() and TryRemove() method
    /// that takes in  Key and returns teh cached result from in-memory or generated from teh value factory via very efficient blocking-cache pattern!
    ///
    /// NOTE: A Significant difference between this and the DefaultLazyCache is that this is a Static in-memory holder of values that does inherently support garbage collection
    ///         or release/reclaiming of resources as the full blown .Net Memory Cache does; unless you manually implement your own weak references that are stored in this cache.
    /// </summary>
    public class LazyStaticInMemoryCache<TKey, TValue>
    {
        protected readonly ConcurrentDictionary<TKey, Lazy<TValue>> _lazySyncCache;
        protected readonly ConcurrentDictionary<TKey, Lazy<Task<TValue>>> _lazyAsyncCache;
        protected readonly object _padLock = new object();

        public LazyStaticInMemoryCache(IEqualityComparer<TKey> keyComparer = null)
        {
            _lazySyncCache = keyComparer == null
                ? new ConcurrentDictionary<TKey, Lazy<TValue>>()
                : new ConcurrentDictionary<TKey, Lazy<TValue>>(keyComparer);

            _lazyAsyncCache = keyComparer == null
                ? new ConcurrentDictionary<TKey, Lazy<Task<TValue>>>()
                : new ConcurrentDictionary<TKey, Lazy<Task<TValue>>>(keyComparer);
        }

        /// Initialize a new Synchronous value factory for lazy loading a value from an expensive Async process, and execute the value factory at most one time (ever, across any/all threads).
        /// This provides a robust blocking cache mechanism backed by the Lazy<> class for high performance lazy loading of data that rarely ever changes.
        /// The resulting value will be immediately returned as fast as possible, and if another thread already initialized it and is working on it then you will benefit from the work
        /// already completed.
        public virtual TValue GetOrAdd(TKey key, Func<TKey, TValue> cacheValueFactory)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            if (cacheValueFactory == null) throw new ArgumentNullException(nameof(cacheValueFactory));

            var localKeyRef = key;
            try
            {
                var cachedLazy = _lazySyncCache.GetOrAdd(localKeyRef,
                    new Lazy<TValue>(() =>
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

        /// <summary>
        /// Attempt to remove the synchronous value factory, for the specified Key, with minimal runtime impact.
        /// The value factory will be immediately discarded and will not execute even if it has
        ///     never been initialized/executed before.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public virtual bool TryRemove(TKey key)
        {
            var localKeyRef = key;
            return _lazySyncCache.TryRemove(localKeyRef, out _);
        }

        /// <summary>
        /// Initialize a new Async value factory for lazy loading a value from an expensive Async process, and execute the value factory at most one time (ever, across any/all threads).
        /// This provides a robust blocking cache mechanism backed by the Lazy<> class for high performance lazy loading of data that rarely ever changes.
        /// The resulting value will be immediately returned as fast as possible, and if another thread already initialized it and is working on it then you will benefit from the work
        /// already completed.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="cacheValueFactoryAsync"></param>
        /// <returns></returns>
        public virtual async Task<TValue> GetOrAddAsync(TKey key, Func<TKey, Task<TValue>> cacheValueFactoryAsync)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            if (cacheValueFactoryAsync == null) throw new ArgumentNullException(nameof(cacheValueFactoryAsync));

            var localKeyRef = key;
            try
            {
                var cachedAsyncLazy = _lazyAsyncCache.GetOrAdd(localKeyRef,
                    new Lazy<Task<TValue>>(async () =>
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

        /// <summary>
        /// Attempt to remove the Async value factory, for the specified Key, with minimal runtime impact.
        /// The value factory will be immediately discarded and will not execute even if it has
        ///     never been initialized/executed before.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public virtual bool TryRemoveAsyncValue(TKey key)
        {
            var localKeyRef = key;
            return _lazyAsyncCache.TryRemove(key, out _);
        }

        public virtual int GetCacheCount()
        {
            return _lazyAsyncCache.Count + _lazySyncCache.Count;
        }

        public virtual int ClearCache()
        {
            int clearCount = 0;
            if (GetCacheCount() > 0)
                lock (_padLock)
                    if (GetCacheCount() > 0)
                    {
                        clearCount += _lazyAsyncCache.Count;
                        _lazyAsyncCache.Clear();

                        clearCount += _lazySyncCache.Count;
                        _lazySyncCache.Clear();
                    }

            return clearCount;
        }
    }
}