﻿using System;
using System.Collections.Generic;
using GranDen.Orleans.NetCoreGenericHost.CommonLib;
using McMaster.NETCore.Plugins;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using Serilog.Exceptions;
using Serilog.Sinks.SystemConsole.Themes;

namespace SqlDbDemoHost
{
    class Program
    {
        static void Main(string[] args)
        {
            Log.Logger = CreateLogConfig().CreateLogger();

            var genericHostBuilder = OrleansSiloBuilderExtension.CreateHostBuilder(args, configFilePrefix: "appsettings").ApplySerilog();
#if DEBUG
            genericHostBuilder.UseEnvironment(Environments.Development);
#endif
            try
            {
                var genericHost = genericHostBuilder.Build();
                PluginCache = OrleansSiloBuilderExtension.PlugInLoaderCache;
                genericHost.Run();
            }
            catch (OperationCanceledException ex)
            {
                //do nothing
                Log.Information(ex, "Temporary error occurred.");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Orleans Silo Host error");
                throw;
            }
            finally
            {
                PluginCache.Dispose();
            }
        }

        private static LoggerConfiguration CreateLogConfig() =>
            new LoggerConfiguration()
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

        public static Dictionary<string, PluginLoader> PluginCache { get; set; }
    }
}
