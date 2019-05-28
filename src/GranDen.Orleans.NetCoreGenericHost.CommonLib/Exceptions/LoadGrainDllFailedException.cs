using System;

namespace GranDen.Orleans.NetCoreGenericHost.CommonLib.Exceptions
{
    public class LoadGrainDllFailedException : Exception
    {
        public string AssemblyName { get; }

        public LoadGrainDllFailedException(string assemblyName)
        {
            AssemblyName = assemblyName;
        }
    }
}
