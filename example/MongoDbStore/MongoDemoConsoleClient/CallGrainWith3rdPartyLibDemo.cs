using System;
using System.Threading.Tasks;
using GranDen.Orleans.Client.CommonLib;
using IGrainWith3rdPartyLib;
using Microsoft.Extensions.Logging;

namespace MongoDemoConsoleClient
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
            var (clusterInfo, providerOption) = ConfigUtil.GetConfigSettings();

            try
            {
                using (var client =
                    OrleansClientBuilder.CreateClient(_logger,
                        clusterInfo,
                        providerOption,
                        new[] { typeof(IUtilityGrain) }))
                {
                    await client.ConnectWithRetryAsync();
                    _logger.LogInformation("Client successfully connect to silo host");

                    var grain = client.GetGrain<IUtilityGrain>("test");

                    var testInput = new InputDTO { Name = "Alice", Email = "alice@example.com" };

                    var jsonStr = await grain.OutputJsonStr(testInput);
                    _logger.LogInformation($"Client side get OutputJsonStr() result= {jsonStr}");

                    var bsonDoc = await grain.ToBson(testInput);
                    _logger.LogInformation($"Client side get ToBson() result= {bsonDoc}");

                    await client.Close();
                    _logger.LogInformation("Client successfully close connection to silo host");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "call TestRpc() error");
                throw;
            }
        }
    }
}