﻿using System;
using System.Collections.Generic;
using GranDen.Orleans.NetCoreGenericHost.CommonLib;
using McMaster.NETCore.Plugins;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using Serilog.Exceptions;
using Serilog.Formatting.Json;
using Serilog.Sinks.SystemConsole.Themes;

namespace MongoDemoSiloHost
{
    class Program
    {
        static void Main(string[] args)
        {
            var logConfig = new LoggerConfiguration()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("Orleans.RuntimeSiloLogStatistics", LogEventLevel.Warning)
                .MinimumLevel.Override("Orleans.Runtime.Management.ManagementGrain", LogEventLevel.Warning)
                .MinimumLevel.Override("Orleans.Runtime.SiloControl", LogEventLevel.Warning)
                .MinimumLevel.Override("Orleans.Runtime.MembershipService.MembershipTableManager", LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .Enrich.WithProcessId()
                .Enrich.WithProcessName()
                .Enrich.WithThreadId()
                .Enrich.WithExceptionDetails()
                .WriteTo.Console(theme: AnsiConsoleTheme.Code)
                .WriteTo.Trace()
                .WriteTo.Debug();

            Log.Logger = logConfig.CreateLogger();

            var genericHostBuilder = OrleansSiloBuilderExtension.CreateHostBuilder(args).ApplySerilog();

#if DEBUG
            genericHostBuilder.UseEnvironment(EnvironmentName.Development);
#endif

            try
            {
                var genericHost = genericHostBuilder.Build();
                PluginCache = OrleansSiloBuilderExtension.PlugInLoaderCache;
                genericHost.Run();
            }
            catch (OperationCanceledException)
            {
                //do nothing
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Orleans Silo Host error");
                throw;
            }
            finally
            {
                foreach (var kv in PluginCache)
                {
                    kv.Value.Dispose();
                }
            }
        }

        public static Dictionary<string, PluginLoader> PluginCache { get; set; }
    }
}
