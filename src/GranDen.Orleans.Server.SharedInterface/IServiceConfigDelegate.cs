using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Orleans.ApplicationParts;
using System;
using System.Diagnostics;

namespace GranDen.Orleans.Server.SharedInterface
{
    /// <summary>
    /// .NET Core Generic Host Dependency Injection implementation interface
    /// </summary>
    public interface IServiceConfigDelegate
    {
        /// <summary>
        /// Define Orleans Grain's DI Service registration implementation.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
        Action<HostBuilderContext, IServiceCollection> ServiceConfigurationAction { get; }
    }

    /// <summary>
    /// To make Orleans's Grain project be able to use DI services in Grain implementation.
    /// </summary>
    public interface IGrainServiceConfigDelegate : IServiceConfigDelegate
    {
        /// <summary>
        /// ApplicationPartManager configuration implementation
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
        Action<IApplicationPartManager> AppPartConfigurationAction { get; }
    }
}
