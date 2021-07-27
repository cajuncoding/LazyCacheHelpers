using LazyCacheHelpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.Caching;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Attr = System.ComponentModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LazyCacheHelpersTests
{
    /// <summary>
    /// BBernard
    /// Original Source (MIT License): https://github.com/raerae1616/LazyCacheHelpers
    /// 
    /// Synchronous Tests for LazyStaticInMemoryCache
    /// 
    /// Unit Tests for LazyStaticInMemoryCache classes to demo and validate functionality
    /// 
    /// </summary>
    [TestClass]
    public class StaticInMemoryCacheTests
    {
        [TestMethod]
        public void TestCacheLoadingAndHits()
        {
            var enumCache = new LazyStaticInMemoryCache<Type, ILookup<Enum, string>>();
            
            var attempts = 5;
            int runCount = 0, attemptCount = 0;

            ILookup<Enum, string> cachedEnumDescriptions = null;
            for (var x = 0; x < attempts; x++)
            {
                attemptCount++;
                cachedEnumDescriptions = enumCache.GetOrAdd(typeof(StarWarsEnum), (t) =>
                {
                    runCount++;
                    var enumDescriptionLookup = GetEnumDescriptionsLookup<StarWarsEnum>();

                    return enumDescriptionLookup;
                });
            }

            Assert.IsNotNull(cachedEnumDescriptions);
            Assert.AreEqual(attempts, attemptCount);
            Assert.AreEqual(1, runCount);

            Assert.AreEqual("Anakin Skywalker", cachedEnumDescriptions[StarWarsEnum.DarthVader].FirstOrDefault());
            Assert.AreEqual("Luke Skywalker", cachedEnumDescriptions[StarWarsEnum.LukeSkywalker].FirstOrDefault());
            Assert.AreEqual("Obi-Wan Kenobi", cachedEnumDescriptions[StarWarsEnum.ObiWanKenobi].FirstOrDefault());
            Assert.AreEqual("R2-D2", cachedEnumDescriptions[StarWarsEnum.R2D2].FirstOrDefault());
            Assert.AreEqual("C-3PO", cachedEnumDescriptions[StarWarsEnum.C3PO].FirstOrDefault());
        }

        [TestMethod]
        public void TestCacheExceptions()
        {
            var enumCache = new LazyStaticInMemoryCache<Type, ILookup<Enum, string>>();

            var attempts = 5;
            int runCount = 0, attemptCount = 0;

            ILookup<Enum, string> cachedEnumDescriptions = null;
            for (var x = 0; x < attempts; x++)
            {
                attemptCount++;
                try
                {
                    cachedEnumDescriptions = enumCache.GetOrAdd(typeof(StarWarsEnum), (t) =>
                    {
                        runCount++;
                        throw new Exception("Intentionally fail within the value factory...");
                    });

                }
                catch
                {
                    //DO NOTHING!
                }
            }

            Assert.IsNull(cachedEnumDescriptions);
            Assert.AreEqual(attempts, attemptCount);
            //When exceptions are thrown then the value should never be cached so execution count should match the number of attempts!
            Assert.AreEqual(attempts, runCount);
        }

        [TestMethod]
        public async Task TestCacheLoadingAndHitsAsync()
        {
            var enumCache = new LazyStaticInMemoryCache<Type, ILookup<Enum, string>>();

            var attempts = 5;
            int runCount = 0, attemptCount = 0;

            ILookup<Enum, string> cachedEnumDescriptions = null;
            for (var x = 0; x < attempts; x++)
            {
                attemptCount++;
                cachedEnumDescriptions = await enumCache.GetOrAddAsync(typeof(StarWarsEnum), (t) =>
                {
                    runCount++;
                    var enumDescriptionLookup = GetEnumDescriptionsLookup<StarWarsEnum>();

                    //Simulate Async operation by returning the results as a Task!
                    return Task.FromResult(enumDescriptionLookup);
                });
            }

            Assert.IsNotNull(cachedEnumDescriptions);
            Assert.AreEqual(attempts, attemptCount);
            Assert.AreEqual(1, runCount);

            Assert.AreEqual("Anakin Skywalker", cachedEnumDescriptions[StarWarsEnum.DarthVader].FirstOrDefault());
            Assert.AreEqual("Luke Skywalker", cachedEnumDescriptions[StarWarsEnum.LukeSkywalker].FirstOrDefault());
            Assert.AreEqual("Obi-Wan Kenobi", cachedEnumDescriptions[StarWarsEnum.ObiWanKenobi].FirstOrDefault());
            Assert.AreEqual("R2-D2", cachedEnumDescriptions[StarWarsEnum.R2D2].FirstOrDefault());
            Assert.AreEqual("C-3PO", cachedEnumDescriptions[StarWarsEnum.C3PO].FirstOrDefault());
        }

        [TestMethod]
        public async Task TestCacheExceptionsAsync()
        {
            var enumCache = new LazyStaticInMemoryCache<Type, ILookup<Enum, string>>();

            var attempts = 5;
            int runCount = 0, attemptCount = 0;

            ILookup<Enum, string> cachedEnumDescriptions = null;
            for (var x = 0; x < attempts; x++)
            {
                attemptCount++;
                try
                {
                    cachedEnumDescriptions = await enumCache.GetOrAddAsync(typeof(StarWarsEnum), (t) =>
                    {
                        runCount++;
                        throw new Exception("Intentionally fail within the value factory...");
                    });

                }
                catch
                {
                    //DO NOTHING!
                }
            }

            Assert.IsNull(cachedEnumDescriptions);
            Assert.AreEqual(attempts, attemptCount);
            //When exceptions are thrown then the value should never be cached so execution count should match the number of attempts!
            Assert.AreEqual(attempts, runCount);
        }

        #region Private Helpers

        private enum StarWarsEnum
        {
            [Attr.Description("Anakin Skywalker")]
            DarthVader,
            [Attr.Description("Luke Skywalker")]
            LukeSkywalker,
            [Attr.Description("Obi-Wan Kenobi")]
            ObiWanKenobi,
            [Attr.Description("R2-D2")]
            R2D2,
            [Attr.Description("C-3PO")]
            C3PO
        }

        public static ILookup<Enum, string> GetEnumDescriptionsLookup<TEnum>() where TEnum: Enum
        {
            var enumDescriptionLookup = ((TEnum[])Enum.GetValues(typeof(TEnum))).ToLookup(
                ev => (Enum)ev,
                ev => GetDescriptionForEnum(ev)
            );

            return enumDescriptionLookup;
        }

        public static string GetDescriptionForEnum(Enum value)
        {
            FieldInfo fieldInfo = value.GetType().GetField(value.ToString());
            if (fieldInfo == null)
            {
                return null;
            }

            Attr.DescriptionAttribute attribute = (Attr.DescriptionAttribute)fieldInfo.GetCustomAttribute(typeof(Attr.DescriptionAttribute));
            return attribute?.Description;
        }

        #endregion
    }
}
