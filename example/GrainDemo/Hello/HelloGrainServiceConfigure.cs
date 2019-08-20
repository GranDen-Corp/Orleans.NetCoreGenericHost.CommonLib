﻿using GranDen.Orleans.Server.SharedInterface;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

namespace GrainDemo.Hello
{
    // ReSharper disable once UnusedMember.Global
    public class HelloGrainServiceConfigure : AbstractServiceConfigDelegate<Greeter>
    {
        public override Action<HostBuilderContext, IServiceCollection> ServiceConfigurationAction =>
            (ctx, service) =>
            {
                service.AddTransient<IGreeter, Greeter>();
            };
    }
}
