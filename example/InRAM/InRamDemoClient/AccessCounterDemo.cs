﻿using GranDen.Orleans.Client.CommonLib;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using IGrainDemo;

namespace InRamDemoClient
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
            var (clusterInfo, _) = ConfigUtil.GetConfigSettings();
            
            try
            {
                using (var client =
                    OrleansClientBuilder.CreateLocalhostClient(_logger,
                        gatewayPort: 8310,
                        serviceId: clusterInfo.ServiceId,
                        clusterId: clusterInfo.ClusterId))
                {
                    await client.ConnectWithRetryAsync();
                    _logger.LogInformation("Client successfully connect to silo host");

                    var helloGrain = client.GetGrain<IHello>(0);
                    _logger.LogInformation("Get greeting grain, start calling RPC method...");

                    var returnValue = await helloGrain.SayHello("Hello Orleans");
                    _logger.LogInformation($"RPC method return value is \r\n\r\n{{{returnValue}}}\r\n\r\n");

                    var counterGrain = client.GetGrain<ICounter>(0);

                    _logger.LogInformation("Get counter grain, start calling RPC methods...");

                    await counterGrain.Add(1);
                    await Task.Delay(new TimeSpan(0, 0, 1));

                    await counterGrain.Add(2);
                    await Task.Delay(new TimeSpan(0, 0, 1));

                    await counterGrain.Add(3);
                    var current = await counterGrain.CurrentValue();
                    _logger.LogInformation("Current counter value is '{0}'", current);

                    await client.Close();
                    _logger.LogInformation("Client successfully close connection to silo host");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "call RunCounter() error");
                throw;
            }
        }
    }
}
