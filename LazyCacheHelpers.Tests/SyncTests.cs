using LazyCacheHelpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Caching;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace LazyCacheHelpersTests
{
    [TestClass]
    public class SyncTests
    {
        /// <summary>
        /// BBernard
        /// Original Source (MIT License): https://github.com/cajuncoding/LazyCacheHelpers
        /// 
        /// Synchronous Tests for LazyCacheHelpers
        /// 
        /// Unit Tests for LazyCacheHandler classes to demo and validate functionality
        /// 
        /// </summary>
        [TestMethod]
        public void TestCacheHits()
        {
            string key = $"CachedDataWithSameKey[{nameof(TestCacheHits)}]";
            var result1 = GetTestDataWithCaching(key);
            var result2 = GetTestDataWithCaching(key);
            var result3 = GetTestDataWithCaching(key);
            var result4 = GetTestDataWithCaching(key);
            
            Assert.AreEqual(result1, result2);
            Assert.AreSame(result1, result2);

            Assert.AreEqual(result3, result4);
            Assert.AreSame(result3, result4);

            //Compare First and Last to ensure that ALL are the same!
            Assert.AreEqual(result1, result4);
            Assert.AreSame(result1, result4);
        }

        [TestMethod]
        public void TestCacheHitsForSelfExpiringResults()
        {
            string key = $"CachedDataWithSameKey[{nameof(TestCacheHitsForSelfExpiringResults)}]";

            var result1 = GetTestDataWithCachingForSelfExpiringResults(key);
            var result2 = GetTestDataWithCachingForSelfExpiringResults(key);
            var result3 = GetTestDataWithCachingForSelfExpiringResults(key);
            var result4 = GetTestDataWithCachingForSelfExpiringResults(key);

            Assert.AreEqual(result1, result2);
            Assert.AreSame(result1, result2);

            Assert.AreEqual(result3, result4);
            Assert.AreSame(result3, result4);

            //Compare First and Last to ensure that ALL are the same!
            Assert.AreEqual(result1, result4);
            Assert.AreSame(result1, result4);
        }

        [TestMethod]
        public void TestCacheMisses()
        {
            int c = 0;
            string key = $"CachedDataWithDifferentKey[{nameof(TestCacheMisses)}]";
            var results = new List<string>()
            {
                GetTestDataWithCaching($"{key}[{++c}]"),
                GetTestDataWithCaching($"{key}[{++c}]"),
                GetTestDataWithCaching($"{key}[{++c}]"),
                GetTestDataWithCaching($"{key}[{++c}]")
            };

            var distinctCount = results.Distinct().Count();

            //Ensure that ALL Items are Distinctly Different!
            Assert.AreEqual(results.Count, distinctCount);
        }

        [TestMethod]
        public void TestCacheMissesForSelfExpiringResults()
        {
            int c = 0;
            string key = $"CachedDataWithDifferentKey[{nameof(TestCacheMisses)}]";
            var results = new List<string>()
            {
                GetTestDataWithCachingForSelfExpiringResults($"{key}[{++c}]"),
                GetTestDataWithCachingForSelfExpiringResults($"{key}[{++c}]"),
                GetTestDataWithCachingForSelfExpiringResults($"{key}[{++c}]"),
                GetTestDataWithCachingForSelfExpiringResults($"{key}[{++c}]")
            };

            var distinctCount = results.Distinct().Count();

            //Ensure that ALL Items are Distinctly Different!
            Assert.AreEqual(results.Count, distinctCount);
        }

        [TestMethod]
        public void TestCacheMissesBecauseOfDisabledPolicy()
        {
            string key = $"CachedDataWithSameKey[{nameof(TestCacheMissesBecauseOfDisabledPolicy)}]";
            var result1 = GetTestDataWithCaching(key, LazyCachePolicy.DisabledCachingPolicy);
            var result2 = GetTestDataWithCaching(key, LazyCachePolicy.DisabledCachingPolicy);
            var result3 = GetTestDataWithCaching(key, LazyCachePolicy.DisabledCachingPolicy);
            var result4 = GetTestDataWithCaching(key, LazyCachePolicy.DisabledCachingPolicy);

            Assert.AreNotEqual(result1, result2);
            Assert.AreNotSame(result1, result2);

            Assert.AreNotEqual(result3, result4);
            Assert.AreNotSame(result3, result4);

            //Compare First and Last to ensure that ALL are the same!
            Assert.AreNotEqual(result1, result4);
            Assert.AreNotSame(result1, result4);
        }

        [TestMethod]
        public void TestCacheKeysWithDisabledValues()
        {
            var ttlDisabledByOffValue = LazyCacheConfig.GetCacheTTLFromConfig("CacheTTL.Disabled.ByOffValue", LazyCacheConfig.NeverCacheTTL);
            Assert.AreEqual(ttlDisabledByOffValue, LazyCacheConfig.NeverCacheTTL);

            var ttlDisabledByNegativeValue = LazyCacheConfig.GetCacheTTLFromConfig("CacheTTL.Disabled.ByOffNegativeValue", LazyCacheConfig.NeverCacheTTL);
            Assert.AreEqual(ttlDisabledByNegativeValue, LazyCacheConfig.NeverCacheTTL);

            //Test that a disabled Cache Key doesn't actually result in using the Fallback
            //  because it's actually overriding to Disable the Cache!
            var configKeys = new string[] {
                $"CacheTTL.{Guid.NewGuid()}.NEVER_FOUND",
                $"CacheTTL.Disabled.ByOffValue",
                $"CacheTTL.Default"
            };

            var ttlDisabledWithFallback = LazyCacheConfig.GetCacheTTLFromConfig(configKeys, LazyCacheConfig.NeverCacheTTL);
            Assert.AreEqual(ttlDisabledWithFallback, LazyCacheConfig.NeverCacheTTL);
        }

        [TestMethod]
        public void TestCacheKeysWithDefaultMinimum()
        {
            var ttlExistingConfigValueIsLarger = LazyCacheConfig.GetCacheTTLFromConfig("CacheTTL.TestCacheParams", LazyCacheConfig.NeverCacheTTL);
            Assert.IsTrue(ttlExistingConfigValueIsLarger > LazyCacheConfig.NeverCacheTTL);

            var ttlConfigValueDoesNotExist = LazyCacheConfig.GetCacheTTLFromConfig($"CacheTTL.{Guid.NewGuid()}_NEVER_EXISTS", LazyCacheConfig.NeverCacheTTL);
            Assert.AreEqual(ttlConfigValueDoesNotExist, LazyCacheConfig.NeverCacheTTL);

            var ttlDefaultMinimumIsLarger = LazyCacheConfig.GetCacheTTLFromConfig("CacheTTL.TestCacheParams", LazyCacheConfig.ForeverCacheTTL);
            Assert.AreEqual(ttlDefaultMinimumIsLarger, LazyCacheConfig.ForeverCacheTTL);
        }

        [TestMethod]
        public void TestCachePoliciesFromDisabledValues()
        {
            var policyDisabledByOffValue = LazyCachePolicyFromConfig.NewAbsoluteExpirationPolicy("CacheTTL.Disabled.ByOffValue");
            Assert.IsFalse(LazyCachePolicy.IsPolicyEnabled(policyDisabledByOffValue));

            var policyDisabledByNegativeValue = LazyCachePolicyFromConfig.NewAbsoluteExpirationPolicy("CacheTTL.Disabled.ByOffNegativeValue");
            Assert.IsFalse(LazyCachePolicy.IsPolicyEnabled(policyDisabledByNegativeValue));
        }

        [TestMethod]
        public void TestCacheRemoval()
        {
            string key = $"CachedDataWithSameKey[{nameof(TestCacheRemoval)}]";
            var result1 = GetTestDataWithCaching(key);
            var result2 = GetTestDataWithCaching(key);

            Assert.AreEqual(result1, result2);
            Assert.AreSame(result1, result2);

            TestCacheFacade.RemoveCachedData(new TestCacheParams(key));

            //Since it's removed then it MUST be newly Initialized
            //  resulting in a Different Result being returned from Result1
            var result3 = GetTestDataWithCaching(key);
            Assert.AreNotEqual(result1, result3);
            Assert.AreNotSame(result1, result3);
        }

        [TestMethod]
        public void TestCacheCountAndClearing()
        {   
            //Log the initial size of the Cache because other tests likely have populated into our global Cache
            //NOTE: This is only an issue for this Test which validates Counts and must have a Reference Point!
            var initialCacheCount = TestCacheFacade.CacheCount();

            string key = $"CachedDataWithSameKey[{nameof(TestCacheCountAndClearing)}]";
            var result1 = GetTestDataWithCaching(key, isLongRunning: false);
            var result2 = GetTestDataWithCaching(key, isLongRunning: false);

            //Validate our initial records match!
            Assert.AreEqual(result1, result2);
            Assert.AreSame(result1, result2);

            int addItemsCount = 100;
            //Populate with additional values to be cleared!
            var results = Enumerable
                .Range(1, addItemsCount)
                .Select(i => GetTestDataWithCaching($"{key}[{i}]", isLongRunning: false))
                .ToList();

            Assert.AreEqual(initialCacheCount + 1 + addItemsCount, TestCacheFacade.CacheCount());

            //THIS will reset the cache completely!
            TestCacheFacade.ClearCache();

            Assert.AreEqual(0, TestCacheFacade.CacheCount());

            //Since it's removed then it MUST be newly Initialized
            //  resulting in a Different Result being returned from Result1
            var result3 = GetTestDataWithCaching(key);
            Assert.AreNotEqual(result1, result3);
            Assert.AreNotSame(result1, result3);
            Assert.AreEqual(1, TestCacheFacade.CacheCount());
        }

        [TestMethod]
        public void TestCacheEvictionByAbsoluteExpiration()
        {
            string key = $"CachedDataWithSameKey[{nameof(TestCacheEvictionByAbsoluteExpiration)}]";
            int secondsTTL = 5;
            var result1 = GetTestDataWithCachingAndTTL(key, secondsTTL);
            var result2 = GetTestDataWithCachingAndTTL(key, secondsTTL);

            Assert.AreEqual(result1, result2);
            Assert.AreSame(result1, result2);

            //SLEEP to wait for Eviction via Absolute Expiration...
            Thread.Sleep(secondsTTL * 1000);

            //Since it's removed then it MUST be newly Initialized
            //  resulting in a Different Result being returned from Result1
            var result3 = GetTestDataWithCachingAndTTL(key, secondsTTL);
            Assert.AreNotEqual(result1, result3);
            Assert.AreNotSame(result1, result3);
        }

        [TestMethod]
        public async Task TestCacheThreadSafetyWithLazyInitialization()
        {
            string key = $"CachedDataWithSameKey[{nameof(TestCacheThreadSafetyWithLazyInitialization)}]";
            int secondsTTL = 300;
            int threadCount = 20;
            var globalCount = 0;
            var timer = Stopwatch.StartNew();

            var tasks = new List<Task<string>>();
            for (int x = 0; x < threadCount; x++)
            {
                //Simulated MANY threads running at the same time attempting to get the same data for the same Cache key!!!
                tasks.Add(Task.Run(() =>
                {
                    //THIS RUNS ON IT'S OWN THREAD, but the Lazy Cache initialization will ensure that the Value Factory Function
                    //  is only executed by the FIRST thread, and all other threads will immediately benefit from the result!
                    return TestCacheFacade.GetCachedData(new TestCacheParams(key, secondsTTL), () =>
                    {
                        //TEST that this Code ONLY ever runs ONE TIME by ONE THREAD via Lazy<> initialization!
                        //  meaning globalCount is only ever incremented 1 time!
                        Interlocked.Increment(ref globalCount);

                        //TEST that the cached data is never re-generated so only ONE Value is ever created!
                        var longTaskResult = SomeLongRunningMethod(DateTime.Now);
                        return longTaskResult;
                    });
                }));
            }

            //Allow all threads to complete and get the results...
            var results = await Task.WhenAll(tasks.ToArray());
            var distinctCount = results.Distinct().Count();
            timer.Stop();

            //TEST that this Code ONLY ever runs ONE TIME by ONE THREAD via Lazy<> initialization!
            //  meaning globalCount is only ever incremented 1 time!
            Assert.AreEqual(1, globalCount);

            //Ensure ONLY ONE item was ever generated and ALL others were identical from Cache!
            Assert.AreEqual(1, distinctCount);

            //Ensure that the Total time takes barely longer than one iteration of the Long Running Task!
            Assert.IsTrue(timer.ElapsedMilliseconds < (LongRunningTaskMillis * 2));
        }

        [TestMethod]
        public async Task TestCacheThreadSafetyForSelfExpiringCacheResults()
        {
            string key = $"CachedDataWithSameKey[{nameof(TestCacheThreadSafetyForSelfExpiringCacheResults)}]";
            int secondsTTL = 300;
            int threadCount = 1000;
            var globalCount = 0;
            var timer = Stopwatch.StartNew();

            var tasks = new List<Task<string>>();
            for (int x = 0; x < threadCount; x++)
            {
                //Simulated MANY threads running at the same time attempting to get the same data for the same Cache key!!!
                tasks.Add(Task.Run(() =>
                {
                    //THIS RUNS ON IT'S OWN THREAD, but the Lazy Cache initialization will ensure that the Value Factory Function
                    //  is only executed by the FIRST thread, and all other threads will immediately benefit from the result!
                    return TestCacheFacade.GetCachedSelfExpiringData(new TestCacheParams(key), () =>
                    {
                        //TEST that this Code ONLY ever runs ONE TIME by ONE THREAD via Lazy<> initialization!
                        //  meaning globalCount is only ever incremented 1 time!
                        Interlocked.Increment(ref globalCount);

                        //TEST that the cached data is never re-generated so only ONE Value is ever created!
                        var longTaskResult = SomeLongRunningMethod(DateTime.Now);
                        return LazySelfExpiringCacheResult.From(longTaskResult, secondsTTL);
                    });
                }));
            }

            //Allow all threads to complete and get the results...
            var results = await Task.WhenAll(tasks.ToArray());
            var distinctCount = results.Distinct().Count();
            timer.Stop();

            //TEST that this Code ONLY ever runs ONE TIME by ONE THREAD via Lazy<> initialization!
            //  meaning globalCount is only ever incremented 1 time!
            Assert.AreEqual(1, globalCount);

            //Ensure ONLY ONE item was ever generated and ALL others were identical from Cache!
            Assert.AreEqual(1, distinctCount);

            //Ensure that the Total time takes barely longer than one iteration of the Long Running Task!
            Assert.IsTrue(timer.ElapsedMilliseconds < (LongRunningTaskMillis * 2));
        }

        #region Private Helpers

        protected static readonly int LongRunningTaskMillis = 1000;

        public static string GetTestDataWithCaching(string key, CacheItemPolicy overrideCacheItemPolicy = null, bool isLongRunning = true)
        {
            return TestCacheFacade.GetCachedData(new TestCacheParams(key, overrideCacheItemPolicy), () =>
            {
                return SomeLongRunningMethod(DateTime.Now, isLongRunning);
            });
        }

        public static string GetTestDataWithCachingForSelfExpiringResults(string key, int secondsTTL = 5, bool isLongRunning = true)
        {
            return TestCacheFacade.GetCachedSelfExpiringData(new TestCacheParams(key), () =>
            {
                var cacheResult = SomeLongRunningMethod(DateTime.Now, isLongRunning);
                return LazySelfExpiringCacheResult.From(cacheResult, secondsTTL);
            });
        }

        public static string GetTestDataWithCachingAndTTL(string key, int secondsTTL = 5)
        {
            return TestCacheFacade.GetCachedData(
                new TestCacheParams(key, secondsTTL), 
                () =>
                {
                    return SomeLongRunningMethod(DateTime.Now);
                }
            );
        }

        public static string SomeLongRunningMethod(DateTime dateTimeParam, bool isLongRunning = true)
        {
            //SOME Code that takes A LOT of time and/or work to compute/retrieve/etc. or never changes?
            if(isLongRunning) Thread.Sleep(LongRunningTaskMillis);

            var guid = Guid.NewGuid();
            var dateTimeString = XmlConvert.ToString(dateTimeParam, XmlDateTimeSerializationMode.Utc);
            var result = $"[{guid}] + {dateTimeString}";

            return result;
        }

        #endregion
    }
}
