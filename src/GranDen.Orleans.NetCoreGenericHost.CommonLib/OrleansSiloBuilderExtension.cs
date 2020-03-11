using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using GranDen.CallExtMethodLib;
using GranDen.Orleans.NetCoreGenericHost.CommonLib.Exceptions;
using GranDen.Orleans.NetCoreGenericHost.CommonLib.Helpers;
using GranDen.Orleans.NetCoreGenericHost.CommonLib.HostTypedOptions;
using GranDen.Orleans.Server.SharedInterface;
using McMaster.NETCore.Plugins;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.UserSecrets;
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
        private const string HostEnvPrefix = "ORLEANS_HOST_";
        private const string AppEnvPrefix = "ORLEANS_HOST_APP_";
        private const string ConfigFilePrefix = "hostsettings";

        /// <summary>
        /// Create .NET Core Generic HostBuilder using various default configuration
        /// </summary>
        /// <param name="args">Command line arguments</param>
        /// <param name="hostEnvPrefix">Configuration Environment variable name prefix, default would be "ORLEANS_HOST_"</param>
        /// <param name="appEnvPrefix"></param>
        /// <param name="configFilePrefix"></param>
        /// <param name="logBuilderAction"></param>
        /// <returns></returns>
        // ReSharper disable once UnusedMember.Global
        public static IHostBuilder CreateHostBuilder(
            string[] args,
            string hostEnvPrefix = HostEnvPrefix,
            string appEnvPrefix = AppEnvPrefix,
            string configFilePrefix = ConfigFilePrefix,
            Action<ILoggingBuilder> logBuilderAction = null)
        {
            if (PlugInLoaderCache != null)
            {
                PlugInLoaderCache = null;
            }

            var hostBuilder = new HostBuilder();
            hostBuilder.ConfigureLogging(logBuilderAction ?? DefaultLoggerHelper.DefaultLogAction)
                .UseHostConfiguration(args, hostEnvironmentPrefix: hostEnvPrefix)
                .UseAppConfiguration(args, configEnvironmentPrefix: appEnvPrefix, configFilePrefix: configFilePrefix)
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
            string hostEnvironmentPrefix = HostEnvPrefix)
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
            string configEnvironmentPrefix = AppEnvPrefix,
            string configFilePrefix = ConfigFilePrefix)
        {
            if (configureDelegate == null)
            {
                hostBuilder.ConfigureAppConfiguration((hostBuilderContext, configurationBuilder) =>
                {
                    configurationBuilder
                        .SetBasePath(GetContextCwd())
                        .AddJsonFile($"{configFilePrefix}.json", optional: true)
                        .AddJsonFile($"{configFilePrefix}.{hostBuilderContext.HostingEnvironment.EnvironmentName}.json",
                            optional: true)
                        .AddEnvironmentVariables(prefix: configEnvironmentPrefix)
                        .AddCommandLine(args);

                    if (hostBuilderContext.HostingEnvironment.IsDevelopment())
                    {
                        var mainAssembly = AssemblyUtil.GetMainAssembly();
                        var userSecretsAttribute = mainAssembly.GetCustomAttribute<UserSecretsIdAttribute>();
                        if (userSecretsAttribute != null)
                        {
                            configurationBuilder.AddUserSecrets(AssemblyUtil.GetMainAssembly());
                        }
                    }
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
        // ReSharper disable once MemberCanBePrivate.Global
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
                void ConfigureDelegate(HostBuilderContext context, IServiceCollection services)
                {
                    services.AddOptions();

                    var orleansSettings = context.Configuration.GetSection(orleansConfigSection);

                    services.Configure<SiloConfigOption>(orleansSettings.GetSection(siloConfigSection));
                    services.Configure<GrainLoadOption>(orleansSettings.GetSection(grainLoadOptionSection));
                    services.Configure<OrleansProviderOption>(orleansSettings.GetSection(orleansProviderSection));
                    services.Configure<MultiClusterOptions>(orleansSettings.GetSection(orleansMultiClusterSection));
                    services.Configure<OrleansDashboardOption>(
                        orleansSettings.GetSection(orleansDashboardOptionSection));
                }

                configureDelegate = ConfigureDelegate;
            }

            builder.ConfigureServices(configureDelegate);
            return builder;
        }

        // For default logger label
        // ReSharper disable once ClassNeverInstantiated.Local
        class OrleansSiloBuilder
        {
        }

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
        // ReSharper disable once MemberCanBePrivate.Global
        public static IHostBuilder ApplyOrleansSettings(this IHostBuilder hostBuilder,
            Func<HostBuilderContext, IConfigurationSection> configurationGetterFunc = null,
            string orleansConfigSection = "Orleans",
            string siloConfigSection = "SiloConfig",
            string orleansProviderSection = "Provider",
            string grainLoadOptionSection = "GrainOption",
            string orleansDashboardOptionSection = "Dashboard")
        {
            if (configurationGetterFunc != null)
            {
                Action<HostBuilderContext, ISiloBuilder> ConfigureDelegate(HostBuilderContext context,
                    ISiloBuilder siloBuilder, Func<HostBuilderContext, IConfigurationSection> configGetter)
                {
                    var orleansSettings = configGetter(context);

                    void ConfigSiloBuilderDelegate(HostBuilderContext hostBuilderContext, ISiloBuilder iSiloBuilder)
                    {
                        var siloConfig = orleansSettings.GetTypedConfig<SiloConfigOption>(siloConfigSection);
                        var orleansProviderConfig =
                            orleansSettings.GetTypedConfig<OrleansProviderOption>(orleansProviderSection);
                        var grainLoadOption = orleansSettings.GetTypedConfig<GrainLoadOption>(grainLoadOptionSection);
                        var orleansDashboardOption =
                            orleansSettings.GetTypedConfig<OrleansDashboardOption>(orleansDashboardOptionSection);
                        var logger = (new DefaultLoggerHelper()).CreateDefaultLogger<OrleansSiloBuilder>();
                        siloBuilder.ConfigSiloBuilder(siloConfig, orleansProviderConfig, grainLoadOption,
                            orleansDashboardOption, logger);
                    }

                    return ConfigSiloBuilderDelegate;
                }

                void OrleansConfigDelegate(HostBuilderContext hostBuilderContext, ISiloBuilder siloBuilder)
                {
                    var configAction = ConfigureDelegate(hostBuilderContext, siloBuilder, configurationGetterFunc);
                    configAction(hostBuilderContext, siloBuilder);
                }

                hostBuilder.UseOrleans(OrleansConfigDelegate);
            }
            else
            {
                hostBuilder.UseOrleans((context, siloBuilder) =>
                {
                    var orleansSettings = context.Configuration.GetSection(orleansConfigSection);
                    var siloConfig = orleansSettings.GetTypedConfig<SiloConfigOption>(siloConfigSection);
                    var orleansProviderConfig =
                        orleansSettings.GetTypedConfig<OrleansProviderOption>(orleansProviderSection);
                    var grainLoadOption = orleansSettings.GetTypedConfig<GrainLoadOption>(grainLoadOptionSection);
                    var orleansDashboardOption =
                        orleansSettings.GetTypedConfig<OrleansDashboardOption>(orleansDashboardOptionSection);
                    var logger = (new DefaultLoggerHelper()).CreateDefaultLogger<OrleansSiloBuilder>();
                    siloBuilder.ConfigSiloBuilder(siloConfig, orleansProviderConfig, grainLoadOption,
                        orleansDashboardOption, logger);
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
            SiloConfigOption siloConfig, OrleansProviderOption orleansProvider, GrainLoadOption grainLoadOption,
            OrleansDashboardOption orleansDashboard, ILogger logger)
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
                siloBuilder.ConfigureEndpoints(Dns.GetHostName(), siloConfig.SiloPort, siloConfig.GatewayPort,
                    listenOnAnyHostAddress: siloConfig.ListenOnAnyHostAddress);
            }
            else if (IpAddressNotSpecified(siloConfig.AdvertisedIp))
            {
                siloBuilder.ConfigureEndpoints(siloConfig.SiloPort, siloConfig.GatewayPort,
                    listenOnAnyHostAddress: siloConfig.ListenOnAnyHostAddress);
            }
            else
            {
                try
                {
                    var advertisedIp = IPAddress.Parse(siloConfig.AdvertisedIp.Trim());
                    siloBuilder.ConfigureEndpoints(advertisedIp, siloConfig.SiloPort, siloConfig.GatewayPort,
                        siloConfig.ListenOnAnyHostAddress);
                }
                catch (FormatException)
                {
                    siloBuilder.ConfigureEndpoints(Dns.GetHostName(), siloConfig.SiloPort, siloConfig.GatewayPort,
                        listenOnAnyHostAddress: siloConfig.ListenOnAnyHostAddress);
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
                try
                {
                    var helper = new ExtMethodInvoker("Orleans.TelemetryConsumers.AI");
                    siloBuilder = helper.Invoke<ISiloBuilder>(
                        new ExtMethodInfo { MethodName = "AddApplicationInsightsTelemetryConsumer", ExtendedType = typeof(ISiloBuilder) }, 
                        siloBuilder, siloConfig.AzureApplicationInsightKey);
                }
                catch (Exception exception)
                {
                   throw new AzureApplicationInsightLibLoadFailedException(exception);
                }
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
                            throw new OrleansSiloHostConfigException(
                                $"Module \'{dllPaths}\' has no orleans support code gen");
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

            if (siloConfig.ExlcudeGrains.Any())
            {
                siloBuilder.Configure<GrainClassOptions>(options =>
                {
                    options.ExcludedGrainTypes.AddRange(siloConfig.ExlcudeGrains);
                });
            }

            switch (orleansProvider.DefaultProvider)
            {
                case "MongoDB":
                {
                    var mongoDbConfig = orleansProvider.MongoDB;

                    var mongoDbClusterConfig = mongoDbConfig.Cluster;
                    var mongoDbStorageConfig = mongoDbConfig.Storage;
                    var mongoDbReminderConfig = mongoDbConfig.Reminder;

                    ExtMethodInvoker helper;
                    try
                    {
                        helper = new ExtMethodInvoker("Orleans.Providers.MongoDB");

                        if (!string.IsNullOrEmpty(mongoDbClusterConfig.DbConn))
                        {
                            siloBuilder = helper.Invoke<ISiloBuilder>(
                                new ExtMethodInfo
                                {
                                    MethodName = "UseMongoDBClient", ExtendedType = typeof(ISiloBuilder)
                                },
                                siloBuilder, mongoDbClusterConfig.DbConn);
                        }
                        else if (!string.IsNullOrEmpty(mongoDbStorageConfig.DbConn))
                        {
                            siloBuilder = helper.Invoke<ISiloBuilder>(
                                new ExtMethodInfo
                                {
                                    MethodName = "UseMongoDBClient", ExtendedType = typeof(ISiloBuilder)
                                },
                                siloBuilder, mongoDbStorageConfig.DbConn);
                        }
                        else if (!string.IsNullOrEmpty(mongoDbReminderConfig.DbConn))
                        {
                            siloBuilder = helper.Invoke<ISiloBuilder>(
                                new ExtMethodInfo
                                {
                                    MethodName = "UseMongoDBClient", ExtendedType = typeof(ISiloBuilder)
                                },
                                siloBuilder, mongoDbReminderConfig.DbConn);
                        }
                    }
                    catch (Exception ex)
                    {
                        throw new MongoDbLibLoadFailedException(ex);
                    }

                    try
                    {
                        var mongoDbMembershipTableOptionsType =
                            helper.ExtensionLibAssembly.GetType(
                                "Orleans.Providers.MongoDB.Configuration.MongoDBMembershipTableOptions", true);
                        var mongoDbMembershipTableOptionsValue = new Dictionary<string, object>
                        {
                            ["DatabaseName"] = mongoDbClusterConfig.DbName
                        };
                        if (!string.IsNullOrEmpty(mongoDbClusterConfig.CollectionPrefix))
                        {
                            mongoDbMembershipTableOptionsValue["CollectionPrefix"] =
                                mongoDbClusterConfig.CollectionPrefix;
                        }

                        var configMongoDbClusteringAction =
                            CreateDelegateHelper.CreateAssignValueAction(mongoDbMembershipTableOptionsType, "options",
                                mongoDbMembershipTableOptionsValue);

                        siloBuilder = helper.Invoke<ISiloBuilder>(
                            new ExtMethodInfo
                            {
                                MethodName = "UseMongoDBClustering", ExtendedType = typeof(ISiloBuilder)
                            },
                            siloBuilder, configMongoDbClusteringAction);
                    }
                    catch (Exception ex)
                    {
                        throw new MongoDbClusterLibLoadFailedException(ex);
                    }

                    try
                    {
                        var mongoDbGrainStorageOptionsType =
                            helper.ExtensionLibAssembly.GetType(
                                "Orleans.Providers.MongoDB.Configuration.MongoDBGrainStorageOptions", true);
                        var mongoDbGrainStorageOptionsValue = new Dictionary<string, object>
                        {
                            ["DatabaseName"] = mongoDbStorageConfig.DbName,
                        };
                        if (!string.IsNullOrEmpty(mongoDbStorageConfig.CollectionPrefix))
                        {
                            mongoDbGrainStorageOptionsValue["CollectionPrefix"] = mongoDbStorageConfig.CollectionPrefix;
                        }

                        var configMongoDbGrainStorageAction =
                            CreateDelegateHelper.CreateAssignValueAction(mongoDbGrainStorageOptionsType, "options",
                                mongoDbGrainStorageOptionsValue);

                        siloBuilder = helper.Invoke<ISiloBuilder>(
                            new ExtMethodInfo
                            {
                                MethodName = "AddMongoDBGrainStorage", ExtendedType = typeof(ISiloBuilder)
                            },
                            siloBuilder, "Default", configMongoDbGrainStorageAction);
                    }
                    catch (Exception ex)
                    {
                        throw new MongoDbGrainStorageLibLoadFailedException(ex);
                    }

                    try
                    {
                        var mongoDbRemindersOptionsType =
                            helper.ExtensionLibAssembly.GetType(
                                "Orleans.Providers.MongoDB.Configuration.MongoDBRemindersOptions", true);
                        var mongoDbRemindersOptionsValue = new Dictionary<string, object>
                        {
                            ["DatabaseName"] = mongoDbReminderConfig.DbName
                        };
                        if (!string.IsNullOrEmpty(mongoDbReminderConfig.CollectionPrefix))
                        {
                            mongoDbRemindersOptionsValue["CollectionPrefix"] = mongoDbReminderConfig.CollectionPrefix;
                        }

                        var configMongoDbRemindersAction =
                            CreateDelegateHelper.CreateAssignValueAction(mongoDbRemindersOptionsType, "options",
                                mongoDbRemindersOptionsValue);

                        siloBuilder = helper.Invoke<ISiloBuilder>(
                            new ExtMethodInfo {MethodName = "UseMongoDBReminders", ExtendedType = typeof(ISiloBuilder)},
                            siloBuilder, configMongoDbRemindersAction);
                    }
                    catch (Exception ex)
                    {
                        throw new MongoDbReminderLibLoadFailedException(ex);
                    }
                }
                    break;

                case "SQLDB":
                {
                    var sqlDbConfig = orleansProvider.SQLDB;

                    try
                    {
                        var clusterExtMethodHelper = new ExtMethodInvoker("Orleans.Clustering.AdoNet");

                        var adoNetClusteringSiloOptionsType =
                            clusterExtMethodHelper.ExtensionLibAssembly.GetType(
                                "Orleans.Configuration.AdoNetClusteringSiloOptions", true);
                        var adoNetClusteringSiloOptionsValue = new Dictionary<string, object>
                        {
                            ["Invariant"] = sqlDbConfig.Cluster.Invariant ?? @"System.Data.SqlClient",
                            ["ConnectionString"] = sqlDbConfig.Cluster.DbConn
                        };
                        var configAdoNetClusteringAction =
                            CreateDelegateHelper.CreateAssignValueAction(adoNetClusteringSiloOptionsType, "options",
                                adoNetClusteringSiloOptionsValue);
                        siloBuilder = clusterExtMethodHelper.Invoke<ISiloBuilder>(
                            new ExtMethodInfo {MethodName = "UseAdoNetClustering", ExtendedType = typeof(ISiloBuilder)},
                            siloBuilder, configAdoNetClusteringAction);
                    }
                    catch (Exception ex)
                    {
                        throw new SqlServerClusterLibLoadFailedException(ex);
                    }

                    try
                    {
                        var storageExtMethodHelper = new ExtMethodInvoker("Orleans.Persistence.AdoNet");
                        var adoNetGrainStorageOptionsType =
                            storageExtMethodHelper.ExtensionLibAssembly.GetType(
                                "Orleans.Configuration.AdoNetGrainStorageOptions", true);
                        var adoNetGrainStorageOptionsValue = new Dictionary<string, object>
                        {
                            ["Invariant"] = sqlDbConfig.Storage.Invariant ?? @"System.Data.SqlClient",
                            ["ConnectionString"] = sqlDbConfig.Storage.DbConn
                        };
                        var configAdoNetStorageAction =
                            CreateDelegateHelper.CreateAssignValueAction(adoNetGrainStorageOptionsType, "options",
                                adoNetGrainStorageOptionsValue);
                        siloBuilder = storageExtMethodHelper.Invoke<ISiloBuilder>(
                            new ExtMethodInfo
                            {
                                MethodName = "AddAdoNetGrainStorage", ExtendedType = typeof(ISiloBuilder)
                            },
                            siloBuilder, "Default", configAdoNetStorageAction);
                    }
                    catch (Exception ex)
                    {
                        throw new SqlServerGrainStorageLibLoadFailedException(ex);
                    }

                    try
                    {
                        var reminderExtMethodHelper = new ExtMethodInvoker("Orleans.Reminders.AdoNet");
                        var adoNetReminderTableOptionsType =
                            reminderExtMethodHelper.ExtensionLibAssembly.GetType(
                                "Orleans.Configuration.AdoNetReminderTableOptions", true);
                        var adoNetReminderTableOptionsValue = new Dictionary<string, object>
                        {
                            ["Invariant"] = sqlDbConfig.Reminder.Invariant ?? @"System.Data.SqlClient",
                            ["ConnectionString"] = sqlDbConfig.Reminder.DbConn
                        };
                        var configAdoNetReminderAction =
                            CreateDelegateHelper.CreateAssignValueAction(adoNetReminderTableOptionsType, "options",
                                adoNetReminderTableOptionsValue);
                        siloBuilder = reminderExtMethodHelper.Invoke<ISiloBuilder>(
                            new ExtMethodInfo
                            {
                                MethodName = "UseAdoNetReminderService", ExtendedType = typeof(ISiloBuilder)
                            },
                            siloBuilder, configAdoNetReminderAction);
                    }
                    catch (Exception ex)
                    {
                        throw new SqlServerReminderLibLoadFailedException(ex);
                    }
                }
                    break;

                case "MYSQL":
                {
                    var config = orleansProvider.SQLDB;

                    try
                    {
                        var clusterExtMethodHelper = new ExtMethodInvoker("Orleans.Clustering.AdoNet");
                        var adoNetClusteringSiloOptionsType =
                            clusterExtMethodHelper.ExtensionLibAssembly.GetType(
                                "Orleans.Configuration.AdoNetClusteringSiloOptions", true);
                        var adoNetClusteringSiloOptionsValue = new Dictionary<string, object>
                        {
                            ["Invariant"] = config.Cluster.Invariant ?? @"MySql.Data.MySqlClient",
                            ["ConnectionString"] = config.Cluster.DbConn
                        };
                        var configAdoNetClusteringAction =
                            CreateDelegateHelper.CreateAssignValueAction(adoNetClusteringSiloOptionsType, "options",
                                adoNetClusteringSiloOptionsValue);
                        siloBuilder = clusterExtMethodHelper.Invoke<ISiloBuilder>(
                            new ExtMethodInfo {MethodName = "UseAdoNetClustering", ExtendedType = typeof(ISiloBuilder)},
                            siloBuilder, configAdoNetClusteringAction);
                    }
                    catch (Exception ex)
                    {
                        throw new MySqlClusterLibLoadFailedException(ex);
                    }

                    try
                    {
                        var storageExtMethodHelper = new ExtMethodInvoker("Orleans.Persistence.AdoNet");
                        var adoNetGrainStorageOptionsType =
                            storageExtMethodHelper.ExtensionLibAssembly.GetType(
                                "Orleans.Configuration.AdoNetGrainStorageOptions", true);
                        var adoNetGrainStorageOptionsValue = new Dictionary<string, object>
                        {
                            ["Invariant"] = config.Storage.Invariant ?? @"MySql.Data.MySqlClient",
                            ["ConnectionString"] = config.Storage.DbConn
                        };
                        var configAdoNetStorageAction =
                            CreateDelegateHelper.CreateAssignValueAction(adoNetGrainStorageOptionsType, "options",
                                adoNetGrainStorageOptionsValue);
                        siloBuilder = storageExtMethodHelper.Invoke<ISiloBuilder>(
                            new ExtMethodInfo
                            {
                                MethodName = "AddAdoNetGrainStorage", ExtendedType = typeof(ISiloBuilder)
                            },
                            siloBuilder, "Default", configAdoNetStorageAction);
                    }
                    catch (Exception ex)
                    {
                        throw new MySqlGrainStorageLibLoadFailedException(ex);
                    }

                    try
                    {
                        var reminderExtMethodHelper = new ExtMethodInvoker("Orleans.Reminders.AdoNet");
                        var adoNetReminderTableOptionsType =
                            reminderExtMethodHelper.ExtensionLibAssembly.GetType(
                                "Orleans.Configuration.AdoNetReminderTableOptions", true);
                        var adoNetReminderTableOptionsValue = new Dictionary<string, object>
                        {
                            ["Invariant"] = config.Reminder.Invariant ?? @"MySql.Data.MySqlClient",
                            ["ConnectionString"] = config.Reminder.DbConn
                        };
                        var configAdoNetReminderAction =
                            CreateDelegateHelper.CreateAssignValueAction(adoNetReminderTableOptionsType, "options",
                                adoNetReminderTableOptionsValue);
                        siloBuilder = reminderExtMethodHelper.Invoke<ISiloBuilder>(
                            new ExtMethodInfo
                            {
                                MethodName = "UseAdoNetReminderService", ExtendedType = typeof(ISiloBuilder)
                            },
                            siloBuilder, configAdoNetReminderAction);
                    }
                    catch (Exception ex)
                    {
                        throw new MySqlReminderLibLoadFailedException(ex);
                    }
                }
                    break;

                case "InMemory":
                {
                    if (IpAddressNotSpecified(siloConfig.AdvertisedIp))
                    {
                        siloBuilder.UseDevelopmentClustering(option =>
                        {
                            option.PrimarySiloEndpoint = new IPEndPoint(IPAddress.Loopback, siloConfig.SiloPort);
                        });

                        if (string.IsNullOrEmpty(siloConfig.AdvertisedIp.Trim()) || siloConfig.AdvertisedIp == "*")
                        {
                            siloBuilder.ConfigureEndpoints("localhost", siloConfig.SiloPort, siloConfig.GatewayPort,
                                listenOnAnyHostAddress: siloConfig.ListenOnAnyHostAddress);
                        }
                        else
                        {
                            siloBuilder.ConfigureEndpoints(siloConfig.AdvertisedIp, siloConfig.SiloPort,
                                siloConfig.GatewayPort, listenOnAnyHostAddress: siloConfig.ListenOnAnyHostAddress);
                        }
                    }
                    else
                    {
                        var advertisedIp = IPAddress.Parse(siloConfig.AdvertisedIp.Trim());
                        siloBuilder.UseDevelopmentClustering(option =>
                            {
                                option.PrimarySiloEndpoint = new IPEndPoint(advertisedIp, siloConfig.SiloPort);
                            })
                            .ConfigureEndpoints(advertisedIp, siloConfig.SiloPort, siloConfig.GatewayPort,
                                siloConfig.ListenOnAnyHostAddress);
                    }

                    try
                    {
                        siloBuilder.UseInMemoryGrainStorage()
                            .UseInMemoryReminderService();
                    }
                    catch (Exception ex)
                    {
                        throw new InMemoryGrainStorageLibLoadFailedException(ex);
                    }
                }
                    break;

                default:
                {
                    siloBuilder.UseLocalhostClustering(
                            serviceId: siloConfig.ServiceId,
                            clusterId: siloConfig.ClusterId,
                            siloPort: siloConfig.SiloPort,
                            gatewayPort: siloConfig.GatewayPort)
                        .UseInMemoryGrainStorage()
                        .UseInMemoryReminderService();
                }
                    break;
            }

            return siloBuilder;
        }

        private static ISiloBuilder UseInMemoryGrainStorage(this ISiloBuilder siloBuilder)
        {
            try
            {
                var storageExtMethodHelper = new ExtMethodInvoker("OrleansProviders");
                siloBuilder = storageExtMethodHelper.Invoke<ISiloBuilder>(
                    new ExtMethodInfo {MethodName = "AddMemoryGrainStorage", ExtendedType = typeof(ISiloBuilder)},
                    siloBuilder, "Default", Type.Missing);

                return siloBuilder;
            }
            catch (Exception ex)
            {
                throw new InMemoryGrainStorageLibLoadFailedException(ex);
            }
        }
    }
}
