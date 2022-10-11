using System;
using System.Collections.Concurrent;
using System.Runtime.Caching;

namespace LazyCacheHelpers
{
    /// <summary>
    /// BBernard
    /// Original Source (MIT License): https://github.com/cajuncoding/LazyCacheHelpers
    /// 
    /// Facade implementation for Local in-memory Dictionary based static cache for the LazyCacheHandler.
    /// This allows lazy initialization and self-populating flow to be implemented on top of a very lightweight
    /// simplified dictionary based cache storage mechanism.
    /// 
    /// NOTE: This cache mechanism has limited support for cache policies and has
    ///         no support for Garbage Collection or memory pressure re-claiming of resources.
    /// NOTE: This currently supports ONLY AbsoluteExpiration based cache item policy.
    /// </summary>
    public class LazyDictionaryCacheRepository : ILazyCacheRepository, IDisposable
    {
        private readonly ConcurrentDictionary<string, DictionaryCacheEntry> _cacheDictionary = new ConcurrentDictionary<string, DictionaryCacheEntry>();

        public object AddOrGetExisting(string key, object value, CacheItemPolicy cacheItemPolicy)
        {
            var newEntry = new DictionaryCacheEntry(value, cacheItemPolicy);

            var resultEntry = _cacheDictionary.AddOrUpdate(key, newEntry, (existingKey, existingEntry)
                //Manually enforce Absolute Expiration from the CachePolicy to support Expiring data on retrieve/update
                => existingEntry?.CachePolicy?.AbsoluteExpiration > DateTimeOffset.UtcNow
                        ? existingEntry
                        : newEntry
            );

            return resultEntry.Value;
        }

        public void Remove(string key) => _cacheDictionary.TryRemove(key, out _);

        public void ClearAll() => _cacheDictionary.Clear();

        public long CacheEntryCount() => _cacheDictionary.Count;

        private class DictionaryCacheEntry
        {
            public DictionaryCacheEntry(object value, CacheItemPolicy cacheItemPolicy)
            {
                this.Value = value;
                this.CachePolicy = cacheItemPolicy;
            }

            internal object Value { get; }
            internal CacheItemPolicy CachePolicy { get; }
        }

        public bool CacheItemExists(string key) => _cacheDictionary.ContainsKey(key);

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                ClearAll();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
