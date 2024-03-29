﻿using System;
using System.Collections.Concurrent;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Caching;
using System.Runtime.CompilerServices;

namespace LazyCacheHelpers
{
    /// <summary>
    /// BBernard
    /// Original Source (MIT License): https://github.com/cajuncoding/LazyCacheHelpers
    /// 
    /// Class to support reading cache configuration values from Configuration (e.g. TTL Seconds defined in Configuration).
    /// </summary>
    public class LazyCacheConfig
    {
        private static Func<string, string> _readConfigValueDelegateFunc = null;

        public static void BootstrapConfigValueReader(Func<string, string> configValueReaderFunc)
        {
            _readConfigValueDelegateFunc = configValueReaderFunc ?? throw new ArgumentNullException(nameof(configValueReaderFunc));
        }

        //Provide a reference to the Default Minimum Cache TTL we enforce; to make code more readable.
        public static readonly TimeSpan DefaultMinimumCacheTTL = TimeSpan.FromSeconds(60);
        //Provide a reference to Never Cache TTL; to make code more readable.
        public static readonly TimeSpan NeverCacheTTL = TimeSpan.Zero;
        //Provide a reference to Disabled Cache TTL; to make code more readable.
        public static readonly TimeSpan MissingCacheTTL = TimeSpan.MinValue;
        //Provide a reference to the Maximum/Forever Cache TTL; to make code more readable.
        public static readonly TimeSpan ForeverCacheTTL = TimeSpan.MaxValue;

        //ConcurrentDictionary to hold all ThreadSafe Lazy initializers for CacheTTL values retrieved from Configuration
        //NOTE: This is NOT the same as caching dynamic data, this is only for caching Configuration values that will remain
        //      in memory and static for the duration of the Application lifecycle.  
        //NOTE: The number and size of these values are are finite and/or known therefore this is the most efficient way to 
        //      manage them for Performance and Thread Safety; this does NOT support Garbage Collection or Resource Reclaiming 
        //      as the .Net MemoryCache would.
        private static readonly ConcurrentDictionary<string, Lazy<TimeSpan>> _cacheTTLDictionary = new ConcurrentDictionary<string, Lazy<TimeSpan>>();

        /// <summary>
        /// Initialize the Cache TTL Seconds from Configuration in a fully ThreadSafe way by finding the configuration value
        /// of the first valid config key specified; searching in the order defined int the array.
        /// </summary>
        /// <param name="configKeysToSearch"></param>
        /// <param name="defaultMinimumTTL"></param>
        /// <returns></returns>
        public static TimeSpan GetCacheTTLFromConfig(string[] configKeysToSearch, TimeSpan defaultMinimumTTL)
        {
            foreach (var configKey in configKeysToSearch)
            {
                var timeSpanToLive = LazyCacheConfig.GetCacheTTLFromConfig(configKey, LazyCacheConfig.MissingCacheTTL);
                if (timeSpanToLive == NeverCacheTTL || timeSpanToLive.TotalMilliseconds > 0)
                {
                    return timeSpanToLive;
                }
            }

            return defaultMinimumTTL;
        }

        /// <summary>
        /// Initialize the internal Cache of TTL Seconds from Configuration in a fully ThreadSafe way, with fallback logic for when the 
        ///     value does not exist or is less than the Default Minimum Value specified.
        /// NOTE: We do this so that we don't have to read the Configuration every time we retrieve the TTL Settings.
        /// NOTE: We don't have to worry about Manual locking because we use the ConcurrentDictionary in combination with Lazy<> thread-safe
        ///         initialization to Guarantee that the initialization code is only every called once, while offering great performance
        ///         for all subsequent readers of the value!
        /// </summary>
        /// <param name="configName"></param>
        /// <param name="defaultMinimumTTL"></param>
        /// <returns></returns>
        public static TimeSpan GetCacheTTLFromConfig(string configName, TimeSpan defaultMinimumTTL)
        {
            //The ConcurrentDictionary and Lazy<> initializer provide complete Thread Safety and Guarantee that the code in
            //  our value factory lambda method ONLY EVER RUNS ONCE!
            //NOTE: We don't have to worry about Manual locking because we use the ConcurrentDictionary in combination with Lazy<> thread-safe
            //        initialization to Guarantee that the initialization code is only every called once, while offering great performance
            //        for all subsequent readers of the value!
            var cacheTTLLazyInitializer = _cacheTTLDictionary.GetOrAdd(configName, key =>
                new Lazy<TimeSpan>(() =>
                {
                    var configTTLSeconds = SafelyReadTTLConfigValue(configName);

                    //Return the final TimeSpan after computing the value from Configuration
                    return configTTLSeconds;
                })
            );

            //Always enforce our own internal Default Minimum cache value, which the caller can specify
            //  to be anything they want for advanced use-case logic.
            var cacheTimeToLiveSeconds = cacheTTLLazyInitializer.Value;

            //Because we use a Thread safe Lazy Initializer we must call the Value property to get the value!
            return MaxTimeSpan(cacheTimeToLiveSeconds, defaultMinimumTTL);
        }

        #region Private Helpers

        /// <summary>
        /// Private Helper method to read the value from configuration and parse it safely as an Int (TTL in Seconds).
        /// </summary>
        /// <param name="configKeyName"></param>
        /// <returns></returns>
        private static TimeSpan SafelyReadTTLConfigValue(String configKeyName)
        {
            var configValue = _readConfigValueDelegateFunc?.Invoke(configKeyName) ?? null;

            //If not defined return MissingCacheTTL
            if (string.IsNullOrWhiteSpace(configValue))
            {
                return LazyCacheConfig.MissingCacheTTL;
            }
            //If it exists but contains a keyword set to 'off' or a negative value then return NeverCacheTTL
            else if (configValue.Equals("off", StringComparison.OrdinalIgnoreCase))
            {
                return LazyCacheConfig.NeverCacheTTL;
            }
            //If it exists and contains a colon ':' then parse the TimeSpan
            else if (configValue.Contains(":"))
            {
                TimeSpan.TryParse(configValue, out TimeSpan ttlTimeSpan);
                return ttlTimeSpan;
            }
            //If it exists, is not a keyword, and does not contain a Colon ':' then parse as Integer Seconds
            else
            {
                int.TryParse(configValue, out int ttlSeconds);
                var parsedTimeSpan = TimeSpan.FromSeconds(ttlSeconds);

                //Return the Max value between the parsed value and NeverCacheTTL value...
                //NOTE: If the parsed value is negative (e.g. less than 0) then we just return NeverCacheTTL.
                return MaxTimeSpan(parsedTimeSpan, LazyCacheConfig.NeverCacheTTL);
            }
        }

        private static TimeSpan MaxTimeSpan(params TimeSpan[] timeSpanItems)
        {
            var maxTimeSpan = timeSpanItems.Max();
            return maxTimeSpan;
        }

        #endregion

    }
}
