using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;

namespace DemoConsoleClient
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration().WriteTo.Console(theme: AnsiConsoleTheme.Code).CreateLogger();

            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);

            var serviceProvider = serviceCollection.BuildServiceProvider();

            var logger = GetLogger<Program>(serviceProvider);

            logger.LogInformation("Press space key to start demo");
            do
            {
                while (!Console.KeyAvailable)
                {
                    //wait
                }
            } while (Console.ReadKey(true).Key != ConsoleKey.Spacebar);

            try
            {
                //var demo = serviceProvider.GetService<AccessCounterDemo>();
                //await demo.RunCounter();

                var demo2 = serviceProvider.GetService<CallGrainWith3rdPartyLibDemo>();
                await demo2.TestRpc();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "error occured!");
                throw;
            }

            logger.LogInformation("Press enter to exit");
            Console.ReadLine();
        }

        private static void ConfigureServices(ServiceCollection services)
        {
            services.AddLogging(configure => configure.AddSerilog());

            services.AddTransient<AccessCounterDemo>();
            services.AddTransient<CallGrainWith3rdPartyLibDemo>();
        }

        private static ILogger<T> GetLogger<T>(IServiceProvider serviceProvider)
        {
            return serviceProvider.GetService<ILogger<T>>();
        }
    }
}
