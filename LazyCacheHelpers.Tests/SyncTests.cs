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
        /// Original Source (MIT License): https://github.com/raerae1616/LazyCacheHelpers
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
        public void TestCacheMisses()
        {
            int c = 0;
            string key = "CachedDataWithDifferentKey";
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
            Assert.AreEqual(ttlDisabledByOffValue, LazyCacheConfig.NeverCacheTTL);
        }

        [TestMethod]
        public void TestCachePoliciesFromDisabledValues()
        {
            var policyDisabledByOffValue = LazyCachePolicy.NewAbsoluteExpirationPolicy("CacheTTL.Disabled.ByOffValue");
            Assert.IsFalse(LazyCachePolicy.IsPolicyEnabled(policyDisabledByOffValue));

            var policyDisabledByNegativeValue = LazyCachePolicy.NewAbsoluteExpirationPolicy("CacheTTL.Disabled.ByOffNegativeValue");
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

            //Ensure ONLY ONE item was ever generated and ALL other's were identical from Cache!
            Assert.AreEqual(1, distinctCount);

            //Ensure that the Total time takes barely longer than one iteration of the Long Running Task!
            Assert.IsTrue(timer.ElapsedMilliseconds < (LongRunningTaskMillis * 2));
        }

        #region Private Helpers

        protected static readonly int LongRunningTaskMillis = 1000;

        public static string GetTestDataWithCaching(string key, CacheItemPolicy overrideCacheItemPolicy = null)
        {
            return TestCacheFacade.GetCachedData(new TestCacheParams(key, overrideCacheItemPolicy), () =>
            {
                return SomeLongRunningMethod(DateTime.Now);
            });
        }

        public static string GetTestDataWithCachingAndTTL(string key, int secondsTTL)
        {
            return TestCacheFacade.GetCachedData(
                new TestCacheParams(key, secondsTTL), 
                () =>
                {
                    return SomeLongRunningMethod(DateTime.Now);
                }
            );
        }

        public static string SomeLongRunningMethod(DateTime dateTimeParam)
        {
            //SOME Code that takes A LOT of time and/or work to compute/retrieve/etc. or never changes?
            Thread.Sleep(LongRunningTaskMillis);

            var guid = Guid.NewGuid();
            var dateTimeString = XmlConvert.ToString(dateTimeParam, XmlDateTimeSerializationMode.Utc);
            var result = $"[{guid}] + {dateTimeString}";

            return result;
        }

        #endregion
    }
}
