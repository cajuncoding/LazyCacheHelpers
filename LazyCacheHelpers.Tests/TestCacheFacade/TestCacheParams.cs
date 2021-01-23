using LazyCacheHelpers;
using System;
using System.Runtime.Caching;

namespace LazyCacheHelpersTests
{
    public class TestCacheParams : ILazyCacheParams
    {
        private readonly string _variableName;
        private readonly TimeSpan _ttlOverrideTimeSpan;
        private readonly CacheItemPolicy _overrideCacheItemPolicy;

        /// <summary>
        /// Create Cache Params with default TTL dynamically loaded from App Config.
        /// </summary>
        /// <param name="keyNameVariable"></param>
        /// <param name="overrideCachePolicy"></param>
        public TestCacheParams(string keyNameVariable, CacheItemPolicy overrideCachePolicy = null)
        {
            _variableName = keyNameVariable ?? string.Empty;
            _overrideCacheItemPolicy = overrideCachePolicy;
        }

        /// <summary>
        /// Provide Overload that allows specifying an Override for the TTL
        /// </summary>
        /// <param name="keyNameVariable"></param>
        /// <param name="secondsTTL"></param>
        public TestCacheParams(string keyNameVariable, int secondsTTL)
        {
            _variableName = keyNameVariable ?? string.Empty;
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

            if (_overrideCacheItemPolicy != null)
            {
                return _overrideCacheItemPolicy;
            }
            else if (_ttlOverrideTimeSpan == TimeSpan.Zero)
            {
                return LazyCachePolicyFromConfig.NewAbsoluteExpirationPolicy(configKeys);
            }
            else
            {
                return LazyCachePolicy.NewAbsoluteExpirationPolicy(_ttlOverrideTimeSpan);
            }
        }
    }
}
