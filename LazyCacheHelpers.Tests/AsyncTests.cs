using System;
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

        #region Private Helpers

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
            await Task.Delay(1000);

            var guid = Guid.NewGuid();
            var dateTimeString = XmlConvert.ToString(dateTimeParam, XmlDateTimeSerializationMode.Utc);
            var result = $"[{guid}] + {dateTimeString}";

            return result;
        }

        #endregion
    }
}
