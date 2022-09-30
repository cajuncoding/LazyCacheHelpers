using LazyCacheHelpers;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
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
        public async Task TestClearingStaticInMemoryCacheSyncAndAsync()
        {
            var enumCache = new LazyStaticInMemoryCache<Type, ILookup<Enum, string>>();
            int runCount = 0;

            var loadCacheAsync = new Func<Task>(async () =>
            {
                //#1 Load Async Cache item
                var asyncCachedEnumDescriptions = await enumCache.GetOrAddAsync(typeof(StarWarsEnum), (t) =>
                {
                    runCount++;
                    var enumDescriptionLookup = GetEnumDescriptionsLookup<StarWarsEnum>();
                    //Simulate Async operation by returning the results as a Task!
                    return Task.FromResult(enumDescriptionLookup);
                });

                //#2 Load Sync Cache item
                var syncCachedEnumDescriptions = enumCache.GetOrAdd(typeof(StarWarsEnum), (t) =>
                {
                    runCount++;
                    return GetEnumDescriptionsLookup<StarWarsEnum>();
                });
            });

            //New Cache shoudl always have Zero entries...
            Assert.AreEqual(0, enumCache.GetCacheCount());

            await loadCacheAsync.Invoke();

            Assert.AreEqual(runCount, enumCache.GetCacheCount());
            Assert.AreEqual(runCount, enumCache.ClearCache());
            Assert.AreEqual(0, enumCache.GetCacheCount());
            runCount = 0;

            //Test Reloading after being cleared with the Exact Same Data, Keys, etc.
            await loadCacheAsync.Invoke();
            Assert.AreEqual(runCount, enumCache.GetCacheCount());
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

        private static ILookup<Enum, string> GetEnumDescriptionsLookup<TEnum>() where TEnum: Enum
        {
            var enumDescriptionLookup = Enum.GetValues(typeof(TEnum)).Cast<TEnum>().ToLookup(
                ev => (Enum)ev,
                ev => GetDescriptionForEnum(ev)
            );

            return enumDescriptionLookup;
        }

        private static string GetDescriptionForEnum(Enum value)
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
