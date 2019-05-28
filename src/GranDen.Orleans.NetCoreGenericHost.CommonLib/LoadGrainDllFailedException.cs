using System;
using System.Collections.Generic;
using System.Text;

namespace GranDen.Orleans.NetCoreGenericHost.CommonLib
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
