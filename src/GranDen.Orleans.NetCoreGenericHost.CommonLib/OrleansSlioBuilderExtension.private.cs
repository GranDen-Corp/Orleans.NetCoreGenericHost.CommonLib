using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using GranDen.Orleans.NetCoreGenericHost.CommonLib.Exceptions;
using GranDen.Orleans.NetCoreGenericHost.CommonLib.Helpers;
using GranDen.Orleans.NetCoreGenericHost.CommonLib.HostTypedOptions;
using GranDen.Orleans.Server.SharedInterface;
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
                var dllFileInfo = new FileInfo(pathResolveFunc(path));
                var assembly = Assembly.LoadFile(dllFileInfo.FullName);
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

                    ret.Add(serviceConfigDelegate);
                }
            }
            return ret;
        }

        #endregion
    }
}
