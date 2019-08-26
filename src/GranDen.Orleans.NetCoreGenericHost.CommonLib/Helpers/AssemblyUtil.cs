using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace GranDen.Orleans.NetCoreGenericHost.CommonLib.Helpers
{
    internal static class AssemblyUtil
    {
        public static string GetMainAssemblyFolder()
        {
            return System.IO.Path.GetDirectoryName(GetMainAssemblyPath());
        }

        public static string GetMainAssemblyPath()
        {
            return Assembly.GetExecutingAssembly().Location;
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
