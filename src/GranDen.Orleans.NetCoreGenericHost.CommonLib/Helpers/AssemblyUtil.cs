using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace GranDen.Orleans.NetCoreGenericHost.CommonLib.Helpers
{
    public static class AssemblyUtil
    {
        public static string GetCurrentAssemblyPath()
        {
            return System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        }

        public static IEnumerable<Type> GetLoadableTypes(this Assembly assembly)
        {
            if (assembly == null) throw new ArgumentNullException(nameof(assembly));
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
