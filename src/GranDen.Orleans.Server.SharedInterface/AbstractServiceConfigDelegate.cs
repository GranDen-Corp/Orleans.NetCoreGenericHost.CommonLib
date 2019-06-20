using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Orleans.ApplicationParts;

namespace GranDen.Orleans.Server.SharedInterface
{
    /// <summary>
    /// Helper class for easier configure Dependency Injection
    /// </summary>
    public abstract class AbstractServiceConfigDelegate : IGrainServiceConfigDelegate
    {
        /// <inheritdoc />
        public virtual Action<IApplicationPartManager> AppPartConfigurationAction =>
            (part) =>
            {
                //default doesn't do anything
            };

        /// <inheritdoc />
        public abstract Action<HostBuilderContext, IServiceCollection> ServiceConfigurationAction { get; }
    }
}
