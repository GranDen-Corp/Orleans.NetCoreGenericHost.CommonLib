using System.Collections.Generic;

namespace GranDen.Orleans.NetCoreGenericHost.CommonLib.HostTypedOptions
{
    public class GrainLoadOption
    {
        public List<string> LoadPaths { get; } = new List<string>();

        public List<string> ExcludedTypeFullNames { get; } = new List<string>();
    }
}
