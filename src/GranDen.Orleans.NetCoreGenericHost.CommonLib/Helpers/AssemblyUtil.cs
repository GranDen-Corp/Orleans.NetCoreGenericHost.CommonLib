using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using McMaster.NETCore.Plugins.Loader;

namespace GranDen.Orleans.NetCoreGenericHost.CommonLib.Helpers
{
    internal static class AssemblyUtil
    {
        public static string GetCurrentAssemblyFolder()
        {
            return System.IO.Path.GetDirectoryName(GetMainAssemblyPath());
        }

        public static string GetMainAssemblyPath()
        {
            return Assembly.GetExecutingAssembly().Location;
        }

        public static AssemblyLoadContext GetPluginAssemblyLoadContext(string path)
        {
            var extensionDllFolder = Path.GetDirectoryName(path);
            var extensionDepJsonPath = Path.Combine(extensionDllFolder, Path.GetFileNameWithoutExtension(path) + ".deps.json");
            var extensionRuntimeJsonPath =
                Path.Combine(extensionDllFolder, Path.GetFileNameWithoutExtension(path) + ".runtimeconfig.json");

            var assemblyLoadContextBuilder = (new AssemblyLoadContextBuilder())
                .SetMainAssemblyPath(path)
                .AddProbingPath(extensionDllFolder)
                .AddDependencyContext(extensionDepJsonPath)
                .TryAddAdditionalProbingPathFromRuntimeConfig(extensionRuntimeJsonPath, true, out _)
                .PreferDefaultLoadContext(true);

            var assemblyLoadContext = assemblyLoadContextBuilder.Build();

            return assemblyLoadContext;
        }

        public static IEnumerable<Type> GetLoadableTypes(this Assembly assembly)
        {
            if (assembly == null) { throw new ArgumentNullException(nameof(assembly)); }
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException e)
            {
                return e.Types.Where(t => t != null);
            }
        }
    }
}
