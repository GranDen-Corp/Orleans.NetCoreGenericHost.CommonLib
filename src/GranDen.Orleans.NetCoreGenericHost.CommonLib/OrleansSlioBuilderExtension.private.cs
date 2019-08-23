using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using GranDen.Orleans.NetCoreGenericHost.CommonLib.Exceptions;
using GranDen.Orleans.NetCoreGenericHost.CommonLib.Helpers;
using GranDen.Orleans.NetCoreGenericHost.CommonLib.HostTypedOptions;
using GranDen.Orleans.Server.SharedInterface;
using McMaster.NETCore.Plugins;
using McMaster.NETCore.Plugins.Loader;
using Orleans.ApplicationParts;

namespace GranDen.Orleans.NetCoreGenericHost.CommonLib
{
    public static partial class OrleansSiloBuilderExtension
    {
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

        private static void ConfigOtherFolderGrainLoad(IApplicationPartManager applicationPartManager, IEnumerable<string> pathsList, Func<string, string> pathResolveFunc)
        {
            foreach (var path in pathsList)
            {
                var fullPath = pathResolveFunc(path);
                var assembly = GetAssemblyUsingMcMasterPlugin(fullPath);
                applicationPartManager.AddDynamicPart(assembly);
            }
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
                var assemblyDll = GetAssemblyUsingMcMasterPlugin(fullPath);

                var types = assemblyDll.GetLoadableTypes();

                //List<Type> needServiceConfigureClasses = new List<Type>();

                //foreach (var type in assemblyDll.GetLoadableTypes())
                //{
                //    if (type.GetInterface(nameof(IGrainServiceConfigDelegate)) != null)
                //    {
                //        if (type.IsClass && !type.IsAbstract)
                //        {
                //            if (!excludedTypeFullNames.Contains(type.FullName))
                //            {
                //                needServiceConfigureClasses.Add(type);
                //            }
                //        }
                //    }

                //}

                var needServiceConfigureClasses = types.Where(x =>
                        typeof(IGrainServiceConfigDelegate).IsAssignableFrom(x)
                        && !x.IsAbstract
                        && x.IsClass
                        && !excludedTypeFullNames.Contains(x.FullName)).ToList();

                foreach (var serviceConfigureClass in needServiceConfigureClasses)
                {
                    //var serviceConfigDelegate = (IGrainServiceConfigDelegate) Activator.CreateInstance(serviceConfigureClass);

                    if (!(Activator.CreateInstance(serviceConfigureClass) is IGrainServiceConfigDelegate serviceConfigDelegate))
                    {
                        throw new LoadGrainDllFailedException(serviceConfigureClass.FullName);
                    }

                    ret.Add(serviceConfigDelegate);
                }
            }
            return ret;
        }

        private static Assembly GetAssemblyUsingMcMasterPlugin(string path)
        {
            var extensionDllFolder = Path.GetDirectoryName(path);
            var extensionDepJsonPath = Path.Combine(extensionDllFolder, Path.GetFileNameWithoutExtension(path) + ".deps.json");

            var assemblyLoader = (new AssemblyLoadContextBuilder())
                .SetBaseDirectory(AssemblyUtil.GetMainAssemblyPath())
                .AddProbingPath(extensionDllFolder)
                .AddDependencyContext(extensionDepJsonPath)
                
                .PreferDefaultLoadContext(true)
                .Build();

            var assembly = assemblyLoader.LoadFromAssemblyPath(path);

            return assembly;
        }

        #endregion
    }
}
