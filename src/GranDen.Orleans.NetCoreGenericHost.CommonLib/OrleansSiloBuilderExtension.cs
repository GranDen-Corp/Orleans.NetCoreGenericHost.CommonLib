using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using GranDen.Orleans.NetCoreGenericHost.CommonLib.Exceptions;
using GranDen.Orleans.NetCoreGenericHost.CommonLib.Helpers;
using GranDen.Orleans.NetCoreGenericHost.CommonLib.HostTypedOptions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;
using Orleans.Statistics;
using HostBuilderContext = Microsoft.Extensions.Hosting.HostBuilderContext;

namespace GranDen.Orleans.NetCoreGenericHost.CommonLib
{
    /// <summary>
    /// Orleans Silo Host builder library
    /// </summary>
    public static partial class OrleansSiloBuilderExtension
    {
        /// <summary>
        /// Create .NET Core Generic HostBuilder using various default configuration
        /// </summary>
        /// <param name="args">Command line arguments</param>
        /// <param name="hostEnvPrefix">Configuration Environment variable name prefix, default would be "ORLEANS_HOST_"</param>
        /// <param name="logBuilderAction"></param>
        /// <returns></returns>
        // ReSharper disable once UnusedMember.Global
        public static IHostBuilder CreateHostBuilder(string[] args, string hostEnvPrefix = "ORLEANS_HOST_", Action<ILoggingBuilder> logBuilderAction = null)
        {
            var ret = new HostBuilder();
            ret.ConfigureLogging(logBuilderAction ?? DefaultLoggerHelper.DefaultLogAction)
               .UseHostConfiguration(args, hostEnvironmentPrefix: hostEnvPrefix)
               .UseAppConfiguration(args)
               .UseConfigurationOptions()
               .ApplyOrleansSettings();

            ret.UseConsoleLifetime();
            return ret;
        }

        /// <summary>
        /// Initialize .NET Core generic host's host configuration
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="args"></param>
        /// <param name="configureDelegate"></param>
        /// <param name="hostEnvironmentPrefix"></param>
        /// <returns></returns>
        public static IHostBuilder UseHostConfiguration(this IHostBuilder builder,
            string[] args,
            Action<IConfigurationBuilder> configureDelegate = null,
            string hostEnvironmentPrefix = "ORLEANS_HOST_")
        {
            if (configureDelegate == null)
            {
                builder.ConfigureHostConfiguration(configHost =>
                {
                    configHost
                        .AddEnvironmentVariables(prefix: hostEnvironmentPrefix)
                        .AddCommandLine(args);
                });
            }
            else
            {
                builder.ConfigureHostConfiguration(configureDelegate);
            }
            return builder;
        }

        /// <summary>
        /// Initialize .NET Core generic host's app configuration
        /// </summary>
        /// <param name="hostBuilder"></param>
        /// <param name="args"></param>
        /// <param name="configureDelegate"></param>
        /// <param name="configEnvironmentPrefix"></param>
        /// <param name="configFilePrefix"></param>
        /// <returns></returns>
        public static IHostBuilder UseAppConfiguration(this IHostBuilder hostBuilder,
            string[] args,
            Action<HostBuilderContext, IConfigurationBuilder> configureDelegate = null,
            string configEnvironmentPrefix = "ORLEANS_HOST_APP_",
            string configFilePrefix = "hostsettings")
        {
            if (configureDelegate == null)
            {
                hostBuilder.ConfigureAppConfiguration((hostContext, configurationBuilder) =>
                {
                    var cwdEnv = Environment.CurrentDirectory;
                    var cwd = Directory.GetCurrentDirectory();

                    configurationBuilder
                        .SetBasePath(string.IsNullOrEmpty(cwdEnv) ? cwd : cwdEnv)
                        .AddJsonFile($"{configFilePrefix}.json", optional: false)
                        .AddJsonFile($"{configFilePrefix}.{hostContext.HostingEnvironment.EnvironmentName}.json", optional: true)
                        .AddEnvironmentVariables(prefix: configEnvironmentPrefix)
                        .AddCommandLine(args);
                });
            }
            else
            {
                hostBuilder.ConfigureAppConfiguration(configureDelegate);
            }
            return hostBuilder;
        }

