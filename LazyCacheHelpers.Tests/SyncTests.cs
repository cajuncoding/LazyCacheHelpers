using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Xml;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LazyCacheHelpersTests
{
    [TestClass]
    public class SyncTests
    {
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
        public void TestCacheRemoval()
        {
            string key = $"CachedDataWithSameKey[{nameof(TestCacheRemoval)}]";
            var result1 = GetTestDataWithCaching(key);
            var result2 = GetTestDataWithCaching(key);

            Assert.AreEqual(result1, result2);
            Assert.AreSame(result1, result2);

            DemoCacheHelper.RemoveCachedData(key);

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

        #region Private Helpers

        public static string GetTestDataWithCaching(string key)
        {
            return DemoCacheHelper.GetCachedData(key, () =>
            {
                return SomeLongRunningMethod(DateTime.Now);
            });
        }

        public static string GetTestDataWithCachingAndTTL(string key, int secondsTTL)
        {
            return DemoCacheHelper.GetCachedData(
                key, 
                () =>
                {
                    return SomeLongRunningMethod(DateTime.Now);
                },
                secondsTTL
            );
        }

        public static string SomeLongRunningMethod(DateTime dateTimeParam)
        {
            //SOME Code that takes A LOT of time and/or work to compute/retrieve/etc. or never changes?
            Thread.Sleep(1000);

            var guid = Guid.NewGuid();
            var dateTimeString = XmlConvert.ToString(dateTimeParam, XmlDateTimeSerializationMode.Utc);
            var result = $"[{guid}] + {dateTimeString}";

            return result;
        }

        #endregion
    }
}
