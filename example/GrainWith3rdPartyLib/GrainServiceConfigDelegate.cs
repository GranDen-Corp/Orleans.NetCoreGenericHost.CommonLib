using System;
using GranDen.Orleans.Server.SharedInterface;
using IGrainWith3rdPartyLib;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Orleans.CodeGeneration;

[assembly: KnownAssembly(typeof(IUtilityGrain))]
namespace GrainWith3rdPartyLib
{
    // ReSharper disable once UnusedMember.Global
    public class GrainServiceConfigDelegate : AbstractServiceConfigDelegate<UtilityGrain>
    {
        // ReSharper disable once UnassignedGetOnlyAutoProperty
        public override Action<HostBuilderContext, IServiceCollection> ServiceConfigurationAction { get; }
    }
}