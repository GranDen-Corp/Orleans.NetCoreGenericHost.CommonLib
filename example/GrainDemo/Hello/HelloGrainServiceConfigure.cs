using GranDen.Orleans.Server.SharedInterface;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Orleans.ApplicationParts;
using System;

namespace GrainDemo.Hello
{
    // ReSharper disable once UnusedMember.Global
    public class HelloGrainServiceConfigure : AbstractServiceConfigDelegate
    {
        public override Action<IApplicationPartManager> AppPartConfigurationAction =>
            (part) =>
            {
                part.AddDynamicPart(typeof(Greeter).Assembly);
            };

        public override Action<HostBuilderContext, IServiceCollection> ServiceConfigurationAction =>
            (ctx, service) =>
            {
                service.AddTransient<IGreeter, Greeter>();
            };
    }
}
