using System;

namespace GranDen.Orleans.NetCoreGenericHost.OrleansDashboard
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true)]
    public sealed class NoProfilingAttribute : Attribute
    {
    }
}
