using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using GranDen.Orleans.NetCoreGenericHost.CommonLib.Exceptions;
using GranDen.Orleans.NetCoreGenericHost.CommonLib.Helpers;
using GranDen.Orleans.NetCoreGenericHost.CommonLib.HostTypedOptions;
using GranDen.Orleans.Server.SharedInterface;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Orleans;
using Orleans.ApplicationParts;
using Orleans.Configuration;
using Orleans.Hosting;
using Serilog;
using HostBuilderContext = Microsoft.Extensions.Hosting.HostBuilderContext;

namespace GranDen.Orleans.NetCoreGenericHost.CommonLib
{
    /// <summary>
    /// Orleans Silo Host builder library
    /// </summary>
    public static class OrleansSiloBuilderExtension
    {
        /// <summary>
        /// Create .NET Core Generic HostBuilder using various default configuration
        /// </summary>
        /// <param name="args">Command line arguments</param>
        /// <returns></returns>
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            new HostBuilder()
                .UseHostConfiguration(args)
                .UseAppConfiguration(args)
                .UseConfigurationOptions()
                .ApplyOrleansSettings()
                .ConfigureLogging(logging => logging.AddSerilog(dispose: true))
                .UseConsoleLifetime()
                .UseSerilog();

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
        /// <param name="builder"></param>
        /// <param name="args"></param>
        /// <param name="configureDelegate"></param>
        /// <param name="configEnvironmentPrefix"></param>
        /// <param name="configFilePrefix"></param>
        /// <returns></returns>
        public static IHostBuilder UseAppConfiguration(this IHostBuilder builder,
            string[] args,
            Action<HostBuilderContext, IConfigurationBuilder> configureDelegate = null,
            string configEnvironmentPrefix = "ORLEANS_HOST_APP_",
            string configFilePrefix = "hostsettings")
        {
            if (configureDelegate == null)
            {
                builder.ConfigureAppConfiguration((hostContext, configurationBuilder) =>
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
                builder.ConfigureAppConfiguration(configureDelegate);
            }
            return builder;
        }

        /// <summary>
        /// Setup Orleans Host configuration
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="configureDelegate"></param>
        /// <param name="orleansConfigSection"></param>
        /// <param name="siloConfigSection"></param>
        /// <param name="orleansProviderSection"></param>
        /// <param name="orleansDashboardOptionSection"></param>
        /// <param name="grainLoadOptionSection"></param>
        /// <returns></returns>
        public static IHostBuilder UseConfigurationOptions(this IHostBuilder builder,
            Action<HostBuilderContext, IServiceCollection> configureDelegate = null,
            string orleansConfigSection = "Orleans",
            string siloConfigSection = "SiloConfig",
            string orleansProviderSection = "Provider",
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
                    services.Configure<OrleansDashboardOption>(orleansSettings.GetSection(orleansDashboardOptionSection));
                });
            }
            else
            {
                builder.ConfigureServices(configureDelegate);
            }

