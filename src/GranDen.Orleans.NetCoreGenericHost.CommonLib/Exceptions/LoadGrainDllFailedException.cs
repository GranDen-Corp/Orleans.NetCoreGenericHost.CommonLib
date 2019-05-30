using System;

namespace GranDen.Orleans.NetCoreGenericHost.CommonLib.Exceptions
{
    /// <summary>
    /// Throws if any configured grain dll path load assembly failed.
    /// </summary>
    public class LoadGrainDllFailedException : Exception
    {
        /// <summary>
        /// Load failed assembly name
        /// </summary>
        public string AssemblyName { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="assemblyName"></param>
        public LoadGrainDllFailedException(string assemblyName)
        {
            AssemblyName = assemblyName;
        }
    }
}