        /// <summary>
        /// Setup Orleans Host configuration
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="configureDelegate"></param>
        /// <param name="orleansConfigSection"></param>
        /// <param name="siloConfigSection"></param>
        /// <param name="orleansProviderSection"></param>
        /// <param name="orleansMultiClusterSection"></param>
        /// <param name="orleansDashboardOptionSection"></param>
        /// <param name="grainLoadOptionSection"></param>
        /// <returns></returns>
        public static IHostBuilder UseConfigurationOptions(this IHostBuilder builder,
            Action<HostBuilderContext, IServiceCollection> configureDelegate = null,
            string orleansConfigSection = "Orleans",
            string siloConfigSection = "SiloConfig",
            string orleansProviderSection = "Provider",
            string orleansMultiClusterSection = "MultiCluster",
            string orleansDashboardOptionSection = "Dashboard",
            string grainLoadOptionSection = "GrainOption")
        {
            if (configureDelegate == null)
            {
                builder.ConfigureServices((context, services) =>
                {
                    services.AddOptions();

                    var orleansSettings = context.Configuration.GetSection(orleansConfigSection);

                    services.Configure<SiloConfigOption>(orleansSettings.GetSection(siloConfigSection));
                    services.Configure<GrainLoadOption>(orleansSettings.GetSection(grainLoadOptionSection));
                    services.Configure<OrleansProviderOption>(orleansSettings.GetSection(orleansProviderSection));
                    services.Configure<MultiClusterOptions>(orleansSettings.GetSection(orleansMultiClusterSection));
                    services.Configure<OrleansDashboardOption>(orleansSettings.GetSection(orleansDashboardOptionSection));
                });
            }
            else
            {
                builder.ConfigureServices(configureDelegate);
            }
            return builder;
        }

        // For default logger label
        class OrleansSiloBuilder { }

        /// <summary>
        /// Config Orleans Silo Builder using default or custom Orleans Host configuration
        /// </summary>
        /// <param name="hostBuilder"></param>
        /// <param name="configurationGetterFunc"></param>
        /// <param name="orleansConfigSection"></param>
        /// <param name="siloConfigSection"></param>
        /// <param name="orleansProviderSection"></param>
        /// <param name="grainLoadOptionSection"></param>
        /// <param name="orleansDashboardOptionSection"></param>
        /// <returns></returns>
        public static IHostBuilder ApplyOrleansSettings(this IHostBuilder hostBuilder,
            Func<HostBuilderContext, IConfigurationSection> configurationGetterFunc = null,
            string orleansConfigSection = "Orleans",
            string siloConfigSection = "SiloConfig",
            string orleansProviderSection = "Provider",
            string grainLoadOptionSection = "GrainOption",
            string orleansDashboardOptionSection = "Dashboard")
        {
            var logger = (new DefaultLoggerHelper()).CreateDefaultLogger<OrleansSiloBuilder>();

            if (configurationGetterFunc != null)
            {
                Action<HostBuilderContext, ISiloBuilder>
                    ConfigureDelegate(HostBuilderContext context, ISiloBuilder siloBuilder, Func<HostBuilderContext, IConfigurationSection> configGetter)
                {
                    var orleansSettings = configGetter(context);
                    return (hostBuilderContext, iSiloBuilder) =>
                    {
                        var siloConfig = orleansSettings.GetTypedConfig<SiloConfigOption>(siloConfigSection);
                        var orleansProviderConfig = orleansSettings.GetTypedConfig<OrleansProviderOption>(orleansProviderSection);
                        var grainLoadOption = orleansSettings.GetTypedConfig<GrainLoadOption>(grainLoadOptionSection);
                        var orleansDashboardOption = orleansSettings.GetTypedConfig<OrleansDashboardOption>(orleansDashboardOptionSection);

                        siloBuilder.ConfigSiloBuilder(siloConfig, orleansProviderConfig, grainLoadOption, orleansDashboardOption, logger);
                    };
                }

                hostBuilder.UseOrleans((ctx, siloBuilder) =>
                {
                    var configAction = ConfigureDelegate(ctx, siloBuilder, configurationGetterFunc);
                    configAction(ctx, siloBuilder);
                });
            }
            else
            {
                void ConfigSiloBuilderDelegate(HostBuilderContext context, ISiloBuilder siloBuilder)
                {
                    var orleansSettings = context.Configuration.GetSection(orleansConfigSection);
                    var siloConfig = orleansSettings.GetTypedConfig<SiloConfigOption>(siloConfigSection);
                    var orleansProviderConfig = orleansSettings.GetTypedConfig<OrleansProviderOption>(orleansProviderSection);
                    var grainLoadOption = orleansSettings.GetTypedConfig<GrainLoadOption>(grainLoadOptionSection);
                    var orleansDashboardOption = orleansSettings.GetTypedConfig<OrleansDashboardOption>(orleansDashboardOptionSection);

                    siloBuilder.ConfigSiloBuilder(siloConfig, orleansProviderConfig, grainLoadOption, orleansDashboardOption, logger);
                }

                hostBuilder.UseOrleans(ConfigSiloBuilderDelegate);
            }
            return hostBuilder;
        }

