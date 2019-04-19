using System;
using System.Threading.Tasks;
using LazyCacheHelpers;

namespace LazyCacheHelpersTests
{
    /// <summary>
    /// BBernad
    /// Original Source (MIT License): https://github.com/raerae1616/LazyCacheHelpers
    /// 
    /// Sample Facade class to illustrate how a real applicaion might utilize LazyCacheHelpers classes in
    /// their own context with a CacheHelper that wraps it up and handles things like Cache Configurations,
    /// Cache Keys, etc. with a more simplified Facade interface to de-couple and simplify the rest of the
    /// consuming code within the application.
    /// </summary>
    public class TestCacheFacade
    {
        public static string GetCachedData(TestCacheParams cacheParams, Func<string> fnValueFactory, int secondsTTL = 60)
        {
            var result = DefaultLazyCache.GetOrAddFromCache(cacheParams, fnValueFactory, cacheParams);
            return result;
        }

        public static async Task<string> GetCachedDataAsync(TestCacheParams cacheParams, Func<Task<string>> fnValueFactory)
        {
            var result = await DefaultLazyCache.GetOrAddFromCacheAsync<ILazyCacheKey, string>(
                cacheParams,
                //NOTE: We wrap the value factory Func in a new Lambda to allow enable it to be 
                //      dynamically down cast to Func<Task<object>> of the underlying generic cache!
                async () => {
                    var factoryResult = await fnValueFactory();
                    return (object)factoryResult;
                },
                cacheParams
            );

            return result;
        }

        public static void RemoveCachedData(string cacheKeyVariable)
        {
            var cacheKey = new TestCacheParams(cacheKeyVariable);
            DefaultLazyCache.RemoveFromCache(cacheKey);
        }
    }


}
