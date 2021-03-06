﻿using System;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

// ReSharper disable once CheckNamespace
namespace GranDen.Orleans.NetCoreGenericHost.CommonLib
{
    /// <summary>
    /// Extension method for add Serilog to Generic Host
    /// </summary>
    public static class SerilogExtension
    {
        /// <summary>
        /// Make HostBuilder use Serilog
        /// </summary>
        /// <param name="hostBuilder"></param>
        /// <returns></returns>
        public static IHostBuilder ApplySerilog(this IHostBuilder hostBuilder)
        {
            return hostBuilder.ConfigureLogging(DefaultConfigureLoggingBuilderAction).UseSerilog();
        }

        /// <summary>
        /// Make HostBuilder use Serilog
        /// </summary>
        /// <param name="hostBuilder"></param>
        /// <param name="configureLoggingBuilderAction"></param>
        /// <returns></returns>
        public static IHostBuilder ApplySerilog(this IHostBuilder hostBuilder, Action<ILoggingBuilder> configureLoggingBuilderAction)
        {
            return hostBuilder.ConfigureLogging(configureLoggingBuilderAction).UseSerilog();
        }

        private static void DefaultConfigureLoggingBuilderAction(ILoggingBuilder loggingBuilder)
        {
            //because this management grain is very noisy when using Orleans Dashboard
            loggingBuilder.AddFilter("Orleans.Runtime.Management.ManagementGrain", LogLevel.Warning)
                      .AddFilter("Orleans.Runtime.SiloControl", LogLevel.Warning);

            loggingBuilder.AddSerilog(dispose: true);
        }
    }
}
