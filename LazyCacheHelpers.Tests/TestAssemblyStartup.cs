using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LazyCacheHelpers.Tests
{
    [TestClass]
    internal class TestAssemblyStartup
    {
        [AssemblyInitialize]
        public static void AssemblyInit(TestContext context)
        {
            LazyCacheConfigurationManager.BootstrapConfigurationManager();
        }
    }
}
