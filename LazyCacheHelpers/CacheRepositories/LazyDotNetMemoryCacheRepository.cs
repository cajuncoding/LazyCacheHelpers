using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Threading.Tasks;

namespace LazyCacheHelpers
{
    /// <summary>
    /// BBernard
    /// Original Source (MIT License): https://github.com/cajuncoding/LazyCacheHelpers
    /// 
    /// Facade implementation for the default .Net MemoryCache implementation for the LazyCacheHandler.
    /// This allows lazy initialization and self-populating flow to be implemented on top of the out-of-the-box
    /// .Net Memory Cache.
    /// 
    /// NOTE: .Net MemoryCache may provide support for Garbage Collection or memory pressure re-claiming of resources, etc.
    ///         based on the CacheItemPriority values.
    ///         https://docs.microsoft.com/en-us/dotnet/api/system.runtime.caching.cacheitempriority?view=netframework-4.7.2
    /// </summary>
    public class LazyDotNetMemoryCacheRepository : ILazyCacheRepository, IDisposable
    {
        private Lazy<MemoryCache> _lazyCacheHolder;

        public LazyDotNetMemoryCacheRepository()
            : this(null)
        { }

        public LazyDotNetMemoryCacheRepository(MemoryCache memoryCache)
        {
            _lazyCacheHolder = memoryCache != null
                ? new Lazy<MemoryCache>(() => memoryCache)
                : new Lazy<MemoryCache>(InitializeCacheInternal);
        }

        protected virtual MemoryCache InitializeCacheInternal()
            => new MemoryCache(nameof(LazyDotNetMemoryCacheRepository));

        public object AddOrGetExisting(string key, object value, CacheItemPolicy cacheItemPolicy) 
            => _lazyCacheHolder.Value.AddOrGetExisting(key, value, cacheItemPolicy);

        public void Remove(string key) => _lazyCacheHolder.Value.Remove(key);

        public void ClearAll()
        {
            var existingLazy = _lazyCacheHolder;
            try
            {
                //To Clear efficiently we First Replace/Reset the Lazy Cache Holder...
                //NOTE: We rely on the Lazy to then control thread safety as our work consists of ONLY swapping the Reference to an uninitialized Lazy<>!
                _lazyCacheHolder = new Lazy<MemoryCache>(InitializeCacheInternal);
            }
            finally
            {
                //Now other threads can leverage the newly initialized Lazy while we dispose of the prior one;
                //  of which the Disposal should Clear all entries and release resources!
                existingLazy.Value.Dispose();
                existingLazy = null;
            }
        }

        public long CacheEntryCount() => _lazyCacheHolder.Value.GetCount();

        public bool CacheItemExists(string key) => _lazyCacheHolder.Value.Contains(key);

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.ClearAll();
                var memoryCache = _lazyCacheHolder.Value;
                memoryCache?.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