        /// <summary>
        /// Actual implementation of SiloBuilder various configuration
        /// </summary>
        /// <param name="siloBuilder"></param>
        /// <param name="siloConfig"></param>
        /// <param name="orleansProvider"></param>
        /// <param name="grainLoadOption"></param>
        /// <param name="orleansDashboard"></param>
        /// <param name="logger"></param>
        /// <returns></returns>
        public static ISiloBuilder ConfigSiloBuilder(this ISiloBuilder siloBuilder,
            SiloConfigOption siloConfig, OrleansProviderOption orleansProvider, GrainLoadOption grainLoadOption, OrleansDashboardOption orleansDashboard, ILogger logger)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                var onDockerFlag = Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER");

                if (onDockerFlag != null && onDockerFlag.ToLower().Equals("true"))
                {
                    // Default config will cause bug when running on Linux Docker Container:
                    // https://github.com/dotnet/orleans/issues/5552#issuecomment-486938815

                    logger.LogInformation("Running in Linux Container, turn off \"FastKillOnProcessExit\" setting.");
                    siloBuilder.Configure<ProcessExitHandlingOptions>(options =>
                    {
                        options.FastKillOnProcessExit = false;
                    });
                }
            }

            if (orleansDashboard.Enable)
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    siloBuilder.UseLinuxEnvironmentStatistics();
                }

                logger.LogInformation($"Enable Orleans Dashboard (https://github.com/OrleansContrib/OrleansDashboard) on this host {orleansDashboard.Port} port");
                siloBuilder.UseDashboard(options =>
                {
                    options.Port = orleansDashboard.Port;
                });
            }

            if (!string.IsNullOrEmpty(siloConfig.SiloName))
            {
                siloBuilder.Configure<SiloOptions>(options => { options.SiloName = siloConfig.SiloName; });
            }

            siloBuilder.Configure<SiloMessagingOptions>(options =>
            {
                options.ResponseTimeout = TimeSpan.FromMinutes(siloConfig.ResponseTimeoutMinutes);
                options.ResponseTimeoutWithDebugger = TimeSpan.FromMinutes(siloConfig.ResponseTimeoutMinutes + 60);
            }).Configure<ClusterOptions>(options =>
            {
                options.ClusterId = siloConfig.ClusterId;
                options.ServiceId = siloConfig.ServiceId;
            });

            if (!string.IsNullOrEmpty(siloConfig.AzureApplicationInsightKey))
            {
                siloBuilder.AddApplicationInsightsTelemetryConsumer(siloConfig.AzureApplicationInsightKey);
            }

            if (siloConfig.IsMultiCluster.HasValue && siloConfig.IsMultiCluster.Value)
            {
                if (siloConfig.GossipChannels == null || siloConfig.GossipChannels.Any())
                {
                    throw new OrleansSiloHostConfigException("Gossip Channels configuration value(s) required.");
                }

                siloBuilder.Configure<MultiClusterOptions>(options =>
                {
                    options.HasMultiClusterNetwork = true;

                    if (siloConfig.DefaultMultiCluster != null && siloConfig.DefaultMultiCluster.Any())
                    {
                        options.DefaultMultiCluster = siloConfig.DefaultMultiCluster;
                    }
                    else
                    {
                        options.DefaultMultiCluster.Add(siloConfig.ClusterId);
                    }

                    options.GossipChannels = siloConfig.GossipChannels;
                    options.UseGlobalSingleInstanceByDefault = siloConfig.UseGlobalSingleInstanceByDefault;
                });
            }

            siloBuilder.ConfigureApplicationParts(parts =>
            {
                parts.AddFromApplicationBaseDirectory().WithReferences();

                var dllPaths = grainLoadOption.LoadPaths;

                ConfigOtherFolderGrainLoad(parts, dllPaths, PathResolver);

            });

            foreach (var serviceConfigAction in GetGrainServiceConfigurationAction(grainLoadOption, PathResolver))
            {
                logger.LogInformation($"Configure DI using {serviceConfigAction}");

                siloBuilder.ConfigureApplicationParts(serviceConfigAction.AppPartConfigurationAction);
                siloBuilder.ConfigureServices(serviceConfigAction.ServiceConfigurationAction);
            }

            if (IpAddressNotSpecified(siloConfig.AdvertisedIp))
            {
                siloBuilder.ConfigureEndpoints(siloConfig.SiloPort, siloConfig.GatewayPort,
                    listenOnAnyHostAddress: siloConfig.ListenOnAnyHostAddress);
            }
            else
            {
                var advertisedIp = IPAddress.Parse(siloConfig.AdvertisedIp.Trim());
                siloBuilder.ConfigureEndpoints(advertisedIp, siloConfig.SiloPort, siloConfig.GatewayPort, siloConfig.ListenOnAnyHostAddress);
            }

            switch (orleansProvider.DefaultProvider)
            {
                case "MongoDB":
                    var mongoDbConfig = orleansProvider.MongoDB;
                    siloBuilder.UseMongoDBClustering(options =>
                    {
                        var cluster = mongoDbConfig.Cluster;

                        options.ConnectionString = cluster.DbConn;
                        options.DatabaseName = cluster.DbName;

                        if (!string.IsNullOrEmpty(cluster.CollectionPrefix))
                        {
                            options.CollectionPrefix = cluster.CollectionPrefix;
                        }
                    })
                    .AddMongoDBGrainStorageAsDefault(optionsBuilder =>
                    {
                        var storage = mongoDbConfig.Storage;
                        optionsBuilder.Configure(options =>
                        {
                            options.ConnectionString = storage.DbConn;
                            options.DatabaseName = storage.DbName;

                            if (!string.IsNullOrEmpty(storage.CollectionPrefix))
                            {
                                options.CollectionPrefix = storage.CollectionPrefix;
                            }
                        });
                    })
                    .UseMongoDBReminders(options =>
                    {
                        var reminder = mongoDbConfig.Reminder;

                        options.ConnectionString = reminder.DbConn;
                        options.DatabaseName = reminder.DbName;

                        if (!string.IsNullOrEmpty(reminder.CollectionPrefix))
                        {
                            options.CollectionPrefix = reminder.CollectionPrefix;
                        }
                    });
                    break;

                case "InMemory":
                    siloBuilder.UseDevelopmentClustering(option =>
                        {
                            if (IpAddressNotSpecified(siloConfig.AdvertisedIp))
                            {
                                option.PrimarySiloEndpoint = new IPEndPoint(IPAddress.Loopback, siloConfig.SiloPort);
                            }
                            else
                            {
                                var advertisedIp = IPAddress.Parse(siloConfig.AdvertisedIp.Trim());
                                option.PrimarySiloEndpoint = new IPEndPoint(advertisedIp, siloConfig.SiloPort);
                            }
                        })
                        .Configure<EndpointOptions>(options =>
                        {
                            options.AdvertisedIPAddress = IPAddress.Loopback;
                            options.GatewayPort = siloConfig.GatewayPort;
                            options.SiloPort = siloConfig.SiloPort;
                        })
                        .AddMemoryGrainStorageAsDefault()
                        .UseInMemoryReminderService();
                    break;

                default:
                    siloBuilder.UseLocalhostClustering(
                        serviceId: siloConfig.ServiceId,
                        clusterId: siloConfig.ClusterId,
                        siloPort: siloConfig.SiloPort,
                        gatewayPort: siloConfig.GatewayPort)
                        .AddMemoryGrainStorageAsDefault()
                        .UseInMemoryReminderService();
                    break;
            }

            return siloBuilder;
        }
    }
}
