using System.IO;
using GranDen.Orleans.Client.CommonLib;
using GranDen.Orleans.Client.CommonLib.TypedOptions;
using Microsoft.Extensions.Configuration;

namespace SqlDbDemoClient
{
    public class ConfigUtil
    {
        public static (ClusterInfoOption, OrleansProviderOption) GetConfigSettings()
        {
            IConfigurationBuilder builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json");

            var configRoot = builder.Build();

            return configRoot.GetSiloSettings();
        }
    }
}