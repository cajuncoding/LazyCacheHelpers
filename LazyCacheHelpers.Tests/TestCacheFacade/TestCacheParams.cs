using LazyCacheHelpers;
using System;
using System.Runtime.Caching;

namespace LazyCacheHelpersTests
{
    public class TestCacheParams : ILazyCacheParams
    {
        private string _variableName = String.Empty;
        private TimeSpan _ttlOverrideTimeSpan = TimeSpan.Zero;
        private CacheItemPolicy _overrdieCacheItemPolicy = null;

        /// <summary>
        /// Create Cache Params with default TTL dynamically loaded from App Config.
        /// </summary>
        /// <param name="keyNameVariable"></param>
        public TestCacheParams(String keyNameVariable, CacheItemPolicy overrideCachePolicy = null)
        {
            _variableName = keyNameVariable;
            _overrdieCacheItemPolicy = overrideCachePolicy;
        }

        /// <summary>
        /// Provide Overload that allows specifying an Override for the TTL
        /// </summary>
        /// <param name="keyNameVariable"></param>
        /// <param name="secondsTTL"></param>
        public TestCacheParams(String keyNameVariable, int secondsTTL)
        {
            _variableName = keyNameVariable;
            _ttlOverrideTimeSpan = TimeSpan.FromSeconds(secondsTTL);
        }

        public string GenerateKey()
        {
            return $"{nameof(TestCacheParams)}::{this._variableName}";
        }

        public CacheItemPolicy GeneratePolicy()
        {
            var configKeys = new string[] {
                $"CacheTTL.{Guid.NewGuid()}.NEVER_FOUND",
                $"CacheTTL.{nameof(TestCacheParams)}",
                $"CacheTTL.Default"
            };

            //Compute/Load the TTL from Configuration or from static class values, etc.
            //NOTE: During high load the cache timings could be Distributed to prevent multiple misses at one time.
            //var timeSpanTTL = TimeSpan.FromSeconds(300);
            //var timeSpanTTL = LazyCachePolicy.RandomizeCacheTTLDistribution(TimeSpan.FromSeconds(secondsTTL), 60);
            //var timeSpanTTL = LazyCacheConfig.GetCacheTTLFromConfig(configKey);

            if (_overrdieCacheItemPolicy != null)
            {
                return _overrdieCacheItemPolicy;
            }
            else if (_ttlOverrideTimeSpan == TimeSpan.Zero)
            {
                return LazyCachePolicy.NewAbsoluteExpirationPolicy(configKeys);
            }
            else
            {
                return LazyCachePolicy.NewAbsoluteExpirationPolicy(_ttlOverrideTimeSpan);
            }
        }
    }
}
