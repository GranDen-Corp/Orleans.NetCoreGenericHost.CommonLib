using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace GranDen.Orleans.NetCoreGenericHost.CommonLib.Helpers
{
    class DefaultLoggerHelper
    {
        private readonly ServiceProvider _serviceProvider;

        public DefaultLoggerHelper(IServiceCollection serviceCollection = null)
        {
            var serviceCollection1 = serviceCollection ?? new ServiceCollection();
            serviceCollection1.AddLogging(DefaultLogAction);
            
            _serviceProvider = serviceCollection1.BuildServiceProvider();
        }

        public static Action<ILoggingBuilder> DefaultLogAction => (logBuilder) =>
        {
            logBuilder.AddConsole();
            logBuilder.AddDebug();
            logBuilder.AddEventSourceLogger();

            //because this management grain is very noisy when using Orleans Dashboard
            logBuilder.AddFilter("Orleans.Runtime.Management.ManagementGrain", LogLevel.Warning)
                      .AddFilter("Orleans.Runtime.SiloControl", LogLevel.Warning);
        };

        public ILogger<T> CreateDefaultLogger<T>()
        {
            return _serviceProvider.GetService<ILogger<T>>();
        }

        public ILoggerFactory CreateDefaultLoggerFactory()
        {
            return LoggerFactory.Create(DefaultLogAction);
        }
    }
}
