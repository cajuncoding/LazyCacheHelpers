using System;
using System.Threading.Tasks;
using System.Runtime.Caching;

namespace LazyCacheHelpers
{
    /// <summary>
    /// BBernard
    /// Original Source (MIT License): https://github.com/cajuncoding/LazyCacheHelpers
    /// 
    /// LazyCache Handler for easy cache implementations at all layers of an application with support for both Sync & Async
    /// Lazy operations to maximize server utilization and performance!
    /// 
    /// This Handler supports changing the underlying Cache Repository with different implementations via ILazyCacheRepository implementation.
    /// 
    /// This class provides a completely ThreadSafe cache with Lazy Loading capabilities in an easy to use implementation that can work at 
    ///     all levels of an application (classes, controllers, etc.).
    ///     
    /// The use of Lazy<T> loading facilitates self-populating cache so that the long running processes are never 
    ///     executed more than once.  Even if they are triggered at the exact same time, no more than one thread will
    ///     ever perform the work, dramatically decreasing server utilization under high load.
    ///     
    /// NOTE: A wrapper implementation for MemoryCache is implemented (via default ICacheRepository implementation as LazyDotNetMemoryCacheRepository) to 
    ///     make working with MemoryCache with greatly simplified support for self-populating (Lazy) initialization.
    /// </summary>
    public class LazyCacheHandler<TValue> : ILazyCacheHandler<TValue> where TValue : class
    {
        //Added methods to CacheHelper to work with MemoryCache more easily.
        //NOTE: .Net MemoryCache supports this does NOT support Garbage Collection and Resource Reclaiming so it should
        //      be used whenever caching dynamic runtime data.
        private readonly ILazyCacheRepository _cacheRepository;

        /// <summary>
        /// Initializes with the specified cacheRepository; if null is specified then DotNetMemoryCacheRepository (leveraging MemoryCache.Default) is used.
        /// </summary>
        public LazyCacheHandler(ILazyCacheRepository cacheRepository)
        {
            _cacheRepository = cacheRepository ?? new LazyDotNetMemoryCacheRepository();
        }

        /// <summary>
        /// Initializes with default DotNetMemoryCacheRepository (leveraging MemoryCache.Default).
        /// </summary>
        public LazyCacheHandler()
            : this(null)
        {}

        /// <summary>
        /// BBernard
        /// A wrapper implementation for ICacheRepository to make working with Thread Safety significantly easier.
        /// This provides completely ThreadSafe cache with Lazy Loading capabilities in an easy to use function; 
        /// Lazy<typeparamref name="TValue"/> loading facilitates self-populating cache so that the long running processes are never 
        /// executed more than once, even if they are triggered at approx. the same time.
        /// 
        /// This method handles the Add or Retrieval of an item from Memory Cache using a Lambda initialization function that will
        /// only ever be executed once for the Cache Key provided; by leveraging the .Net Lazy<> initializer. 
        /// 
        /// More information on this Cache design pattern for ease of use can be found here:
        /// https://blog.falafel.com/working-system-runtime-caching-memorycache/
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="fnValueFactory"></param>
        /// <param name="cacheItemPolicy"></param>
        /// <returns></returns>
        public virtual TValue GetOrAddFromCache<TKey>(TKey key, Func<TValue> fnValueFactory, CacheItemPolicy cacheItemPolicy)
        {
            //We support either ILazyCacheKey interface or any object for the Cache Key as long as it's ToString() 
            //  implementation creates a valid unique Key for us, so here we initialize the Cache Key to use.
            string cacheKey = GenerateCacheKeyHelper(key);

            //We support a lambda initializer function to initialize the Cached Item value if it does not already exist, but we
            //must ensure it only ever run's one time for the Cache Key provided, so we leverage the .Net Lazy<> initializer which
            //guarantees that this code will be Thread Safe and Lazily initialized to only run one time!
            var newValueLazyInitializer = new Lazy<TValue>(fnValueFactory);

            //Using .Net Memory Cache (which supports garbage collection and resource re-claim as needed) we use the Thread safe
            //AddOrGetExisting() method to efficiently initialize and/or retrieve our item from Cache.
            //NOTE: Because Unknown code may run during the Initialization and execution of the Lambda function, we must
            //      provide Error Trapping Here to minimize impact to Cache and propagate the error up to calling code!
            var existingCachedLazyInitializer = _cacheRepository.AddOrGetExisting(cacheKey, newValueLazyInitializer, cacheItemPolicy) as Lazy<TValue>;

            try
            {
                //NOTE: We use the cached existing initializer if it is not null, but fall back to the new one.  
                //      This is very important because it is the existing Cached initializer that will have already executed
                //      the value factory lambda/code and therefore guarantees that our initialization code only runs once.
                //      Only if null do we use the new Lazy<> created that was also just added to the Cache, executing it's 
                //      lambda code for the first and only time!
                return (existingCachedLazyInitializer ?? newValueLazyInitializer).Value;
            }
            catch
            {
                // Handle cached lazy exception by evicting from cache. Thanks to Denis Borovnev for pointing this out!
                _cacheRepository.Remove(cacheKey);
                throw;
            }
        }

