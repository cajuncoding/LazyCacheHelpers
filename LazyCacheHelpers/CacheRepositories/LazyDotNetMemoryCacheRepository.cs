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
    public class LazyDotNetMemoryCacheRepository : ILazyCacheRepository
    {
        private readonly MemoryCache _cache;

        public LazyDotNetMemoryCacheRepository()
            : this(null)
        {
        }

        public LazyDotNetMemoryCacheRepository(MemoryCache memoryCache)
        {
            _cache = memoryCache ?? MemoryCache.Default;
        }

        public object AddOrGetExisting(string key, object value, CacheItemPolicy cacheItemPolicy)
        {
            return _cache.AddOrGetExisting(key, value, cacheItemPolicy);
        }

        public void Remove(string key)
        {
            _cache.Remove(key);
        }
    }
}
