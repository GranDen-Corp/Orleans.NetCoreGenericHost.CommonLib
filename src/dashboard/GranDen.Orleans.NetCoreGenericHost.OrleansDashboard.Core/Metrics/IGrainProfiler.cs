using System;
using System.Runtime.CompilerServices;

namespace GranDen.Orleans.NetCoreGenericHost.OrleansDashboard.Metrics
{
    public interface IGrainProfiler
    {
        void Track(double elapsedMs, Type grainType, [CallerMemberName] string methodName = null, bool failed = false);
    }
}
