using System;
using System.Collections.Generic;
using System.Text;
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
            serviceCollection1.AddLogging(logBuilder =>
            {
                logBuilder.AddConsole();
                logBuilder.AddDebug();
                logBuilder.AddEventSourceLogger();
            });
            
            _serviceProvider = serviceCollection1.BuildServiceProvider();
        }

        public ILogger<T> CreateDefaultLogger<T>()
        {
            return _serviceProvider.GetService<ILogger<T>>();
        }
    }
}
