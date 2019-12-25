using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using GranDen.Orleans.NetCoreGenericHost.CommonLib.Exceptions;
using GranDen.Orleans.NetCoreGenericHost.CommonLib.Helpers;
using GranDen.Orleans.NetCoreGenericHost.CommonLib.HostTypedOptions;
using GranDen.Orleans.Server.SharedInterface;
using McMaster.NETCore.Plugins;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;
using Orleans.Runtime.Configuration;
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
            if (PlugInLoaderCache != null)
            {
                PlugInLoaderCache = null;
            }

            var hostBuilder = new HostBuilder();
            hostBuilder.ConfigureLogging(logBuilderAction ?? DefaultLoggerHelper.DefaultLogAction)
               .UseHostConfiguration(args, hostEnvironmentPrefix: hostEnvPrefix)
               .UseAppConfiguration(args)
               .UseConfigurationOptions()
               .ApplyOrleansSettings();

            hostBuilder.UseConsoleLifetime();
            return hostBuilder;
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
        // ReSharper disable once ClassNeverInstantiated.Local
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
                Action<HostBuilderContext, ISiloBuilder> ConfigureDelegate(HostBuilderContext context, ISiloBuilder siloBuilder, Func<HostBuilderContext, IConfigurationSection> configGetter)
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
                hostBuilder.UseOrleans((context, siloBuilder) =>
                {
                    var orleansSettings = context.Configuration.GetSection(orleansConfigSection);
                    var siloConfig = orleansSettings.GetTypedConfig<SiloConfigOption>(siloConfigSection);
                    var orleansProviderConfig = orleansSettings.GetTypedConfig<OrleansProviderOption>(orleansProviderSection);
                    var grainLoadOption = orleansSettings.GetTypedConfig<GrainLoadOption>(grainLoadOptionSection);
                    var orleansDashboardOption = orleansSettings.GetTypedConfig<OrleansDashboardOption>(orleansDashboardOptionSection);

                    siloBuilder.ConfigSiloBuilder(siloConfig, orleansProviderConfig, grainLoadOption, orleansDashboardOption, logger);
                });
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
                if (IsRunningOnContainer())
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

            if (IsRunningOnContainer())
            {
                logger.LogInformation("Running in Container, use autodetect Dns Host Name instead of configuration");
                siloBuilder.ConfigureEndpoints(Dns.GetHostName(), siloConfig.SiloPort, siloConfig.GatewayPort, listenOnAnyHostAddress: siloConfig.ListenOnAnyHostAddress);
            }
            else if (IpAddressNotSpecified(siloConfig.AdvertisedIp))
            {
                siloBuilder.ConfigureEndpoints(siloConfig.SiloPort, siloConfig.GatewayPort, listenOnAnyHostAddress: siloConfig.ListenOnAnyHostAddress);
            }
            else
            {
                try
                {
                    var advertisedIp = IPAddress.Parse(siloConfig.AdvertisedIp.Trim());
                    siloBuilder.ConfigureEndpoints(advertisedIp, siloConfig.SiloPort, siloConfig.GatewayPort, siloConfig.ListenOnAnyHostAddress);
                }
                catch (FormatException)
                {
                    siloBuilder.ConfigureEndpoints(Dns.GetHostName(), siloConfig.SiloPort, siloConfig.GatewayPort, listenOnAnyHostAddress: siloConfig.ListenOnAnyHostAddress);
                }
            }

            if (orleansDashboard.Enable)
            {
                siloBuilder.ApplyOrleansDashboard(orleansDashboard, logger);
            }

            if (!string.IsNullOrEmpty(siloConfig.SiloName))
            {
                siloBuilder.Configure<SiloOptions>(options => { options.SiloName = siloConfig.SiloName; });
            }

            siloBuilder.Configure<StatisticsOptions>(options =>
            {
                options.CollectionLevel = StatisticsLevel.Critical;
            });

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

            var dllPaths = grainLoadOption.LoadPaths.Select(PathResolver).ToList();
            if (dllPaths.Count > 0)
            {
                PlugInLoaderCache = new Dictionary<string, PluginLoader>(dllPaths.Count);
            }

            siloBuilder.ConfigureApplicationParts(parts =>
            {
                parts.AddFromApplicationBaseDirectory().WithReferences();

                if (dllPaths.Count > 0)
                {
                    foreach (var assembly in dllPaths.Select(GetNonMainExeFolderAssembly))
                    {
                        if (assembly.GetTypes().Select(t => t.Namespace).Any(x => x == "OrleansGeneratedCode"))
                        {
                            parts.AddDynamicPart(assembly);
                        }
                        else
                        {
                            throw new OrleansSiloHostConfigException($"Module \'{dllPaths}\' has no orleans support code gen");
                        }
                    }
                }
            });

            foreach (var serviceConfigAction in GetGrainServiceConfigurationAction(grainLoadOption, PathResolver))
            {
                logger.LogInformation($"Configure DI using {serviceConfigAction}");

                siloBuilder.ConfigureApplicationParts(serviceConfigAction.AppPartConfigurationAction);

                var diConfigAction = serviceConfigAction.ServiceConfigurationAction;

                if (diConfigAction != null)
                {
                    siloBuilder.ConfigureServices(diConfigAction);
                }
            }

            if(siloConfig.ExlcudeGrains.Any())
            {
                siloBuilder.Configure<GrainClassOptions>(options => {
                    options.ExcludedGrainTypes.AddRange(siloConfig.ExlcudeGrains);
                });
            }

            switch (orleansProvider.DefaultProvider)
            {
                case "MongoDB":
                    var mongoDbConfig = orleansProvider.MongoDB;

                    var mongoDbClusterConfig = mongoDbConfig.Cluster;
                    var mongoDbStorageConfig = mongoDbConfig.Storage;
                    var mongoDbReminderConfig = mongoDbConfig.Reminder;

                    if (!string.IsNullOrEmpty(mongoDbClusterConfig.DbConn))
                    {
                        siloBuilder.UseMongoDBClient(mongoDbClusterConfig.DbConn);
                    }
                    else if (!string.IsNullOrEmpty(mongoDbStorageConfig.DbConn))
                    {
                        siloBuilder.UseMongoDBClient(mongoDbStorageConfig.DbConn);
                    }
                    else if (!string.IsNullOrEmpty(mongoDbReminderConfig.DbConn))
                    {
                        siloBuilder.UseMongoDBClient(mongoDbReminderConfig.DbConn);
                    }

                    siloBuilder.UseMongoDBClustering(options =>
                    {
                        options.DatabaseName = mongoDbClusterConfig.DbName;

                        if (!string.IsNullOrEmpty(mongoDbClusterConfig.CollectionPrefix))
                        {
                            options.CollectionPrefix = mongoDbClusterConfig.CollectionPrefix;
                        }
                    })
                    .AddMongoDBGrainStorageAsDefault(optionsBuilder =>
                    {
                        optionsBuilder.Configure(options =>
                        {
                            options.DatabaseName = mongoDbStorageConfig.DbName;

                            if (!string.IsNullOrEmpty(mongoDbStorageConfig.CollectionPrefix))
                            {
                                options.CollectionPrefix = mongoDbStorageConfig.CollectionPrefix;
                            }
                        });
                    })
                    .UseMongoDBReminders(options =>
                    {
                        options.DatabaseName = mongoDbReminderConfig.DbName;

                        if (!string.IsNullOrEmpty(mongoDbReminderConfig.CollectionPrefix))
                        {
                            options.CollectionPrefix = mongoDbReminderConfig.CollectionPrefix;
                        }
                    });
                    break;

                case "SQLDB":
                    var sqlDbConfig = orleansProvider.SQLDB;
                    siloBuilder.UseAdoNetClustering(options =>
                    {
                        options.Invariant = sqlDbConfig.Cluster.Invariant ?? @"System.Data.SqlClient";
                        options.ConnectionString = sqlDbConfig.Cluster.DbConn;
                    }).AddAdoNetGrainStorageAsDefault(options =>
                    {
                        options.Invariant = sqlDbConfig.Storage.Invariant ?? @"System.Data.SqlClient";
                        options.ConnectionString = sqlDbConfig.Storage.DbConn;
                    }).UseAdoNetReminderService(options =>
                    {
                        options.Invariant = sqlDbConfig.Reminder.Invariant ?? @"System.Data.SqlClient";
                        options.ConnectionString = sqlDbConfig.Reminder.DbConn;
                    });
                    break;

                case "MYSQL":
                    var mysqlConfig = orleansProvider.SQLDB;
                    siloBuilder.UseAdoNetClustering(options =>
                    {
                        options.Invariant = mysqlConfig.Cluster.Invariant ?? @"MySql.Data.MySqlClient";
                        options.ConnectionString = mysqlConfig.Cluster.DbConn;
                    }).AddAdoNetGrainStorageAsDefault(options =>
                    {
                        options.Invariant = mysqlConfig.Storage.Invariant ?? @"MySql.Data.MySqlClient";
                        options.ConnectionString = mysqlConfig.Storage.DbConn;
                    }).UseAdoNetReminderService(options =>
                    {
                        options.Invariant = mysqlConfig.Reminder.Invariant ?? @"MySql.Data.MySqlClient";
                        options.ConnectionString = mysqlConfig.Reminder.DbConn;
                    });
                    break;

                case "InMemory":

                    if (IpAddressNotSpecified(siloConfig.AdvertisedIp))
                    {
                        siloBuilder.UseDevelopmentClustering(option =>
                        {
                            option.PrimarySiloEndpoint = new IPEndPoint(IPAddress.Loopback, siloConfig.SiloPort);
                        });

                        if (string.IsNullOrEmpty(siloConfig.AdvertisedIp.Trim()) || siloConfig.AdvertisedIp == "*")
                        {
                            siloBuilder.ConfigureEndpoints("localhost", siloConfig.SiloPort, siloConfig.GatewayPort, listenOnAnyHostAddress: siloConfig.ListenOnAnyHostAddress);
                        }
                        else
                        {
                            siloBuilder.ConfigureEndpoints(siloConfig.AdvertisedIp, siloConfig.SiloPort, siloConfig.GatewayPort, listenOnAnyHostAddress: siloConfig.ListenOnAnyHostAddress);
                        }
                    }
                    else
                    {
                        var advertisedIp = IPAddress.Parse(siloConfig.AdvertisedIp.Trim());
                        siloBuilder.UseDevelopmentClustering(option =>
                        {
                            option.PrimarySiloEndpoint = new IPEndPoint(advertisedIp, siloConfig.SiloPort);
                        })
                        .ConfigureEndpoints(advertisedIp, siloConfig.SiloPort, siloConfig.GatewayPort, siloConfig.ListenOnAnyHostAddress);
                    }

                    siloBuilder
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