            return builder;
        }

        /// <summary>
        /// Config Orleans Silo Builder using default or custom Orleans Host configuration
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="configDelegate"></param>
        /// <param name="orleansConfigSection"></param>
        /// <param name="siloConfigSection"></param>
        /// <param name="orleansProviderSection"></param>
        /// <param name="grainLoadOptionSection"></param>
        /// <param name="orleansDashboardOptionSection"></param>
        /// <returns></returns>
        public static IHostBuilder ApplyOrleansSettings(this IHostBuilder builder,
            Func<HostBuilderContext, IConfigurationSection> configDelegate = null,
            string orleansConfigSection = "Orleans",
            string siloConfigSection = "SiloConfig",
            string orleansProviderSection = "Provider",
            string grainLoadOptionSection = "GrainOption",
            string orleansDashboardOptionSection = "Dashboard")
        {
            if (configDelegate == null)
            {
                builder.UseOrleans((context, siloBuilder) =>
                {
                    var orleansSettings = context.Configuration.GetSection(orleansConfigSection);
                    var siloConfig = orleansSettings.GetTypedConfig<SiloConfigOption>(siloConfigSection);
                    var orleansProviderConfig =
                        orleansSettings.GetTypedConfig<OrleansProviderOption>(orleansProviderSection);
                    var grainLoadOption = orleansSettings.GetTypedConfig<GrainLoadOption>(grainLoadOptionSection);
                    var orleansDashboardOption =
                        orleansSettings.GetTypedConfig<OrleansDashboardOption>(orleansDashboardOptionSection);

                    ConfigSiloBuilder(siloBuilder, siloConfig, orleansProviderConfig, grainLoadOption, orleansDashboardOption);
                });
            }
            else
            {
                builder.UseOrleans((context, siloBuilder) =>
                {
                    var orleansSettings = configDelegate(context);
                    var siloConfig = orleansSettings.GetTypedConfig<SiloConfigOption>(siloConfigSection);
                    var orleansProviderConfig =
                        orleansSettings.GetTypedConfig<OrleansProviderOption>(orleansProviderSection);
                    var grainLoadOption = orleansSettings.GetTypedConfig<GrainLoadOption>(grainLoadOptionSection);
                    var orleansDashboardOption =
                        orleansSettings.GetTypedConfig<OrleansDashboardOption>(orleansDashboardOptionSection);

                    ConfigSiloBuilder(siloBuilder, siloConfig, orleansProviderConfig, grainLoadOption, orleansDashboardOption);
                });
            }
            return builder;
        }

        #region Private Util Methods

        private static void ConfigSiloBuilder(ISiloBuilder siloBuilder,
            SiloConfigOption siloConfig, OrleansProviderOption orleansProvider, GrainLoadOption grainLoadOption, OrleansDashboardOption orleansDashboard)
        {
            if (orleansDashboard.Enable)
            {
                Log.Information($"Enable Orleans Dashboard (https://github.com/OrleansContrib/OrleansDashboard) on this host {orleansDashboard.Port} port");
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
                });
            }

            var pathResolver = new Func<string, string>(path =>
            {
                if (!path.Contains("{GrainLoadPath}"))
                {
                    return Path.GetFullPath(path, AssemblyUtil.GetCurrentAssemblyPath());
                }

                var loadPathStr = Environment.GetEnvironmentVariable("GrainLoadPath");
                if (string.IsNullOrEmpty(loadPathStr))
                {
                    return Path.GetFullPath(path, AssemblyUtil.GetCurrentAssemblyPath());
                }
                var expendedPathStr = path.Replace("{GrainLoadPath}", loadPathStr);

                var ret = expendedPathStr;

                var envCwd = Environment.CurrentDirectory;
                var cwd = Directory.GetCurrentDirectory();

                if (!Path.IsPathRooted(expendedPathStr))
                {
                    ret = !string.IsNullOrEmpty(envCwd)
                        ? Path.GetFullPath(expendedPathStr, envCwd)
                        : Path.GetFullPath(ret, cwd);
                }

                return ret;
            });

            siloBuilder.ConfigureApplicationParts(parts =>
            {
                parts.AddFromApplicationBaseDirectory().WithReferences();

                var dllPaths = grainLoadOption.LoadPaths;

                ConfigOtherFolderGrainLoad(parts, dllPaths, pathResolver);
            });

            foreach (var serviceConfigAction in GetGrainServiceConfigurationAction(grainLoadOption, pathResolver))
            {
                siloBuilder.ConfigureServices(serviceConfigAction);
            }

            if (IpAddressNotSpecified(siloConfig.AdvertisedIp))
            {
                siloBuilder.ConfigureEndpoints(siloConfig.SiloPort, siloConfig.GatewayPort,
                    listenOnAnyHostAddress: siloConfig.ListenOnAnyHostAddress);
            }
            else
            {
                var advertisedIp = IPAddress.Parse(siloConfig.AdvertisedIp.Trim());
                siloBuilder.ConfigureEndpoints(advertisedIp, siloConfig.SiloPort, siloConfig.GatewayPort,
                    siloConfig.ListenOnAnyHostAddress);
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

                default:
                    siloBuilder.UseLocalhostClustering().UseInMemoryReminderService();
                    break;
            }
        }

        private static bool IpAddressNotSpecified(string ipString)
        {
            if (ipString == null) { return true; }

            return string.IsNullOrEmpty(ipString.Trim()) || "*".Equals(ipString.Trim());
        }

        private static void ConfigOtherFolderGrainLoad(IApplicationPartManager applicationPartManager, IEnumerable<string> pathsList, Func<string, string> pathResolveFunc)
        {
            foreach (var path in pathsList)
            {
                var dllFileInfo = new FileInfo(pathResolveFunc(path));
                var assembly = Assembly.LoadFile(dllFileInfo.FullName);
                applicationPartManager.AddApplicationPart(assembly).WithReferences();
            }
        }

        private static IEnumerable<Action<IServiceCollection>> GetGrainServiceConfigurationAction(GrainLoadOption grainLoadOption, Func<string, string> pathResolveFunc)
        {
            var dllPaths = grainLoadOption.LoadPaths;
            var excludedTypeFullNames = grainLoadOption.ExcludedTypeFullNames;

            return GetAllNeedServiceConfigure(dllPaths, excludedTypeFullNames, pathResolveFunc);
        }

        private static IEnumerable<Action<IServiceCollection>> GetAllNeedServiceConfigure(IEnumerable<string> pathsList, ICollection<string> excludedTypeFullNames, Func<string, string> pathResolveFunc)
        {
            var ret = new List<Action<IServiceCollection>>();
            foreach (var path in pathsList)
            {
                var fullPath = pathResolveFunc(path);
                var dllFileInfo = new FileInfo(fullPath);

                var assemblyDll = Assembly.LoadFrom(dllFileInfo.FullName);
                var types = assemblyDll.GetLoadableTypes();

                var needServiceConfigureClasses = types.Where(x =>
                        typeof(IGrainServiceConfigDelegate).IsAssignableFrom(x)
                        && !x.IsAbstract
                        && !x.IsInterface
                        && !excludedTypeFullNames.Contains(x.FullName)).ToList();

                foreach (var serviceConfigureClass in needServiceConfigureClasses)
                {
                    if (!(Activator.CreateInstance(serviceConfigureClass) is IGrainServiceConfigDelegate serviceConfigDelegate))
                    {
                        throw new LoadGrainDllFailedException(serviceConfigureClass.FullName);
                    }

                    var loadAction = serviceConfigDelegate.ServiceConfigurationAction;
                    ret.Add(loadAction);
                }
            }

            return ret;
        }

        #endregion
    }
}
