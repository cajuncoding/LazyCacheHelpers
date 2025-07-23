using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LazyCacheHelpers.Tests
{
    [TestClass]
    public class TestAssemblyStartup
    {
        [AssemblyInitialize]
        public static void AssemblyInit(TestContext testContext)
        {
            LazyCacheConfigurationManager.BootstrapConfigurationManager();
        }
    }
}
