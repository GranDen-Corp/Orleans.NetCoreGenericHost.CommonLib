using System.IO;
using System.Threading.Tasks;
using GranDen.Orleans.Client.CommonLib;
using GranDen.Orleans.Client.CommonLib.TypedOptions;
using IGrainWith3rdPartyLib;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DemoConsoleClient
{
    public class CallGrainWith3rdPartyLibDemo
    {
        private readonly ILogger<CallGrainWith3rdPartyLibDemo> _logger;

        public CallGrainWith3rdPartyLibDemo(ILogger<CallGrainWith3rdPartyLibDemo> logger)
        {
            _logger = logger;
        }

        public async Task TestRpc()
        {
            var (clusterInfo, providerOption) = GetConfigSettings();
            using (var client = OrleansClientBuilder.CreateClient(_logger, clusterInfo, providerOption, new[] { typeof(IUtilityGrain) }))
            {
                await client.ConnectWithRetryAsync();
                _logger.LogInformation("Client successfully connect to silo host");

                var grain = client.GetGrain<IUtilityGrain>("test");

                var testInput = new { Name = "Alice", Email = "alice@example.com" };

                var jsonStr = await grain.OutputJsonStr(testInput);
                _logger.LogInformation($"Client side get OutputJsonStr() result= {jsonStr}");

                var bsonDoc = await grain.ToBson(testInput);
                _logger.LogInformation("Client side get ToBson() result= {@BsonDoc}", bsonDoc);

                await client.Close();
                _logger.LogInformation("Client successfully close connection to silo host");
            }
        }

        private static (ClusterInfoOption, OrleansProviderOption) GetConfigSettings()
        {
            IConfigurationBuilder builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json");

            var configRoot = builder.Build();

            return configRoot.GetSiloSettings();
        }
    }
}