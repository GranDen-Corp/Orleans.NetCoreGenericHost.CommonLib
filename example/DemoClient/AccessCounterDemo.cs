using GranDen.Orleans.Client.CommonLib;
using GranDen.Orleans.Client.CommonLib.TypedOptions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RpcShareInterface;
using System;
using System.IO;
using System.Threading.Tasks;

namespace DemoClient
{
    class AccessCounterDemo
    {
        private readonly ILogger<AccessCounterDemo> _logger;

        public AccessCounterDemo(ILogger<AccessCounterDemo> logger)
        {
            _logger = logger;
        }

        public async Task RunCounter()
        {
            var (clusterInfo, providerOption) = GetConfigSettings();
            using (var client = OrleansClientBuilder.CreateClient(_logger, clusterInfo, providerOption))
            {
                await client.ConnectWithRetryAsync();
                _logger.LogInformation("Client successfully connect to silo host");

                var grain = client.GetGrain<ICounter>(0);

                _logger.LogInformation("Get counter grain, start calling RPC methods...");

                await grain.Add(1);
                await Task.Delay(new TimeSpan(0, 0, 1));

                await grain.Add(2);
                await Task.Delay(new TimeSpan(0, 0, 1));

                await grain.Add(3);
                var current = await grain.CurrentValue();
                _logger.LogInformation("Current counter value is '{0}'", current);

                await client.Close();
                _logger.LogInformation("Client successfully close connection to silo host");
            }
        }

        private static (ClusterInfoOption, OrleansProviderOption) GetConfigSettings()
        {
            IConfigurationBuilder builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json");

            var configRoot = builder.Build();

            return configRoot.GetSiloSettings();
        }
    }
}
