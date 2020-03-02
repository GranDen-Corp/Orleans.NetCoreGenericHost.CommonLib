using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using Serilog.Exceptions;
using Serilog.Sinks.SystemConsole.Themes;

namespace StubCodeGenDemoClient
{
    class Program
    {
        static async Task Main(string[] _)
        {
            Log.Logger = new LoggerConfiguration()
                //Turn off Orleans Statistics: ^^^ noisy logs
                .MinimumLevel.Override("Orleans.RuntimeClientLogStatistics", LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .Enrich.WithProcessId()
                .Enrich.WithProcessName()
                .Enrich.WithThreadId()
                .Enrich.WithExceptionDetails()
                .WriteTo.Console(theme: AnsiConsoleTheme.Code)
                .WriteTo.Debug()
                .CreateLogger();

            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);

            var serviceProvider = serviceCollection.BuildServiceProvider();
            var logger = GetLogger<Program>(serviceProvider);

            logger.LogInformation("Press space key to start demo");
            WaitForKey(ConsoleKey.Spacebar);

            try
            {
                var demo = serviceProvider.GetService<CallGrainDemo>();
                await demo.DemoRun();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "error occured!");
                throw;
            }

            logger.LogInformation("Press enter to exit");
            Console.ReadLine();
        }

        private static void WaitForKey(ConsoleKey key)
        {
            do
            {
                while (!Console.KeyAvailable)
                {
                    //wait
                    Task.Delay(new TimeSpan(0, 0, 1)).Wait();
                }
            } while (Console.ReadKey(true).Key != key);
        }

        private static void ConfigureServices(ServiceCollection services)
        {
            services.AddLogging(configure => configure.AddSerilog());
            services.AddTransient<CallGrainDemo>();
        }

        private static ILogger<T> GetLogger<T>(ServiceProvider serviceProvider)
        {
            return serviceProvider.GetService<ILogger<T>>();
        }
    }
}
