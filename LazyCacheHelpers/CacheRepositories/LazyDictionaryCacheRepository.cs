using System;
using System.Collections.Concurrent;
using System.Runtime.Caching;

namespace LazyCacheHelpers
{
    /// <summary>
    /// BBernard
    /// Facade implementaiton for Local in-memory Dictionary based static cache for the LazyCacheHandler.
    /// This allows lazy initialization and self-populating flow to be implemented on top of a very lightweight
    /// simplified dictionary based cache storage mechanism.
    /// 
    /// NOTE: This cache mechanism has limited support for cache policies and has
    ///         no support for Garbage Collection or memory pressure re-claiming of resources.
    /// NOTE: This currenly supports ONLY AbsoluteExpiration based cache item policy.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class LazyDictionaryCacheRepository : ILazyCacheRepository
    {
        ConcurrentDictionary<string, DictionaryCacheEntry> _cacheDictionary = new ConcurrentDictionary<String, DictionaryCacheEntry>();

        public object AddOrGetExisting(string key, object value, CacheItemPolicy cacheItemPolicy)
        {
            var newEntry = new DictionaryCacheEntry(key, value, cacheItemPolicy);

            var resultEntry = _cacheDictionary.AddOrUpdate(key, newEntry, (existingKey, existingEntry) =>
            {
                //Manually enforce Absolute Expiration from the CachePolicy to support Expiring data on retrieve/update
                return existingEntry?.CachePolicy?.AbsoluteExpiration > DateTimeOffset.UtcNow
                        ? existingEntry
                        : newEntry;
            });

            return resultEntry.Value;
        }

        public void Remove(string key)
        {
            DictionaryCacheEntry removedEntry = null;
            _cacheDictionary.TryRemove(key, out removedEntry);
        }

        private class DictionaryCacheEntry
        {
            public DictionaryCacheEntry(string key, object value, CacheItemPolicy cacheItemPolicy)
            {
                this.CacheKey = key;
                this.Value = value;
                this.CachePolicy = cacheItemPolicy;
            }

            public string CacheKey { get; private set; }
            public object Value { get; private set; }
            public CacheItemPolicy CachePolicy { get; private set; }
        }

    }
}
