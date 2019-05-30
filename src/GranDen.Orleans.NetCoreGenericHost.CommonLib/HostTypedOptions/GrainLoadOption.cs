using System.Collections.Generic;

namespace GranDen.Orleans.NetCoreGenericHost.CommonLib.HostTypedOptions
{
    /// <summary>
    /// Typed Configuration class for Orleans Silo Host loading Orlean Grain's assembly files
    /// </summary>
    public class GrainLoadOption
    {
        /// <summary>
        /// Assembly load pathes, can add <c>{GrainLoadPath}</c> so that will be variable pathes by assign environment varaible "GrainLoadPath"
        /// </summary>
        public List<string> LoadPaths { get; } = new List<string>();

        /// <summary>
        /// Assembly Full type name for not load classes.
        /// </summary>
        public List<string> ExcludedTypeFullNames { get; } = new List<string>();
    }
}
