﻿using GranDen.Orleans.Server.SharedInterface;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using Orleans.CodeGeneration;
using RpcShareInterface;

[assembly: KnownAssembly(typeof(IHello))]
namespace GrainDemo.Hello
{
    // ReSharper disable once UnusedMember.Global
    public class HelloGrainServiceConfigure : AbstractServiceConfigDelegate<HelloGrain>
    {
        public override Action<HostBuilderContext, IServiceCollection> ServiceConfigurationAction =>
            (ctx, service) =>
            {
                service.AddTransient<IGreeter, Greeter>();
            };
    }
}
