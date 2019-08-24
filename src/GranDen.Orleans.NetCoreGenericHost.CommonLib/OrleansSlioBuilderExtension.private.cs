using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Loader;
using GranDen.Orleans.NetCoreGenericHost.CommonLib.Exceptions;
using GranDen.Orleans.NetCoreGenericHost.CommonLib.Helpers;
using GranDen.Orleans.NetCoreGenericHost.CommonLib.HostTypedOptions;
using GranDen.Orleans.Server.SharedInterface;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Hosting;
using Orleans.Statistics;

namespace GranDen.Orleans.NetCoreGenericHost.CommonLib
{
    /// <summary>
    /// AssemblyLoadContext cache for plugin dll
    /// </summary>
    public class AssemblyResolveCache
    {
        /// <summary>
        /// PlugIn's <code>AssemblyLoadContext</code>
        /// </summary>
        public AssemblyLoadContext AssemblyLoadContext { get; set; }

        /// <summary>
        /// Event handler for hook on Default AssemblyLoadContext's <code>Resolving</code> event handler
        /// </summary>
        public Func<AssemblyLoadContext, AssemblyName, Assembly> ResolvingHandler { get; set; }

    }

    public static partial class OrleansSiloBuilderExtension
    {
        /// <summary>
        /// AssemblyLoadContext References for non-Main Executable folder assemblies.
        /// </summary>
        public static Dictionary<string, AssemblyResolveCache> PluginAssemblyLoadContextCache { get; private set; }

        

        #region SiloBuilder Internal Configuration Methods

        private static ISiloBuilder ApplyOrleansDashboard(this ISiloBuilder siloBuilder, OrleansDashboardOption orleansDashboard, ILogger logger)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                siloBuilder.UseLinuxEnvironmentStatistics();
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                siloBuilder.UsePerfCounterEnvironmentStatistics();
            }

            logger.LogInformation($"Enable Orleans Dashboard (https://github.com/OrleansContrib/OrleansDashboard) on this host {orleansDashboard.Port} port");
            siloBuilder.UseDashboard(options =>
            {
                options.Port = orleansDashboard.Port;
            });

            return siloBuilder;
        }

        #endregion

        #region Assembly Path Resolver

        private static string PathResolver(string path)
        {
            if (!path.Contains("{GrainLoadPath}"))
            {
                return Path.GetFullPath(path, AssemblyUtil.GetCurrentAssemblyFolder());
            }

            var loadPathStr = Environment.GetEnvironmentVariable("GrainLoadPath");
            if (string.IsNullOrEmpty(loadPathStr))
            {
                return Path.GetFullPath(path, AssemblyUtil.GetCurrentAssemblyFolder());
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
            AssemblyLoadContext assemblyLoadContext;
            if (!PluginAssemblyLoadContextCache.ContainsKey(fullPath))
            {
                assemblyLoadContext = AssemblyUtil.GetPluginAssemblyLoadContext(fullPath);
                
                var defaultContext = AssemblyLoadContext.Default;

                Assembly OnResolving(AssemblyLoadContext context, AssemblyName name)
                {
                    var targetAssembly = context.LoadFromAssemblyPath(fullPath);
                    if (targetAssembly == null)
                    {
                        targetAssembly = assemblyLoadContext.LoadFromAssemblyPath(fullPath);
                    }

                    return targetAssembly;
                }

                defaultContext.Resolving += OnResolving;

                PluginAssemblyLoadContextCache.TryAdd(fullPath, new AssemblyResolveCache
                {
                    AssemblyLoadContext = assemblyLoadContext,
                    ResolvingHandler = OnResolving,
                });
            }
            else
            {
                assemblyLoadContext = PluginAssemblyLoadContextCache[fullPath].AssemblyLoadContext;
            }

            var assembly = assemblyLoadContext.LoadFromAssemblyPath(fullPath);

            return assembly;
        }

        #endregion
    }
}
