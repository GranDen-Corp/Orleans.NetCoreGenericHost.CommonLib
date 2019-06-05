using GranDen.Orleans.Server.SharedInterface;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Orleans.ApplicationParts;
using System;

namespace GrainDemo.Hello
{
    public class HelloGrainServiceConfigure : IGrainServiceConfigDelegate
    {
        public Action<IApplicationPartManager> AppPartConfigurationAction =>
            (part) =>
            {
                part.AddDynamicPart(typeof(Greeter).Assembly);
            };

        public Action<HostBuilderContext, IServiceCollection> ServiceConfigurationAction =>
            (ctx, service) =>
            {
                service.AddTransient<IGreeter, Greeter>();
            };
    }
}
