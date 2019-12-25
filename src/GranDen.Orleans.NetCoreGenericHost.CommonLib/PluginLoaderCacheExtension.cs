using System.Collections.Generic;
using McMaster.NETCore.Plugins;

namespace GranDen.Orleans.NetCoreGenericHost.CommonLib
{
    /// <summary>
    /// Helper method for manipulate dynamic load module
    /// </summary>
    public static class PluginLoaderCacheExtension
    {
        /// <summary>
        /// Dispose dynamic loaded Plugin Loader
        /// </summary>
        /// <param name="plugInLoaderCache"></param>
        public static void Dispose(this Dictionary<string, PluginLoader> plugInLoaderCache)
        {
            if(plugInLoaderCache != null)
            {
                foreach (var kv in plugInLoaderCache)
                {
                    kv.Value.Dispose();
                }
            }
        }

    }
}
