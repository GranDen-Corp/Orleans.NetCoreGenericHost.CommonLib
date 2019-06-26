using System.Collections.Generic;

namespace GranDen.Orleans.NetCoreGenericHost.CommonLib.HostTypedOptions
{
    /// <summary>
    /// Typed Configuration class for Orleans Silo Host loading Orlean Grain's assembly files
    /// </summary>
    public class GrainLoadOption
    {
        /// <summary>
        /// Set this to true to run Service Delegate method(s) that are inside the main Silo Execution Assembly
        /// </summary>
        public bool CallMainExecutionPathServiceConfigDelegate { get; set; } = false;

        /// <summary>
        /// Assembly load paths, can add <c>{GrainLoadPath}</c> so that will be variable paths by assign environment variable "GrainLoadPath"
        /// </summary>
        public List<string> LoadPaths { get; } = new List<string>();

        /// <summary>
        /// Assembly Full type name for not load classes.
        /// </summary>
        public List<string> ExcludedTypeFullNames { get; } = new List<string>();
    }
}
