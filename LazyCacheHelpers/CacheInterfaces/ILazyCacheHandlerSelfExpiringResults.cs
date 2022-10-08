using System;
using System.Runtime.Caching;
using System.Threading.Tasks;

namespace LazyCacheHelpers
{
    /// <summary>
    /// BBernard
    /// Original Source (MIT License): https://github.com/cajuncoding/LazyCacheHelpers
    /// 
    /// Public Interface for the Lazy Cache Handler which adds support for the value factories to be self-managing/self-expiring
    /// by returning both the result and the policy!  This is very useful in use cases such as loading Auth Tokens or other
    /// data from external APIs whereby the API response determines how long the result is valid for, and therefore it does
    /// not need to be refreshed/reloaded until that time has lapsed. So the logic can build a Cache Expiration policy based
    /// on the information in the response and create a valid CachePolicy.
    ///
    /// NOTE: This is intended to be an extension of the base ILazyCacheHandler, but is separate now as a composable interface to
    /// to mitigate potentially breaking existing custom implementations of the ILazyCacheHandler.
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    public interface ILazyCacheHandlerSelfExpiringResults<TValue>
    {
        TValue GetOrAddFromCache<TKey>(TKey key, Func<ILazySelfExpiringCacheResult<TValue>> fnValueFactory);
        Task<TValue> GetOrAddFromCacheAsync<TKey>(TKey key, Func<Task<ILazySelfExpiringCacheResult<TValue>>> fnAsyncValueFactory);
    }
}