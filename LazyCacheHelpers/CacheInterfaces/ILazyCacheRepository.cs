using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Caching;

namespace LazyCacheHelpers
{
    /// <summary>
    /// BBernard
    /// Original Source (MIT License): https://github.com/cajuncoding/LazyCacheHelpers
    /// 
    /// Interface facade for the underlying Cache repository to be utilized.  This de-couples the LazyCacheHandler
    /// class from specific caching implementations and allows new LazyCacheHandlers to be created supporting
    /// a variety of caching implementations such as in-memory, distributed, cloud based, etc.
    /// </summary>
    public interface ILazyCacheRepository
    {
        object AddOrGetExisting(string key, object value, CacheItemPolicy cacheItemPolicy);
        void Remove(string key);
        void ClearAll();
        long CacheEntryCount();
    }
}
