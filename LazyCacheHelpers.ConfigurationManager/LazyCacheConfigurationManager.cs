using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

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
        public static string LazyCacheHelpersFallbackConfigFileName { get; } = "LazyCacheHelpers.config";
        
        private static Configuration LazyCacheHelpersConfig { get; set; }

        public static void BootstrapConfigurationManager()
        {
            var lazyCacheHelpersFallbackConfigFilePath = Path.Combine(AppContext.BaseDirectory, LazyCacheHelpersFallbackConfigFileName);
            if (LazyCacheHelpersConfig is null && File.Exists(lazyCacheHelpersFallbackConfigFilePath))
            {
                LazyCacheHelpersConfig = ConfigurationManager.OpenMappedExeConfiguration(
                    new ExeConfigurationFileMap() { ExeConfigFilename = lazyCacheHelpersFallbackConfigFilePath },
                    ConfigurationUserLevel.None
                );
            }

            LazyCacheConfig.BootstrapConfigValueReader(configKeyName =>
            {
                #if DEBUG
                var defaultConfigFilePath = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None).FilePath;
                Debug.WriteLine($"Looking for Configuration value [{configKeyName}] in Default configuration file [{defaultConfigFilePath}]...");
                #endif

                string configValue = ConfigurationManager.AppSettings[configKeyName];
                if (string.IsNullOrWhiteSpace(configValue) && LazyCacheHelpersConfig != null)
                {
                    #if DEBUG
                    Debug.WriteLine($"Configuration value [{configKeyName}] was not found in the Default configuration file...");
                    Debug.WriteLine($"Now looking for Configuration value in fallback configuration file: [{LazyCacheHelpersFallbackConfigFileName}]...");
                    #endif

                    configValue = LazyCacheHelpersConfig.AppSettings?.Settings[configKeyName]?.Value;
                }

                #if DEBUG
                Debug.WriteLine($"Configuration value [{configKeyName}] == [{ configValue ?? "null" }]");
                #endif

                return configValue;
            });
        }
    }
}
