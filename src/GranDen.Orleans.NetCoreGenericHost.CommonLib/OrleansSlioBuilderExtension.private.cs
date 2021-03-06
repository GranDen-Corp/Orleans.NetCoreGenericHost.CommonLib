﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using GranDen.CallExtMethodLib;
using GranDen.Orleans.NetCoreGenericHost.CommonLib.Exceptions;
using GranDen.Orleans.NetCoreGenericHost.CommonLib.Helpers;
using GranDen.Orleans.NetCoreGenericHost.CommonLib.HostTypedOptions;
using GranDen.Orleans.Server.SharedInterface;
using McMaster.NETCore.Plugins;
using Microsoft.Extensions.Logging;
using Orleans.Hosting;

namespace GranDen.Orleans.NetCoreGenericHost.CommonLib
{
    public static partial class OrleansSiloBuilderExtension
    {
        /// <summary>
        /// PlugIn Loader References for non-Main Executable folder assemblies.
        /// </summary>
        public static Dictionary<string, PluginLoader> PlugInLoaderCache { get; private set; }

        #region SiloBuilder Internal Configuration Methods

        private static ISiloBuilder ApplyOrleansDashboard(this ISiloBuilder siloBuilder, OrleansDashboardOption orleansDashboard, ILogger logger)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                try
                {
                    var helper = new ExtMethodInvoker("Orleans.TelemetryConsumers.Linux");
                    siloBuilder = helper.Invoke<ISiloBuilder>(
                        new ExtMethodInfo { MethodName = "UseLinuxEnvironmentStatistics", ExtendedType = typeof(ISiloBuilder) },
                        siloBuilder);
                }
                catch (Exception exception)
                {
                    throw new LinuxTelemetryLibLoadFailedException(exception);
                }
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                try
                {
                    var helper = new ExtMethodInvoker("Orleans.TelemetryConsumers.Counters");
                    siloBuilder = helper.Invoke<ISiloBuilder>(
                        new ExtMethodInfo { MethodName = "UsePerfCounterEnvironmentStatistics", ExtendedType = typeof(ISiloBuilder) },
                        siloBuilder);
                }
                catch (Exception exception)
                {
                    throw new WindowsPerformanceCountersTelemetryLibLoadFailedException(exception);
                }
            }

            logger.LogInformation($"Enable Orleans Dashboard on this host {orleansDashboard.Port} port");

            try
            {
#if NETCOREAPP2_1
                var helper = new ExtMethodInvoker("GranDen.Orleans.NetCoreGenericHost.OrleansDashboard");
                var dashboardOptionsType = helper.ExtensionLibAssembly.GetType("GranDen.Orleans.NetCoreGenericHost.OrleansDashboard.DashboardOptions", true);
#else
                var helper = new ExtMethodInvoker("OrleansDashboard");
                var dashboardOptionsType = helper.ExtensionLibAssembly.GetType("OrleansDashboard.DashboardOptions", true);
#endif
                var configAction =
                    CreateDelegateHelper.CreateAssignValueAction(dashboardOptionsType, "options", new Dictionary<string, object> { ["Port"] = orleansDashboard.Port });

                siloBuilder = helper.Invoke<ISiloBuilder>(
                    new ExtMethodInfo { MethodName = "UseDashboard", ExtendedType = typeof(ISiloBuilder) }, 
                    siloBuilder, configAction);

                return siloBuilder;
            }
            catch (Exception exception)
            {
                throw new DashboardLoadFailedException(exception);
            }
        }

        #endregion

        #region Assembly Path Resolver

        private static string PathResolver(string path)
        {
            if (!path.Contains("{GrainLoadPath}"))
            {
                return Path.GetFullPath(path, AssemblyUtil.GetMainAssemblyFolder());
            }

            var loadPathStr = Environment.GetEnvironmentVariable("GrainLoadPath");
            if (string.IsNullOrEmpty(loadPathStr))
            {
                return Path.GetFullPath(path, AssemblyUtil.GetMainAssemblyFolder());
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
        }

        #endregion

        #region Private Util Methods

        private static string GetContextCwd()
        {
            var cwdEnv = Environment.CurrentDirectory;
            var cwd = Directory.GetCurrentDirectory();
            return string.IsNullOrEmpty(cwdEnv) ? cwd : cwdEnv;
        }

        private static bool IsRunningOnContainer()
        {
            var onDockerFlag = Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER");

            return onDockerFlag != null && onDockerFlag.ToLower().Equals("true");
        }

        private static bool IpAddressNotSpecified(string ipString)
        {
            if (ipString == null) { return true; }

            return string.IsNullOrEmpty(ipString.Trim()) || "*".Equals(ipString.Trim());
        }

        private static IEnumerable<IGrainServiceConfigDelegate> GetGrainServiceConfigurationAction(GrainLoadOption grainLoadOption, Func<string, string> pathResolveFunc)
        {
            var dllPaths = grainLoadOption.LoadPaths;
            var excludedTypeFullNames = grainLoadOption.ExcludedTypeFullNames;

            if (grainLoadOption.CallMainExecutionPathServiceConfigDelegate)
            {
                dllPaths.Add(AssemblyUtil.GetMainAssemblyPath());
            }

            return GetAllNeedServiceConfigure(dllPaths, excludedTypeFullNames, pathResolveFunc);
        }

        private static IEnumerable<IGrainServiceConfigDelegate> GetAllNeedServiceConfigure(IEnumerable<string> pathsList, ICollection<string> excludedTypeFullNames, Func<string, string> pathResolveFunc)
        {
            var ret = new List<IGrainServiceConfigDelegate>();
            foreach (var path in pathsList)
            {
                var fullPath = pathResolveFunc(path);
                var assemblyDll = GetNonMainExeFolderAssembly(fullPath);

                var types = assemblyDll.GetLoadableTypes();

                var needServiceConfigureClasses = types.Where(x =>
                        typeof(IGrainServiceConfigDelegate).IsAssignableFrom(x)
                        && !x.IsAbstract
                        && x.IsClass
                        && !excludedTypeFullNames.Contains(x.FullName)).ToList();

                foreach (var serviceConfigureClass in needServiceConfigureClasses)
                {
                    if (!(Activator.CreateInstance(serviceConfigureClass) is IGrainServiceConfigDelegate serviceConfigDelegate))
                    {
                        throw new LoadGrainDllFailedException(serviceConfigureClass.FullName);
                    }

                    ret.Add(serviceConfigDelegate);
                }
            }
            return ret;
        }

        private static Assembly GetNonMainExeFolderAssembly(string fullPath)
        {
            PluginLoader loader;
            if (!PlugInLoaderCache.ContainsKey(fullPath))
            {
                loader = PluginLoader.CreateFromAssemblyFile(fullPath, pluginConfig =>
                {
                    pluginConfig.PreferSharedTypes = true;
                });
                PlugInLoaderCache.TryAdd(fullPath, loader);
            }
            else
            {
                loader = PlugInLoaderCache[fullPath];
            }

            var assembly = loader.LoadAssemblyFromPath(fullPath);

            return assembly;
        }

        #endregion
    }
}