        /// <summary>
        /// BBernard
        /// 
        /// An Async wrapper implementation for using ICacheRepository to make working with Thread Safety for 
        /// Asynchronous processes significantly easier. This provides completely ThreadSafe Async cache with Lazy Loading capabilities 
        /// in an easy to use function; Lazy<typeparamref name="TValue"/> loading facilitates self-populating cache so that the long running processes are never 
        /// executed more than once, even if they are triggered at approx. the same time.
        /// 
        /// This method handles the Asynchronous Add or Retrieval of an item from Memory Cache using a Lambda initialization function that will
        /// only ever be executed once for the Cache Key provided; by leveraging the an AsyncLazy<> initializer. 
        /// 
        /// NOTE: Using .Net MemoryCache via LazyDotNetMemoryCacheRepository we support garbage collection and resource re-claims as needed
        /// NOTE: We also prevent negative caching so exceptions thrown during long running processes are not cached!
        /// 
        /// More information on this Cache design pattern for ease of use can be found here:
        /// https://blog.falafel.com/working-system-runtime-caching-memorycache/
        /// https://cpratt.co/thread-safe-strongly-typed-memory-caching-c-sharp/
        /// 
        /// BBernard - 03/06/2018
        /// NOTE: WE DO NOT IMPLEMENT AsyncLazy() in the same was as the above blogs because it results in the process ALWAYS running on
        ///         background threads using Task.Factory.StartNew (e.g. Task.Run is the best practice), but in our caching we don't
        ///         want to force anything to run on different threads, instead we simply want to ensure that we correctly support
        ///         the Async/Await Task Based Asynchronous pattern from top to bottom and even with our caching!
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="fnAsyncValueFactory"></param>
        /// <param name="cacheItemPolicy"></param>
        /// <returns></returns>
        public virtual async Task<TValue> GetOrAddFromCacheAsync<TKey>(TKey key, Func<Task<TValue>> fnAsyncValueFactory, CacheItemPolicy cacheItemPolicy)
        {
            //We support eitehr ILazyCacheKey interface or any object for the Cache Key as long as it's ToString() 
            //  implementation creates a valid unique Key for us, so here we initialize the Cache Key to use.
            string cacheKey = GenerateCacheKeyHelper(key);

            //BBernard - 03/06/2018
            //We support a lambda initializer function to initialize the Cached Item value if it does not already exist, but we
            //must ensure it only ever run's one time for the Cache Key provided, so we leverage the .Net Lazy<> initializer which
            //guarantees that this code will be Thread Safe and Lazily initialized to only run one time!
            var newValueAsyncLazyInitializer = new Lazy<Task<TValue>>(fnAsyncValueFactory);

            //BBernard - 03/06/2018
            //Using .Net Memory Cache (which supports garbage collection and resource re-claim as needed) we use the Thread safe
            //AddOrGetExisting() method to efficiently initialize and/or retrieve our item from Cache.
            //NOTE: Because Unknown code may run during the Initialization and execution of the Lambda function, we must
            //      provide Error Trapping Here to minimize impact to Cache and propagate the error up to calling code!
            var existingCachedAsyncLazyInitializer = _cacheRepository.AddOrGetExisting(cacheKey, newValueAsyncLazyInitializer, cacheItemPolicy) as Lazy<Task<TValue>>;
            try
            {
                //BBernard - 03/06/2018
                //NOTE: We use the cached existing initializer if it is not null, but fall back to the new one.  
                //      This is very important because it is the existing Cached initializer that will have already executed
                //      the value factory lambda/code and therefore guarantees that our initialization code only runs once.
                //      Only if null do we use the new Lazy<> created that was also just added to the Cache, executing it's 
                //      lambda code for the first and only time!
                Task<TValue> asyncResultTask = (existingCachedAsyncLazyInitializer ?? newValueAsyncLazyInitializer).Value;

                //BBernard - 03/06/2018
                //NOTE: Since this is an Async process we need to validate that the Task<T> is not faulted or in an exception state
                //      because we do NOT allow negative caching (e.g. caching of failed results).
                if (asyncResultTask == null || asyncResultTask.IsFaulted || asyncResultTask.IsCanceled)
                {
                    _cacheRepository.Remove(cacheKey);
                }

                //BBernard - 03/06/2018
                //FINALLY we await the actual Task before returning, this ensures that the Async/Await pattern is correctly
                //followed, allowing the Task to execute before continuing, which ensures that our Try/Catch block error handling
                //is fully supported for Async Tasks!
                return await asyncResultTask;
            }
            catch
            {
                //BBernard
                //No Exceptions should be cached, so we handle cached lazy exceptions by evicting from cache!
                _cacheRepository.Remove(cacheKey);
                throw;
            }
        }

        /// <summary>
        /// BBernard
        /// 
        /// Remove the item corresponding to the specified Cache Key from the Cache.
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="key"></param>
        public virtual void RemoveFromCache<TKey>(TKey key)
        {
            //We support either ILazyCacheKey interface or any object for the Cache Key as long as it's ToString() 
            //  implementation creates a valid unique Key for us, so here we initialize the Cache Key to use.
            var cacheKey = GenerateCacheKeyHelper(key);

            //Remove the Item from the underlying Cache Repository
            _cacheRepository.Remove(cacheKey);
        }

        #region Private Helpers

        /// <summary>
        /// Dynamically determine the Cache Key from the specified object that implements either 
        /// ILazyCacheKey or provides a ToString() method that renders a valid Cache Key.
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="cacheKeyGenerator"></param>
        /// <returns></returns>
        protected virtual string GenerateCacheKeyHelper<TKey>(TKey cacheKeyGenerator)
        {
            //If Null then throw Argument Exception...
            if (cacheKeyGenerator == null) throw new ArgumentNullException(
                nameof(cacheKeyGenerator), 
                $"The cache key object is null; a valid object that implements either {nameof(ILazyCacheKey)} or ToString() override must be specified."
            );

            var generatedKey = (cacheKeyGenerator as ILazyCacheKey)?.GenerateKey() ?? cacheKeyGenerator.ToString();
            return generatedKey;
        }

        #endregion

    }
}
