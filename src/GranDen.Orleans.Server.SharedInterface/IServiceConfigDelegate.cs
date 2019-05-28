using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace GranDen.Orleans.Server.SharedInterface
{
    public interface IServiceConfigDelegate
    {
        /// <summary>
        /// Define Orleans Grain's DI Service registration implementation.
        /// </summary>
        Action<IServiceCollection> ServiceConfigurationAction { get; }
    }

    /// <summary>
    /// To make Orleans's Grain project be able to use DI services in Grain implementation.
    /// </summary>
    public interface IGrainServiceConfigDelegate : IServiceConfigDelegate
    { }
}
