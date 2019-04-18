using System;
using System.Linq;
using System.Collections.Concurrent;
using System.Configuration;

namespace LazyCacheHelpers
{
    /// <summary>
    /// BBernard
    /// Original Source (MIT License): https://github.com/raerae1616/LazyCacheHelpers
    /// 
    /// Class to support reading cache configuration values from Configuration (e.g. TTL Seconds defined in Configuration).
    /// </summary>
    public class LazyCacheConfig
    {
        //Provide a reference to the Default Minimum Cache TTL we enforce; to make code more readable.
        public static readonly TimeSpan DefaultMinimumCacheTTL = TimeSpan.FromSeconds(60);
        //Provide a reference to Never Cache TTL; to make code more readable.
        public static readonly TimeSpan NeverCacheTTL = TimeSpan.FromSeconds(0);
        //Provide a reference to the Maximum/Forever Cache TTL; to make code more readable.
        public static readonly TimeSpan ForeverCacheTTL = TimeSpan.MaxValue;

        //ConcurrentDictionary to hold all ThreadSafe Lazy initializers for CacheTTL values retrieved from Configuration
        //NOTE: This is NOT the same as caching dynamic data, this is only for caching Configuration values that will remain
        //      in memory and static for the duration of the Application lifecycle.  
        //NOTE: The number and size of these values are are finite and/or known therefore this is the most efficient way to 
        //      manage them for Performance and Thread Safety; this does NOT support Garbage Collection or Resource Reclaiming 
        //      as the .Net MemoryCache would.
        private static ConcurrentDictionary<string, Lazy<TimeSpan>> _cacheTTLDictionary = new ConcurrentDictionary<string, Lazy<TimeSpan>>();

        /// <summary>
        /// Initialize the Cache TTL Seconds from Configuration in a fully ThreadSafe way.
        /// NOTE: We don't have to worry about Manual locking because we use the ConcurrentDictionary in combination with Lazy<> threadsafe
        ///         initializers to Guarantee that the initialization code is only every called once, while offerring great perfromance
        ///         for all subsequent readers of the value!
        /// </summary>
        /// <param name="configName"></param>
        /// <returns></returns>
        public static TimeSpan GetCacheTTLFromConfig(string configName)
        {
            return GetCacheTTLFromConfig(configName, DefaultMinimumCacheTTL);
        }

        /// <summary>
        /// Initialize the internal Cache of TTL Seconds from Configuration in a fully ThreadSafe way, with fallback logic for when the 
        ///     value does not exist or is less than the Default Minimum Value specified.
        /// NOTE: We do this so that we don't have to read the Configuration every time we retrieve the TTL Settings.
        /// NOTE: We don't have to worry about Manual locking because we use the ConcurrentDictionary in combination with Lazy<> threadsafe
        ///         initializers to Guarantee that the initialization code is only every called once, while offerring great perfromance
        ///         for all subsequent readers of the value!
        /// </summary>
        /// <param name="configName"></param>
        /// <returns></returns>
        public static TimeSpan GetCacheTTLFromConfig(string configName, TimeSpan defaultMinimumTTL)
        {
            //The ConcurrentDictionary and Lazy<> initializer provide complete Thread Safety and Guarantee that the code in
            //  our value factory lambda method ONLY EVER RUNS ONCE!
            //NOTE: We don't have to worry about Manual locking because we use the ConcurrentDictionary in combination with Lazy<> threadsafe
            //        initializers to Guarantee that the initialization code is only every called once, while offerring great perfromance
            //        for all subsequent readers of the value!
            var cacheTTLLazyInitializer = _cacheTTLDictionary.GetOrAdd(configName, key =>
                new Lazy<TimeSpan>(() =>
                {
                    var configTTLSeconds = SafelyReadTTLSecondsConfigValue(configName);

                    //Always enforce our own internal Default Minimum cache value, which the caller can specify
                    //  to be anything they want for advanced use-case logic.
                    var cacheTimeToLiveSeconds = Math.Max(configTTLSeconds, defaultMinimumTTL.TotalSeconds);

                    //Return the final TimeSpan after computing the value from Configuration
                    return TimeSpan.FromSeconds(cacheTimeToLiveSeconds);
                })
            );

            //Because we use a Thread safe Lazy Initializer we must call the Value property to get the value!
            return cacheTTLLazyInitializer.Value;
        }

        #region Private Helpers

        /// <summary>
        /// Private Helper method to read the value from configuraiton and parse it safely as an Int (TTL in Seconds).
        /// </summary>
        /// <param name="configKeyName"></param>
        /// <returns></returns>
        private static int SafelyReadTTLSecondsConfigValue(String configKeyName)
        {
            var appSettings = ConfigurationManager.AppSettings;
            String configValue = appSettings[configKeyName];

            int ttlSeconds = int.MinValue;
            if (!String.IsNullOrEmpty(configValue))
            {
                Int32.TryParse(configValue, out ttlSeconds);
            }

            return ttlSeconds;
        }

        #endregion

    }
}
