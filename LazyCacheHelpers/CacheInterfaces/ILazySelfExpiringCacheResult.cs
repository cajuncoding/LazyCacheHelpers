using System.Runtime.Caching;

namespace LazyCacheHelpers
{
    public interface ILazySelfExpiringCacheResult<out TValue>
    {
        CacheItemPolicy CachePolicy { get; }
        TValue CacheItem { get; }
    }
}