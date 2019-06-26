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
        /// <summary>
        /// ApplicationPartManager configuration implementation, if Grain project is directly reference from Silo, it is not necessary to implement this method.
        /// </summary>
        public virtual Action<IApplicationPartManager> AppPartConfigurationAction =>
            part =>
            {
                //default doesn't do anything
            };

        /// <summary>
        /// Define Orleans Grain's DI Service registration implementation, if the Grain has using .net core default Microsoft.Extensions.DependencyInjection DI mechanism.
        /// </summary>
        public abstract Action<HostBuilderContext, IServiceCollection> ServiceConfigurationAction { get; }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class AbstractServiceConfigDelegate<T> : AbstractServiceConfigDelegate
    {
        /// <summary>
        /// Default ApplicationPartManager configuration implementation
        /// </summary>
        public override Action<IApplicationPartManager> AppPartConfigurationAction =>
            part =>
            {
                part.AddDynamicPart(typeof(T).Assembly);
            };
    }
}
