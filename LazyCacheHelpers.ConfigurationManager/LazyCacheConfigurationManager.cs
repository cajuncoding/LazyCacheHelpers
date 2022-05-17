using System;
using System.Configuration;
using System.Diagnostics;

namespace LazyCacheHelpers
{
    /// <summary>
    /// BBernard
    /// Original Source (MIT License): https://github.com/cajuncoding/LazyCacheHelpers
    /// 
    /// Class to support reading cache configuration values from Configuration (e.g. TTL Seconds defined in Configuration).
    /// </summary>
    public class LazyCacheConfigurationManager
    {
        public static void BootstrapConfigurationManager()
        {
            LazyCacheConfig.BootstrapConfigValueReader(configKeyName =>
            {
                #if DEBUG
                var configFilePath = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None).FilePath;
                Debug.WriteLine($"Looking for Configuration File: [{configFilePath}]");
                #endif

                var appSettings = ConfigurationManager.AppSettings;
                String configValue = appSettings[configKeyName];
                return configValue;
            });
        }
    }
}
