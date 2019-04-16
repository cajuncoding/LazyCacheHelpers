using System;
using System.Runtime.Caching;
using System.Threading.Tasks;

namespace LazyCacheHelpers
{
    /// <summary>
    /// BBernard
    /// Public Interface for the Lazy Cache Handler which is the core class for processing Lazy loaded/initialized 
    /// cache values with a self-populating flow that ensures that only one thread ever performs the work needed to 
    /// populate the cache (e.g. only one thread ever executes the value factory) enabling all other threads to
    /// instantly benefit from the work already completed -- significantly decreasing server utilization and
    /// improving performance.
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    public interface ILazyCacheHandler<TValue>
    {
        TValue GetOrAddFromCache<TKey>(TKey key, Func<TValue> fnValueFactory, CacheItemPolicy cacheItemPolicy);
        Task<TValue> GetOrAddFromCacheAsync<TKey>(TKey key, Func<Task<TValue>> fnAsyncValueFactory, CacheItemPolicy cacheItemPolicy);
        void RemoveFromCache<TKey>(TKey key);
    }
}