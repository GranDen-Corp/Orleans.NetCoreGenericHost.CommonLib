using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Exceptions;
using Serilog.Sinks.SystemConsole.Themes;

namespace DemoClient
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
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

            var demo = serviceProvider.GetService<AccessCounterDemo>();
            var demo2 = serviceProvider.GetService<CallGrainWith3rdPartyLibDemo>();

            var logger = GetLogger<Program>(serviceProvider);

            logger.LogInformation("Press space key to start demo");
            do
            {
                while (!Console.KeyAvailable)
                {
                    //wait
                    await Task.Delay(new TimeSpan(0, 0, 1));
                }
            } while (Console.ReadKey(true).Key != ConsoleKey.Spacebar);

            try
            {
                await demo.RunCounter();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "error occured!");
                throw;
            }

            logger.LogInformation("\r\n===\r\nPress space key to Run 2nd Demo\r\n===");

            do
            {
                while (!Console.KeyAvailable)
                {
                    //wait
                    await Task.Delay(new TimeSpan(0, 0, 1));
                }
            } while (Console.ReadKey(true).Key != ConsoleKey.Spacebar);

            try
            {
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

        private static ILogger<T> GetLogger<T>(ServiceProvider serviceProvider)
        {
            return serviceProvider.GetService<ILogger<T>>();
        }
    }
}