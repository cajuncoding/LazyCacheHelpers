using System;
using System.Diagnostics;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LazyCacheHelpersTests
{

    /// <summary>
    /// BBernard
    /// Original Source (MIT License): https://github.com/raerae1616/LazyCacheHelpers
    /// 
    /// ASYNC Tests for Async support of LazyCacheHelpers
    /// 
    /// Unit Tests for LazyCacheHandler classes to demo and validate functionality
    /// 
    /// </summary>
    [TestClass]
    public class AsyncTests
    {
        [TestMethod]
        public async Task TestCacheHitsAsync()
        {
            string key = $"CachedDataWithSameKey[{nameof(TestCacheHitsAsync)}]";
            var result1 = await GetTestDataWithCachingAsync(key);
            var result2 = await GetTestDataWithCachingAsync(key);
            var result3 = await GetTestDataWithCachingAsync(key);
            var result4 = await GetTestDataWithCachingAsync(key);

            Assert.AreEqual(result1, result2);
            Assert.AreSame(result1, result2);

            Assert.AreEqual(result3, result4);
            Assert.AreSame(result3, result4);

            //Compare First and Last to ensure that ALL are the same!
            Assert.AreEqual(result1, result4);
            Assert.AreSame(result1, result4);
        }

        [TestMethod]
        public async Task TestCacheMissesAsync()
        {
            int c = 0;
            string key = "CachedDataWithDifferentKey";
            var results = new List<string>()
            {
                await GetTestDataWithCachingAsync($"{key}[{++c}]"),
                await GetTestDataWithCachingAsync($"{key}[{++c}]"),
                await GetTestDataWithCachingAsync($"{key}[{++c}]"),
                await GetTestDataWithCachingAsync($"{key}[{++c}]")
            };

            var distinctCount = results.Distinct().Count();

            //Ensure that ALL Items are Distinctly Different!
            Assert.AreEqual(results.Count, distinctCount);
        }

        [TestMethod]
        public async Task TestCacheRemovalAsync()
        {
            string key = $"CachedDataWithSameKey[{nameof(TestCacheRemovalAsync)}]";
            var result1 = await GetTestDataWithCachingAsync(key);
            var result2 = await GetTestDataWithCachingAsync(key);

            Assert.AreEqual(result1, result2);
            Assert.AreSame(result1, result2);

            TestCacheFacade.RemoveCachedData(key);

            //Since it's removed then it MUST be newly Initialized
            //  resulting in a Different Result being returned from Result1
            var result3 = await GetTestDataWithCachingAsync(key);
            Assert.AreNotEqual(result1, result3);
            Assert.AreNotSame(result1, result3);
        }

        [TestMethod]
        public async Task TestCacheEvictionByAbsoluteExpirationAsync()
        {
            string key = $"CachedDataWithSameKey[{nameof(TestCacheEvictionByAbsoluteExpirationAsync)}]";
            int secondsTTL = 5;
            var result1 = await GetTestDataWithCachingAndTTLAsync(key, secondsTTL);
            var result2 = await GetTestDataWithCachingAndTTLAsync(key, secondsTTL);

            Assert.AreEqual(result1, result2);
            Assert.AreSame(result1, result2);

            //SLEEP to wait for Eviction via Absolute Expiration...
            //ASYNC Sleep to simulate long running Async Task!
            await Task.Delay(secondsTTL * 1000);

            //Since it's removed then it MUST be newly Initialized
            //  resulting in a Different Result being returned from Result1
            var result3 = await GetTestDataWithCachingAndTTLAsync(key, secondsTTL);
            Assert.AreNotEqual(result1, result3);
            Assert.AreNotSame(result1, result3);
        }

        [TestMethod]
        public async Task TestCacheThreadSafetyWithLazyInitializationAsync()
        {
            string key = $"CachedDataWithSameKey[{nameof(TestCacheThreadSafetyWithLazyInitializationAsync)}]";
            int secondsTTL = 300;
            int threadCount = 20;
            var globalCount = 0;
            var timer = Stopwatch.StartNew();

            var tasks = new List<Task<string>>();
            for (int x = 0; x < threadCount; x++)
            {
                //Simulated MANY threads running at the same time attempting to get the same data for the same Cache key!!!
                tasks.Add(Task.Run(async () =>
                {
                    //THIS RUNS ON IT'S OWN THREAD, but the Lazy Cache initialization will ensure that the Value Factory Function
                    //  is only executed by the FIRST thread, and all other threads will immediately benefit from the result!
                    return await TestCacheFacade.GetCachedDataAsync(key, async () =>
                        {
                            //TEST that this Code ONLY ever runs ONE TIME by ONE THREAD via Lazy<> initialization!
                            //  meaning globalCount is only ever incremented 1 time!
                            Interlocked.Increment(ref globalCount);

                            //TEST that the cached data is never re-generated so only ONE Value is ever created!
                            var longTaskResult = await SomeLongRunningMethodAsync(DateTime.Now);
                            return longTaskResult;
                        },
                        secondsTTL
                    );
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

        public static async Task<string> GetTestDataWithCachingAsync(string key)
        {
            return await TestCacheFacade.GetCachedDataAsync(key, async () =>
            {
                return await SomeLongRunningMethodAsync(DateTime.Now);
            });
        }

        public static async Task<string> GetTestDataWithCachingAndTTLAsync(string key, int secondsTTL)
        {
            return await TestCacheFacade.GetCachedDataAsync(
                key, 
                async () =>
                {
                    string result = await SomeLongRunningMethodAsync(DateTime.Now);
                    return result;
                },
                secondsTTL
            );
        }

        public static async Task<string> SomeLongRunningMethodAsync(DateTime dateTimeParam)
        {
            //SOME Code that takes A LOT of time and/or work to compute/retrieve/etc. or never changes?
            //ASYNC Sleep to simulate long running Async Task!
            await Task.Delay(LongRunningTaskMillis);

            var guid = Guid.NewGuid();
            var dateTimeString = XmlConvert.ToString(dateTimeParam, XmlDateTimeSerializationMode.Utc);
            var result = $"[{guid}] + {dateTimeString}";

            return result;
        }

        #endregion
    }
}
